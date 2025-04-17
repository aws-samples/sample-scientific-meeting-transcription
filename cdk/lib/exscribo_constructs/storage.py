"""
// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    aws_s3 as s3,
    aws_ssm as ssm,
    aws_iam as iam,
    aws_ec2 as ec2,
        Stack,

)
from constructs import Construct
from cdk_nag import NagSuppressions

from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.network import NetworkConstruct

class StorageConstruct(Construct):
    def __init__(self, scope: Construct,  id: str, 
                 securityConstruct: SecurityConstruct,
                 networkConstruct: NetworkConstruct):
        super().__init__(scope, id)

        # Create S3 logs bucket first with minimal dependencies
        self.logs_bucket = s3.Bucket(self, "S3 Access Logs Bucket",
            encryption=s3.BucketEncryption.KMS,
            encryption_key=securityConstruct.encryption_key,
            block_public_access=s3.BlockPublicAccess.BLOCK_ALL,
            # removal_policy=RemovalPolicy.DESTROY,
            # auto_delete_objects=True,
            versioned=True,
            enforce_ssl=True,
            
        )
        self.logs_bucket.add_to_resource_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=["s3:PutObject"],
                principals=[iam.ServicePrincipal("delivery.logs.amazonaws.com")],
                resources=[f"{self.logs_bucket.bucket_arn}/*", self.logs_bucket.bucket_arn],
                conditions={
                    "StringEquals": {
                        "aws:SourceAccount": Stack.of(self).account,
                        "s3:x-amz-acl": "bucket-owner-full-control"
                    },
                    "ArnLike": {
                        "aws:SourceArn": f"arn:aws:logs:{Stack.of(self).region}:{Stack.of(self).account}:*"
                    }
                }
            )
        )
        self.logs_bucket.add_to_resource_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=["s3:GetBucketAcl"],
                principals=[iam.ServicePrincipal("delivery.logs.amazonaws.com")],
                resources=[f"arn:aws:s3:::{self.logs_bucket.bucket_name}"],
                conditions={
                    "StringEquals": {
                        "aws:SourceAccount": Stack.of(self).account,
                    },
                    "ArnLike": {
                        "aws:SourceArn": f"arn:aws:logs:{Stack.of(self).region}:{Stack.of(self).account}:*"
                    }
                }
            )         
        )

        
        # Create VPC Flow Logs after logs bucket exists
        flow_logs = ec2.FlowLog(
            self, "VPCFlowLogs",
            flow_log_name="VPCFlowLogs",
            resource_type=ec2.FlowLogResourceType.from_vpc(networkConstruct.vpc),
            max_aggregation_interval=ec2.FlowLogMaxAggregationInterval.ONE_MINUTE,
            traffic_type=ec2.FlowLogTrafficType.ALL,
            destination=ec2.FlowLogDestination.to_s3(
                bucket=self.logs_bucket,
                key_prefix="vpcflows/"
            )
        )

        # Create S3 bucket with server access logging enabled
        self.s3bucket = s3.Bucket(self, "Working S3 Bucket",
            encryption=s3.BucketEncryption.KMS,
            encryption_key=securityConstruct.encryption_key,
            block_public_access=s3.BlockPublicAccess.BLOCK_ALL,
            versioned=True,
            enforce_ssl=True,
            # removal_policy=RemovalPolicy.DESTROY,
            # auto_delete_objects=True,
            cors=[s3.CorsRule(
                    allowed_headers=["*"],
                    allowed_methods=[s3.HttpMethods.PUT, s3.HttpMethods.GET],
                    allowed_origins=["*"])
                ],
            server_access_logs_bucket=self.logs_bucket,
            server_access_logs_prefix="working-bucket-logs/"
        )


        self.s3_list_statement = iam.PolicyStatement(
            effect=iam.Effect.ALLOW,
            actions=[
                "s3:ListBucket"
            ],
            resources=[
                f"arn:aws:s3:::{self.s3bucket.bucket_name}",
                f"arn:aws:s3:::{self.s3bucket.bucket_name}/*"

            ]
        )
        
        self.s3_rw_statement = iam.PolicyStatement(
                    effect=iam.Effect.ALLOW,
                    actions=[
                        "s3:PutObject",
                        "s3:DeleteObject",
                        "s3:GetObject",
                        "s3:PutObjectACL",
                        "s3:ListObject",
                        "s3:PutObjectTagging"
                    ],
                    resources=[
                        f"arn:aws:s3:::{self.s3bucket.bucket_name}/*",
                        f"arn:aws:s3:::{self.s3bucket.bucket_name}",

                    ]
                )        

        self.s3_policy = iam.Policy(self, "S3Policy", 
            policy_name="ExscriboS3Policy",
            statements=[ self.s3_list_statement, self.s3_rw_statement ]
        )        

        self.transcribe_role = iam.Role(
            self, "ExscriboTranscribeServiceRole",
            assumed_by=iam.ServicePrincipal("transcribe.amazonaws.com"),
            inline_policies={
                "ExscriboS3Policy": iam.PolicyDocument(
                    minimize=False,
                    statements=[ self.s3_list_statement, self.s3_rw_statement ]
                )
            },
            role_name="ExscriboTranscribeServiceRole"
        )
        securityConstruct.api_lambda_service_role.attach_inline_policy(self.s3_policy)
        securityConstruct.stepfunction_lambda_service_role.attach_inline_policy(self.s3_policy)

        securityConstruct.encryption_key.grant_decrypt(self.transcribe_role)

        
        # Store bucket names in Parameter Store
        self.s3_bucket_param = ssm.StringParameter(self, "S3BucketName",
            parameter_name="/exscribo/storage/bucket-name",
            string_value=self.s3bucket.bucket_name,
        )
        
        # Store logs bucket name in Parameter Store
        self.logs_bucket_param = ssm.StringParameter(self, "S3LogsBucketName",
            parameter_name="/exscribo/storage/logs-bucket-name",
            string_value=self.logs_bucket.bucket_name,
        )

        self.s3_bucket_param.grant_read(securityConstruct.api_lambda_service_role)
        self.s3_bucket_param.grant_read(securityConstruct.stepfunction_lambda_service_role)
