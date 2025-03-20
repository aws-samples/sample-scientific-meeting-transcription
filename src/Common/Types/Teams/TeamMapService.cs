// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.Collections.Generic;
using AutoMapper;

namespace Common.Types.Teams
{
    public class TeamMapService(IMapper mapper)
    {
        public TeamDatabaseType? ConvertToDatabase(TeamRequestType? requestType)
        {
            var dbModel = mapper.Map<TeamDatabaseType>(requestType);
            return dbModel;
        }

        public TeamRequestType ConvertToRequest(TeamDatabaseType dbType)
        {
            return mapper.Map<TeamRequestType>(dbType);
        }

        public TeamDatabaseType? MergeToDatabase(TeamRequestType requestType, TeamDatabaseType dbType)
        {
            return mapper.Map(requestType, dbType);
        }

        public TeamResponseType? ConvertToResponse(TeamDatabaseType? dbType)
        {
            return mapper.Map<TeamResponseType>(dbType);
        }

        public List<TeamResponseType?> ConvertToResponse(List<TeamDatabaseType> dbType)
        {
            return mapper.Map<List<TeamResponseType?>>(dbType);
        }


        public TeamRequestType MergeToResponse(TeamDatabaseType dbType, TeamRequestType requestType)
        {
            return mapper.Map(dbType, requestType);
        }
    }
}