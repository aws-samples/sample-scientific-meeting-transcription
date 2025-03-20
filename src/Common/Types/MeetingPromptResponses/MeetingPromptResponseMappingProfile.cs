// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using AutoMapper;

namespace Common.Types.MeetingPromptResponses
{
    public class MeetingPromptResponsesMappingProfile : Profile
    {
        public MeetingPromptResponsesMappingProfile()
        {
            // Map from Request to Database model
            CreateMap<MeetingPromptResponseRequestType, MeetingPromptResponseDatabaseType>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;

            CreateMap<MeetingPromptResponseDatabaseType, MeetingPromptResponseRequestType>()
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;
            CreateMap<MeetingPromptResponseRequestType, MeetingPromptResponseDatabaseType>()
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;
        }
    }
}