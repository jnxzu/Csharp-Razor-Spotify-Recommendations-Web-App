using Microsoft.EntityFrameworkCore;

namespace SpotifyR
{
    public class PolecankoDBContext : DbContext
    {
        public PolecankoDBContext (DbContextOptions<PolecankoDBContext> options) : base(options) 
        {}

        public DbSet<UserDB> users { get; set; }
        public DbSet<TrackDB> tracks { get; set; }
        public DbSet<AlbumDB> albums { get; set; }
        public DbSet<ArtistDB> artists { get; set; }
        public DbSet<Rating> ratings { get; set; }
    }
}