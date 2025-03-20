// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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