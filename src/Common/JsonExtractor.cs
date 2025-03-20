// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Common;

public static class JsonExtractor
{
    public static T? NaiveJsonFromText<T>(string text)
    {
        try
        {
            AppDomain domain = AppDomain.CurrentDomain;
            domain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));
            // Match pattern for JSON content - matches everything between { and } and []
            var match = Regex.Match(text, @"(?:\{|\[)[\s\S]*(?:\}|\])");
            if (!match.Success)
            {
                return default;
            }

            // Parse the matched JSON string
            return JsonSerializer.Deserialize<T>(match.Value);
        }
        catch
        {
            return default;
        }
    }

    // Overload that returns JsonDocument if you don't know the type
    public static JsonDocument? NaiveJsonFromText(string text)
    {
        try
        {
            AppDomain domain = AppDomain.CurrentDomain;
            domain.SetData("REGEX_DEFAULT_MATCH_TIMEOUT", TimeSpan.FromSeconds(2));
            var match = Regex.Match(text, @"(?:\{|\[)[\s\S]*(?:\}|\])");
            if (!match.Success)
            {
                return null;
            }

            return JsonDocument.Parse(match.Value);
        }
        catch
        {
            return null;
        }
    }
}