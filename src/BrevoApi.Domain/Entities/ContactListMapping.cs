using BrevoApi.Domain.Common;

namespace BrevoApi.Domain.Entities;

public class ContactListMapping : BaseEntity
{
    public int ContactId { get; set; }
    public int EmailListId { get; set; }
    public Contact Contact { get; set; } = null!;
    public EmailList EmailList { get; set; } = null!;
}
