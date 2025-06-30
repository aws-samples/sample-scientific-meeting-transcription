// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common
{
    /// <summary>
    /// Static utility class for handling API responses and exceptions
    /// Provides standardized methods for creating API responses and handling errors
    /// </summary>
    public static class ApiActions
    {
        /// <summary>
        /// Handles exceptions in API calls and creates appropriate error responses
        /// </summary>
        /// <param name="exception">The exception to handle</param>
        /// <param name="logger">Logger for tracking errors</param>
        /// <param name="payloadObject">Optional payload object for additional error context</param>
        /// <returns>An appropriate IActionResult based on the exception</returns>
        public static IActionResult? HandleApiReturnException(Exception? exception, ILogger logger, object? payloadObject = null)
        {
            // Log the payload if provided
            if (payloadObject != null)
            {
                logger.LogError("Payload JSON: {@payloadObject}", payloadObject);
            }

            // Log the full exception details
            logger.LogError("Exception: {@exception}", exception);

            // Check for exception messages at different levels and return appropriate responses
            if (exception?.Message != null)
            {
                return CreateResponse(HttpStatusCode.UnprocessableEntity, exception?.Message);
            }

            if (exception?.InnerException?.Message != null)
            {
                return CreateResponse(HttpStatusCode.UnprocessableEntity, exception.InnerException.Message);
            }

            if (exception?.InnerException?.InnerException != null)
            {
                return CreateResponse(HttpStatusCode.UnprocessableEntity, exception.InnerException.InnerException.Message);
            }

            // Return the full exception if no specific message is available
            return CreateResponse(HttpStatusCode.UnprocessableEntity, exception);
        }

        /// <summary>
        /// Creates a standardized API response based on HTTP status code
        /// </summary>
        /// <param name="httpCode">The HTTP status code for the response</param>
        /// <param name="information">Optional information/payload to include in the response</param>
        /// <returns>An appropriate IActionResult for the status code</returns>
        public static IActionResult? CreateResponse(HttpStatusCode? httpCode, object? information = null)
        {
            // Map HTTP status codes to appropriate ActionResults
            return httpCode switch
            {
                // Success responses
                HttpStatusCode.OK => new OkObjectResult(information),
                HttpStatusCode.Created => new CreatedResult((string?)null, information),
                HttpStatusCode.Accepted => new AcceptedResult((string?)null, information),
                HttpStatusCode.NoContent => new NoContentResult(),
                
                // Client error responses
                HttpStatusCode.BadRequest => new BadRequestObjectResult(information),
                HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(information),
                HttpStatusCode.NotFound => new NotFoundObjectResult(information),
                HttpStatusCode.Conflict => new ConflictObjectResult(information),
                HttpStatusCode.Forbidden => new ForbidResult(),
                HttpStatusCode.Unauthorized => new UnauthorizedResult(),
                
                // Default case
                _ => null
            };
        }
    }
}
