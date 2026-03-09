using BrevoApi.Domain.Enums;

namespace BrevoApi.Application.DTOs.Contact;

public class ContactDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public ContactStatus Status { get; set; }
    public string? BrevoContactId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<EmailListDto> Lists { get; set; } = new();
}

public class CreateContactDto
{
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public List<int>? ListIds { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

public class UpdateContactDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public ContactStatus? Status { get; set; }
    public Dictionary<string, string>? Attributes { get; set; }
}

public class EmailListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? BrevoListId { get; set; }
    public int TotalContacts { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateEmailListDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateEmailListDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
