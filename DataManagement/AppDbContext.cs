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
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure the column to store UUID as NVARCHAR(40)
            modelBuilder.Entity<FileRepository>()
                .Property(e => e.InternalId)
                .HasMaxLength(40)
                .IsRequired();
        }

        //public void Configure(EntityTypeBuilder<FileRepository> builder)
        //{

        //}

        ///dbsets <summary>
        /// GenericDbset
        /// </summary>

        public Microsoft.EntityFrameworkCore.DbSet<FileRepository>? FileRepositories { get; set; }
    }
}
