// Copyright © Amazon.com and Affiliates: This deliverable is considered Developed Content as defined in the AWS Service Terms

using AutoMapper;

namespace Common.Types.CustomVocabularies
{
    public class CustomVocabularyMappingProfile : Profile
    {
        public CustomVocabularyMappingProfile()
        {
            // Map from Request to Database model
            CreateMap<CustomVocabularyRequestType, CustomVocabularyDatabaseType>()
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
            CreateMap<CustomVocabularyDatabaseType, CustomVocabularyResponseType>()
                .ForAllMembers(opts =>
                {
                    opts.AllowNull();
                    opts.Condition((src, dest, srcMember) => srcMember != null);
                });
            ;
        }
    }
}