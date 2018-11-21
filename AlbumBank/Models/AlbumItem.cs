using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumBank.Models
{
    public class AlbumItem
    {
        public int Id { get; set; }
        public string AlbumTitle { get; set; }
        public string ImageUrl { get; set; }
        public string AlbumArtist { get; set; }
        public string Length { get; set; }
        public int Tracks { get; set; }
    }
}
