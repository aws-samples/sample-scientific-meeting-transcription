// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using Amazon.BedrockAgentRuntime;
using Amazon.BedrockAgentRuntime.Model;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Runtime.Documents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ExscriboAPI
{
    /// <summary>
    /// Lambda entry point for the application when running in AWS Lambda environment
    /// This class handles the translation between API Gateway requests and the ASP.NET Core framework
    /// </summary>
    public class LambdaEntryPoint : APIGatewayProxyFunction
    {
        /// <summary>
        /// Configures the web host builder with the application's Startup class
        /// </summary>
        /// <param name="builder">The web host builder to configure</param>
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseStartup<Startup>();
        }

        /// <summary>
        /// Use this override to customize the services registered with the IHostBuilder. 
        /// 
        /// It is recommended not to call ConfigureWebHostDefaults to configure the IWebHostBuilder inside this method.
        /// Instead customize the IWebHostBuilder in the Init(IWebHostBuilder) overload.
        /// </summary>
        /// <param name="builder">The IHostBuilder to configure.</param>
        protected override void Init(IHostBuilder builder)
        {
        }
    }
}