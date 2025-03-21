"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    Duration,
    Stack,
    aws_stepfunctions as sfn,
    aws_stepfunctions_tasks as tasks,
    aws_iam as iam,
    aws_ssm as ssm,
)
from cdk_nag import NagSuppressions
from constructs import Construct

from exscribo_constructs.statemachines_lambda import StatemachinesLambda
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.security import SecurityConstruct

class CustomModelWorkflow(Construct):
    def __init__(self, scope: Construct, id: str,                 
                lambdaConstruct: StatemachinesLambda,
                securityConstruct: SecurityConstruct,
                storageConstruct: StorageConstruct
        ) -> None:
        super().__init__(scope, id)

        # Check if model exists using ListLanguageModels
        check_model_exists = tasks.CallAwsService(
            self, "CheckIfModelExists",
            service="transcribe",
            action="listLanguageModels",
            parameters={
                "NameContains.$": "$.custommodel.custommodel.id"
            },
            result_path="$.custommodel.existingModel",
            iam_resources=["*"]
        )

        # Delete existing model if found
        delete_existing_model = tasks.CallAwsService(
            self, "DeleteExistingModel",
            service="transcribe",
            action="deleteLanguageModel",
            parameters={
                "ModelName.$": "$.custommodel.custommodel.id"
            },
            result_path="$.custommodel.deleteResult",
            iam_resources=["*"]
        )

        # Wait for model deletion
        wait_for_deletion = sfn.Wait(
            self, "WaitForDeletion",
            time=sfn.WaitTime.duration(Duration.seconds(30))
        )

        # Verify model deletion using ListLanguageModels
        verify_deletion = tasks.CallAwsService(
            self, "VerifyDeletion",
            service="transcribe",
            action="listLanguageModels",
            parameters={
                "NameContains.$": "$.custommodel.custommodel.id"
            },
            result_path="$.custommodel.deletionCheck",
            iam_resources=["*"]
        )

        # Handle any errors
        handle_error = tasks.LambdaInvoke(
            self, "HandleError",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            payload=sfn.TaskInput.from_object({
                "step_function_job_type": "CustomModel",
                "custommodel": {
                    "custom_model_progress_status": "TrainingFailed",
                    "model_setup_message.$": "$.error",
                    "id.$": "$.custommodel.custommodel.id"
                }
            }),
            result_path=sfn.JsonPath.DISCARD
        )

        # Check active custom models
        check_active_models = tasks.CallAwsService(
            self, "CheckActiveModels",
            service="transcribe",
            action="listLanguageModels",
            parameters={},
            result_path="$.custommodel.modelStatus.activeModels",
            iam_resources=["*"]
        )

        # Process active models count
        get_training_model_count = sfn.Pass(
            self, "GetTrainingModelCount",
            parameters={
                "count.$": "States.ArrayLength($.custommodel.modelStatus.activeModels.Models[?(@.ModelStatus == 'IN_PROGRESS')])"
            },
            result_path="$.custommodel.modelStatus.customTrainingQueue"
        )

        # Wait state for queued jobs (10 minutes)
        wait_if_queued = sfn.Wait(
            self, "WaitIfQueued",
            time=sfn.WaitTime.duration(Duration.seconds(300))
        )

        # Update Database for Queued Status
        update_queued_status = tasks.LambdaInvoke(
            self, "UpdateQueuedStatus",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            payload=sfn.TaskInput.from_object({
                "step_function_job_type": "CustomModel",
                "custommodel": {
                    "custom_model_progress_status": "TrainingQueued",
                    "id.$": "$.custommodel.custommodel.id"
                }
            }),
            result_path=sfn.JsonPath.DISCARD,
            retry_on_service_exceptions=True
        )

        # Start Custom Language Model Training
        start_custom_model_training = tasks.CallAwsService(
            self, "StartCustomLanguageModel",
            service="transcribe",
            action="createLanguageModel",
            parameters={
                "ModelName.$": "$.custommodel.custommodel.id",
                "BaseModelName": "WideBand",
                "InputDataConfig": {
                    "DataAccessRoleArn": f"{storageConstruct.transcribe_role.role_arn}",
                    "S3Uri.$": "$.custommodel.custommodel.training_data_s3_uri_folder"
                },
                "LanguageCode.$": "$.custommodel.custommodel.language_code"
            },
            result_path="$.custommodel.modelStatus",
            iam_resources=["*"]
        )

        # Wait state before checking status
        wait_before_check = sfn.Wait(
            self, "WaitBeforeCheckingState",
            time=sfn.WaitTime.duration(Duration.seconds(60))
        )

        # Check Custom Language Model Status
        check_model_status = tasks.CallAwsService(
            self, "CheckCustomModelStatus",
            service="transcribe",
            action="describeLanguageModel",
            parameters={
                "ModelName.$": "$.custommodel.custommodel.id"
            },
            result_path="$.custommodel.modelStatus",
            iam_resources=["*"]
        )
        for task in [check_active_models, delete_existing_model, verify_deletion, start_custom_model_training, check_model_exists, check_model_status]:
            if isinstance(task, tasks.CallAwsService):
                task.add_catch(
                    errors=["States.ALL"],
                    result_path="$.error",
                    handler=handle_error
                )

        # Add retry policy for status check
        check_model_status.add_retry(
            errors=["ModelInUseException"],
            interval=Duration.seconds(120),
            max_attempts=120,
            backoff_rate=1.5
        )

        # Update Database Lambda Task
        update_database = tasks.LambdaInvoke(
            self, "UpdateCustomModelStatus",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            payload=sfn.TaskInput.from_object({
                "step_function_job_type": "CustomModel",
                "custommodel": {
                    "custom_model_progress_status": "ModelReady",
                    "id.$": "$.custommodel.custommodel.id"
                }
            })
        )

        # Create Success and Fail states
        success_state = sfn.Succeed(self, "SuccessState")
        fail_state = sfn.Fail(self, "FailState", cause="Custom Model Training Failed")

        # Choice state to check if model exists
        model_exists = sfn.Choice(self, "DoesModelExist?")
        model_exists.when(
            sfn.Condition.and_(
                sfn.Condition.is_present("$.custommodel.existingModel.Models[0]"),
                sfn.Condition.not_(sfn.Condition.is_null("$.custommodel.existingModel.Models[0]"))
            ),
            delete_existing_model
        ).otherwise(
            check_active_models
        )

        # Choice state to check if we can proceed
        can_proceed = sfn.Choice(self, "CanProceedWithTraining?")
        
        # Choice state to check if training is complete
        is_complete = sfn.Choice(self, "IsTrainingComplete?")

        # Define the conditions
        can_proceed.when(
            sfn.Condition.and_(
                sfn.Condition.is_present("$.custommodel.modelStatus.customTrainingQueue.count"),
                sfn.Condition.number_less_than("$.custommodel.modelStatus.customTrainingQueue.count", 3)
            ),
            start_custom_model_training
        ).otherwise(
            update_queued_status
        )

        # Modify the is_complete choice state
        is_complete.when(
            sfn.Condition.string_equals("$.custommodel.modelStatus.LanguageModel.ModelStatus", "COMPLETED"),
            update_database.next(success_state)
        ).otherwise(
            wait_before_check
        )

        # Add proper termination for handle_error
        handle_error.next(fail_state)

        # Add choice state for deletion verification
        deletion_complete = sfn.Choice(self, "IsDeletionComplete")
        deletion_complete.when(
            sfn.Condition.and_(
                sfn.Condition.is_present("$.custommodel.deletionCheck.Models[0]"),
                sfn.Condition.not_(sfn.Condition.is_null("$.custommodel.deletionCheck.Models[0]"))
            ),
            wait_for_deletion
        ).otherwise(
            check_active_models
        )


        # Chain the workflow
        check_model_exists.next(model_exists)
        delete_existing_model.next(wait_for_deletion)
        wait_for_deletion.next(verify_deletion)
        verify_deletion.next(deletion_complete)
        check_active_models.next(get_training_model_count)
        get_training_model_count.next(can_proceed)
        wait_if_queued.next(check_active_models)
        update_queued_status.next(wait_if_queued)
        start_custom_model_training.next(wait_before_check)
        wait_before_check.next(check_model_status)
        check_model_status.next(is_complete)



    

        # Create the state machine
        self.state_machine = sfn.StateMachine(
            self, "CustomModelTrainingWorkflow",
            state_machine_name="CustomModelTrainingWorkflow",
            definition_body=sfn.DefinitionBody.from_chainable(check_model_exists),
            logs=sfn.LogOptions(
                destination=securityConstruct.workflow_logs,
                level=sfn.LogLevel.ALL
            ),
            timeout=Duration.hours(12),
            tracing_enabled=True
        )

        # Add Transcribe permissions
        self.state_machine.add_to_role_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[            
                    "transcribe:DeleteLanguageModel",
                    "transcribe:ListLanguageModels",
                    "transcribe:DescribeLanguageModel",
                    "transcribe:CreateLanguageModel"
                ],
                resources=[
                    f"arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:languagemodel/*"
                ]
            )
        )
        self.state_machine.role.grant_pass_role(storageConstruct.transcribe_role)
        storageConstruct.transcribe_role.grant_pass_role(self.state_machine.role)

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/CustomModel Workflow/CustomModelTrainingWorkflow/Role/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Default CDK created role for StepFunction"
                }
            ]
        )
    
        securityConstruct.stepfunction_lambda_service_role.grant(self.state_machine.role, "lambda:InvokeFunction")
        storageConstruct.transcribe_role.grant_assume_role(self.state_machine)

        # Store state machine ARN in Parameter Store
        self.state_machine_arn = ssm.StringParameter(
            self, "CustomModelStateMachineARN",
            parameter_name="/exscribo/custom-model/state-machine-arn",
            string_value=self.state_machine.state_machine_arn,
        )
