# Common Library

This directory contains common utilities, services, and data access components used throughout the Exscribo application.

## Key Components

### ApiActions
Static utility class for handling API responses and exceptions. Provides standardized methods for creating API responses and handling errors.

Methods:
- `HandleApiReturnException`: Handles exceptions in API calls and creates appropriate error responses
- `CreateResponse`: Creates a standardized API response based on HTTP status code

### ApplicationDbContextFactory
Implements `IDesignTimeDbContextFactory<ApplicationDbContext>` to support Entity Framework Core design-time tools like migrations.

Methods:
- `CreateDbContext`: Creates a database context with development connection string for migrations

### JsonExtractor
Utility class for extracting JSON from text content using regular expressions.

Methods:
- `NaiveJsonFromText<T>`: Extracts and deserializes JSON from text to a specified type
- `NaiveJsonFromText`: Extracts JSON from text and returns as JsonDocument when type is unknown

### Directory Structure

- `/AWSServices`: AWS service integrations and utilities
- `/DAO`: Data Access Objects for database operations
- `/Migrations`: Entity Framework Core database migrations
- `/Types`: Common type definitions and models
- `/Utilities`: General utility classes and helpers

## Usage

This library is referenced by other projects in the Exscribo solution to provide shared functionality and maintain consistent patterns across the application.
