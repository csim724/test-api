using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlbumBank.Models;
using AlbumBank.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;

namespace AlbumBank.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlbumController : ControllerBase
    {
        private readonly AlbumBankContext _context;
        private IConfiguration _configuration;


        public AlbumController(AlbumBankContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/Album
        [HttpGet]
        public IEnumerable<AlbumItem> GetAlbumItem()
        {
            return _context.AlbumItem;
        }

        // GET: api/Album/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlbumItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var albumItem = await _context.AlbumItem.FindAsync(id);

            if (albumItem == null)
            {
                return NotFound();
            }

            return Ok(albumItem);
        }

        // PUT: api/Album/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAlbumItem([FromRoute] int id, [FromBody] AlbumItem albumItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != albumItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(albumItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AlbumItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Album
        [HttpPost]
        public async Task<IActionResult> PostAlbumItem([FromBody] AlbumItem albumItem)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.AlbumItem.Add(albumItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAlbumItem", new { id = albumItem.Id }, albumItem);
        }

        // DELETE: api/Album/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlbumItem([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var albumItem = await _context.AlbumItem.FindAsync(id);
            if (albumItem == null)
            {
                return NotFound();
            }

            _context.AlbumItem.Remove(albumItem);
            await _context.SaveChangesAsync();

            return Ok(albumItem);
        }

        private bool AlbumItemExists(int id)
        {
            return _context.AlbumItem.Any(e => e.Id == id);
        }

        // GET: api/Meme/Tags
        [Route("AlbumArtist")]
        [HttpGet]
        public async Task<List<string>> GetTags()
        {
            var memes = (from m in _context.AlbumItem
                         select m.AlbumArtist).Distinct();

            var returned = await memes.ToListAsync();

            return returned;
        }
        [HttpPost, Route("upload")]
        public async Task<IActionResult> UploadFile([FromForm]AlbumImageItem album)
        {
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return BadRequest($"Expected a multipart request, but got {Request.ContentType}");
            }
            try
            {
                using (var stream = album.Image.OpenReadStream())
                {
                    var cloudBlock = await UploadToBlob(album.Image.FileName, null, stream);
                    //// Retrieve the filename of the file you have uploaded
                    //var filename = provider.FileData.FirstOrDefault()?.LocalFileName;
                    if (string.IsNullOrEmpty(cloudBlock.StorageUri.ToString()))
                    {
                        return BadRequest("An error has occured while uploading your file. Please try again.");
                    }

                    AlbumItem albumItem = new AlbumItem();
                    albumItem.AlbumTitle = album.AlbumTitle;
                    albumItem.AlbumArtist = album.AlbumArtist;

                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    albumItem.ImageUrl = cloudBlock.SnapshotQualifiedUri.AbsoluteUri;
                    _context.AlbumItem.Add(albumItem);
                    await _context.SaveChangesAsync();

                    return Ok($"File: {album.AlbumTitle} has successfully uploaded");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error has occured. Details: {ex.Message}");
            }


        }

        private async Task<CloudBlockBlob> UploadToBlob(string filename, byte[] imageBuffer = null, System.IO.Stream stream = null)
        {

            var accountName = _configuration["AzureBlob:name"];
            var accountKey = _configuration["AzureBlob:key"]; ;
            var storageAccount = new CloudStorageAccount(new StorageCredentials(accountName, accountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imagesContainer = blobClient.GetContainerReference("images");

            string storageConnectionString = _configuration["AzureBlob:connectionString"];

            // Check whether the connection string can be parsed.
            if (CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                try
                {
                    // Generate a new filename for every new blob
                    var fileName = Guid.NewGuid().ToString();
                    fileName += GetFileExtention(filename);

                    // Get a reference to the blob address, then upload the file to the blob.
                    CloudBlockBlob cloudBlockBlob = imagesContainer.GetBlockBlobReference(fileName);

                    if (stream != null)
                    {
                        await cloudBlockBlob.UploadFromStreamAsync(stream);
                    }
                    else
                    {
                        return new CloudBlockBlob(new Uri(""));
                    }

                    return cloudBlockBlob;
                }
                catch (StorageException ex)
                {
                    return new CloudBlockBlob(new Uri(""));
                }
            }
            else
            {
                return new CloudBlockBlob(new Uri(""));
            }

        }

        private string GetFileExtention(string fileName)
        {
            if (!fileName.Contains("."))
                return ""; //no extension
            else
            {
                var extentionList = fileName.Split('.');
                return "." + extentionList.Last(); //assumes last item is the extension 
            }
        }
    }
}