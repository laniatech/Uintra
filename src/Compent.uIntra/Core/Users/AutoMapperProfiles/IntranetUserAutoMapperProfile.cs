using AutoMapper;
using Uintra.Core.User;
using Uintra.Users;

namespace Compent.Uintra.Core.Users
{
    public class IntranetUserAutoMapperProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<IntranetUser, ProfileViewModel>()
                .ForMember(dst => dst.EditingUser, o => o.MapFrom(user => user));



            Mapper.CreateMap<IntranetUser, ProfileEditModel>()
                .ForMember(dst => dst.MediaRootId, o => o.Ignore())
                .ForMember(dst => dst.NewMedia, o => o.Ignore())
                .ForMember(dst => dst.MemberNotifierSettings, o => o.Ignore());

            Mapper.CreateMap<ProfileEditModel, ExtendedProfileEditModel>()
                .ForMember(dst => dst.TagIdsData, o => o.MapFrom(i => string.Empty));

            Mapper.CreateMap<IntranetUser, IntranetUserDTO>()
                .ForMember(dst => dst.DeleteMedia, o => o.Ignore())
                .ForMember(dst => dst.NewMedia, o => o.Ignore());

            base.Configure();
        }
    }
}