// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Text.Json.Serialization;
using Common.Types.Meetings;
using Common.Types.Prompts;
using Common.Types.Teams;
using Common.Utilities;

namespace Common.Types.MeetingDocuments
{
    public class MeetingDocumentDatabaseType : IEntityDate
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public Guid? TeamId { get; set; }
        public virtual TeamDatabaseType? Team { get; set; }
        public Guid? MeetingId { get; set; }
        public virtual MeetingDatabaseType? Meeting { get; set; }
        public string? Filename { get; set; }
        public virtual string DocumenLocation => $"teams/{TeamId}/meetings/{Id}/documents/{Filename}";
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public virtual string? MimeType => TextUtilities.GetMimeType(Filename);
    }

    public class MeetingDocumentRequestType
    {
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("filename")] public string? Filename { get; set; }
    }

    public class MeetingDocumentResponseType : DateTimeStamps
    {
        [JsonPropertyName("id")] public Guid? Id { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("team_id")] public Guid? TeamId { get; set; }
        [JsonPropertyName("meeting_id")] public Guid? MeetingId { get; set; }
        [JsonPropertyName("filename")] public string? Filename { get; set; }

        [JsonPropertyName("mimetype")] public string? MimeType => TextUtilities.GetMimeType(Filename);
    }
}