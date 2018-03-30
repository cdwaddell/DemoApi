using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DemoApi.Data;
using DemoApi.Data.Projections;
using DemoApi.Models;
using DemoApi.Models.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using File = DemoApi.Data.Entities.File;

namespace DemoApi.Controllers
{
    [Route("Users/{sub}/Files")]
    public class FileController: Controller
    {
        private readonly DemoAppDbContext _context;

        public FileController(DemoAppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Produces(typeof(IEnumerable<FileViewModel>))]
        public async Task<IActionResult> GetFiles([FromRoute] string sub)
        {
            return Ok(await _context.Files.Where(f => f.OwnerSub == sub).ToFileViewModel().ToListAsync());
        }

        [HttpPost]
        [Produces(typeof(FileViewModel))]
        public async Task<IActionResult> AddFile([FromRoute] string sub, IFormFile model)
        {
            if (sub != User.FindFirst("sub").Value)
                return Forbid();

            var fileEntity = new File
            {
                Name = model.Name,
                OwnerSub = sub
            };

            using (var memoryStream = new MemoryStream())
            {
                await model.CopyToAsync(memoryStream);
                fileEntity.Data = memoryStream.ToArray();
            }

            var entity = await _context.Files.AddAsync(fileEntity);
            await entity.Reference(x => x.OwnerProfile).LoadAsync();

            await _context.SaveChangesAsync();

            return Ok(fileEntity.ToFileViewModel());
        }

        [HttpGet("{fileId}")]
        [Produces(typeof(FileViewModel))]
        public async Task<IActionResult> GetFile([FromRoute] string sub, [FromRoute] int fileId)
        {
            return Ok(await _context.Files.Where(f => f.OwnerSub == sub && f.Id == fileId).ToFileViewModel().SingleOrDefaultAsync());
        }

        [HttpPost("{fileId}")]
        [Produces(typeof(FileViewModel))]
        public async Task<IActionResult> UpdateFile([FromRoute] string sub, [FromRoute] int fileId, FileUpdateViewModel model)
        {
            if (sub != User.FindFirst("sub").Value)
                return Forbid();

            var fileEntity = await _context.Files.Include(x => x.OwnerProfile)
                .Where(f => f.OwnerSub == sub && f.Id == fileId).SingleOrDefaultAsync();

            fileEntity.Name = model.Name;
            fileEntity.MimeType = model.MimeType;

            await _context.SaveChangesAsync();

            return Ok(fileEntity.ToFileViewModel());
        }

        [HttpGet("{fileId}/Content")]
        [ProducesResponseType(typeof(Stream), 200)]
        public async Task<FileResult> GetFileContent([FromRoute] string sub, [FromRoute] int fileId)
        {
            var fileEntity = await _context.Files.Include(x => x.OwnerProfile)
                .Where(f => f.OwnerSub == sub && f.Id == fileId).SingleOrDefaultAsync();

            return File(fileEntity.Data, System.Net.Mime.MediaTypeNames.Application.Octet, fileEntity.Name);
        }
    }
}
