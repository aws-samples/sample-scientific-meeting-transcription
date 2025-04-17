"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    aws_ec2 as ec2,

)
import aws_cdk as cdk

from cdk_nag import NagSuppressions
from constructs import Construct

class NetworkConstruct(Construct):
    def __init__(self, scope: Construct, id: str):
        super().__init__(scope, id)

        vpcID = ""

        if vpcID != "":
            self.vpc = ec2.Vpc.from_lookup(self, "ExscriboVPC", vpc_id=vpcID)
        else:    
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
     
            NagSuppressions.add_resource_suppressions_by_path(
                stack=cdk.Stack.of(self),
                path="/ExscriboStack/ExscriboNetwork/ApiVpc/Resource",
                suppressions=[
                    {
                        "id": "AwsSolutions-VPC7",
                        "reason": "KB requires full access to the vector collection"
                    }
                ]
            )

  

   

   
    