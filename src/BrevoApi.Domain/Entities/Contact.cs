using BrevoApi.Domain.Common;
using BrevoApi.Domain.Enums;

namespace BrevoApi.Domain.Entities;

public class Contact : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public ContactStatus Status { get; set; } = ContactStatus.Active;
    public string? BrevoContactId { get; set; }
    public string? AttributesJson { get; set; }
    public ICollection<ContactListMapping> ContactListMappings { get; set; } = new List<ContactListMapping>();
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();
}
