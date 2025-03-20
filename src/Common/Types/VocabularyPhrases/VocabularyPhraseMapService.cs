// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.VocabularyPhrases
{
    public class VocabularyPhraseMapService(IMapper mapper)
    {
        public VocabularyPhraseDatabaseType? ConvertToDatabase(VocabularyPhraseRequestType requestType)
        {
            var dbModel = mapper.Map<VocabularyPhraseDatabaseType>(requestType);
            return dbModel;
        }

        public VocabularyPhraseRequestType ConvertToRequest(VocabularyPhraseDatabaseType dbType)
        {
            return mapper.Map<VocabularyPhraseRequestType>(dbType);
        }

        public VocabularyPhraseResponseType ConvertToResponse(VocabularyPhraseDatabaseType dbType)
        {
            return mapper.Map<VocabularyPhraseResponseType>(dbType);
        }

        public List<VocabularyPhraseResponseType?> ConvertToResponse(List<VocabularyPhraseDatabaseType> dbType)
        {
            return mapper.Map<List<VocabularyPhraseResponseType?>>(dbType);
        }

        public VocabularyPhraseDatabaseType? MergeToDatabase(VocabularyPhraseRequestType requestType, VocabularyPhraseDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public VocabularyPhraseDatabaseType? Merge(VocabularyPhraseDatabaseType requestType, VocabularyPhraseDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public VocabularyPhraseRequestType MergeToResponse(VocabularyPhraseDatabaseType dbType, VocabularyPhraseRequestType requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}