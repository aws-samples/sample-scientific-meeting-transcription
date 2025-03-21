"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from typing import Sequence
from aws_cdk import (
    Duration,
    Stack,
    aws_stepfunctions as sfn,
    aws_stepfunctions_tasks as tasks
)
from cdk_nag import NagSuppressions
from constructs import Construct

from exscribo_constructs.statemachines_lambda import StatemachinesLambda
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.security import SecurityConstruct

class PromptWorkflow(Construct):
    def __init__(self, scope: Construct, id: str,                 
                lambdaConstruct: StatemachinesLambda,
                storageConstruct: StorageConstruct,
                securityConstruct: SecurityConstruct
        ) -> None:
        super().__init__(scope, id)

        # Process Single Prompt Task
        process_prompts = tasks.LambdaInvoke(
            self, "ProcessSinglePrompt",
            lambda_function=lambdaConstruct.stepfunctions_exscribo_lambda,
            result_path="$.taskResult"
        )

        # Create the state machine
        self.state_machine = sfn.StateMachine(
            self, "PromptProcessingWorkflow",
            state_machine_name="PromptProcessingWorkflow",
            definition_body=sfn.DefinitionBody.from_chainable(process_prompts),
            logs=sfn.LogOptions(
                destination=securityConstruct.workflow_logs,
                level=sfn.LogLevel.ALL
            ),
            timeout=Duration.hours(24),
            tracing_enabled=True
        )

        securityConstruct.stepfunction_lambda_service_role.grant(self.state_machine.role, "lambda:InvokeFunction")

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/Prompt Workflow/PromptProcessingWorkflow/Role/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Default CDK created role for StepFunction"
                }
            ]
        )

    