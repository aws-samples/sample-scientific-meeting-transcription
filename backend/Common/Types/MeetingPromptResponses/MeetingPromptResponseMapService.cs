// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


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