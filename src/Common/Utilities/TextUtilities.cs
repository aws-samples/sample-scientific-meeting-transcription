// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using Microsoft.AspNetCore.StaticFiles;

namespace Common.Utilities;

public static class TextUtilities
{
    public static string GetMimeType(string? fileName)
    {
        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(fileName!, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }
}