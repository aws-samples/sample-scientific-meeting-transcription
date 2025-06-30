// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.Types;

public class DateTimeToDateTimeUtc : ValueConverter<DateTime, DateTime>
{
    public DateTimeToDateTimeUtc() : base(c => DateTime.SpecifyKind(c, DateTimeKind.Utc), c => c)
    {
    }
}