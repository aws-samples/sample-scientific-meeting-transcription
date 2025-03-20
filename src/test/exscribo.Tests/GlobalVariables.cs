// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System;
using System.Net.Http;

namespace exscribo.Tests
{
    static class GlobalVariables
    {
        public static HttpClient? HttpClient;

        public static Guid? SessionTeamId;
        public static Guid? PromptSetId;
    }
}