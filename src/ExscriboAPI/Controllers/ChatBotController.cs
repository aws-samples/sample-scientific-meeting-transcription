// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Text.Json;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Common.AWSServices;
using Common.Types.Teams;
using Microsoft.AspNetCore.Mvc;
using FilterAttribute = Amazon.BedrockAgentRuntime.Model.FilterAttribute;
using KnowledgeBaseRetrieveAndGenerateConfiguration = Amazon.BedrockAgentRuntime.Model.KnowledgeBaseRetrieveAndGenerateConfiguration;
using OrchestrationConfiguration = Amazon.BedrockAgentRuntime.Model.OrchestrationConfiguration;
using QueryTransformationConfiguration = Amazon.BedrockAgentRuntime.Model.QueryTransformationConfiguration;
using QueryTransformationType = Amazon.BedrockAgentRuntime.QueryTransformationType;
using RetrievalFilter = Amazon.BedrockAgentRuntime.Model.RetrievalFilter;
using RetrieveAndGenerateConfiguration = Amazon.BedrockAgentRuntime.Model.RetrieveAndGenerateConfiguration;
using RetrieveAndGenerateType = Amazon.BedrockAgentRuntime.RetrieveAndGenerateType;

namespace ExscriboAPI.Controllers
{
    /// <summary>
    /// Controller for handling chatbot interactions using Amazon Bedrock
    /// Provides endpoints for querying the chatbot with team-specific knowledge bases
    /// </summary>
    [Produces("application/json")]
    [Route("/teams")]
    public class ChatBotController(IAmazonBedrockAgentRuntime bedrockAgentRuntimeClient, ILoggerFactory logger) : Controller
    {
        private readonly IAmazonBedrockAgentRuntime _bedrockAgentRuntimeClient = bedrockAgentRuntimeClient ?? throw new ArgumentNullException(nameof(bedrockAgentRuntimeClient));
        private readonly string? _bedrockKbId = EnvironmentHelper.BEDROCK_KB_ID;
        private readonly ILogger<ChatBotController> _logger = logger.CreateLogger<ChatBotController>() ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Processes a chatbot question and returns a generated response using Amazon Bedrock
        /// </summary>
        /// <param name="teamsId">The ID of the team whose knowledge base should be queried</param>
        /// <param name="chatbotRequest">The request containing the question and optional meeting ID</param>
        /// <returns>A response containing the generated answer or an error message</returns>
        /// <remarks>
        /// This endpoint filters the knowledge base by team ID and optionally by meeting ID.
        /// It uses Amazon Bedrock's retrieve and generate capability to provide contextual answers.
        /// </remarks>
        [Route("{teamsId:Guid}/chatbot")]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChatBotQuestion([FromRoute] Guid teamsId, [FromBody] ChatBotRequestType? chatbotRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            // Configure the retrieval filter based on team ID and optional meeting ID
            RetrievalFilter retrievalFilter = new RetrievalFilter();
            if (chatbotRequest?.MeetingId != null)
            {
                // Filter by both team ID and meeting ID
                retrievalFilter.AndAll = new()
                {
                    new()
                    {
                        Equals = new FilterAttribute
                        {
                            Key = "TeamId",
                            Value = new Amazon.Runtime.Documents.Document(teamsId.ToString())
                        }
                    },
                    new()
                    {
                        Equals = new FilterAttribute
                        {
                            Key = "MeetingId",
                            Value = new Amazon.Runtime.Documents.Document(chatbotRequest.MeetingId.ToString())
                        }
                    }
                };
            }
            else
            {
                // Filter by team ID only
                retrievalFilter.Equals = new FilterAttribute
                {
                    Key = "TeamId",
                    Value = new Amazon.Runtime.Documents.Document(teamsId.ToString())
                };
            }

            // Validate the question
            if (chatbotRequest?.Question == null) return BadRequest("Question is null");
            if (chatbotRequest.Question?.Length == 0) return BadRequest("Question is empty");
            if (chatbotRequest.Question?.Length > 1000) return BadRequest("Question is too long");
            
            RetrieveAndGenerateResponse? response;
            try
            {
                // Create the retrieve and generate request
                var request = new RetrieveAndGenerateRequest
                {
                    Input = new RetrieveAndGenerateInput
                    {
                        Text = chatbotRequest.Question,
                    },
                    RetrieveAndGenerateConfiguration = new RetrieveAndGenerateConfiguration
                    {
                        KnowledgeBaseConfiguration = new KnowledgeBaseRetrieveAndGenerateConfiguration
                        {
                            OrchestrationConfiguration = new OrchestrationConfiguration()
                            {
                                QueryTransformationConfiguration = new QueryTransformationConfiguration()
                                {
                                    Type = QueryTransformationType.QUERY_DECOMPOSITION
                                }
                            },
                            KnowledgeBaseId = _bedrockKbId,
                            ModelArn = EnvironmentHelper.BEDROCK_MODEL_INFERENCE_ARN
                        },
                        Type = RetrieveAndGenerateType.KNOWLEDGE_BASE,
                    }
                };
                _logger.LogInformation("Query request: {@request}", request);

                // Send the request to Bedrock
                response = await _bedrockAgentRuntimeClient.RetrieveAndGenerateAsync(request);
            }
            catch (AmazonBedrockAgentRuntimeException ex)
            {
                _logger.LogError(ex, "Bedrock Error: {Message}", ex.Message);
                return new BadRequestObjectResult(new { error = $"Bedrock Error: {ex.Message}" });
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Exception Error: {Message}", ex.Message);
                return new BadRequestObjectResult(new { error = $"Exception Error: {ex.Message}" });
            }

            // Log the response and return the result
            _logger.LogInformation("KB Response Code:  {@Output}", JsonSerializer.Serialize(response.HttpStatusCode));
            _logger.LogInformation("KB Response: {@Output}", JsonSerializer.Serialize(response.Output));
            return new OkObjectResult(new { result = response.Output.Text });
        }
    }
}