#!/usr/bin/env python3

"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
 """

import os

import aws_cdk as cdk
from cdk_nag import AwsSolutionsChecks, NagSuppressions
from exscribo_constructs.exscribo_stack import ExscriboStack

app = cdk.App()

# Add cdk-nag AwsSolutions Pack
cdk.Aspects.of(app).add(AwsSolutionsChecks())

cdk.Annotations.of(app).acknowledge_warning(
    '@aws-cdk/aws-lambda:snapStartRequirePublish'
)

# Create environment configuration
env = cdk.Environment(
    account=os.environ.get('CDK_DEFAULT_ACCOUNT'),
    region=os.environ.get('CDK_DEFAULT_REGION')
)

ExscriboStack(app, "ExscriboStack", env=env)

# Add suppressions for required "*" resources and other necessary exceptions
NagSuppressions.add_stack_suppressions(app.node.find_child("ExscriboStack"), [

       
    # Cognito related suppressions
    {"id": "AwsSolutions-COG2", "reason": "MFA is optional for this application"},
    {"id": "AwsSolutions-APIG2", "reason": "Request validation is performed by the AspNet Framework"},
    
        
    # CORS related suppressions
    {"id": "AwsSolutions-S10", "reason": "CORS configuration is required for web application functionality"},
    
    # Lambda related suppressions
    {"id": "AwsSolutions-L1", "reason": "The Lambda function is designed for .NET 8 which is the latest runtime for this application"},
    
    {
        "id": "AwsSolutions-IAM5",
        "reason": "Default Policy for CDK grant_read_write access. Storage access to S3 is required for a range of services and limited to /teams* prefix",
        "appliesTo": [
            {
                "regex" :  "/ExscriboStorageWorkingS3Bucket/"
            },
            {
                "regex" :  "/Action::s3:/"
            },
            
        ]
    }
])



app.synth()
