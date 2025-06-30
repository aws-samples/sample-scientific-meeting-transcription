/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */

export enum StatusEnum {
  Active = 'Active',
  Inactive = 'Inactive'
}

export enum TranscribeBaseModel {
  NarrowBand = 'NarrowBand',
  WideBand = 'WideBand'
}

export interface PaginatedList<T> {
  records?: T[];
  page_index: number;
  total_pages: number;
  has_previous_page: boolean;
  has_next_page: boolean;
  total_records: number;
}

export const languageCodes = [
  {
    id: "en-US",
    name: "English (United States)",
  },
  {
    id: "hi-IN",
    name: "Hindi (India)",
  },
  {
    id: "es-US",
    name: "Spanish (United States)",
  },
  {
    id: "en-GB",
    name: "English (United Kingdom)",
  },
  {
    id: "en-AU",
    name: "English (Australia)",
  },
  {
    id: "de-DE",
    name: "German (Germany)",
  },
  {
    id: "ja-JP",
    name: "Japanese (Japan)",
  },
];