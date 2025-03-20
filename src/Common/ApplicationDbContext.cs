// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SecretsManager.Extensions.Caching;
using Common.AWSServices;
using Common.Types;
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

namespace Common;

public sealed class ApplicationDbContext : DbContext
{
    private readonly SecretsManagerCache _secretsCache = new();
    private readonly DbContextOptions<ApplicationDbContext>? _options;

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        GenerateOnUpdate();
        return base.SaveChangesAsync(cancellationToken);
    }

    public ApplicationDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        OnConfiguring(optionsBuilder);
        _options = optionsBuilder.Options;
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        _options = options;
    }


    public override int SaveChanges()
    {
        GenerateOnUpdate();
        return base.SaveChanges();
    }

    public void GenerateOnUpdate()
    {
        var now = DateTime.UtcNow;
        foreach (var changedEntity in ChangeTracker.Entries())
        {
            if (changedEntity.Entity is IEntityDate entity)
            {
                switch (changedEntity.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = now;
                        entity.UpdatedAt = now;
                        break;

                    case EntityState.Modified:
                        Entry(entity).Property(x => x.CreatedAt).IsModified = false;
                        entity.UpdatedAt = now;
                        break;
                }
            }
        }
    }

    // Keep your existing constructor

    private string? GetConnectionStringAsync()
    {
        try
        {
            string? dbSecretKey = EnvironmentHelper.DB_SECRET_KEY;
            string dbSecretResponse = _secretsCache.GetSecretString(dbSecretKey).Result;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var secretsData = JsonSerializer.Deserialize<DatabaseCredentialsType>(dbSecretResponse, options);
            return secretsData?.GetConnectionString();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            bool isLambda = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));
            if (isLambda)
            {
                string? connectionString = GetConnectionStringAsync();
                optionsBuilder
                    .UseNpgsql(connectionString, options => options
                        .EnableRetryOnFailure(3)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                        .CommandTimeout(100))
                    .AddXRayInterceptor(true)
                    .EnableSensitiveDataLogging(false)
                    .EnableDetailedErrors(false);
                optionsBuilder.AddXRayInterceptor();
            }
            else
            {
                var connectionString = "server=localhost;port=3306;database=dummy;Username=dummy;password=dummy";
                optionsBuilder.UseNpgsql(connectionString);
            }
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion(typeof(DateTimeToDateTimeUtc));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CustomModelDatabaseType>(entity =>
        {
            entity.ToTable("custom_models");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.ModelName).IsRequired().HasColumnName("modelname").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(15);
            entity.Property(e => e.DataAccessRoleArn).HasColumnName("data_access_role_arn").HasMaxLength(55);
            entity.Property(e => e.TrainingDataS3Uri).HasColumnName("training_data_s3_uri").HasMaxLength(512);
            entity.Property(e => e.TranscribedDataS3Uri).HasColumnName("transcribed_data_s3_uri").HasMaxLength(512);
            entity.Property(e => e.TrainingDataS3UriFolder).HasColumnName("training_data_s3_uri_folder").HasMaxLength(512);
            entity.Property(e => e.ModelSetupProgress).HasColumnName("model_setup_progress").HasMaxLength(3).HasDefaultValue(CustomModelSetupProgressEnum.Created);
            entity.Property(e => e.PreSignedUrl).HasColumnName("presigned_url").HasMaxLength(4096);
            entity.Property(e => e.StateMachineExecutionArn).HasColumnName("state_machine_execution_arn").HasMaxLength(255);
            entity.Property(e => e.TranscribeBaseModel).HasColumnName("transcribe_base_model_name").HasDefaultValue(TranscribeBaseModel.WideBand);
            entity.Property(e => e.AwsModelStatus).HasColumnName("aws_model_status").HasMaxLength(50);
            entity.Property(e => e.ModelSetupMessage).HasColumnName("model_setup_status").HasMaxLength(512);
            entity.Property(e => e.PreSignedUrlExpire).HasColumnName("presigned_url_expire");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue(StatusEnum.Active);
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");

            entity.HasMany(x => x.Meetings)
                .WithOne(x => x.CustomModel)
                .HasForeignKey(x => x.CustomModelId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<MeetingDocumentDatabaseType>(entity =>
        {
            entity.ToTable("meeting_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.TeamId).IsRequired().HasColumnName("team_id");
            entity.Property(e => e.MeetingId).IsRequired().HasColumnName("meeting_id");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.Filename).HasColumnName("filename").IsRequired().HasMaxLength(512);
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
        });
        modelBuilder.Entity<CustomVocabularyDatabaseType>(entity =>
        {
            entity.ToTable("custom_vocabularies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.TeamId).IsRequired();
            entity.Property(e => e.VocabularyName).IsRequired().HasColumnName("vocabulary_name").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.LanguageCode).HasColumnName("language_code").HasMaxLength(15);
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue(StatusEnum.Active);
            entity.Property(e => e.CurrentStep).HasColumnName("current_step").HasDefaultValue(CustomVocabularyCurrentStepEnum.Created);
            entity.Property(e => e.PublishError).HasColumnName("publish_error");
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");

            entity.HasMany(x => x.Phrases)
                .WithOne(x => x.CustomVocabulary)
                .HasForeignKey(x => x.CustomVocabularyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<VocabularyPhraseDatabaseType>(entity =>
        {
            entity.ToTable("vocabulary_phrases");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.CustomVocabularyId).IsRequired();
            entity.Property(e => e.TeamId).IsRequired();
            entity.Property(e => e.Phrase).IsRequired().HasColumnName("vocabulary_name").HasMaxLength(255);
            entity.Property(e => e.DisplayAs).HasColumnName("display_as").HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
        });

        modelBuilder.Entity<TeamDatabaseType>(entity =>
        {
            entity.ToTable("teams");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.Team).IsRequired().HasColumnName("team").HasMaxLength(100);
            entity.Property(e => e.IdpGroup).IsRequired().HasColumnName("idp_group").HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasColumnName("status").HasDefaultValue(StatusEnum.Active);
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
            entity.HasMany(e => e.Meetings)
                .WithOne(x => x.Team)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.CustomModels)
                .WithOne(x => x.Team)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.PromptSets)
                .WithOne(x => x.Team)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.MeetingPromptResponses)
                .WithOne(x => x.Team)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.CustomVocabularies)
                .WithOne(x => x.Team)
                .HasForeignKey(e => e.TeamId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MeetingDatabaseType>(entity =>
        {
            entity.ToTable("meetings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TeamId).HasColumnName("team_id").IsRequired();
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd().HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.Title).IsRequired().HasColumnName("title").HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
            entity.Property(e => e.Status).IsRequired().HasColumnName("status").HasDefaultValue(StatusEnum.Active);
            entity.Property(e => e.Date).IsRequired().HasColumnName("meeting_date");
            entity.Property(e => e.PreSignedUrl).HasColumnName("pre_signe_url").HasMaxLength(4096);
            entity.Property(e => e.S3RecordingFullPath).HasColumnName("s3_recording_full_path").HasMaxLength(512);
            entity.Property(e => e.S3TranscribedFullPath).HasColumnName("s3_transcribed_fullpath").HasMaxLength(512);
            entity.Property(e => e.StateMachineExecutionArn).HasColumnName("state_machine_execution_arn").HasMaxLength(512);
            entity.Property(e => e.CurrentStep).HasColumnName("current_step").HasDefaultValue(CurrentStepEnum.Created);
            entity.Property(e => e.CustomModelId).HasColumnName("custom_model_id");
            entity.Property(e => e.CustomVocabularyId).HasColumnName("custom_vocabulary_id");
            entity.Property(e => e.S3OutputBucketName).HasColumnName("s3_output_bucket_name").HasMaxLength(255);
            entity.Property(e => e.S3OutputKeyName).HasColumnName("s3_output_key_name").HasMaxLength(255);
            entity.Property(e => e.TranscribeError).HasColumnName("transcribe_error").HasMaxLength(512);
            entity.Property(e => e.MeetingNotes).HasColumnName("meeting_notes").HasColumnType("text");
            entity.Property(e => e.MeetingNotesVersion).HasColumnName("meeting_notes_version").HasDefaultValue(1);

            entity.HasMany(x => x.MeetingDocuments)
                .WithOne(x => x.Meeting)
                .HasForeignKey(x => x.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.MeetingPromptResponses)
                .WithOne(x => x.Meeting)
                .HasForeignKey(x => x.MeetingId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.CustomModel)
                .WithMany(x => x.Meetings)
                .HasForeignKey(x => x.CustomModelId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.CustomVocabulary)
                .WithMany(x => x.Meetings)
                .HasForeignKey(x => x.CustomVocabularyId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.PromptSet)
                .WithMany(x => x.Meetings)
                .HasForeignKey(x => x.PromptSetId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<PromptSetDatabaseType>(entity =>
        {
            entity.ToTable("promptsets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.PromptSetName).IsRequired().HasColumnName("prompt_set_name").HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired().HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.Status).IsRequired().HasColumnName("status").HasMaxLength(3).HasDefaultValue(StatusEnum.Active);
            entity.Property(e => e.CreatePromptsFromDescription).HasColumnName("create_prompts_from_description");
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");

            entity.HasMany(x => x.Prompts)
                .WithOne(x => x.PromptSet)
                .HasForeignKey(e => e.PrompSetId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Meetings)
                .WithOne(x => x.PromptSet)
                .HasForeignKey(x => x.PromptSetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<PromptDatabaseType>(entity =>
        {
            entity.ToTable("prompts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.Prompt).IsRequired().HasColumnName("prompt").HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255);
            entity.Property(e => e.PrompSetId).IsRequired().HasColumnName("prompt_set_id");
            entity.Property(e => e.Status).IsRequired().HasColumnName("status").HasMaxLength(3).HasDefaultValue(StatusEnum.Active);
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");
            entity.Property(e => e.Order).IsRequired().HasColumnName("order").HasDefaultValue(1);

            entity.HasOne(x => x.PromptSet)
                .WithMany(x => x.Prompts)
                .HasForeignKey(x => x.PrompSetId)
                .IsRequired();
        });
        modelBuilder.Entity<MeetingPromptResponseDatabaseType>(entity =>
        {
            entity.ToTable("meeting_prompt_responses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").IsRequired().ValueGeneratedOnAdd()
                .HasValueGenerator<GuidGenerator>();
            entity.Property(e => e.PromptResponse)
                .IsRequired().HasColumnType("text")
                .HasColumnName("prompt_response");

            entity.Property(e => e.TeamId).IsRequired().HasColumnName("TeamId").IsRequired();
            entity.Property(e => e.MeetingId).IsRequired().HasColumnName("MeetingId").IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnName("updated_at");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnName("created_at");


            entity.HasOne(x => x.Meeting)
                .WithMany(x => x.MeetingPromptResponses)
                .HasForeignKey(x => x.MeetingId);
            entity.HasOne(x => x.Team)
                .WithMany(x => x.MeetingPromptResponses)
                .HasForeignKey(x => x.TeamId);
        });
    }

    public DbSet<CustomModelDatabaseType> CustomModels { get; set; } = null!;
    public DbSet<TeamDatabaseType> Teams { get; set; } = null!;
    public DbSet<MeetingDocumentDatabaseType> MeetingDocuments { get; set; } = null!;
    public DbSet<MeetingDatabaseType> Meetings { get; set; } = null!;
    public DbSet<PromptSetDatabaseType> PromptSets { get; set; } = null!;
    public DbSet<PromptDatabaseType> Prompts { get; set; } = null!;
    public DbSet<CustomVocabularyDatabaseType> CustomVocabularies { get; set; } = null!;
    public DbSet<VocabularyPhraseDatabaseType> VocabularyPhrases { get; set; } = null!;
    public DbSet<MeetingPromptResponseDatabaseType> MeetingPromptResponses { get; set; } = null!;
}