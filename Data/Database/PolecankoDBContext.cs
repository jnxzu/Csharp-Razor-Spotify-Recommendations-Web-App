using System;
using Microsoft.EntityFrameworkCore;

namespace SpotifyR
{
    public class PolecankoDBContext : DbContext
    {
        public PolecankoDBContext (DbContextOptions<PolecankoDBContext> options) : base(options) 
        {}

        public DbSet<UserDB> users { get; set; }
        public DbSet<ArtistDB> artists { get; set; }
        public DbSet<Rating> ratings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>()
                .HasKey(r => new { r.userId, r.artistId });
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.artist)
                .WithMany(a => a.ratings)
                .HasForeignKey(r => r.artistId);
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.user)
                .WithMany(u => u.ratings)
                .HasForeignKey(r => r.userId);
        }
    }
}