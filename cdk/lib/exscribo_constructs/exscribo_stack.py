"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    Stack,
)
import aws_cdk as cdk
from cdk_nag import NagSuppressions

from constructs import Construct
from exscribo_constructs.network import NetworkConstruct
from exscribo_constructs.database import DatabaseConstruct
from exscribo_constructs.prompt_workflow import PromptWorkflow
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.auth import AuthConstruct
from exscribo_constructs.transcription_workflow import TranscribeAndPromptWorkflow
from exscribo_constructs.api import ApiConstruct
from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.custommodel_workflow import CustomModelWorkflow
from exscribo_constructs.statemachines_lambda import StatemachinesLambda
from exscribo_constructs.bedrock_kb import BedrockKBConstruct
from exscribo_constructs.customvocabulary_workflow import CustomVocabularyWorkflow
from exscribo_constructs.sealmeeting_workflow import SealMeetingWorkflow

class ExscriboStack(Stack):
    def __init__(self, scope: Construct, construct_id: str, **kwargs) -> None:
        super().__init__(scope, construct_id, **kwargs)

        # Create network infrastructure
        networkConstruct = NetworkConstruct(self, "ExscriboNetwork")
        
        #Create security infrastructure first
        securityConstruct = SecurityConstruct(self, "ExscriboSecurity", 
            networkConstruct=networkConstruct
        )
        securityConstruct.node.add_dependency(networkConstruct)

        # # Create authentication infrastructure
        authenticationConstruct = AuthConstruct(self, "ExscriboAuthentication")




        # # Create storage infrastructure before database
        storageConstruct = StorageConstruct(self, "ExscriboStorage",
            securityConstruct=securityConstruct,
            networkConstruct=networkConstruct,

        )

        # Create database infrastructure after storage
        databaseConstruct = DatabaseConstruct(self, "Exscribo Database",
            networkConstruct=networkConstruct,
            securityConstruct=securityConstruct,
        )
        bedrockKBConstruct = BedrockKBConstruct(self, "Exscribo Bedrock KB",
            networkConstruct=networkConstruct,
            securityConstruct=securityConstruct,
            storageConstruct=storageConstruct,
            databaseConstruct=databaseConstruct
        )
        statemachines_lambdas = StatemachinesLambda(self, "Exscribo Statemachines Lambdas",
            networkConstruct=networkConstruct,
            securityConstruct=securityConstruct,
            databaseConstruct=databaseConstruct,
            storageConstruct=storageConstruct,
            bedrockKBConstruct=bedrockKBConstruct
        )             
        
        # # Pass only necessary Lambda ARNs instead of full construct
        transcription_workflow = TranscribeAndPromptWorkflow(self, "Transcription Workflow",
            lambdaConstruct=statemachines_lambdas,
            securityConstruct=securityConstruct,
            storageConstruct=storageConstruct
        )

        custommodel_workflow = CustomModelWorkflow(self, "CustomModel Workflow",
            lambdaConstruct=statemachines_lambdas,
            securityConstruct=securityConstruct,
            storageConstruct=storageConstruct
        )
        prompt_workflow = PromptWorkflow(self, "Prompt Workflow",
            lambdaConstruct=statemachines_lambdas,
            securityConstruct=securityConstruct,
            storageConstruct=storageConstruct
        )

        customvocabulary_workflow = CustomVocabularyWorkflow(self, "Custom Vocabulary Workflow",
            lambdaConstruct=statemachines_lambdas,
            securityConstruct=securityConstruct,
            storageConstruct=storageConstruct
            
        )
        sealmeeting_workflow = SealMeetingWorkflow(self, "Seal Meeting Workflow",
            lambdaConstruct=statemachines_lambdas,
            securityConstruct=securityConstruct,
            storageConstruct=storageConstruct
            
        )

 

        # # Create API infrastructure last, after all other resources
        api = ApiConstruct(self, "Exscribo Api",
            networkConstruct=networkConstruct,
            authenticationConstruct=authenticationConstruct,
            securityConstruct=securityConstruct,
            databaseConstruct=databaseConstruct,
            storageConstruct=storageConstruct,
            transcriptionWorkflowConstruct=transcription_workflow,
            custommodelWorkflowConstruct=custommodel_workflow,
            promptWorkflowConstruct=prompt_workflow,
            customVocabularyConstruct=customvocabulary_workflow,
            sealMeetingWorkflowConstruct=sealmeeting_workflow,
            bedrockKBConstruct=bedrockKBConstruct
        )
        
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path=[
                "/ExscriboStack/ExscriboSecurity/ExscriboStepFunctionLambdaRole/Resource",
                "/ExscriboStack/ExscriboSecurity/ExscriboStepFunctionLambdaRole/DefaultPolicy/Resource"
            ],
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "StepFunction Lambda function requires full access to Transcribe Service for jobs, custom models, vocabularies",
                    "appliesTo": [
                        f"Resource::arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:languagemodel/*",
                        f"Resource::arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:vocabulary/*",
                        f"Resource::arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:transcriptionjob/*",

                    ]
                }
            ],
            apply_to_children=True
        )
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path=[
                "/ExscriboStack/ExscriboSecurity/ExscriboLambdaRole/DefaultPolicy/Resource",
                "/ExscriboStack/ExscriboSecurity/ExscriboStepFunctionLambdaRole/DefaultPolicy/Resource"
            ],
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Default CDK Created Default Policy",
                    "appliesTo": [
                        f"Action::kms:ReEncrypt*",
                        f"Resource::*",
                        f"Action::kms:GenerateDataKey*",

                    ]
                }
            ],
            apply_to_children=True
        )
        
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/ExscriboSecurity/ExscriboLambdaRole/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM4",
                    "reason": "API Lambda function requires exection and VPC access",
                    "appliesTo": [
                        "Policy::arn:<AWS::Partition>:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
                    ]
                },
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Default CDK Created Default Policy",
                    "appliesTo": [
                        "Action::kms:GenerateDataKey*",
                        "Action::kms:ReEncrypt*",
                        "Resource::*"
                    ]
                }
            ]
        )
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/LogRetentionaae0aa3c5b4d4f87b02d85b201efdd8a/ServiceRole/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM4",
                    "reason": "Default LogRetention CDK Default Policy",
                    "appliesTo": [
                        "Policy::arn:<AWS::Partition>:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole",
                    ]
                }
            ]
        )
        
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/LogRetentionaae0aa3c5b4d4f87b02d85b201efdd8a/ServiceRole/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Default LogRetention CDK Default Policy",
                    "appliesTo": [
                        "Resource::*",
                    ]
                }
            ]
        )