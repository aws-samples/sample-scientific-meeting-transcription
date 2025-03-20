// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using AutoMapper;
using Common.Types.PromptSets;

public class PromptSetMappingProfile : Profile
{
    public PromptSetMappingProfile()
    {
        CreateMap<int?, int>().ConvertUsing((src, dest) => src ?? dest);

        // Map from Request to Database model
        CreateMap<PromptSetRequestType, PromptSetDatabaseType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()).ForAllMembers(opts =>
            {
                opts.AllowNull();
                opts.Condition((src, dest, srcMember) => srcMember != null);
            });

        CreateMap<PromptSetDatabaseType, PromptSetResponseType>()
            .ForAllMembers(opts =>
            {
                opts.AllowNull();
                opts.Condition((src, dest, srcMember) => srcMember != null);
            });
    }
}