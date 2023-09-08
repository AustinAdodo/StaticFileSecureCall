using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StaticFileSecureCall.DataManagement
{
    /// <summary>
    /// ***********Object layer Management.
    /// </summary>
    public class AppDbContext : IdentityDbContext
    {
        //Private readonly IUserResolver
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            //FileRepositories = Set<FileRepository>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<FileRepository>()
                .Property(e => e.InternalId)
                .HasMaxLength(40)
                .IsRequired();
        }
        //public void Configure(EntityTypeBuilder<FileRepository> builder) { }
        public DbSet<FileRepository> FileRepositories { get; set; }
    }
}
