// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.

using Common.Types;
using Microsoft.AspNetCore.Mvc;

namespace ExscriboAPI.Controllers
{
    /// <summary>
    /// Controller for retrieving system information and version details
    /// </summary>
    [Route("/teams")]
    [ApiController]
    public class SystemInfoController() : ControllerBase
    {
        /// <summary>
        /// Gets the current application version and build date
        /// </summary>
        /// <returns>An object containing version and build date information</returns>
        [HttpGet]
        [Route("version")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetVersion()
        {
            return Ok(new
            {
                version = VersionInfo.Version,
                buildDate = VersionInfo.BuildDate
            });
        }
    }
}
