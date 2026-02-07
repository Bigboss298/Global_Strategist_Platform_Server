using Microsoft.EntityFrameworkCore;
using Global_Strategist_Platform_Server.Model.Entities;

namespace Global_Strategist_Platform_Server.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Field> Fields { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Reaction> Reactions { get; set; }
    public DbSet<CorporateAccount> CorporateAccounts { get; set; }
    public DbSet<CorporateInvite> CorporateInvites { get; set; }
    public DbSet<CorporatePayment> CorporatePayments { get; set; }

    // Chat entities
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatParticipant> ChatParticipants { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        // Configure relationships
        modelBuilder.Entity<User>()
            .HasOne(u => u.CorporateAccount)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CorporateAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CorporateAccount>()
            .HasMany(c => c.Invites)
            .WithOne(i => i.CorporateAccount)
            .HasForeignKey(i => i.CorporateAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CorporateAccount>()
            .HasMany(c => c.Payments)
            .WithOne(p => p.CorporateAccount)
            .HasForeignKey(p => p.CorporateAccountId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CorporatePayment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.DateCreated = DateTime.UtcNow;
                    entry.Entity.IsDeleted = false;
                    
                    // Handle Comment specific timestamps
                    if (entry.Entity is Comment comment)
                    {
                        comment.CreatedAt = DateTime.UtcNow;
                    }
                    
                    // Handle Reaction specific timestamps
                    if (entry.Entity is Reaction reaction)
                    {
                        reaction.CreatedAt = DateTime.UtcNow;
                    }
                    break;
                case EntityState.Modified:
                    entry.Entity.DateUpdated = DateTime.UtcNow;
                    
                    // Handle Comment specific timestamps
                    if (entry.Entity is Comment commentModified)
                    {
                        commentModified.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
                case EntityState.Deleted:
                    // Soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DateUpdated = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

