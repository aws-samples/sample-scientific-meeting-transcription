// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using AutoMapper;
using Common.Types.MeetingPromptResponses;
using Common.Types.PromptSets;

namespace Common.Types.MeetingDocuments
{
    public class MeetingDocumentMappingProfile : Profile
    {
        public MeetingDocumentMappingProfile()
        {
            // Map from Request to Database model
            CreateMap<MeetingDocumentResponseType, MeetingDocumentDatabaseType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;

            CreateMap<MeetingDocumentDatabaseType, MeetingDocumentResponseType>(MemberList.Destination)
                .ForAllMembers(opts => { opts.Condition((src, dest, srcMember) => srcMember != null); });
            ;
            CreateMap<MeetingDocumentRequestType, MeetingDocumentDatabaseType>()
                .IgnoreAllNonExisting()
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;
        }
    }
}