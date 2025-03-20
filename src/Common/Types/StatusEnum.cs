// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using System.ComponentModel;

namespace Common.Types
{
    public enum StatusEnum
    {
        [field: Description("Active")] Active,

        [field: Description("Inactive")] Inactive
    }
}