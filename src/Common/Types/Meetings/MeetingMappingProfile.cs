// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms 
// Sample code, software libraries, command line tools, proofs of concept, templates, or other related technology are provided as AWS Content or Third-Party Content under the AWS Customer Agreement, or the relevant written agreement between you and AWS (whichever applies). You should not use this AWS Content or Third-Party Content in your production accounts, or on production or other critical data. You are responsible for testing, securing, and optimizing the AWS Content or Third-Party Content, such as sample code, as appropriate for production grade use based on your specific quality control practices and standards. Deploying AWS Content or Third-Party Content may incur AWS charges for creating or using AWS chargeable resources, such as running Amazon EC2 instances or using Amazon S3 storage.


using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Common.Types.MeetingDocuments;
using Common.Types.MeetingPromptResponses;

namespace Common.Types.Meetings
{
    public class MeetingMappingProfile : Profile
    {
        public MeetingMappingProfile()
        {
            // Map from Request to Database model
            CreateMap<MeetingRequestType, MeetingDatabaseType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;

            // Map from Database to Request model
            CreateMap<MeetingRequestType, MeetingDatabaseType>()
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            CreateMap<MeetingDatabaseType, MeetingResponseType>(MemberList.Destination)
                .ForMember(dest => dest.MeetingDocuments, opt => opt.MapFrom((src, dest, destValue, context) =>
                {
                    List<MeetingDocumentResponseType>? mappedChildren = null;
                    if (src.MeetingDocuments != null)
                    {
                        mappedChildren = src.MeetingDocuments.Select(child =>
                        {
                            var destProperties = typeof(MeetingDocumentResponseType).GetProperties()
                                .Select(p => p.Name)
                                .ToList();
                            var result = context.Mapper.Map<MeetingDocumentResponseType>(child);
                            return result;
                        }).ToList();
                    }

                    return mappedChildren;
                }))
                .ForAllMembers(opts => { opts.Condition((src, dest, srcMember) => srcMember != null); });

            CreateMap<MeetingPromptResponseDatabaseType, MeetingPromptResponseSlimType>()
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
        }
    }
}