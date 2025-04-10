"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    Duration,
    Stack,
    aws_cognito as cognito,
    aws_logs as logs,
    aws_ssm as ssm,
    RemovalPolicy,
    aws_iam as iam,
    CfnOutput
)
from aws_cdk import aws_wafv2 as wafv2
from cdk_nag import NagSuppressions
from constructs import Construct

class AuthConstruct(Construct):
    def __init__(self, scope: Construct, id: str):
        super().__init__(scope, id)

        log_group = logs.LogGroup(self, "CognitoLogGroup",
            log_group_name="/aws/cognito/exscribo-user-pool",
            retention=logs.RetentionDays.ONE_WEEK,  # Adjust retention period as needed
            removal_policy=RemovalPolicy.DESTROY
        )
                
        # Create Cognito User Pool with enhanced security settings
        self.user_pool = cognito.UserPool(self, "ExscriboUserPool",
            user_pool_name="exscribo-user-pool",
            self_sign_up_enabled=False,
            sign_in_aliases=cognito.SignInAliases(
                email=True,
                username=True
            ),
            standard_attributes=cognito.StandardAttributes(
                email=cognito.StandardAttribute(required=True, mutable=True)
            ),
            password_policy=cognito.PasswordPolicy(
                min_length=8,
                require_lowercase=True,
                require_uppercase=True,
                require_digits=True,
                require_symbols=True
            ),
            account_recovery=cognito.AccountRecovery.EMAIL_ONLY,
            removal_policy=RemovalPolicy.DESTROY
        )

        waf_acl = wafv2.CfnWebACL(self, "CognitoWAFWebACL",
            default_action=wafv2.CfnWebACL.DefaultActionProperty(
                allow={}
            ),
            scope="REGIONAL",
            visibility_config=wafv2.CfnWebACL.VisibilityConfigProperty(
                cloud_watch_metrics_enabled=True,
                metric_name="CognitoWAFWebACL",
                sampled_requests_enabled=True
            ),
            rules=[
                # Rate limiting rule
                wafv2.CfnWebACL.RuleProperty(
                    name="RateLimitRule",
                    priority=1,
                    statement=wafv2.CfnWebACL.StatementProperty(
                        rate_based_statement=wafv2.CfnWebACL.RateBasedStatementProperty(
                            aggregate_key_type="IP",
                            limit=2000  # Requests per 5 minutes per IP
                        )
                    ),
                    action=wafv2.CfnWebACL.RuleActionProperty(
                        block={}
                    ),
                    visibility_config=wafv2.CfnWebACL.VisibilityConfigProperty(
                        sampled_requests_enabled=True,
                        cloud_watch_metrics_enabled=True,
                        metric_name="RateLimitRule"
                    )
                ),
                # AWS Managed Rules - Common Rule Set
                wafv2.CfnWebACL.RuleProperty(
                    name="AWSManagedRulesCommonRuleSet",
                    priority=2,
                    override_action=wafv2.CfnWebACL.OverrideActionProperty(
                        none={}
                    ),
                    statement=wafv2.CfnWebACL.StatementProperty(
                        managed_rule_group_statement=wafv2.CfnWebACL.ManagedRuleGroupStatementProperty(
                            vendor_name="AWS",
                            name="AWSManagedRulesCommonRuleSet"
                        )
                    ),
                    visibility_config=wafv2.CfnWebACL.VisibilityConfigProperty(
                        sampled_requests_enabled=True,
                        cloud_watch_metrics_enabled=True,
                        metric_name="AWSManagedRulesCommonRuleSet"
                    )
                )
            ]
        )

        # Associate WAF Web ACL with Cognito User Pool
        wafv2.CfnWebACLAssociation(self, "CognitoWAFAssociation",
            resource_arn=self.user_pool.user_pool_arn,
            web_acl_arn=waf_acl.attr_arn
        )

        # Apply the new threat protection settings using L1 construct
        cfn_user_pool = self.user_pool.node.default_child
        cfn_user_pool.user_pool_add_ons = cognito.CfnUserPool.UserPoolAddOnsProperty(
            advanced_security_mode="ENFORCED"  # This is still required for CDK-nag
        )

        # Create Client App with enhanced security settings
        self.user_pool_client = self.user_pool.add_client("ExscriboWebClient",
            access_token_validity=Duration.minutes(60),
            id_token_validity=Duration.days(1),
            refresh_token_validity=Duration.days(1),
            auth_flows=cognito.AuthFlow(
                admin_user_password=True,
                user_password=True,
                user_srp=True
            ),
            prevent_user_existence_errors=True,  # Enhanced security feature
            enable_token_revocation=True,        # Enhanced security feature
            o_auth=cognito.OAuthSettings(
                flows=cognito.OAuthFlows(
                    authorization_code_grant=True,
                    implicit_code_grant=True
                ),
                scopes=[cognito.OAuthScope.EMAIL, cognito.OAuthScope.OPENID, cognito.OAuthScope.PROFILE],
                callback_urls=["http://localhost:3000"]
            )
        )      
        log_group_arn_raw = f"arn:aws:logs:{Stack.of(self).region}:{Stack.of(self).account}:log-group:{log_group.log_group_name}"
        # Add logging configuration to the user pool
        cfn_log_config = cognito.CfnLogDeliveryConfiguration(self, "CognitoLogging",
            user_pool_id=self.user_pool.user_pool_id,
            log_configurations=[
                cognito.CfnLogDeliveryConfiguration.LogConfigurationProperty(
                    cloud_watch_logs_configuration=cognito.CfnLogDeliveryConfiguration.CloudWatchLogsConfigurationProperty(
                        log_group_arn=log_group_arn_raw
                    ),
                    event_source="userAuthEvents",  # Logs authentication events
                    log_level="INFO"  # Can be INFO or ERROR
                ),
                cognito.CfnLogDeliveryConfiguration.LogConfigurationProperty(
                    cloud_watch_logs_configuration=cognito.CfnLogDeliveryConfiguration.CloudWatchLogsConfigurationProperty(
                        log_group_arn=log_group_arn_raw
                    ),
                    event_source="userNotification",
                    log_level="INFO"
                )
            ]
        )

        # Create an Identity Pool
        self.identity_pool = cognito.CfnIdentityPool(self, "ExscriboIdentityPool",
            allow_unauthenticated_identities=False,
            cognito_identity_providers=[
                cognito.CfnIdentityPool.CognitoIdentityProviderProperty(
                    client_id=self.user_pool_client.user_pool_client_id,
                    provider_name=self.user_pool.user_pool_provider_name
                )
            ]
        )  

        # Create roles for authenticated users
        authenticated_role = iam.Role(
            self, 
            "CognitoDefaultAuthenticatedRole",
            assumed_by=iam.FederatedPrincipal(
                "cognito-identity.amazonaws.com",
                conditions={
                    "StringEquals": {
                        "cognito-identity.amazonaws.com:aud": self.identity_pool.ref
                    },
                    "ForAnyValue:StringLike": {
                        "cognito-identity.amazonaws.com:amr": "authenticated"
                    }
                },
                assume_role_action="sts:AssumeRoleWithWebIdentity"
            )
        )

        
        # Attach policies to the authenticated role as needed
        authenticated_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "cognito-sync:*",
                    "cognito-identity:*"
                ],
                resources=["*"]
            )
        )
        # Attach roles to Identity Pool
        cognito.CfnIdentityPoolRoleAttachment(
            self, 
            "IdentityPoolRoleAttachment",
            identity_pool_id=self.identity_pool.ref,
            roles={
                "authenticated": authenticated_role.role_arn
            }
        )
        
        # Store Identity Pool ID in SSM for future reference
        self.identity_pool_id = ssm.StringParameter(
            self, 
            "ExscriboIdentityPoolId",
            parameter_name="/exscribo/identity-pool/id",
            string_value=self.identity_pool.ref,
        )

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path="/ExscriboStack/ExscriboAuthentication/CognitoDefaultAuthenticatedRole/DefaultPolicy/Resource",
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Authenticated role requires broad permissions as per Cognito documentation: https://docs.aws.amazon.com/cognito/latest/developerguide/iam-roles.html"
                }
            ]
        )             
        
        # Create admin group
        self.group = cognito.UserPoolGroup(self, "AdminGroup",
            group_name="admins",
            description="Admins Group",
            user_pool=self.user_pool
        )

        # Store parameters in SSM
        self.user_pool_id = ssm.StringParameter(self, "ExscriboUserPoolId",
            parameter_name="/exscribo/auth/user-pool-id",
            string_value=self.user_pool.user_pool_id,
        )

        self.client_id = ssm.StringParameter(self, "ExscriboUserPoolClientId",
            parameter_name="/exscribo/auth/client-id",
            string_value=self.user_pool_client.user_pool_client_id,
        )
        self.client_id = ssm.StringParameter(self, "ExscriboIdentifyPoolId",
            parameter_name="/exscribo/auth/identity-pool-id",
            string_value=self.identity_pool.ref
        )
        
        # CloudFormation outputs
        CfnOutput(
            self,
            "Exscribo-Cognito-UserPoolId",
            value=self.user_pool.user_pool_id,
            export_name="Exscribo-Cognito-UserPoolId"

        )
        CfnOutput(
            self,
            "Exscribo-Cognito-UserPoolClientId",
            value=self.user_pool_client.user_pool_client_id,
            export_name="Exscribo-Cognito-UserPoolClientId"
        )
        CfnOutput(
            self,
            "Exscribo-Cognito-IdentifyPoolId",
            value=self.identity_pool.ref,
            export_name="Exscribo-Cognito-IdentifyPoolId"
        )