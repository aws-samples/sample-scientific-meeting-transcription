// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Collections.Generic;
using AutoMapper;
using Common.Types.MeetingPromptResponses;

namespace Common.Types.Meetings
{
    public class MeetingMapService(IMapper mapper)
    {
        public MeetingDatabaseType? ConvertToDatabase(MeetingRequestType? requestType)
        {
            return mapper.Map<MeetingDatabaseType>(requestType);
        }

        public MeetingRequestType? ConvertToRequest(MeetingDatabaseType? dbType)
        {
            return mapper.Map<MeetingRequestType>(dbType);
        }

        public MeetingDatabaseType? MergeToDatabase(MeetingRequestType? requestType, MeetingDatabaseType? dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public MeetingPromptResponseSlimType? ConvertMeetingPromptToResponse(MeetingPromptResponseDatabaseType? dbType)
        {
            return mapper.Map<MeetingPromptResponseSlimType>(dbType);
        }

        public MeetingResponseType? ConvertToResponse(MeetingDatabaseType? dbType)
        {
            return mapper.Map<MeetingResponseType>(dbType);
        }

        public List<MeetingResponseType?> ConvertToResponse(List<MeetingDatabaseType>? dbType)
        {
            return mapper.Map<List<MeetingResponseType?>>(dbType);
        }

        public MeetingRequestType? MergeToResponse(MeetingDatabaseType? dbType, MeetingRequestType? requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}