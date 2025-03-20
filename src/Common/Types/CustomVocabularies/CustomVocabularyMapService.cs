// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

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