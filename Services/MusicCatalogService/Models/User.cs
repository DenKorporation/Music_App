using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicCatalogService.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string Login { get; set; } = "";
    
    public int ExternalId { get; set; }

    public List<Playlist> Playlists { get; set; } = new();
}