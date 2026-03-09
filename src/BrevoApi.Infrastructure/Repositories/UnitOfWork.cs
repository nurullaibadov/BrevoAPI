using BrevoApi.Application.Interfaces.Repositories;
using BrevoApi.Domain.Entities;
using BrevoApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BrevoApi.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public IGenericRepository<Contact> Contacts { get; }
    public IGenericRepository<EmailList> EmailLists { get; }
    public IGenericRepository<ContactListMapping> ContactListMappings { get; }
    public IGenericRepository<EmailTemplate> EmailTemplates { get; }
    public IGenericRepository<Campaign> Campaigns { get; }
    public IGenericRepository<EmailLog> EmailLogs { get; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Contacts = new GenericRepository<Contact>(context);
        EmailLists = new GenericRepository<EmailList>(context);
        ContactListMappings = new GenericRepository<ContactListMapping>(context);
        EmailTemplates = new GenericRepository<EmailTemplate>(context);
        Campaigns = new GenericRepository<Campaign>(context);
        EmailLogs = new GenericRepository<EmailLog>(context);
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null) { await _transaction.CommitAsync(); await _transaction.DisposeAsync(); }
    }
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null) { await _transaction.RollbackAsync(); await _transaction.DisposeAsync(); }
    }
    public void Dispose() => _context.Dispose();
}
