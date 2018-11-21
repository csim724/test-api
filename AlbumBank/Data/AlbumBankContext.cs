using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AlbumBank.Models
{
    public class AlbumBankContext : DbContext
    {
        public AlbumBankContext (DbContextOptions<AlbumBankContext> options)
            : base(options)
        {
        }

        public DbSet<AlbumBank.Models.AlbumItem> AlbumItem { get; set; }
    }
}
