using BrevoApi.Domain.Common;

namespace BrevoApi.Domain.Entities;

public class EmailList : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? BrevoListId { get; set; }
    public int TotalContacts { get; set; } = 0;
    public ICollection<ContactListMapping> ContactListMappings { get; set; } = new List<ContactListMapping>();
    public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
}
