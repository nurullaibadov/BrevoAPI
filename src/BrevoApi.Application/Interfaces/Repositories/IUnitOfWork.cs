using BrevoApi.Domain.Entities;

namespace BrevoApi.Application.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IGenericRepository<Contact> Contacts { get; }
    IGenericRepository<EmailList> EmailLists { get; }
    IGenericRepository<ContactListMapping> ContactListMappings { get; }
    IGenericRepository<EmailTemplate> EmailTemplates { get; }
    IGenericRepository<Campaign> Campaigns { get; }
    IGenericRepository<EmailLog> EmailLogs { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
