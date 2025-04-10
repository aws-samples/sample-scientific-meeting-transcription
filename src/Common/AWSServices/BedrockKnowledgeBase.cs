// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockAgent;
using Amazon.BedrockAgent.Model;
using Microsoft.Extensions.Logging;

namespace Common.AWSServices;

/// <summary>
/// Static class for interacting with Amazon Bedrock Knowledge Base
/// Provides methods for ingesting and deleting documents in a knowledge base
/// </summary>
public static class BedrockKnowledgeBase
{
    /// <summary>
    /// Ingests a binary document into the Bedrock Knowledge Base
    /// </summary>
    /// <param name="amazonBedrockAgent">The Bedrock Agent client</param>
    /// <param name="documentId">Unique identifier for the document</param>
    /// <param name="memoryStream">Document content as a memory stream</param>
    /// <param name="mimeType">MIME type of the document</param>
    /// <param name="metaData">Metadata key-value pairs for the document</param>
    /// <param name="logger">Logger for tracking operations</param>
    /// <returns>Async task</returns>
    public static async Task IngestDocumentKnowledgeBaseObject(IAmazonBedrockAgent amazonBedrockAgent, string? documentId, MemoryStream memoryStream, string mimeType,
        Dictionary<string, string?> metaData, ILogger logger)
    {
        try
        {
            // Create request to ingest a binary document into the knowledge base
            IngestKnowledgeBaseDocumentsRequest documentsRequest = new IngestKnowledgeBaseDocumentsRequest()
            {
                DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                Documents = new List<KnowledgeBaseDocument>()
                {
                    new()
                    {
                        // Configure document content with binary data
                        Content = new DocumentContent()
                        {
                            DataSourceType = ContentDataSourceType.CUSTOM,
                            Custom = new CustomContent()
                            {
                                SourceType = CustomSourceType.IN_LINE,
                                InlineContent = new InlineContent()
                                {
                                    Type = InlineContentType.BYTE,
                                    ByteContent = new ByteContentDoc()
                                    {
                                        Data = memoryStream,
                                        MimeType = mimeType
                                    }
                                },
                                CustomDocumentIdentifier = new CustomDocumentIdentifier()
                                {
                                    Id = documentId
                                }
                            }
                        },
                        // Add metadata attributes to the document
                        Metadata = new()
                        {
                            Type = MetadataSourceType.IN_LINE_ATTRIBUTE,
                            InlineAttributes = metaData.Select(x => new MetadataAttribute()
                            {
                                Key = x.Key, Value = new MetadataAttributeValue()
                                {
                                    StringValue = x.Value,
                                    Type = MetadataValueType.STRING
                                }
                            }).ToList()
                        }
                    }
                }
            };
            logger.LogInformation("Bedrock knowledge base document ingestion started {@documentsRequest}", documentsRequest);
            
            // Send the ingestion request
            IngestKnowledgeBaseDocumentsResponse ingestResponse = await amazonBedrockAgent.IngestKnowledgeBaseDocumentsAsync(documentsRequest);
            
            if (ingestResponse.HttpStatusCode == HttpStatusCode.Accepted)
            {
                // Poll for document status until it's indexed or failed
                while (true)
                {
                    var status = await amazonBedrockAgent.GetKnowledgeBaseDocumentsAsync(new GetKnowledgeBaseDocumentsRequest()
                    {
                        DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                        KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                        DocumentIdentifiers =
                        [
                            new DocumentIdentifier()
                            {
                                DataSourceType = ContentDataSourceType.CUSTOM,
                                Custom = new CustomDocumentIdentifier()
                                {
                                    Id = documentId
                                }
                            }
                        ]
                    });
                    
                    // Check if document is successfully indexed
                    if (status.DocumentDetails[0].Status == DocumentStatus.INDEXED)
                    {
                        logger.LogInformation("Bedrock knowledge base document ingestion completed {@status}", status.DocumentDetails[0]);
                        break;
                    }

                    // Check if document ingestion failed
                    if (status.DocumentDetails[0].Status == DocumentStatus.FAILED)
                    {
                        logger.LogError("Bedrock knowledge base document ingestion failed {@status}", status.DocumentDetails[0]);
                        throw new Exception(status.DocumentDetails[0].StatusReason);
                    }

                    // Wait before checking status again
                    Thread.Sleep(2000);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bedrock knowledge base document ingestion failed");
        }
    }

    /// <summary>
    /// Ingests a text document into the Bedrock Knowledge Base
    /// </summary>
    /// <param name="amazonBedrockAgent">The Bedrock Agent client</param>
    /// <param name="documentId">Unique identifier for the document</param>
    /// <param name="content">Text content of the document</param>
    /// <param name="metaData">Metadata key-value pairs for the document</param>
    /// <param name="logger">Logger for tracking operations</param>
    /// <returns>Async task</returns>
    public static async Task IngestTextKnowledgeBaseObject(IAmazonBedrockAgent amazonBedrockAgent, string documentId, string content,
        Dictionary<string, string?> metaData, ILogger logger)
    {
        try
        {
            // Create request to ingest a text document into the knowledge base
            IngestKnowledgeBaseDocumentsRequest documentsRequest = new IngestKnowledgeBaseDocumentsRequest()
            {
                DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                Documents = new List<KnowledgeBaseDocument>()
                {
                    new()
                    {
                        // Configure document content with text data
                        Content = new DocumentContent()
                        {
                            DataSourceType = ContentDataSourceType.CUSTOM,
                            Custom = new CustomContent()
                            {
                                CustomDocumentIdentifier = new()
                                {
                                    Id = documentId
                                },
                                InlineContent = new InlineContent()
                                {
                                    TextContent = new TextContentDoc()
                                    {
                                        Data = content
                                    },
                                    Type = InlineContentType.TEXT
                                },
                                SourceType = CustomSourceType.IN_LINE
                            }
                        },
                        // Add metadata attributes to the document
                        Metadata = new()
                        {
                            Type = MetadataSourceType.IN_LINE_ATTRIBUTE,
                            InlineAttributes = metaData.ToList().Select(x => new MetadataAttribute()
                            {
                                Key = x.Key, Value = new MetadataAttributeValue()
                                {
                                    StringValue = x.Value,
                                    Type = MetadataValueType.STRING
                                }
                            }).ToList()
                        }
                    }
                }
            };
            logger.LogInformation("Bedrock knowledge base document ingestion started {@documentsRequest}", documentsRequest);
            
            // Send the ingestion request
            IngestKnowledgeBaseDocumentsResponse ingestResponse = await amazonBedrockAgent.IngestKnowledgeBaseDocumentsAsync(documentsRequest);
            
            if (ingestResponse.HttpStatusCode == HttpStatusCode.Accepted)
            {
                // Poll for document status until it's indexed or failed
                while (true)
                {
                    var status = await amazonBedrockAgent.GetKnowledgeBaseDocumentsAsync(new GetKnowledgeBaseDocumentsRequest()
                    {
                        DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                        KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                        DocumentIdentifiers =
                        [
                            new DocumentIdentifier()
                            {
                                DataSourceType = ContentDataSourceType.CUSTOM,
                                Custom = new CustomDocumentIdentifier()
                                {
                                    Id = documentId
                                }
                            }
                        ]
                    });
                    
                    // Check if document is successfully indexed
                    if (status.DocumentDetails[0].Status == DocumentStatus.INDEXED)
                    {
                        logger.LogInformation("Bedrock knowledge base document ingestion completed {@status}", status.DocumentDetails[0]);
                        break;
                    }

                    // Check if document ingestion failed
                    if (status.DocumentDetails[0].Status == DocumentStatus.FAILED)
                    {
                        logger.LogError("Bedrock knowledge base document ingestion failed {@status}", status.DocumentDetails[0]);
                        throw new Exception(status.DocumentDetails[0].StatusReason);
                    }

                    // Wait before checking status again
                    Thread.Sleep(2000);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bedrock knowledge base document ingestion failed");
        }
    }

    /// <summary>
    /// Deletes a document from the Bedrock Knowledge Base
    /// </summary>
    /// <param name="amazonBedrockAgent">The Bedrock Agent client</param>
    /// <param name="documentId">Unique identifier for the document to delete</param>
    /// <param name="logger">Logger for tracking operations</param>
    /// <returns>Async task</returns>
    public static async Task DeleteKnowledgeBaseObject(IAmazonBedrockAgent amazonBedrockAgent, string? documentId, ILogger logger)
    {
        try
        {
            // Create request to delete a document from the knowledge base
            var deleteResponse = await amazonBedrockAgent.DeleteKnowledgeBaseDocumentsAsync(new DeleteKnowledgeBaseDocumentsRequest()
            {
                DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                DocumentIdentifiers =
                [
                    new DocumentIdentifier()
                    {
                        DataSourceType = ContentDataSourceType.CUSTOM,
                        Custom = new CustomDocumentIdentifier()
                        {
                            Id = documentId
                        }
                    }
                ]
            });
            
            if (deleteResponse.HttpStatusCode == HttpStatusCode.Accepted)
            {
                // Poll for document status until it's not found or failed
                while (true)
                {
                    var status = await amazonBedrockAgent.GetKnowledgeBaseDocumentsAsync(new GetKnowledgeBaseDocumentsRequest()
                    {
                        DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                        KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                        DocumentIdentifiers =
                        [
                            new DocumentIdentifier()
                            {
                                DataSourceType = ContentDataSourceType.CUSTOM,
                                Custom = new CustomDocumentIdentifier()
                                {
                                    Id = documentId
                                }
                            }
                        ]
                    });
                    
                    // Check if document is successfully deleted
                    if (status.DocumentDetails[0].Status == DocumentStatus.NOT_FOUND)
                    {
                        logger.LogInformation("Bedrock knowledge base document deletion completed {@status}", status.DocumentDetails[0]);
                        break;
                    }

                    // Check if document deletion failed
                    if (status.DocumentDetails[0].Status == DocumentStatus.FAILED)
                    {
                        logger.LogError("Bedrock knowledge base document deletion failed {@status}", status.DocumentDetails[0]);
                        throw new Exception(status.DocumentDetails[0].StatusReason);
                    }

                    // Wait before checking status again
                    Thread.Sleep(2000);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bedrock knowledge base document deletion failed");
        }
    }
}