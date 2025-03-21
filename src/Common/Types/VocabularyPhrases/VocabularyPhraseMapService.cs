// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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