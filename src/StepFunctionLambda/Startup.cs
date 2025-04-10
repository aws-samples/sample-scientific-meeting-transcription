// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Text.Json.Serialization;
using Amazon.Bedrock;
using Amazon.BedrockAgent;
using Amazon.BedrockRuntime;
using Amazon.S3;
using Amazon.SimpleSystemsManagement;
using Amazon.TranscribeService;
using Amazon.XRay;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AutoMapper;
using Common;
using Common.Types.CustomModels;
using Common.Types.CustomVocabularies;
using Common.Types.MeetingPromptResponses;
using Common.Types.Meetings;
using Common.Types.Prompts;
using Common.Types.PromptSets;
using Common.Types.Teams;
using Common.Types.VocabularyPhrases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using StepFunctionLambda.Services;

namespace StepFunctionLambda
{
    /// <summary>
    /// Startup class that configures services for the Lambda function.
    /// Handles dependency injection setup, logging configuration, and AWS service registration.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configuration instance that provides access to application settings
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Constructor that initializes the Startup class with configuration
        /// </summary>
        /// <param name="configuration">Application configuration from environment variables and other sources</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures services for dependency injection.
        /// Sets up logging, JSON serialization options, AutoMapper profiles, database context,
        /// AWS service clients, and application services.
        /// </summary>
        /// <param name="services">Service collection to configure</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Serilog for structured logging
            // Sets up console logging with appropriate log levels for different namespaces
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console()
                .CreateBootstrapLogger();

            // Configure JSON serialization options for HTTP requests/responses
            // Enables enum string conversion, reference cycle handling, and case insensitivity
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            });
            
            // Configure AutoMapper for object-to-object mapping
            // Registers all mapping profiles for different entity types
            MapperConfiguration mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new PromptMappingProfile());
                mc.AddProfile(new TeamMappingProfile());
                mc.AddProfile(new PromptSetMappingProfile());
                mc.AddProfile(new CustomModelMappingProfile());
                mc.AddProfile(new MeetingMappingProfile());
                mc.AddProfile(new MeetingPromptResponsesMappingProfile());
                mc.AddProfile(new CustomVocabularyMappingProfile());
                mc.AddProfile(new VocabularyPhraseMappingProfile());
            });

            // Create and register mapper instance
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            
            // Register database context with transient lifetime
            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Transient);
            
            // Register AWS X-Ray for tracing all AWS SDK calls
            AWSSDKHandler.RegisterXRayForAllServices();
            
            // Configure logging to use Serilog
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(dispose: true, logger: Log.Logger);
            });
            
            // Register AWS service clients for dependency injection
            services.AddAWSService<IAmazonTranscribeService>();
            services.AddAWSService<IAmazonBedrockRuntime>();
            services.AddAWSService<AmazonBedrockRuntimeClient>();
            services.AddAWSService<IAmazonBedrock>();
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonXRay>();
            services.AddAWSService<IAmazonBedrockAgent>();
            services.AddAWSService<IAmazonSimpleSystemsManagement>();

            // Register mapping services with scoped lifetime
            services.AddScoped<MeetingMapService>();
            services.AddScoped<PromptSetMapService>();
            services.AddScoped<PromptMapService>();
            services.AddScoped<TeamMapService>();
            services.AddScoped<MeetingPromptResponseMapService>();
            services.AddScoped<CustomModelMapService>();
            services.AddScoped<CustomVocabularyMapService>();
            services.AddScoped<VocabularyPhraseMapService>();

            // Register business logic services with transient lifetime
            services.AddTransient<ITranscribeService, TranscribeService>();
            services.AddTransient<IPromptProcessService, PromptProcessService>();
            services.AddTransient<ICustomModelTrainingService, CustomModelTrainingTrainingService>();
            services.AddTransient<ICustomVocabularyProcessService, CustomVocabularyProcessService>();
            services.AddTransient<ISealMeetingProcessService, SealMeetingProcessService>();

            // Configure default AWS options from configuration
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
        }
    }
}
