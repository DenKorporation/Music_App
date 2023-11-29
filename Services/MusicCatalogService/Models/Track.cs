using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicCatalogService.Models;

public class Track
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public int ArtistId { get; set; }
    
    public DateOnly ReleaseDate { get; set; }
    
    public float Duration { get; set; }

    public int GenreId { get; set; }

    public Genre? Genre { get; set; }
    public Artist? Artist { get; set; }
    public List<Playlist> Playlists { get; set; } = new();
}