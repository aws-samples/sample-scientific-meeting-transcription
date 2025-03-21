"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    Stack,
    aws_iam as iam,
    aws_kms as kms,
    aws_ec2 as ec2,
    RemovalPolicy,
    aws_logs as logs,
)
from constructs import Construct
from cdk_nag import NagSuppressions

from exscribo_constructs.network import NetworkConstruct

class SecurityConstruct(Construct):
    def __init__(self, scope: Construct, id: str, networkConstruct: NetworkConstruct) -> None:
        super().__init__(scope, id)
        
        # KMS key for encryption
        self.encryption_key = kms.Key(
            self, "ExscriboEncryptionKey",
            enable_key_rotation=True,
            removal_policy=RemovalPolicy.DESTROY
        )

        # Create Lambda role
        self.api_lambda_service_role = iam.Role(
            self, "ExscriboLambdaRole",
            assumed_by=iam.ServicePrincipal("lambda.amazonaws.com"),
            managed_policies=[
                iam.ManagedPolicy.from_aws_managed_policy_name("service-role/AWSLambdaVPCAccessExecutionRole"),
            ],
            role_name="ExscriboLambdaRole",
        )
    

        self.stepfunction_lambda_service_role = iam.Role(
            self, "ExscriboStepFunctionLambdaRole",
            assumed_by=iam.ServicePrincipal("lambda.amazonaws.com"),
            managed_policies=[
                iam.ManagedPolicy.from_aws_managed_policy_name("service-role/AWSLambdaVPCAccessExecutionRole"),
            ],
            role_name="ExscriboStepFunctionLambdaRole"
        )

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/ExscriboSecurity/ExscriboStepFunctionLambdaRole/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM4",
                    "reason": "StepFunction Lambda function requires exection and VPC access",
                    "appliesTo": [
                        "Policy::arn:<AWS::Partition>:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
                    ]
                }
            ]
        )



        # Add Transcribe permissions
        self.stepfunction_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "transcribe:CreateVocabulary",
                    "transcribe:ListVocabularies",
                    "transcribe:UpdateVocabulary",                
                    "transcribe:DeleteVocabulary",                    
                    "transcribe:GetVocabulary",
                    "transcribe:DeleteLanguageModel",
                    "transcribe:ListLanguageModels",
                    "transcribe:CreateLanguageModel",
                    "transcribe:GetLanguageModel",
                    "transcribe:GetTranscriptionJob",
                    "transcribe:StartTranscriptionJob",
                    "transcribe:ListTranscriptionJobs",
                    "transcribe:GetTranscriptionJob",
                ],
                resources=[
                    f"arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:languagemodel/*",
                    f"arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:vocabulary/*",
                    f"arn:aws:transcribe:{Stack.of(self).region}:{Stack.of(self).account}:transcriptionjob/*"

                ]
            )
        )


        self.stepfunction_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "bedrock:GetFoundationModel",
                    "bedrock:InvokeModel",
                    "bedrock:StartIngestionJob",
                    "bedrock:GetKnowledgeBaseDocuments"
                ],
                resources=[
                    f"arn:aws:bedrock:*::foundation-model/anthropic.claude-3-7-sonnet-20250219-v1:0",
                    f"arn:aws:bedrock:*:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0"
                ]
            )
        )   
        self.api_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "bedrock:GetFoundationModel",
                    "bedrock:InvokeModel",
                    "bedrock:DeleteKnowledgeBaseDocuments",
                    "bedrock:GetKnowledgeBaseDocuments"
                ],
                resources=[
                    f"arn:aws:bedrock:{Stack.of(self).region}::foundation-model/amazon.titan-embed-text-v2:0",
                    f"arn:aws:bedrock:{Stack.of(self).region}::foundation-model/anthropic.claude-3-haiku-20240307-v1:0",
                    f"arn:aws:bedrock:*::foundation-model/anthropic.claude-3-7-sonnet-20250219-v1:0",
                    f"arn:aws:bedrock:*:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0"
                ]
            )
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
                    "reason": "Cross region claude access allowed",
                    "appliesTo": [
                        "Resource::arn:aws:bedrock:*::foundation-model/anthropic.claude-3-7-sonnet-20250219-v1:0",
                        f"Resource::arn:aws:bedrock:*:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0"
                    ]
                }
            ]
        )
 
        
        # Add KMS permissions to Lambda role
        self.encryption_key.grant_decrypt(self.api_lambda_service_role)
        self.encryption_key.grant_decrypt(self.stepfunction_lambda_service_role)
        
        self.workflow_logs = logs.LogGroup(self, "ExscriboStepfunctionFlowLogs",
            log_group_name="/aws/stepfunction/exscribo_stepfunctions_logs",                         
            retention=logs.RetentionDays.ONE_WEEK,
            removal_policy=RemovalPolicy.DESTROY
        )
        
        # Create security groups
        self.lambda_security_group = ec2.SecurityGroup(
            self, "LambdaSecurityGroup",
            vpc=networkConstruct.vpc,
            description="Security group for Lambda functions",
            allow_all_outbound=True,
        )

        self.rds_security_group = ec2.SecurityGroup(
            self, "ExscriboRdsSecurityGroup",
            vpc=networkConstruct.vpc,
            description="Security group for Aurora Serverless cluster",
            allow_all_outbound=True,
        )
        
        # Allow inbound access from Lambda to RDS
        self.rds_security_group.add_ingress_rule(
            peer=self.lambda_security_group,
            connection=ec2.Port.tcp(5432),
            description="Allow Lambda to connect to RDS",
        )
