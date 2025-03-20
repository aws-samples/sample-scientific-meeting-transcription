/*
 // Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms and the SOW between the parties dated 2025
 */

export interface MeetingMetadata {
    meetingId: string;
    meetingTitle: string;
    dateTime: string;
    meetingDuration: number;
    meetingType: string;
    virtualOrInPersonOrHybrid: string;
}

export interface MeetingSummary {
    overview: {
        summary: string;
        actionItems: string;
        nextMeeting: {
            date: string;
            agenda: string;
        };
        problem: string;
        solution: string;
    };
}

export interface SpeakingMetrics {
    totalDuration: number;
    speakingTurns: number;
    averageContributionLength: number;
}

export interface Participant {
    id: string;
    name: string;
    role: string;
    department: string;
    expertise: string;
    speakingMetrics: SpeakingMetrics;
    participantLocation: string;
}

export interface Attendance {
    participants: Participant[];
}

export interface ParticipationMetrics {
    activeParticipants: number;
    totalParticipants: number;
    participantParticipationRate: number;
    averageSpeakingTimeForEachParticipant: number;
    totalQuestionsAsked: number;
    totalQuestionsAnswered: number;
}

export interface EngagementAnalytics {
    participationMetrics: ParticipationMetrics;
}

export interface AgendaItem {
    id: string;
    agendaTopic: string;
    topicDuration: number;
    TopicCompletionPercentage: number;
    topicNotes: string;
}

export interface AgendaCoverage {
    summary: {
        totalItemsCovered: number;
        completedTopicItems: number;
        completionTopicRate: number;
    };
    items: AgendaItem[];
}

export interface ActionItem {
    id: string;
    actionItemDescription: string;
    actionItemAssignee: string;
    actionItemDueDate: string;
    actionItemPriority: string;
    actionItemStatus: string;
}

export interface ActionItems {
    summary: {
        totalActionItems: number;
        assignedActionItems: number;
    };
    items: ActionItem[];
    previousMeetingCompletion: any[]; // or specific type if known
}

export interface MeetingObjective {
    meetingObjectiveDescription: string;
    meetingObjectiveAchieved: boolean;
    meetingObjectiveNotes: string;
}

export interface TimeManagement {
    productiveTime: number;
    idleTime: number;
    meetingInterruptions: any[]; // or specific type if known
}

export interface Effectiveness {
    objectives: MeetingObjective[];
    timeManagement: TimeManagement;
}

export interface Feedback {
    overall: {
        averageFeedbackRating: number;
        generalFeedbackSentiment: string;
    };
    participantFeedback: any[]; // or specific type if known
}

export interface MeetingAnalytics {
    meetingMetadata: MeetingMetadata;
    meetingSummary: MeetingSummary;
    attendance: Attendance;
    engagementAnalytics: EngagementAnalytics;
    agendaCoverage: AgendaCoverage;
    decisions: {
        totalDecisions: number;
        items: any[]; // or specific type if known
    };
    actionItems: ActionItems;
    effectiveness: Effectiveness;
    feedback: Feedback;
}

export interface SpeakerTimeChartOptionsInterface {
  labels: string[];
  title: {
    text: 'Speaker Time Distribution',
  },
  chartOptions: {
    chart: {
      type: 'pie',
      height: 200,
    }
  },
}

export interface speakerContributionChartOptionsInterface {
    title?: {
      text: string;
    };
    xaxis?: {
      categories: string[];
    };
    tooltip?: {
      y: {
        formatter: (value: number, { seriesIndex, dataPointIndex }: any) => string;
      };
    };
  }