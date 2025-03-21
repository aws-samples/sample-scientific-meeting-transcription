// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


namespace StepFunctionLambda.Services;

public static class LlmJsonAnalyticsTemplate
{
    public static string SystemPrompt()
    {
        return """
               You are a specialized meeting analytics AI designed to extract structured data from meeting transcripts. Your core functions are:

               PROCESSING RULES:
               1. Only analyze content within <MeetingNotes></MeetingNotes> tags
               2. Identify speakers solely by "Speaker:" prefix
               3. Output only valid JSON data, no additional commentary, text or decorations as the output needs to be used in its raw format
               4. Use "unknown" for unavailable information and "0" where a number can be used to represent nothing.
               5. Take your time and think carefully about the results you create to make sure the information is consistent and correct.

               DATA FORMATTING:
               1. Time measurements: seconds (integer)
               2. Dates: ISO-8601 format (YYYY-MM-DDTHH:mm:ssZ)
               3. Percentages: 0-100 (number)
               4. Word limits: Strictly enforce specified limits
               5. Ratings: 1-5 scale (integer)

               VALIDATION RULES:
               - meetingType: only ["regular", "adhoc", "review"]
               - virtualOrInPersonOrHybrid: only ["virtual", "inPerson", "hybrid", "unknown]
               - participantLocation: ["virtual", "inPerson", "unknown"]
               - impact/priority: only ["high", "medium", "low", "unknown"]
               - sentiment: only ["positive", "neutral", "negative", "unknown"]
               - meetingType: only ["research", "scientific", "review", "planning", "unknown"]",
               - boolean values: true/false
               - IDs: unique within sections
               """;
    }

    public static string UserPrompt(string meetingNotes)
    {
        string template = """
                          You are a specialized meeting analytics AI. Analyze the following meeting transcript and generate a structured JSON output based on these rules:

                          CORE REQUIREMENTS:
                          1. Process only content within <MeetingNotes></MeetingNotes>
                          2. Identify speakers by "Speaker:" prefix
                          3. Output only valid JSON data, no additional commentary, text or decorations as the output needs to be used in its raw format
                          4. Use "unknown" for unavailable information and "0" where a number can be used to represent nothing.
                          5. Follow exact schema structure

                          FORMAT SPECIFICATIONS:
                          - Times in seconds
                          - Dates in ISO-8601
                          - Percentages as 0-100
                          - Word limits enforced
                          - Ratings as 1-5
                          - Boolean as true/false

                          <MeetingNotes>
                          """
                          + meetingNotes +
                          """
                          </MeetingNotes>
                           
                          Use this exact JSON schema for output:

                          {
                            "meetingMetadata": {
                              "meetingId": "string",
                              "meetingTitle": "string",
                              "dateTime": "ISO-8601 timestamp",
                              "meetingDuration": "number (seconds)",
                              "meetingType": "string (regular/adhoc/review)",
                              "virtualOrInPersonOrHybrid": "string",
                              "meetingType": "string (research, scientific, review, planning)",
                            },
                            "meetingSummary": {
                              "overview": {
                                "summary": "string (50 words)",
                                "actionItems": "string (50 words)",
                                "nextMeeting": {
                                  "date": "ISO-8601 timestamp",
                                  "agenda": "string (50 words)"
                                },
                                "problem": "string (50 words)",
                                "solution": "string (50 words)"
                              }
                            },
                            "attendance": {
                              "participants": [
                                {
                                  "id": "string",
                                  "name": "string",
                                  "role": "string",
                                  "department": "string",
                                  "expertise": "string (5 words)",
                                  "speakingMetrics": {
                                    "totalDuration": "number (seconds)",
                                    "speakingTurns": "number",
                                    "averageContributionLength": "number (seconds)"
                                  },
                                  "participantLocation": "boolean"
                                }
                              ]
                            },
                            "engagementAnalytics": {
                              "participationMetrics": {
                                "activeParticipants": "number",
                                "totalParticipants": "number",
                                "participantParticipationRate": "number (percentage)",
                                "averageSpeakingTimeForEachParticipant": "number (seconds)",
                                "totalQuestionsAsked": "number",
                                "totalQuestionsAnswered": "number"
                              }
                            },
                            "agendaCoverage": {
                              "summary": {
                                "totalItemsCovered": "number",
                                "completedTopicItems": "number",
                                "completionTopicRate": "number (percentage)"
                              },
                              "items": [
                                {
                                  "id": "string",
                                  "agendaTopic": "string",
                                  "topicDuration": "number (seconds)",
                                  "TopicCompletionPercentage": "number",
                                  "topicNotes": "string"
                                }
                              ]
                            },
                            "decisions": {
                              "totalDecisions": "number",
                              "items": [
                                {
                                  "id": "string",
                                  "decisionDescription": "string",
                                  "decisionOwner": "string",
                                  "tdecisionTimeSpent": "number (seconds)",
                                  "decisionsImpact": "string (high/medium/low)"
                                }
                              ]
                            },
                            "actionItems": {
                              "summary": {
                                "totalActionItems": "number",
                                "assignedActionItems": "number"
                              },
                              "items": [
                                {
                                  "id": "string",
                                  "actionItemDescription": "string",
                                  "actionItemAssignee": "string",
                                  "actionItemDueDate": "ISO-8601 timestamp",
                                  "actionItemPriority": "string (high/medium/low)",
                                  "actionItemStatus": "string"
                                }
                              ],
                              "previousMeetingCompletion": [
                                {
                                  "assignee": "string",
                                  "completionRate": "number (percentage)"
                                }
                              ]
                            },
                            "effectiveness": {
                              "objectives": [
                                {
                                  "meetingObjectiveDescription": "string (50 words)",
                                  "meetingObjectiveAchieved": "boolean",
                                  "meetingObjectiveNotes": "string"
                                }
                              ],
                              "timeManagement": {
                                "productiveTime": "number (percentage)",
                                "idleTime": "number (percentage)",
                                "meetingInterruptions": [
                                  {
                                    "interruptionType": "string",
                                    "interruptionDuration": "number (seconds)",
                                    "interruptionImpact": "string (high/medium/low)"
                                  }
                                ]
                              }
                            },
                            "feedback": {
                              "overall": {
                                "averageFeedbackRating": "number (1-5)",
                                "generalFeedbackSentiment": "string (positive/neutral/negative)"
                              },
                              "participantFeedback": [
                                {
                                  "participantName": "string",
                                  "participantRating": "number (1-5)",
                                  "participantSentiment": "string (positive/neutral/negative)",
                                  "participantComments": "string"
                                }
                              ]
                            }
                          }
                          """;
        return template;
    }
}