// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.MeetingPromptResponses
{
    public class MeetingPromptResponseMapService(IMapper mapper)
    {
        public MeetingPromptResponseDatabaseType? ConvertToDatabase(MeetingPromptResponseRequestType requestType)
        {
            var dbModel = mapper.Map<MeetingPromptResponseDatabaseType>(requestType);
            return dbModel;
        }

        public MeetingPromptResponseRequestType ConvertToRequest(MeetingPromptResponseDatabaseType dbType)
        {
            return mapper.Map<MeetingPromptResponseRequestType>(dbType);
        }

        public MeetingPromptResponseResponseType ConvertToResponse(MeetingPromptResponseDatabaseType dbType)
        {
            return mapper.Map<MeetingPromptResponseResponseType>(dbType);
        }

        public List<MeetingPromptResponseResponseType?> ConvertToResponse(List<MeetingPromptResponseDatabaseType> dbType)
        {
            return mapper.Map<List<MeetingPromptResponseResponseType?>>(dbType);
        }

        public MeetingPromptResponseDatabaseType? MergeToDatabase(MeetingPromptResponseRequestType requestType, MeetingPromptResponseDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public MeetingPromptResponseRequestType MergeToResponse(MeetingPromptResponseDatabaseType dbType, MeetingPromptResponseRequestType requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}