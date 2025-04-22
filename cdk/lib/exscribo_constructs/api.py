"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    CfnOutput,
    Duration,
    Stack,
    aws_apigateway as apigateway,
    aws_lambda as lambda_,
    aws_ec2 as ec2,
    aws_wafv2 as wafv2,
    aws_logs as logs,
    aws_ssm as ssm,
    RemovalPolicy
)
from cdk_nag import NagSuppressions
from constructs import Construct
import json

from exscribo_constructs.auth import AuthConstruct
from exscribo_constructs.bedrock_kb import BedrockKBConstruct
from exscribo_constructs.custommodel_workflow import CustomModelWorkflow
from exscribo_constructs.database import DatabaseConstruct
from exscribo_constructs.network import NetworkConstruct
from exscribo_constructs.prompt_workflow import PromptWorkflow
from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.transcription_workflow import TranscribeAndPromptWorkflow
from exscribo_constructs.customvocabulary_workflow import CustomVocabularyWorkflow
from exscribo_constructs.sealmeeting_workflow import SealMeetingWorkflow
from aws_cdk import aws_iam as iam

class ApiConstruct(Construct):
    def __init__(self, 
                 scope: Construct, 
                 id: str,      
                networkConstruct: NetworkConstruct,
                authenticationConstruct: AuthConstruct,
                securityConstruct: SecurityConstruct,
                databaseConstruct: DatabaseConstruct,
                storageConstruct: StorageConstruct,
                transcriptionWorkflowConstruct: TranscribeAndPromptWorkflow,
                custommodelWorkflowConstruct: CustomModelWorkflow,
                promptWorkflowConstruct: PromptWorkflow,
                bedrockKBConstruct: BedrockKBConstruct,
                customVocabularyConstruct: CustomVocabularyWorkflow,
                sealMeetingWorkflowConstruct: SealMeetingWorkflow,
                ) -> None:
        super().__init__(scope, id)
        
        # Create log groups first
        self.api_lambda_log_group = logs.LogGroup(self, "ExscriboApiLambdaLogs",
            log_group_name="/aws/lambda/exscribo_api_lambda",   
            removal_policy=RemovalPolicy.DESTROY,                      
            retention=logs.RetentionDays.ONE_WEEK
        )
        
        self.api_gateway_log_group = logs.LogGroup(self, "ExscriboApiGatewayLogs",
            log_group_name="/aws/apigateway/exscribo_api_gateway",   
            removal_policy=RemovalPolicy.DESTROY,                                            
            retention=logs.RetentionDays.ONE_WEEK,
        )

        self.exscribo_lambda = lambda_.Function(
            self,
            "ExscriboAPILambda",
            runtime=lambda_.Runtime.DOTNET_8,
            handler="ExscriboAPI::ExscriboAPI.LambdaEntryPoint::FunctionHandlerAsync",
            code=lambda_.Code.from_asset("../src/ExscriboAPI/asset-output/function.zip"),
            function_name="exscribo_api_lambda_function",
            role=securityConstruct.api_lambda_service_role,
            environment={
                "DB_SECRET_KEY": databaseConstruct.db_credentials.secret_name,
                "S3_UPLOAD_BUCKET_PARAM_ID": storageConstruct.s3_bucket_param.parameter_name,
                "TRANSCRIBE_STATEMACHINE_ARN": transcriptionWorkflowConstruct.state_machine.state_machine_arn,
                "CUSTOMMODEL_STATEMACHINE_ARN": custommodelWorkflowConstruct.state_machine.state_machine_arn,
                "CUSTOMVOCABULARY_STATEMACHINE_ARN": customVocabularyConstruct.state_machine.state_machine_arn,
                "PROMPT_STATEMACHINE_ARN": promptWorkflowConstruct.state_machine.state_machine_arn,
                "BEDROCK_KB_ID": bedrockKBConstruct.knowledge_base_id,
                "BEDROCK_KB_DATASOURCE_ID": bedrockKBConstruct.kb_data_source.attr_data_source_id,
                "SEALMEETING_STATEMACHINE_ARN": sealMeetingWorkflowConstruct.state_machine.state_machine_arn,
                "BEDROCK_MODEL_INFERENCE_ARN": bedrockKBConstruct.bedrock_model_arn
            },
            tracing=lambda_.Tracing.ACTIVE,
            vpc=networkConstruct.vpc,
            vpc_subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            security_groups=[securityConstruct.lambda_security_group],
            log_group=self.api_lambda_log_group,
            timeout=Duration.seconds(120),
            memory_size=512,
            snap_start=lambda_.SnapStartConf.ON_PUBLISHED_VERSIONS
        )

        #grant api lambda start execute access
        transcriptionWorkflowConstruct.state_machine.grant_start_execution(self.exscribo_lambda.grant_principal)
        custommodelWorkflowConstruct.state_machine.grant_start_execution(self.exscribo_lambda.grant_principal)
        customVocabularyConstruct.state_machine.grant_start_execution(self.exscribo_lambda.grant_principal)
        promptWorkflowConstruct.state_machine.grant_start_execution(self.exscribo_lambda.grant_principal)
        sealMeetingWorkflowConstruct.state_machine.grant_start_execution(self.exscribo_lambda.grant_principal)

        databaseConstruct.db_credentials.grant_read(self.exscribo_lambda.grant_principal)
        self.api_lambda_log_group.grant_write(self.exscribo_lambda.grant_principal)
        securityConstruct.encryption_key.grant_decrypt(self.exscribo_lambda.grant_principal)
        databaseConstruct.rds_database.grant_connect(self.exscribo_lambda.grant_principal, "dbuser")   

        #Create WAF for API Gateway
        self.api_waf = wafv2.CfnWebACL(self, "ExscriboApiWAF",
            default_action=wafv2.CfnWebACL.DefaultActionProperty(allow={}),
            scope="REGIONAL",
            visibility_config=wafv2.CfnWebACL.VisibilityConfigProperty(
                cloud_watch_metrics_enabled=True,
                metric_name="ApiWafMetrics",
                sampled_requests_enabled=True
            ),
            rules=[
                wafv2.CfnWebACL.RuleProperty(
                    name="LimitRequests500",
                    priority=1,
                    statement=wafv2.CfnWebACL.StatementProperty(
                        rate_based_statement=wafv2.CfnWebACL.RateBasedStatementProperty(
                            limit=500,
                            aggregate_key_type="IP"
                        )
                    ),
                    visibility_config=wafv2.CfnWebACL.VisibilityConfigProperty(
                        cloud_watch_metrics_enabled=True,
                        metric_name="LimitRequests500",
                        sampled_requests_enabled=True
                    ),
                    action=wafv2.CfnWebACL.RuleActionProperty(block={})
                )
            ]
        )

        #Create API Gateway with API Key required
        self.api = apigateway.RestApi(
            self, 
            "ExscriboAPI",
            endpoint_configuration=apigateway.EndpointConfiguration(
                types=[apigateway.EndpointType.PRIVATE],
                vpc_endpoints=[networkConstruct.api_gateway_endpoint]
            ),
            endpoint_types=[apigateway.EndpointType.REGIONAL],
            rest_api_name="Exscribo API",
            description="API endpoint for the Exscribo platform",
            default_cors_preflight_options=apigateway.CorsOptions(
                allow_origins=apigateway.Cors.ALL_ORIGINS,
                allow_methods=apigateway.Cors.ALL_METHODS
            ),
            deploy_options=apigateway.StageOptions(
                logging_level=apigateway.MethodLoggingLevel.INFO,
                data_trace_enabled=True,
                metrics_enabled=True,
                access_log_destination=apigateway.LogGroupLogDestination(self.api_gateway_log_group),
                tracing_enabled=True
            )
        )
        self.api.add_to_resource_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=["execute-api:Invoke"],
                principals=[iam.ArnPrincipal("*")],
                resources=["execute-api/*"],

                conditions={
                    "IpAddress": {
                        "aws:SourceIp": [ networkConstruct.vpc.vpc_cidr_block ] #update this to allow internal IP's access to this private API gateway
                    }
                }
            )
        )


        CfnOutput(
            self,
            "Exscribo-API-URL",
            export_name="Exscribo-API-URL",
            value=self.api.url
        )
        CfnOutput(
            self,
            "Exscribo-API-ID",
            export_name="Exscribo-API-ID",
            value=self.api.rest_api_id
        )


        # Store API Gateway endpoint in SSM
        self.api_endpoint = ssm.StringParameter(self, "ExscriboApiEndpoint",
            parameter_name="/exscribo/api/endpoint",
            string_value=self.api.url,
        )

        authorizer = apigateway.CognitoUserPoolsAuthorizer(self, "ExscriboAuthorizer",
            cognito_user_pools=[
                authenticationConstruct.user_pool
            ],
        )
        teams_resource = self.api.root.add_resource("teams")

        teams_resource.add_method("ANY",
            authorizer=authorizer,
            integration=apigateway.LambdaIntegration(self.exscribo_lambda),
            api_key_required=False
        )

        teams_resource.add_proxy(
            default_integration=apigateway.LambdaIntegration(self.exscribo_lambda),
            any_method=True,
            default_method_options=apigateway.MethodOptions(
                authorization_type=apigateway.AuthorizationType.COGNITO,
                authorizer=authorizer
            )
        )
        
        # Associate WAF with API Gateway
        self.waf_association = wafv2.CfnWebACLAssociation(self, "ApiWafAssociation",
            resource_arn=f"arn:aws:apigateway:{Stack.of(self).region}:{Stack.of(self).account}:/restapis/{self.api.rest_api_id}/stages/{self.api.deployment_stage.stage_name}",
            web_acl_arn=self.api_waf.attr_arn,
        )
        
    
        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/Exscribo Api/ExscriboAPI/CloudWatchRole/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM4",
                    "reason": "API Gateway Push to CloudWatch Policy is good for a demo/prototype platform",
                    "appliesTo": [
                        "Policy::arn:<AWS::Partition>:iam::aws:policy/service-role/AmazonAPIGatewayPushToCloudWatchLogs",
                    ]
                }
            ]
        )
  
