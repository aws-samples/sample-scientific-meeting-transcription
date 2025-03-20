// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Common
{
    public static class ApiActions
    {
        public static IActionResult? HandleApiReturnException(Exception? exception, ILogger logger, object? payloadObject = null)
        {
            if (payloadObject != null)
            {
                logger.LogError("Payload JSON: {@payloadObject}", payloadObject);
            }

            logger.LogError("Exception: {@exception}", exception);

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

            return CreateResponse(HttpStatusCode.UnprocessableEntity, exception);
        }

        public static IActionResult? CreateResponse(HttpStatusCode? httpCode, object? information = null)
        {
            return httpCode switch
            {
                HttpStatusCode.OK => new OkObjectResult(information),
                HttpStatusCode.Created => new CreatedResult((string?)null, information),
                HttpStatusCode.Accepted => new AcceptedResult((string?)null, information),
                HttpStatusCode.NoContent => new NoContentResult(),
                HttpStatusCode.BadRequest => new BadRequestObjectResult(information),
                HttpStatusCode.UnprocessableEntity => new UnprocessableEntityObjectResult(information),
                HttpStatusCode.NotFound => new NotFoundObjectResult(information),
                HttpStatusCode.Conflict => new ConflictObjectResult(information),
                HttpStatusCode.Forbidden => new ForbidResult(),
                HttpStatusCode.Unauthorized => new UnauthorizedResult(),
                _ => null
            };
        }
    }
}