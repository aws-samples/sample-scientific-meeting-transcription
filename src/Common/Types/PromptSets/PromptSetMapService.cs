// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.PromptSets
{
    public class PromptSetMapService(IMapper mapper)
    {
        public PromptSetDatabaseType? ConvertToDatabase(PromptSetRequestType requestType)
        {
            var dbModel = mapper.Map<PromptSetDatabaseType>(requestType);
            return dbModel;
        }

        public PromptSetRequestType ConvertToRequest(PromptSetDatabaseType dbType)
        {
            return mapper.Map<PromptSetRequestType>(dbType);
        }

        public PromptSetDatabaseType? MergeToDatabase(PromptSetRequestType requestType, PromptSetDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public PromptSetResponseType ConvertToResponse(PromptSetDatabaseType dbType)
        {
            return mapper.Map<PromptSetResponseType>(dbType);
        }

        public List<PromptSetResponseType?> ConvertToResponse(List<PromptSetDatabaseType> dbType)
        {
            return mapper.Map<List<PromptSetResponseType?>>(dbType);
        }


        public PromptSetRequestType MergeToResponse(PromptSetDatabaseType dbType, PromptSetRequestType requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}