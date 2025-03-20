// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Common.Types
{
    public class PaginatedList<T>(List<T> items, int pageIndex, int totalPages, int totalRecords)
    {
        [JsonPropertyName("records")] public List<T>? Records { get; set; } = items;
        [JsonPropertyName("page_index")] public int? PageIndex { get; set; } = pageIndex;
        [JsonPropertyName("total_pages")] public int? TotalPages { get; set; } = totalPages;

        [JsonPropertyName("has_previous_page")]
        public bool? HasPreviousPage => PageIndex > 1;

        [JsonPropertyName("has_next_page")] public bool? HasNextPage => PageIndex < TotalPages;
        [JsonPropertyName("total_records")] public int? TotalRecords { get; set; } = totalRecords;
    }
}