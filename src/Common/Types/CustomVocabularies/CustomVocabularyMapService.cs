// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.CustomVocabularies
{
    public class CustomVocabularyMapService(IMapper mapper)
    {
        public CustomVocabularyDatabaseType? ConvertToDatabase(CustomVocabularyRequestType requestType)
        {
            var dbModel = mapper.Map<CustomVocabularyDatabaseType>(requestType);
            return dbModel;
        }

        public CustomVocabularyRequestType ConvertToRequest(CustomVocabularyDatabaseType dbType)
        {
            return mapper.Map<CustomVocabularyRequestType>(dbType);
        }

        public CustomVocabularyResponseType ConvertToResponse(CustomVocabularyDatabaseType dbType)
        {
            return mapper.Map<CustomVocabularyResponseType>(dbType);
        }

        public List<CustomVocabularyResponseType?> ConvertToResponse(List<CustomVocabularyDatabaseType> dbType)
        {
            return mapper.Map<List<CustomVocabularyResponseType?>>(dbType);
        }

        public CustomVocabularyDatabaseType? MergeToDatabase(CustomVocabularyRequestType requestType, CustomVocabularyDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public CustomVocabularyDatabaseType? Merge(CustomVocabularyDatabaseType requestType, CustomVocabularyDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public CustomVocabularyResponseType MergeToResponse(CustomVocabularyDatabaseType dbType, CustomVocabularyResponseType requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}