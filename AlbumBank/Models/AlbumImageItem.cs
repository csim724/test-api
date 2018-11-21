using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumBank.Models
{
    public class AlbumImageItem
    {
        public string AlbumTitle { get; set; }
        public string AlbumArtist { get; set; }
        public IFormFile Image { get; set; }
    }
}