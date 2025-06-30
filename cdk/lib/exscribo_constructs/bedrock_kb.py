"""
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 """

from aws_cdk import (
    Stack,
    aws_iam as iam,
    aws_bedrock,
    aws_opensearchserverless as oss,
)
from cdk_nag import NagPackSuppression, NagSuppressions
from constructs import Construct
import json

from cdklabs.generative_ai_cdk_constructs import (
    opensearchserverless,
    opensearch_vectorindex,
    bedrock,
)

from exscribo_constructs.network import NetworkConstruct
from exscribo_constructs.security import SecurityConstruct
from exscribo_constructs.storage import StorageConstruct
from exscribo_constructs.database import DatabaseConstruct;

class BedrockKBConstruct(Construct):  
    

    def __init__(self, 
                 scope: Construct, 
                 id: str,
                 networkConstruct: NetworkConstruct,
                 securityConstruct: SecurityConstruct,
                 storageConstruct: StorageConstruct,
                 databaseConstruct: DatabaseConstruct,
                 **kwargs
                 ) -> None:
        super().__init__(scope, id)

        self.bedrock_model_arn = f"arn:aws:bedrock:us-east-1:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0"
        
        collection_name="exscribo-vector-collection"
        
        vectorCollection = opensearchserverless.VectorCollection(
            self, 
            "ExscriboVectorCollection",
            collection_name=collection_name
        )
        vectorIndex = opensearch_vectorindex.VectorIndex(self, "VectorIndex",
            vector_dimensions=1024,
            collection=vectorCollection,
            index_name='bedrock-knowledge-base-default-index',
            vector_field='bedrock-knowledge-base-default-vector',
            precision='float',
            distance_type='l2',
            mappings= [
                opensearch_vectorindex.MetadataManagementFieldProps(
                    mapping_field='AMAZON_BEDROCK_TEXT_CHUNK',
                    data_type='text',
                    filterable=True
                ),
                opensearch_vectorindex.MetadataManagementFieldProps(
                    mapping_field='AMAZON_BEDROCK_METADATA',
                    data_type='text',
                    filterable=False
                )
            ]
        )

        oss.CfnAccessPolicy(
            self,
            "ExscriboVectorDataAccessPolicyLambda",
            name="exscribo-admin-console",
            type="data",
            description="Data access policy for Exscribo vector store",
            policy=json.dumps([{
                "Rules": [
                    {
                        "ResourceType": "collection",
                        "Resource": [f"collection/{collection_name}"],
                        "Permission": [
                            "aoss:*"
                        ]
                    },
                    {
                        "ResourceType": "index",
                        "Resource": [f"index/{collection_name}/*"],
                        "Permission": [
                            "aoss:*"
                        ]
                    }
                ],
                "Principal": [
                    f"arn:aws:iam::{Stack.of(self).account}:role/Admin" #allow console to access indexes and collection

                ],
                "Description": "Access policy for Bedrock knowledge base and Lambda function"
            }])
        )        


        kb_role = iam.Role(
            self, 
            "BedrockKBRole",
            assumed_by=iam.CompositePrincipal(
                iam.ServicePrincipal("bedrock.amazonaws.com"),
                iam.ServicePrincipal("s3.amazonaws.com")
            ),
            inline_policies={
                "BedrockKBRolePolicy": iam.PolicyDocument(
                    statements=[
                        iam.PolicyStatement(
                            effect=iam.Effect.ALLOW,
                            actions=["aoss:APIAccessAll"],
                            resources=[
                                vectorCollection.collection_arn,
                            ]
                        ),
                        iam.PolicyStatement(
                            effect=iam.Effect.ALLOW,
                            actions=[
                                "kms:Decrypt",
                                "kms:DescribeKey",
                                "kms:GenerateDataKey"
                            ],
                            resources=[
                                f"{securityConstruct.encryption_key.key_arn}"
                            ]
                        ),
                        iam.PolicyStatement(
                            effect=iam.Effect.ALLOW,
                            actions=[
                                "bedrock:ListFoundationModels",
                                "bedrock:GetFoundationModel",
                                "bedrock:InvokeModel",
                                "bedrock:StartIngestionJob",
                                "bedrock:ListIngestionJobs",
                                "bedrock:RetrieveAndGenerate",
                                "bedrock:GetInferenceProfile",

                            ],
                            resources=[
                                f"arn:aws:bedrock:{Stack.of(self).region}::foundation-model/amazon.titan-embed-text-v2:0",
                                f"arn:aws:bedrock:{Stack.of(self).region}::foundation-model/anthropic.claude-3-haiku-20240307-v1:0",
                                f"arn:aws:bedrock:*:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0",
                                "arn:aws:bedrock:::foundation-model/anthropic.claude-3-7-sonnet-20250219-v1:0",
                                
                            ]
                        )
                    ]
                )
            }
        )

        vectorCollection.grant_data_access(kb_role)
        kb_role.add_to_policy(storageConstruct.s3_rw_statement)
        kb_role.add_to_policy(storageConstruct.s3_list_statement)

        self.knowledge_base = aws_bedrock.CfnKnowledgeBase(
            self,
            "ExscriboMultiModalKnowledgeBase",
            name="ExscriboMultiModalKB",
            description="Exscribo Multi-Modal KnowledgeBase",
            role_arn=kb_role.role_arn,
            knowledge_base_configuration=aws_bedrock.CfnKnowledgeBase.KnowledgeBaseConfigurationProperty(
                type="VECTOR",
                vector_knowledge_base_configuration=aws_bedrock.CfnKnowledgeBase.VectorKnowledgeBaseConfigurationProperty(
                    embedding_model_arn=f"arn:aws:bedrock:{Stack.of(self).region}::foundation-model/amazon.titan-embed-text-v2:0",
                    embedding_model_configuration=aws_bedrock.CfnKnowledgeBase.EmbeddingModelConfigurationProperty(
                        bedrock_embedding_model_configuration=aws_bedrock.CfnKnowledgeBase.BedrockEmbeddingModelConfigurationProperty(
                           dimensions=1024
                        )
                    ),
                    supplemental_data_storage_configuration=aws_bedrock.CfnKnowledgeBase.SupplementalDataStorageConfigurationProperty(
                        supplemental_data_storage_locations=[
                            aws_bedrock.CfnKnowledgeBase.SupplementalDataStorageLocationProperty(
                                supplemental_data_storage_location_type="S3",
                                s3_location=aws_bedrock.CfnKnowledgeBase.S3LocationProperty(
                                    uri=f"s3://{storageConstruct.s3bucket.bucket_name}/chunk-processor/"
                                )
                            )
                        ]
                    )
                )
            ),
            storage_configuration=aws_bedrock.CfnKnowledgeBase.StorageConfigurationProperty(
                type="OPENSEARCH_SERVERLESS",
                opensearch_serverless_configuration=aws_bedrock.CfnKnowledgeBase.OpenSearchServerlessConfigurationProperty(
                    collection_arn=vectorCollection.collection_arn,
                    field_mapping=aws_bedrock.CfnKnowledgeBase.OpenSearchServerlessFieldMappingProperty(
                        vector_field=vectorIndex.vector_field,
                        text_field="AMAZON_BEDROCK_TEXT_CHUNK",
                        metadata_field="AMAZON_BEDROCK_METADATA"
                    ),
                    vector_index_name=vectorIndex.index_name
                )
            )
        )

        securityConstruct.stepfunction_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "bedrock:StartIngestionJob",
                    "bedrock:GetKnowledgeBase",
                    "bedrock:ListKnowledgeBases",
                    "bedrock:GetIngestionJob",
                    "bedrock:ListIngestionJobs",
                    "bedrock:StopIngestionJob",
                    "bedrock:IngestKnowledgeBaseDocuments",
                    "bedrock:GetKnowledgeBaseDocuments",
                    "bedrock:ListKnowledgeBaseDocuments",
                    "bedrock:DeleteKnowledgebaseDocuments"
                ],
                resources=[
                    f"arn:aws:bedrock:{Stack.of(self).region}:{Stack.of(self).account}:knowledge-base/{self.knowledge_base.attr_knowledge_base_id}"
                ]
            )
        )
        securityConstruct.api_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "bedrock:GetKnowledgeBaseDocuments",
                    "bedrock:IngestKnowledgeBaseDocuments",
                    "bedrock:DeleteKnowledgeBaseDocuments",
                ],
                resources=[
                    f"arn:aws:bedrock:{Stack.of(self).region}:{Stack.of(self).account}:knowledge-base/{self.knowledge_base.attr_knowledge_base_id}"
                ]
            )
        )
        securityConstruct.api_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "bedrock:GetInferenceProfile",
                    "bedrock:ListInferenceProfiles"
                ],
                resources=[
                    f"arn:aws:bedrock:*:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0",
                ]
            )
        )

        self.knowledge_base.node.add_dependency(databaseConstruct)

        securityConstruct.api_lambda_service_role.add_to_policy(
            iam.PolicyStatement(
                effect=iam.Effect.ALLOW,
                actions=[
                    "bedrock:RetrieveAndGenerate",
                    "bedrock:Retrieve",
                    "bedrock:StartIngestionJob",
                    "bedrock:ListIngestionJobs",
                    "bedrock:DeleteKnowledgeBaseDocuments",
                    "bedrock:StopIngestionJob",
                    "bedrock:IngestKnowledgeBaseDocuments",
                    "bedrock:GetKnowledgeBaseDocuments",

                ],
                resources=[
                     self.knowledge_base.attr_knowledge_base_arn,
                ]
            )
        )    
 
        self.kb_data_source = aws_bedrock.CfnDataSource(self, "ExscriboMultiModalSource",
            name="ExscriboMultiModalDataSource",
            knowledge_base_id= self.knowledge_base.ref,  
            description="The Custom data source definition for the Exscribo bedrock knowledge base",
            data_source_configuration=aws_bedrock.CfnDataSource.DataSourceConfigurationProperty(
                type="CUSTOM"
            ),
            vector_ingestion_configuration=aws_bedrock.CfnDataSource.VectorIngestionConfigurationProperty(
                chunking_configuration=aws_bedrock.CfnDataSource.ChunkingConfigurationProperty(
                    chunking_strategy="HIERARCHICAL",
                    hierarchical_chunking_configuration=aws_bedrock.CfnDataSource.HierarchicalChunkingConfigurationProperty(
                        overlap_tokens=100,
                        level_configurations=[
                            aws_bedrock.CfnDataSource.HierarchicalChunkingLevelConfigurationProperty(
                                max_tokens=1500
                            ),
                            aws_bedrock.CfnDataSource.HierarchicalChunkingLevelConfigurationProperty(
                                max_tokens=300
                            )
                        ]
                    )
                ),
                parsing_configuration=aws_bedrock.CfnDataSource.ParsingConfigurationProperty(
                    parsing_strategy="BEDROCK_FOUNDATION_MODEL",
                    bedrock_foundation_model_configuration=aws_bedrock.CfnDataSource.BedrockFoundationModelConfigurationProperty(
                        model_arn=f"arn:aws:bedrock:{Stack.of(self).region}::foundation-model/anthropic.claude-3-haiku-20240307-v1:0",
                        parsing_modality="MULTIMODAL"
                    )
                )
            )
        )
       

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path=[
                "/ExscriboStack/Exscribo Bedrock KB/BedrockKBRole/DefaultPolicy/Resource",
            ],
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "CDK Created Default Policy",
                    "appliesTo": [
                        f"Action::kms:ReEncrypt*",
                        f"Action::kms:GenerateDataKey*",

                    ]
                }
            ],
            apply_to_children=True
        )

        NagSuppressions.add_resource_suppressions_by_path(
            stack=Stack.of(self),
            path=[
                "/ExscriboStack/Exscribo Bedrock KB/BedrockKBRole/Resource",
            ],
            suppressions=[
                {
                    "id": "AwsSolutions-IAM5",
                    "reason": "Cross region inference invoke",
                    "appliesTo": [
                        f"Resource::arn:aws:bedrock:*:{Stack.of(self).account}:inference-profile/us.anthropic.claude-3-7-sonnet-20250219-v1:0",
                    ]
                }
            ],
            apply_to_children=True
        )
        
        self.knowledge_base_id = self.knowledge_base.attr_knowledge_base_id
        self.knowledge_base_arn = self.knowledge_base.attr_knowledge_base_arn

