"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
 """

from aws_cdk import (
    aws_rds as rds,
    aws_ec2 as ec2,
    aws_secretsmanager as secrets,
    aws_ssm as ssm,
    RemovalPolicy,
    aws_ec2 as ec2
)
from constructs import Construct
from cdk_nag import NagSuppressions

from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.network import NetworkConstruct

class DatabaseConstruct(Construct):
    def __init__(self, scope: Construct, id: str, networkConstruct: NetworkConstruct, securityConstruct: SecurityConstruct):
        super().__init__(scope, id)
        
        AURORA_POSTGRES_ENGINE_VERSION = rds.AuroraPostgresEngineVersion.VER_16_6
        rds_engine = rds.DatabaseClusterEngine.aurora_postgres(version=AURORA_POSTGRES_ENGINE_VERSION)
                
        # Create database credentials in Secrets Manager with rotation enabled
        self.db_credentials = secrets.Secret(self, "ExscriboDBCredentials",
            secret_name="exscribo/database/credentials",
            generate_secret_string=secrets.SecretStringGenerator(
                secret_string_template='{"username": "dbuser"}',
                generate_string_key="password",
                exclude_punctuation=True,
                exclude_characters='/@"\'\\;'  # Exclude problematic characters for PostgreSQL
            ),
            encryption_key=securityConstruct.encryption_key,
            removal_policy=RemovalPolicy.DESTROY
        )

        self.db_credentials.grant_read(securityConstruct.api_lambda_service_role)
        self.db_credentials.grant_read(securityConstruct.stepfunction_lambda_service_role)

        rds_db_param_group = rds.ParameterGroup(self, 'ExscriboAuroraPostgreSQLDBParamGroup',
            engine=rds_engine,
            description=f'Custom parameter group for aurora-postgresql{AURORA_POSTGRES_ENGINE_VERSION.aurora_postgres_major_version}',
            parameters={
                'log_min_duration_statement': '15000', # 15 sec
                'default_transaction_isolation': 'read committed',
                'rds.allowed_extensions': '*',
                'shared_preload_libraries': 'pg_stat_statements,pg_similarity'
            }
        )
        rds_cluster_param_group = rds.ParameterGroup(self, 'ExscriboAuroraPostgreSQLClusterParamGroup',
            engine=rds_engine,
            description=f'Custom cluster parameter group for aurora-postgresql{AURORA_POSTGRES_ENGINE_VERSION.aurora_postgres_major_version}',
            parameters={
                'log_min_duration_statement': '15000', # 15 sec
                'default_transaction_isolation': 'read committed',
                'client_encoding': 'UTF8',
                'rds.allowed_extensions': '*',
                'shared_preload_libraries': 'pg_stat_statements,pg_similarity'
            }
        )
            
    
        self.rds_database = rds.DatabaseCluster(
            self, 
            "Exscribo Database",
            serverless_v2_max_capacity=8,
            serverless_v2_min_capacity=0.5,
            engine=rds_engine,
            credentials=rds.Credentials.from_secret(self.db_credentials),
            vpc_subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            vpc=networkConstruct.vpc,
            enable_performance_insights=True,
            performance_insight_retention=rds.PerformanceInsightRetention.DEFAULT,
            security_groups=[securityConstruct.lambda_security_group],
            parameter_group=rds_cluster_param_group, 
            writer=rds.ClusterInstance.serverless_v2(
                "writer",
                enable_performance_insights=True,
                parameter_group=rds_db_param_group,
                publicly_accessible=False
            ),
            readers=[
                rds.ClusterInstance.serverless_v2(
                    "reader1",
                    enable_performance_insights=True,
                    parameter_group=rds_db_param_group,
                    publicly_accessible=False,
                    scale_with_writer=True
                )
            ],
            cluster_identifier="exscribo-db",
            default_database_name="exscribo",
            storage_encrypted=True,
            storage_encryption_key=securityConstruct.encryption_key,
            removal_policy=RemovalPolicy.DESTROY,
            deletion_protection=False,  # Enable deletion protection
            iam_authentication=False    # Enable IAM authentication
        )
        NagSuppressions.add_resource_suppressions(
            self.rds_database,
            suppressions=[
                {
                    "id": "AwsSolutions-RDS6",
                    "reason": "Database is not production, its a demo instance"
                },
                              {
                    "id": "AwsSolutions-RDS10",
                    "reason": "Database is not production, its a demo instance"
                }
            ]
        )
        
        # Add rotation schedule using the compatible method for your CDK version
        secrets.SecretRotation(
            self,
            "ExscriboDatabaseSecretRotation",
            application=secrets.SecretRotationApplication.POSTGRES_ROTATION_SINGLE_USER,
            secret=self.db_credentials,
            target=self.rds_database,
            exclude_characters='/@"\'\\;',
            vpc=networkConstruct.vpc,
            security_group=securityConstruct.lambda_security_group,
            vpc_subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
        )
        
        self.database_secret_param = ssm.StringParameter(self, "Exscribo DatabaseSecretParam",
            parameter_name="/exscribo/db/secret-name",
            string_value=self.db_credentials.secret_name,
            description="Database credentials secret name",
        )
        self.database_secret_param.grant_read(securityConstruct.api_lambda_service_role)
        self.database_secret_param.grant_read(securityConstruct.stepfunction_lambda_service_role)
    