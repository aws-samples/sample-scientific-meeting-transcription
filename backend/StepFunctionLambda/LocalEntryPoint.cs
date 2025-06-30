// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using StepFunctionLambda.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace StepFunctionLambda
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// This class provides local development and testing capabilities for the Lambda function.
    /// </summary>
    public class LocalEntryPoint
    {
        /// <summary>
        /// Entry point for local execution of the application.
        /// Currently empty as this is primarily used for AWS Lambda deployment.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        public static void Main(string[] args)
        {
            // This method is intentionally left empty as the primary execution path
            // is through AWS Lambda rather than local execution
        }

        /// <summary>
        /// Creates and configures a host builder for local execution.
        /// Sets up the web host with the Startup class for service configuration.
        /// </summary>
        /// <param name="args">Command line arguments</param>
        /// <returns>Configured IHostBuilder instance</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}
