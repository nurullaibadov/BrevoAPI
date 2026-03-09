using BrevoApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BrevoApi.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts { get; set; }
    public DbSet<EmailList> EmailLists { get; set; }
    public DbSet<ContactListMapping> ContactListMappings { get; set; }
    public DbSet<EmailTemplate> EmailTemplates { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().ToTable("Users");
        builder.Entity<AppRole>().ToTable("Roles");
        builder.Entity<AppUserRole>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        builder.Entity<AppUser>()
            .HasMany(u => u.UserRoles).WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId);

        builder.Entity<AppRole>()
            .HasMany(r => r.UserRoles).WithOne(ur => ur.Role)
            .HasForeignKey(ur => ur.RoleId);

        builder.Entity<Contact>()
            .HasIndex(c => c.Email).IsUnique();

        builder.Entity<ContactListMapping>()
            .HasOne(m => m.Contact).WithMany(c => c.ContactListMappings)
            .HasForeignKey(m => m.ContactId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ContactListMapping>()
            .HasOne(m => m.EmailList).WithMany(l => l.ContactListMappings)
            .HasForeignKey(m => m.EmailListId).OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Campaign>()
            .HasOne(c => c.Template).WithMany(t => t.Campaigns)
            .HasForeignKey(c => c.TemplateId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Campaign>()
            .HasOne(c => c.EmailList).WithMany(l => l.Campaigns)
            .HasForeignKey(c => c.EmailListId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EmailLog>()
            .HasOne(e => e.Campaign).WithMany(c => c.EmailLogs)
            .HasForeignKey(e => e.CampaignId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EmailLog>()
            .HasOne(e => e.Contact).WithMany(c => c.EmailLogs)
            .HasForeignKey(e => e.ContactId).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<EmailLog>()
            .HasOne(e => e.User).WithMany(u => u.EmailLogs)
            .HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
