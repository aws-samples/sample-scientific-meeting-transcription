"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
 """

from aws_cdk import (
    Duration,
    Stack,
    aws_stepfunctions as sfn,
    aws_stepfunctions_tasks as tasks
)
from cdk_nag import NagSuppressions
from constructs import Construct

from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.statemachines_lambda import StatemachinesLambda
from exscribo_constructs.storage import StorageConstruct

class TranscribeAndPromptWorkflow(Construct):
    def __init__(self, scope: Construct, id: str,       
                lambdaConstruct: StatemachinesLambda,
                storageConstruct: StorageConstruct,
           	    securityConstruct: SecurityConstruct
              ) -> None:
        super().__init__(scope, id)
        

        # Check for existing transcription job
        check_transcription_job = tasks.CallAwsService(
            self, "CheckTranscriptionJob",
            service="transcribe",
            action="listTranscriptionJobs",
            parameters={
                "JobNameContains.$": "$.transcribe.transcription_job.TranscriptionJobName"
            },
            result_path="$.ExistingTranscriptionJob",
            iam_resources=["*"]
        )
        # Delete existing transcription job if found
        delete_transcription_job = tasks.CallAwsService(
            self, "DeleteTranscriptionJob",
            service="transcribe",
            action="deleteTranscriptionJob",
            parameters={
                "TranscriptionJobName.$": "$.transcribe.transcription_job.TranscriptionJobName"
            },
            result_path="$.DeletedTranscriptionJob",
            iam_resources=["*"]
        )

        # Wait after deletion
        wait_after_deletion = sfn.Wait(
            self, "WaitAfterDeletion",
            time=sfn.WaitTime.duration(Duration.seconds(5))
        )

        # Verify deletion
        verify_deletion = tasks.CallAwsService(
            self, "VerifyDeletion",
            service="transcribe",
            action="listTranscriptionJobs",
            parameters={
                "JobNameContains.$": "$.transcribe.transcription_job.TranscriptionJobName"
            },
            result_path="$.VerifyDeletion",
            iam_resources=["*"]
        )

        # Define Step Functions tasks
        start_transcribe = tasks.CallAwsService(
            self, "StartTranscribeJob",
            service="transcribe",
            action="startTranscriptionJob",
            parameters={
                "TranscriptionJobName.$": "$.transcribe.transcription_job.TranscriptionJobName",
                "LanguageCode.$": "$.transcribe.transcription_job.LanguageCode",
                "Media": {
                    "MediaFileUri.$": "$.transcribe.transcription_job.MediaFileUri"
                },
                "ModelSettings": {
                    "LanguageModelName.$": "$.transcribe.transcription_job.LanguageModelName"
                },
                "Settings": {
                    "ShowSpeakerLabels": True,
                    "MaxSpeakerLabels": 30,
                    "ChannelIdentification": False,
                    "ShowAlternatives": True,
                    "MaxAlternatives": 5,
                    "VocabularyName.$": "$.transcribe.transcription_job.VocabularyName"
                },
                "OutputBucketName.$": "$.transcribe.transcription_job.OutputBucketName",
                "OutputKey.$": "$.transcribe.transcription_job.OutputKey",
                "JobExecutionSettings": {
                    "AllowDeferredExecution": True,
                    "DataAccessRoleArn": storageConstruct.transcribe_role.role_arn
                }
            },
            result_path="$.TranscriptionJob",
            iam_resources=["*"]
        )

        wait_state = sfn.Wait(
            self, "WaitForTranscription",
            time=sfn.WaitTime.duration(Duration.seconds(10))
        )

        check_transcription_status = tasks.CallAwsService(
            self, "CheckTranscriptionStatus",
            service="transcribe",
            action="getTranscriptionJob",
            parameters={
                "TranscriptionJobName.$": "$.transcribe.transcription_job.TranscriptionJobName"
            },
            result_path="$.TranscriptionJob",
            iam_resources=["*"]
        )
                
        process_transcription = tasks.LambdaInvoke(
            self, "ProcessTranscription",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            payload=sfn.TaskInput.from_object({
                "step_function_job_type": "Transcribe",
                "transcribe": {
                    "current_step": "Transcribed",
                    "id.$": "$.transcribe.id",
                    "transcription_job.$": "$.transcribe.transcription_job"
                    }
            }),
        )

        handle_transcription_failure = tasks.LambdaInvoke(
            self, "HandleTranscriptionFailure",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            payload=sfn.TaskInput.from_object({
                "step_function_job_type": "Transcribe",
                    "transcribe": {
                        "current_step": "TranscribeFailed",
                        "transcribe_error.$": "$.error",
                        "id.$": "$.transcribe.id"
                    }
            }),
        )
        for task in [check_transcription_job, delete_transcription_job, verify_deletion, start_transcribe, check_transcription_status]:
            task.add_catch(
                errors=["States.ALL"],
                result_path="$.error",
                handler=handle_transcription_failure,
        )

        # Define Choice states
        job_exists = sfn.Choice(self, "DoesTranscriptionJobExist?")
        verify_deletion_choice = sfn.Choice(self, "IsJobDeleted?")
        check_job_status = sfn.Choice(self, "CheckJobStatus")

        # Define workflow
        job_exists.when(
            sfn.Condition.and_(
                sfn.Condition.is_present("$.ExistingTranscriptionJob.TranscriptionJobSummaries[0]"),
                sfn.Condition.not_(sfn.Condition.is_null("$.ExistingTranscriptionJob.TranscriptionJobSummaries[0]"))
            ),
            delete_transcription_job
        ).otherwise(
            start_transcribe
        )

        verify_deletion_choice.when(
            sfn.Condition.and_(
                sfn.Condition.is_present("$.VerifyDeletion.TranscriptionJobSummaries[0]"),
                sfn.Condition.not_(sfn.Condition.is_null("$.VerifyDeletion.TranscriptionJobSummaries[0]"))
            ),
            wait_after_deletion
        ).otherwise(
            start_transcribe
        )

        # Chain the workflow
        check_transcription_job.next(job_exists)
        delete_transcription_job.next(wait_after_deletion)
        wait_after_deletion.next(verify_deletion)
        verify_deletion.next(verify_deletion_choice)
        start_transcribe.next(wait_state)
        wait_state.next(check_transcription_status)
        check_transcription_status.next(check_job_status)

        check_job_status.when(
            sfn.Condition.string_equals("$.TranscriptionJob.TranscriptionJob.TranscriptionJobStatus", "COMPLETED"),
            process_transcription
        ).when(
            sfn.Condition.string_equals("$.TranscriptionJob.TranscriptionJob.TranscriptionJobStatus", "FAILED"),
            handle_transcription_failure
        ).otherwise(wait_state)
                
        # Create state machine with X-Ray tracing enabled
        self.state_machine = sfn.StateMachine(
            self, "TranscriptionWorkflow",
            state_machine_name="TranscriptionWorkflow",
            definition_body=sfn.DefinitionBody.from_chainable(check_transcription_job),
            logs=sfn.LogOptions(
                    destination=securityConstruct.workflow_logs,
                    level=sfn.LogLevel.ALL
                ),
            tracing_enabled=True 
        )        
        
        securityConstruct.stepfunction_lambda_service_role.grant(self.state_machine.role, "lambda:InvokeFunction")
        self.state_machine.role.grant_pass_role(storageConstruct.transcribe_role)
        storageConstruct.transcribe_role.grant_pass_role(self.state_machine.role)

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/ExscriboStorage/ExscriboTranscribeServiceRole/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "CDK created role for StepFunction",
                    "appliesTo": [
                        "Action::kms:GenerateDataKey*",
                        "Action::kms:ReEncrypt*"
                    ]
                }
            ]
        )
 
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/Transcription Workflow/TranscriptionWorkflow/Role/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "CDK created role for StepFunction"
                }
            ]
        )

