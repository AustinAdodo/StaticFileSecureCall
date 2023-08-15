using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StaticFileSecureCall.Models;
using Microsoft.Extensions.Logging;

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

        ///dbsets
        public DbSet<FileRepository> FileRepositories { get; set; }
    }
}
