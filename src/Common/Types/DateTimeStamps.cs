// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;

namespace Common.Types;

public class DateTimeStamps
{
    private static readonly TimeZoneInfo EasternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
    private DateTime _createField;
    private DateTime _updatedField;

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt
    {
        get => TimeZoneInfo.ConvertTimeFromUtc(_createField, EasternZone);
        set => _createField = (DateTime)value!;
    }

    [JsonPropertyName("updated_at")]
    public DateTime? UpdatedAt
    {
        get => TimeZoneInfo.ConvertTimeFromUtc(_updatedField, EasternZone);
        set => _updatedField = (DateTime)value!;
    }
}