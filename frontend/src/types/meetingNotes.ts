/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.
 */


export interface MeetingNotesType {
    jobName: string;
    transcript: string;
    version: number;
    changes: NotesWordChanges[];
    segments: SpeakerSegmentType[];
    words: SpeakerWordType[];
    last_edited_at: string;
    last_edited_by: string;
}

export interface SpeakerSegmentType {
    segment_id: number;
    transcript: string;
    start_time: number;
    end_time: number;
    speaker_label: string;
}

export interface SpeakerWordType {
    word_id: number;
    segment_id: number;
    edited: boolean;
    word_type: WordType;
    changeType: ScriptChangeType;
    confidence: number;
    content: string;
    start_time: number;
    end_time: number;

}

export interface NotesWordChanges {
    word_id: number;
    type: ScriptChangeType;
    timestamp: string;
    user: string;
    original: string;
    change: string;
    undone: boolean;
    redone: boolean;
    word: SpeakerWordType;
}

export enum WordType {
    Pronunciation = "pronunciation",
    Punctuation = "punctuation"
}

export enum ScriptChangeType {
    Speaker = "speaker",
    Content = "content",
    Update = "update",
    Delete = "delete",
    Add = "add",
    None = "none"
}

export interface ConfidenceStats {
    veryHigh: number;
    high: number;
    medium: number;
    low: number;
    veryLow: number;
  }