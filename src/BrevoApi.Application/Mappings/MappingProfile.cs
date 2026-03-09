using AutoMapper;
using BrevoApi.Application.DTOs.Campaign;
using BrevoApi.Application.DTOs.Contact;
using BrevoApi.Application.DTOs.Template;
using BrevoApi.Application.DTOs.User;
using BrevoApi.Domain.Entities;

namespace BrevoApi.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AppUser, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.IsEmailConfirmed, o => o.MapFrom(s => s.EmailConfirmed))
            .ForMember(d => d.Roles, o => o.Ignore());
        CreateMap<UpdateUserDto, AppUser>()
            .ForAllMembers(o => o.Condition((src, dest, val) => val != null));

        CreateMap<Contact, ContactDto>()
            .ForMember(d => d.Lists, o => o.MapFrom(s =>
                s.ContactListMappings.Select(m => m.EmailList)));
        CreateMap<CreateContactDto, Contact>()
            .ForMember(d => d.AttributesJson, o => o.Ignore());
        CreateMap<UpdateContactDto, Contact>()
            .ForAllMembers(o => o.Condition((src, dest, val) => val != null));

        CreateMap<EmailList, EmailListDto>();
        CreateMap<CreateEmailListDto, EmailList>();
        CreateMap<UpdateEmailListDto, EmailList>();

        CreateMap<EmailTemplate, TemplateDto>();
        CreateMap<CreateTemplateDto, EmailTemplate>();
        CreateMap<UpdateTemplateDto, EmailTemplate>()
            .ForAllMembers(o => o.Condition((src, dest, val) => val != null));

        CreateMap<Campaign, CampaignDto>()
            .ForMember(d => d.TemplateName, o => o.MapFrom(s => s.Template != null ? s.Template.Name : null))
            .ForMember(d => d.EmailListName, o => o.MapFrom(s => s.EmailList != null ? s.EmailList.Name : null));
        CreateMap<CreateCampaignDto, Campaign>();
        CreateMap<UpdateCampaignDto, Campaign>()
            .ForAllMembers(o => o.Condition((src, dest, val) => val != null));
    }
}
