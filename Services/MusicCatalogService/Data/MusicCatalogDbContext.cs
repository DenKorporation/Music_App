using Microsoft.EntityFrameworkCore;
using MusicCatalogService.Models;

namespace MusicCatalogService.Data;

public class MusicCatalogDbContext:DbContext
{
    public DbSet<Artist> Artists { get; set; } = null!;
    public DbSet<Genre> Genres { get; set; } = null!;
    public DbSet<Playlist> Playlists { get; set; } = null!;
    public DbSet<Track> Tracks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    
    public MusicCatalogDbContext(DbContextOptions<MusicCatalogDbContext> opt) : base(opt)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
    }
}