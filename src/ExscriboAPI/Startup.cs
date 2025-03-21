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
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

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

            services.AddSingleton(mapper);
            services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Add Swagger services
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

            AWSConfigsS3.UseSignatureVersion4 = true;
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
            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Transient);
            AWSSDKHandler.RegisterXRayForAllServices();

            // Add the mapping services
            services.AddScoped<MeetingMapService>();
            services.AddScoped<PromptSetMapService>();
            services.AddScoped<PromptMapService>();
            services.AddScoped<TeamMapService>();
            services.AddScoped<MeetingPromptResponseMapService>();
            services.AddScoped<CustomModelMapService>();
            services.AddScoped<MeetingDocumentMapService>();
            services.AddScoped<CustomVocabularyMapService>();
            services.AddScoped<VocabularyPhraseMapService>();
            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(dispose: true, logger: Log.Logger);
            });

            services.AddScoped<IMeetingDocumentsRepository, MeetingDocumentsRepository>();
            services.AddScoped<IMeetingRepository, MeetingRepository>();
            services.AddScoped<ITeamRepository, TeamRepository>();
            services.AddScoped<ICustomModelRepository, CustomModelRepository>();
            services.AddScoped<IPromptSetRepository, PromptSetRepository>();
            services.AddScoped<IMeetingPromptResponseRepository, MeetingPromptResponseRepository>();
            services.AddScoped<IPromptRepository, PromptRepository>();
            services.AddScoped<ICustomVocabularyRepository, CustomVocabularyRepository>();
            services.AddScoped<IVocabularyPhraseRepository, VocabularyPhraseRepository>();
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
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            //apply all migration if any are available
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Optionally check for pending migrations first
                    if (dbContext.Database.GetPendingMigrations().Any())
                    {
                        logger.LogInformation("Applying migrations...");;
                        dbContext.Database.Migrate();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying migrations.");
                    throw;
                }
            }

            app.UseXRay("DescribeAPI");
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseSwaggerUI(options => // UseSwaggerUI is called only in Development.
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                    options.RoutePrefix = string.Empty;
                });
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseCors(options =>
            {
                options.SetIsOriginAllowed(s => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}