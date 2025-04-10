# AWS Step Function Lambda Project Documentation

This document provides an overview of the AWS Lambda function project that processes Step Function state machine executions for various job types.

## Project Structure

The project consists of the following key components:

- **Function.cs**: Main Lambda entry point that handles AWS Step Function requests
- **Startup.cs**: Configures services for dependency injection
- **LocalEntryPoint.cs**: Provides local development capabilities
- **Services/**: Contains service interfaces and implementations for different job types

## Job Types

The Lambda function handles the following job types:

1. **Transcribe**: Processes audio files using Amazon Transcribe to generate text transcriptions
2. **PromptProcess**: Processes prompts against language models to generate insights from meeting transcripts
3. **CustomModel**: Creates or updates custom language models for improved transcription accuracy
4. **SealMeeting**: Finalizes meeting data and marks it as complete/sealed
5. **CustomVocabulary**: Creates or updates custom vocabularies to improve transcription of domain-specific terms

## Service Interfaces

The project defines the following service interfaces:

- **ITranscribeService**: Handles audio transcription requests
- **IPromptProcessService**: Handles LLM prompt requests
- **ICustomModelTrainingService**: Handles creation and training of custom language models
- **ISealMeetingProcessService**: Handles meeting finalization
- **ICustomVocabularyProcessService**: Handles custom vocabulary creation and management

## Deployment

The Lambda function can be deployed using the AWS Lambda Tools for .NET CLI:

```bash
dotnet lambda deploy-function
```

## Local Development

For local development and testing, you can use the LocalEntryPoint class which provides a web host setup with the Startup class for service configuration.

## Dependencies

The project uses the following key dependencies:

- AWS SDK for .NET
- Amazon.Lambda.Core
- Microsoft.Extensions.DependencyInjection
- AutoMapper
- Serilog
