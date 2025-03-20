// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.Prompts
{
    public class PromptMapService(IMapper mapper)
    {
        public PromptDatabaseType? ConvertToDatabase(PromptRequestType? requestType)
        {
            var dbModel = mapper.Map<PromptDatabaseType>(requestType);
            return dbModel;
        }

        public PromptRequestType? ConvertToRequest(PromptDatabaseType? dbType)
        {
            return mapper.Map<PromptRequestType>(dbType);
        }

        public PromptDatabaseType? MergeToDatabase(PromptRequestType? requestType, PromptDatabaseType? dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public PromptResponseType ConvertToResponse(PromptDatabaseType? dbType)
        {
            return mapper.Map<PromptResponseType>(dbType);
        }

        public List<PromptResponseType?> ConvertToResponse(List<PromptDatabaseType>? dbType)
        {
            return mapper.Map<List<PromptResponseType?>>(dbType);
        }

        public PromptRequestType? MergeToResponse(PromptDatabaseType? dbType, PromptRequestType? requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}