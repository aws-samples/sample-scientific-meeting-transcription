"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
 """

from aws_cdk import (
    aws_ec2 as ec2,

)
from constructs import Construct

class NetworkConstruct(Construct):
    def __init__(self, scope: Construct, id: str):
        super().__init__(scope, id)

        self.vpc = ec2.Vpc(self, "ApiVpc",
            max_azs=3,
            nat_gateways=1,
            vpc_name="ExscriboVPC",
            enable_dns_support=True,
            subnet_configuration=[
                ec2.SubnetConfiguration(
                    name="Isolated",
                    subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS,
                    cidr_mask=24
                ),
                ec2.SubnetConfiguration(
                    name="Public",
                    subnet_type=ec2.SubnetType.PUBLIC,
                    cidr_mask=24
                )
            ]
        )
        
        # Create security group for VPC endpoints
        self.vpc_endpoint_security_group = ec2.SecurityGroup(
            self, "VPCEndpointSecurityGroup",
            security_group_name="Exscribo Security Group",
            vpc=self.vpc,
            allow_all_outbound=True,
            description="Security group for VPC Interface Endpoints"
        )

        # Add inbound rules for HTTPS (port 443) from private subnets
        self.vpc_endpoint_security_group.add_ingress_rule(
            peer=ec2.Peer.ipv4(self.vpc.vpc_cidr_block),
            connection=ec2.Port.tcp(443),
            description="Allow HTTPS inbound from VPC CIDR"
        )
        self.api_gateway_endpoint = ec2.InterfaceVpcEndpoint(
            self, "APIGatewayEndpoint",
            vpc=self.vpc,
            service=ec2.InterfaceVpcEndpointAwsService.APIGATEWAY,
            subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            private_dns_enabled=True,
            security_groups=[self.vpc_endpoint_security_group]
        )
        self.vpc_secrets_manager_endpoint = ec2.InterfaceVpcEndpoint(
            self, "S3Endpoint",
            vpc=self.vpc,
            service=ec2.InterfaceVpcEndpointAwsService.S3,
            subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            private_dns_enabled=True,
            security_groups=[self.vpc_endpoint_security_group]
        )
        self.vpc_secrets_manager_endpoint = ec2.InterfaceVpcEndpoint(
            self, "SecretsManagerEndpoint",
            vpc=self.vpc,
            service=ec2.InterfaceVpcEndpointAwsService.SECRETS_MANAGER,
            subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            private_dns_enabled=True,
            security_groups=[self.vpc_endpoint_security_group]
        )
        self.vpc_ssm_endpoint = ec2.InterfaceVpcEndpoint(
            self, "SsmEndpoint",
            vpc=self.vpc,
            service=ec2.InterfaceVpcEndpointAwsService.SSM,
            subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            private_dns_enabled=True,
            security_groups=[self.vpc_endpoint_security_group]
        )
        self.vpc_stepfunction_endpoint = ec2.InterfaceVpcEndpoint(
            self, "StepFunctionEndpoint",
            vpc=self.vpc,
            service=ec2.InterfaceVpcEndpointAwsService.STEP_FUNCTIONS,
            subnets=ec2.SubnetSelection(subnet_type=ec2.SubnetType.PRIVATE_WITH_EGRESS),
            private_dns_enabled=True,
            security_groups=[self.vpc_endpoint_security_group]
        )

   
    