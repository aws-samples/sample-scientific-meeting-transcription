// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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