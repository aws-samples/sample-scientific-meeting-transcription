// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console()
                .CreateBootstrapLogger();

            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            });
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

            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Transient);
            AWSSDKHandler.RegisterXRayForAllServices();
            // Register services

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(dispose: true, logger: Log.Logger);
            });
            // Add AWS service clients if needed
            services.AddAWSService<IAmazonTranscribeService>();
            services.AddAWSService<IAmazonBedrockRuntime>();
            services.AddAWSService<AmazonBedrockRuntimeClient>();
            services.AddAWSService<IAmazonBedrock>();
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonXRay>();
            services.AddAWSService<IAmazonBedrockAgent>();
            services.AddAWSService<IAmazonSimpleSystemsManagement>();

            services.AddScoped<MeetingMapService>();
            services.AddScoped<PromptSetMapService>();
            services.AddScoped<PromptMapService>();
            services.AddScoped<TeamMapService>();
            services.AddScoped<MeetingPromptResponseMapService>();
            services.AddScoped<CustomModelMapService>();
            services.AddScoped<CustomVocabularyMapService>();
            services.AddScoped<VocabularyPhraseMapService>();

            services.AddTransient<ITranscribeService, TranscribeService>();
            services.AddTransient<IPromptProcessService, PromptProcessService>();
            services.AddTransient<ICustomModelTrainingService, CustomModelTrainingTrainingService>();
            services.AddTransient<ICustomVocabularyProcessService, CustomVocabularyProcessService>();
            services.AddTransient<ISealMeetingProcessService, SealMeetingProcessService>();

            // Configure AWS options if needed
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
        }
    }
}