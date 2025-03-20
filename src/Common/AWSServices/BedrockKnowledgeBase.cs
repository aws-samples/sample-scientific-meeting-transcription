// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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

public static class BedrockKnowledgeBase
{
    public static async Task IngestDocumentKnowledgeBaseObject(IAmazonBedrockAgent amazonBedrockAgent, string? documentId, MemoryStream memoryStream, string mimeType,
        Dictionary<string, string?> metaData, ILogger logger)
    {
        try
        {
            IngestKnowledgeBaseDocumentsRequest documentsRequest = new IngestKnowledgeBaseDocumentsRequest()
            {
                DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                Documents = new List<KnowledgeBaseDocument>()
                {
                    new()
                    {
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
            IngestKnowledgeBaseDocumentsResponse ingestResponse = await amazonBedrockAgent.IngestKnowledgeBaseDocumentsAsync(documentsRequest);
            if (ingestResponse.HttpStatusCode == HttpStatusCode.Accepted)
            {
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
                    if (status.DocumentDetails[0].Status == DocumentStatus.INDEXED)
                    {
                        logger.LogInformation("Bedrock knowledge base document ingestion completed {@status}", status.DocumentDetails[0]);
                        break;
                    }

                    if (status.DocumentDetails[0].Status == DocumentStatus.FAILED)
                    {
                        logger.LogError("Bedrock knowledge base document ingestion failed {@status}", status.DocumentDetails[0]);
                        throw new Exception(status.DocumentDetails[0].StatusReason);
                    }

                    Thread.Sleep(2000);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bedrock knowledge base document ingestion failed");
        }
    }

    public static async Task IngestTextKnowledgeBaseObject(IAmazonBedrockAgent amazonBedrockAgent, string documentId, string content,
        Dictionary<string, string?> metaData, ILogger logger)
    {
        try
        {
            IngestKnowledgeBaseDocumentsRequest documentsRequest = new IngestKnowledgeBaseDocumentsRequest()
            {
                DataSourceId = EnvironmentHelper.BEDROCK_KB_DATASOURCE_ID,
                KnowledgeBaseId = EnvironmentHelper.BEDROCK_KB_ID,
                Documents = new List<KnowledgeBaseDocument>()
                {
                    new()
                    {
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
            IngestKnowledgeBaseDocumentsResponse ingestResponse = await amazonBedrockAgent.IngestKnowledgeBaseDocumentsAsync(documentsRequest);
            if (ingestResponse.HttpStatusCode == HttpStatusCode.Accepted)
            {
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
                    if (status.DocumentDetails[0].Status == DocumentStatus.INDEXED)
                    {
                        logger.LogInformation("Bedrock knowledge base document ingestion completed {@status}", status.DocumentDetails[0]);
                        break;
                    }

                    if (status.DocumentDetails[0].Status == DocumentStatus.FAILED)
                    {
                        logger.LogError("Bedrock knowledge base document ingestion failed {@status}", status.DocumentDetails[0]);
                        throw new Exception(status.DocumentDetails[0].StatusReason);
                    }

                    Thread.Sleep(2000);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bedrock knowledge base document ingestion failed");
        }
    }

    public static async Task DeleteKnowledgeBaseObject(IAmazonBedrockAgent amazonBedrockAgent, string? documentId, ILogger logger)
    {
        try
        {
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
                    if (status.DocumentDetails[0].Status == DocumentStatus.NOT_FOUND)
                    {
                        logger.LogInformation("Bedrock knowledge base document deletion completed {@status}", status.DocumentDetails[0]);
                        break;
                    }

                    if (status.DocumentDetails[0].Status == DocumentStatus.FAILED)
                    {
                        logger.LogError("Bedrock knowledge base document deletion failed {@status}", status.DocumentDetails[0]);
                        throw new Exception(status.DocumentDetails[0].StatusReason);
                    }

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