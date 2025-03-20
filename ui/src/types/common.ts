/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
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