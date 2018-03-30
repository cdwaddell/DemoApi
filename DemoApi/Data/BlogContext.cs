using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using DemoApi.Data.Entities;
using IdentityModel;
using Microsoft.EntityFrameworkCore;

namespace DemoApi.Data
{
    public class DemoAppDbContext : DbContext
    {
        private readonly ClaimsPrincipal _principal;
        private static readonly Type AuditableType = typeof(AuditableEntity);
        public DemoAppDbContext(DbContextOptions<DemoAppDbContext> options, ClaimsPrincipal principal):base(options)
        {
            _principal = principal;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<File> Files { get; set; }

        public override int SaveChanges()
        {
            AddTimestamps();
            return base.SaveChanges();
        }
 
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default (CancellationToken))
        {
            AddTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => AuditableType.IsAssignableFrom(e.ClrType)))
            {
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasOne(typeof(UserProfile))
                    .WithMany()
                    .HasForeignKey(nameof(AuditableEntity.CreatedBySub))
                    .HasPrincipalKey(nameof(UserProfile.Sub))
                    .OnDelete(DeleteBehavior.Restrict);
                
                modelBuilder
                    .Entity(entityType.ClrType)
                    .HasOne(typeof(UserProfile))
                    .WithMany()
                    .HasForeignKey(nameof(AuditableEntity.LastModifiedBySub))
                    .HasPrincipalKey(nameof(UserProfile.Sub))
                    .OnDelete(DeleteBehavior.Restrict);
            }

            modelBuilder.Entity<File>(t =>
            {
                t.Property(x => x.Name)
                    .HasMaxLength(128);

                t.Property(x => x.MimeType)
                    .HasMaxLength(32);

                t.ToTable("Files");
            });

            modelBuilder.Entity<Blog>(t =>
            {
                t.HasAlternateKey(x => x.Key);

                t.Property(x => x.Key)
                    .HasMaxLength(32);

                t.Property(x => x.DisplayName)
                    .HasMaxLength(128);

                t.HasOne(x => x.OwnerProfile)
                    .WithMany(x => x.OwnedBlogs)
                    .HasForeignKey(x => x.OwnerSub);

                t.ToTable("Blogs");
            });

            modelBuilder.Entity<Message>(t =>
            {
                t.HasQueryFilter(p => !p.IsDeleted);

                t.HasOne(x => x.Blog)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.BlogId);

                t.HasOne(x => x.Publication)
                    .WithMany(x => x.Messages)
                    .HasForeignKey(x => x.PublicationId);

                t.HasOne(x => x.ParentMessage)
                    .WithMany(x => x.ChildMessages)
                    .HasForeignKey(x => x.ParentMessageId);

                t.HasOne(x => x.SenderProfile)
                    .WithMany(x => x.SentMessages)
                    .HasForeignKey(x => x.SenderSub);

                t.Property(x => x.Subject)
                    .HasMaxLength(1024);

                t.ToTable("Messages");
            });

            modelBuilder.Entity<Publication>(t =>
            {
                t.HasOne(x => x.AuthorProfile)
                    .WithMany(x => x.Publications)
                    .HasForeignKey(x => x.AuthorSub);
                
                t.HasOne(x => x.Blog)
                    .WithMany(x => x.Publications)
                    .HasForeignKey(x => x.BlogId);

                t.Property(x => x.Title)
                    .HasMaxLength(1024);

                t.ToTable("Publications");
            });

            modelBuilder.Entity<Page>(t =>
            {
                t.ToTable("Pages");
            });

            modelBuilder.Entity<Post>(t =>
            {
                t.ToTable("Posts");
            });

            modelBuilder.Entity<UserProfile>(t =>
            {
                t.HasKey(x => x.Sub);

                t.Property(x => x.Sub)
                    .HasMaxLength(128);

                t.Property(x => x.DisplayName)
                    .HasMaxLength(64);

                t.Property(x => x.ImageUrl)
                    .HasMaxLength(1024);

                t.ToTable("UserProfiles");
            });

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        private void AddTimestamps()
        {
            var entities = ChangeTracker.Entries()
                .Where(x => 
                    AuditableType.IsInstanceOfType(x.Entity) && 
                    (
                        x.State == EntityState.Added || 
                        x.State == EntityState.Modified
                    )
                );

            var userSub = _principal.FindFirst(c => c.Type == JwtClaimTypes.Subject)?.Value;

            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((AuditableEntity)entity.Entity).CreatedDate = DateTime.UtcNow;
                    ((AuditableEntity)entity.Entity).CreatedBySub = userSub;
                }

                ((AuditableEntity)entity.Entity).LastModifiedDate = DateTime.UtcNow;
                ((AuditableEntity)entity.Entity).LastModifiedBySub = userSub;
            }
        }
    }
}
