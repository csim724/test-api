using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlbumBank.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new AlbumBankContext(
                serviceProvider.GetRequiredService<DbContextOptions<AlbumBankContext>>()))
            {
                // Look for any movies.
                if (context.AlbumItem.Count() > 0)
                {
                    return;   // DB has been seeded
                }

                context.AlbumItem.AddRange(
                    new AlbumItem
                    {
                        AlbumTitle = "Kid A",
                        ImageUrl = "https://lastfm-img2.akamaized.net/i/u/770x0/248cb85037351002251836e5f2f0d76b.jpg#248cb85037351002251836e5f2f0d76b",
                        AlbumArtist = "Radiohead",
                        Length = "49:55",
                        Tracks = 10,
                    }


                );
                context.SaveChanges();
            }
        }
    }
}
