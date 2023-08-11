using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StaticFileSecureCall.Models;
using Microsoft.Extensions.Logging;


namespace StaticFileSecureCall.DataManagement
{
    //Object layer Management.
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
        }
        public void AddCascadingObject(object RootEntity)
        {
            ChangeTracker.TrackGraph(
                RootEntity,
                node => node.Entry.State = !node.Entry.IsKeySet ? EntityState.Added : EntityState.Unchanged
                );
        }

        ///dbsets
        public DbSet<FileRepository> Events { get; set; }
        //public DbSet<Participant> Participants { get; set; }
    }
}
