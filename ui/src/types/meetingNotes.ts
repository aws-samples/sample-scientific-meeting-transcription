/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
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