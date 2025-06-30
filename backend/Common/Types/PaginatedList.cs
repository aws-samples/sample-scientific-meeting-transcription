// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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