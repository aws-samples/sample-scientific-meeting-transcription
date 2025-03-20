// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;
using Common.Types.MeetingDocuments;

namespace Common.Types.MeetingDocuments
{
    public class MeetingDocumentMapService(IMapper mapper)
    {
        public MeetingDocumentDatabaseType? ConvertToDatabase(MeetingDocumentRequestType requestType)
        {
            var dbModel = mapper.Map<MeetingDocumentDatabaseType>(requestType);
            return dbModel;
        }

        public MeetingDocumentRequestType ConvertToRequest(MeetingDocumentDatabaseType dbType)
        {
            return mapper.Map<MeetingDocumentRequestType>(dbType);
        }

        public MeetingDocumentResponseType ConvertToResponse(MeetingDocumentDatabaseType dbType)
        {
            return mapper.Map<MeetingDocumentResponseType>(dbType);
        }

        public List<MeetingDocumentResponseType?> ConvertToResponse(List<MeetingDocumentDatabaseType> dbType)
        {
            return mapper.Map<List<MeetingDocumentResponseType?>>(dbType);
        }

        public MeetingDocumentDatabaseType? MergeToDatabase(MeetingDocumentRequestType requestType, MeetingDocumentDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public MeetingDocumentRequestType MergeToResponse(MeetingDocumentDatabaseType dbType, MeetingDocumentRequestType requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}