// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.CustomModels
{
    public class CustomModelMapService(IMapper mapper)
    {
        public CustomModelDatabaseType? ConvertToDatabase(CustomModelRequestType requestType)
        {
            var dbModel = mapper.Map<CustomModelDatabaseType>(requestType);
            return dbModel;
        }

        public CustomModelRequestType ConvertToRequest(CustomModelDatabaseType dbType)
        {
            return mapper.Map<CustomModelRequestType>(dbType);
        }

        public CustomModelResponseType? ConvertToResponse(CustomModelDatabaseType? dbType)
        {
            return mapper.Map<CustomModelResponseType>(dbType);
        }

        public List<CustomModelResponseType?> ConvertToResponse(List<CustomModelDatabaseType> dbType)
        {
            return mapper.Map<List<CustomModelResponseType?>>(dbType);
        }

        public CustomModelDatabaseType? MergeToDatabase(CustomModelRequestType? requestType, CustomModelDatabaseType? dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public CustomModelDatabaseType? Merge(CustomModelDatabaseType? requestType, CustomModelDatabaseType? dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public CustomModelRequestType? MergeToResponse(CustomModelDatabaseType? dbType, CustomModelRequestType? requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}