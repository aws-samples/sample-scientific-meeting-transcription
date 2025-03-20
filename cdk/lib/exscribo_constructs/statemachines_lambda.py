"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
 """

from aws_cdk import (
    Duration,
    aws_lambda as lambda_,
    aws_ec2 as ec2,
    aws_logs as logs,
    aws_ssm as ssm,
    RemovalPolicy
)
from constructs import Construct
import json

from exscribo_constructs.database import DatabaseConstruct
from exscribo_constructs.network import NetworkConstruct
from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.bedrock_kb import BedrockKBConstruct

class StatemachinesLambda(Construct):
    def __init__(self, 
                scope: Construct, 
                id: str,
                networkConstruct: NetworkConstruct,
            	securityConstruct: SecurityConstruct,
            	databaseConstruct: DatabaseConstruct,
                storageConstruct: StorageConstruct,
                bedrockKBConstruct: BedrockKBConstruct
        ):
        super().__init__(scope, id)
        
        self.stepfunction_lambda_log_group = logs.LogGroup(self, "ExscriboApiLambdaLogs",
            log_group_name="/aws/lambda/exscribo_stepfunction_lambda",                         
            retention=logs.RetentionDays.ONE_WEEK,
            removal_policy=RemovalPolicy.DESTROY,                                            
        )

        self.stepfunctions_exscribo_lambda = lambda_.Function(
            self,
            "StepFunctionLambdaFunction",
            runtime=lambda_.Runtime.DOTNET_8,
            handler="StepFunctionLambda::StepFunctionLambda.LambdaEntryPoint::FunctionHandlerAsync",
            code=lambda_.Code.from_asset("../src/StepFunctionLambda/asset-output/function.zip"),
            function_name="exscribo_stepfunctions_lambda",
            role=securityConstruct.stepfunction_lambda_service_role,
            environment={
                "DB_SECRET_KEY": databaseConstruct.rds_database.secret.secret_name,
                "S3_UPLOAD_BUCKET_PARAM_ID": storageConstruct.s3_bucket_param.parameter_name,
                "BEDROCK_KB_ID": bedrockKBConstruct.knowledge_base_id,
                "BEDROCK_KB_DATASOURCE_ID": bedrockKBConstruct.kb_data_source.attr_data_source_id,
                "BEDROCK_MODEL_INFERENCE_ARN": bedrockKBConstruct.bedrock_model_arn
            },
            log_group=self.stepfunction_lambda_log_group,
            tracing=lambda_.Tracing.ACTIVE,
            vpc=networkConstruct.vpc,
            vpc_subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS) if networkConstruct.vpc else None,
            security_groups=[securityConstruct.lambda_security_group],
            timeout=Duration.minutes(10),
            memory_size=512,
            snap_start=lambda_.SnapStartConf.ON_PUBLISHED_VERSIONS,
            architecture=lambda_.Architecture.X86_64  # Explicitly set architecture for .NET 8
        )

        securityConstruct.encryption_key.grant_decrypt(self.stepfunctions_exscribo_lambda)
        databaseConstruct.rds_database.grant_connect(self.stepfunctions_exscribo_lambda, "dbuser")
        self.stepfunction_lambda_log_group.grant_write(self.stepfunctions_exscribo_lambda)
    
        # Store Lambda ARNs in SSM
        self.stepfunctions_exscribo_lambda_arn = ssm.StringParameter(self, "StepFunctionsLambdaArn",
            parameter_name="/exscribo/api/exscribo_stepfunctions_lambda-arn",
            string_value=self.stepfunctions_exscribo_lambda.function_arn,
        )
        
        
  
