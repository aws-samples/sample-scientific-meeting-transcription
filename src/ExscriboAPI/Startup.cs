// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Text.Json.Serialization;
using Amazon;
using Amazon.Bedrock;
using Amazon.BedrockAgent;
using Amazon.BedrockAgentRuntime;
using Amazon.BedrockRuntime;
using Amazon.S3;
using Amazon.SecretsManager;
using Amazon.SimpleSystemsManagement;
using Amazon.StepFunctions;
using Amazon.TranscribeService;
using Amazon.XRay;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using AutoMapper;
using Common;
using Common.DAO.Classes;
using Common.DAO.Interfaces;
using Common.Types.CustomModels;
using Common.Types.CustomVocabularies;
using Common.Types.MeetingDocuments;
using Common.Types.MeetingPromptResponses;
using Common.Types.Meetings;
using Common.Types.Prompts;
using Common.Types.PromptSets;
using Common.Types.Teams;
using Common.Types.VocabularyPhrases;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

namespace ExscriboAPI
{
    /// <summary>
    /// Startup class for configuring the application services and request pipeline
    /// Uses primary constructor pattern to inject IConfiguration
    /// </summary>
    public class Startup(IConfiguration configuration)
    {
        /// <summary>
        /// Application configuration properties
        /// </summary>
        public IConfiguration Configuration { get; } = configuration;

        /// <summary>
        /// Configures the application services
        /// This method gets called by the runtime to add services to the container
        /// </summary>
        /// <param name="services">The service collection to configure</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Serilog for structured logging with appropriate log levels
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Extensions.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console()
                .CreateBootstrapLogger();

            // Configure AutoMapper with all required mapping profiles
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
                mc.AddProfile(new MeetingDocumentMappingProfile());
            });

            IMapper mapper = mapperConfig.CreateMapper();

            // Register AutoMapper as a singleton
            services.AddSingleton(mapper);
            
            // Configure JSON serialization options for HTTP responses
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            
            // Add controllers with JSON serialization options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Add Swagger services for API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ExscriboAPI",
                    Version = "v1",
                    Description = "ExscriboAPI"
                });
            });

            // Configure AWS services
            AWSConfigsS3.UseSignatureVersion4 = true; // Use Signature Version 4 for S3 authentication
            
            // Register all required AWS service clients
            services.AddAWSService<IAmazonBedrockAgentRuntime>();
            services.AddAWSService<IAmazonStepFunctions>();
            services.AddAWSService<IAmazonBedrock>();
            services.AddAWSService<IAmazonXRay>();
            services.AddAWSService<IAmazonSecretsManager>();
            services.AddAWSService<IAmazonSimpleSystemsManagement>();
            services.AddAWSService<IAmazonS3>();
            services.AddAWSService<IAmazonTranscribeService>();
            services.AddAWSService<IAmazonBedrockRuntime>();
            services.AddAWSService<AmazonBedrockRuntimeClient>();
            services.AddAWSService<IAmazonBedrockAgent>();
            
            // Add database context with transient lifetime
            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Transient);
            
            // Register AWS X-Ray for tracing all AWS service calls
            AWSSDKHandler.RegisterXRayForAllServices();

            // Register mapping services with scoped lifetime
            services.AddScoped<MeetingMapService>();
            services.AddScoped<PromptSetMapService>();
            services.AddScoped<PromptMapService>();
            services.AddScoped<TeamMapService>();
            services.AddScoped<MeetingPromptResponseMapService>();
            services.AddScoped<CustomModelMapService>();
            services.AddScoped<MeetingDocumentMapService>();
            services.AddScoped<CustomVocabularyMapService>();
            services.AddScoped<VocabularyPhraseMapService>();
            
            // Configure logging to use Serilog
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(dispose: true, logger: Log.Logger);
            });

            // Register repositories with scoped lifetime
            services.AddScoped<IMeetingDocumentsRepository, MeetingDocumentsRepository>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ICustomModelRepository, CustomModelRepository>();
            services.AddScoped<IPromptSetRepository, PromptSetRepository>();
            services.AddScoped<IMeetingPromptResponseRepository, MeetingPromptResponseRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<ICustomVocabularyRepository, CustomVocabularyRepository>();
            services.AddScoped<IVocabularyPhraseRepository, VocabularyPhraseRepository>();
            
            // Configure CORS to allow all origins, methods, and headers
            services.AddCors(option =>
            {
                option.AddPolicy("all", builder =>
                {
                    builder.SetIsOriginAllowed(s => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });
            
            // Add default AWS options from configuration
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
        }

        /// <summary>
        /// Configures the HTTP request pipeline
        /// This method gets called by the runtime to configure the HTTP request pipeline
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The web hosting environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Apply database migrations if any are available
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Check for pending migrations before applying them
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        logger.LogInformation("Applying migrations...");
                        dbContext.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying migrations.");
                    throw;
                }
            }

            // Enable AWS X-Ray for request tracing
            app.UseXRay("DescribeAPI");
            
            // Configure development-specific middleware
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseSwaggerUI(options => 
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
                app.UseDeveloperExceptionPage();
            }

            // Configure standard middleware pipeline
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            
            // Configure CORS middleware
            app.UseCors(options =>
            {
                options.SetIsOriginAllowed(s => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
            
            // Map controller endpoints
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}