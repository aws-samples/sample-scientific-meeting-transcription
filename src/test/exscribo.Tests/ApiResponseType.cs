// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace exscribo.Tests
{
    public class ApiResponseType<T>
    {
        [JsonPropertyName("statusCode")] public int statusCode { get; set; }
        [JsonPropertyName("body")] public T body { get; set; }
    }

    public class PaginatedResponseList<T>
    {
        [JsonPropertyName("records")] public List<T> Records { get; set; }
        [JsonPropertyName("page_index")] public int PageIndex { get; set; }
        [JsonPropertyName("total_pages")] public int TotalPages { get; set; }

        [JsonPropertyName("has_previous_page")]
        public bool HasPreviousPage { get; set; }

        [JsonPropertyName("has_next_page")] public bool HasNextPage { get; set; }
        [JsonPropertyName("total_records")] public int TotalRecords { get; set; }
    }
}