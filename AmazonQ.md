# Exscribo Code Documentation

This document provides an overview of the code structure and key components of the Exscribo project, which is a prototype/demo platform showcasing AWS services for recording transcription and GenAI use cases.

## Project Overview

Exscribo is built with:
- Backend: .NET 8.0 deployed to Lambda functions
- Frontend: UI built with Vue.js/TypeScript
- AWS Services: Transcribe, Bedrock, S3, Step Functions, and more

## Key Components

### AWS Services Integration

The project extensively uses AWS services through the following components:

1. **BedrockClaudeClient**: Handles interactions with Amazon Bedrock Claude model for AI text generation
   - Manages API requests and response parsing
   - Configures model parameters and handles errors

2. **BedrockKnowledgeBase**: Manages document storage and retrieval in Bedrock Knowledge Base
   - Provides methods for ingesting binary and text documents
   - Handles document deletion and status monitoring

3. **BedrockNovaClient**: Interfaces with Amazon Bedrock Nova model
   - Similar to Claude client but specific to Nova model

4. **EnvironmentHelper**: Centralizes access to environment variables
   - Validates and retrieves configuration values
   - Provides access to service ARNs, bucket names, etc.

5. **InferenceProfiles**: Manages Bedrock inference profiles
   - Retrieves and filters available inference profiles

6. **BedrockProcessor**: Processes prompts using Amazon Bedrock
   - Analyzes meeting content using Claude model

### API Controllers

The API layer includes controllers for various resources:

1. **MeetingsController**: Manages meeting resources
   - Creates and updates meetings
   - Handles transcription processing
   - Manages meeting notes and analytics

2. **Other Controllers**: Handle teams, custom models, vocabularies, etc.

### Step Function Lambda

The Step Function Lambda handles various processing tasks:

1. **LambdaEntryPoint**: Main entry point for Step Function requests
   - Routes requests based on job type
   - Handles errors and returns appropriate responses

2. **TranscribeService**: Processes transcription requests
   - Converts audio to text using Amazon Transcribe
   - Formats and stores transcription results

### Frontend Services

The UI includes services for interacting with the backend:

1. **MeetingService**: Handles meeting-related API calls
   - Creates, updates, and deletes meetings
   - Manages transcription processing

2. **ChatbotService**: Manages interactions with the AI assistant
   - Submits questions and retrieves responses

## Database Structure

The application uses a PostgreSQL database with tables for:
- Meetings
- Teams
- Custom Models
- Vocabularies
- Prompts and Prompt Sets
- Meeting Documents

## Project Status

This is a prototype/demo platform to showcase AWS services around recording transcription and GenAI use cases. The project was developed as an example and won't be improved or maintained going forward.
