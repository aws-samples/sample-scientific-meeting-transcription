"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
 """

from typing import Sequence
from aws_cdk import (
    Duration,
    Stack,
    aws_stepfunctions as sfn,
    aws_stepfunctions_tasks as tasks,
)
from cdk_nag import NagSuppressions
from constructs import Construct

from exscribo_constructs.statemachines_lambda import StatemachinesLambda
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.security import SecurityConstruct

class CustomVocabularyWorkflow(Construct):
    def __init__(self, scope: Construct, id: str,                 
                lambdaConstruct: StatemachinesLambda,
                securityConstruct: SecurityConstruct,
                storageConstruct: StorageConstruct
        ) -> None:
        super().__init__(scope, id)


        process_prompts = tasks.LambdaInvoke(
            self, "ProcessCustomVocabulary",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            result_path="$.taskResult"
        )


        self.state_machine = sfn.StateMachine(
            self, "CustomVocabularyProcessingWorkflow",
            state_machine_name="CustomVocabularyProcessingWorkflow",
            definition_body=sfn.DefinitionBody.from_chainable(process_prompts),
            logs=sfn.LogOptions(
                destination=securityConstruct.workflow_logs,
                level=sfn.LogLevel.ALL
            ),
            timeout=Duration.hours(24),
            tracing_enabled=True
        )

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/Custom Vocabulary Workflow/CustomVocabularyProcessingWorkflow/Role/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "CDK created role for StepFunction"
                }
            ]
        )

        securityConstruct.stepfunction_lambda_service_role.grant(self.state_machine.role, "lambda:InvokeFunction")

        self.state_machine_arn = self.state_machine.state_machine_arn