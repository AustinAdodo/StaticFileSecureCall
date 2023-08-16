using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StaticFileSecureCall.Models;
using Microsoft.Extensions.Logging;
using System.Xml;

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

        ///dbsets
        public DbSet<FileRepository> FileRepositories { get; set; }
    }
}
