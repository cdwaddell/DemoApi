using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoApi.Data;
using DemoApi.Data.Entities;
using DemoApi.Data.Projections;
using DemoApi.Models;
using DemoApi.Models.Manage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DemoApi.Controllers
{
    /// <summary>
    /// Endpoints related to the root of a blog
    /// </summary>
    [Route("Blogs")]
    public class BlogAdminController : Controller
    {
        private readonly DemoAppDbContext _context;

        public BlogAdminController(DemoAppDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Add a new blog to the platform
        /// </summary>
        /// <param name="blogDefinition">The details about the blog being added</param>
        /// <returns>The details of the newly created blog</returns>
        [HttpPost]
        [Produces(typeof(BlogViewModel))]
        public async Task<IActionResult> AddBlog([FromBody] BlogCreateViewModel blogDefinition)
        {
            var sub = User.FindFirst("sub").Value;
            var entity = await _context.Blogs.AddAsync(new Blog
            {
                Key = blogDefinition.Key,
                DisplayName = blogDefinition.Name,
                Description = blogDefinition.Description,
                OwnerSub = sub
            });

            await entity.Reference(x => x.OwnerProfile).LoadAsync();

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                entity.State = EntityState.Detached;
                return BadRequest($"{blogDefinition.Key} is already taken");
            }

            var blog = entity.Entity;
            
            return Ok(blog.ToBlogViewModel());
        }

        /// <summary>
        /// Update the details of your blog
        /// </summary>
        /// <param name="blogId">The Id of the blog to updated</param>
        /// <param name="model">The new details of the blog</param>
        /// <returns></returns>
        [HttpPost("{blogId:int}")]
        [Produces(typeof(BlogViewModel))]
        public async Task<IActionResult> UpdateBlog([FromRoute]int blogId, [FromBody] BlogUpdateViewModel model)
        {
            var blog = await CheckBlog(blogId);

            if (blog == null) 
                return Forbid();

            blog.DisplayName = model.Name;
            blog.Description = model.Description;

            await _context.SaveChangesAsync();

            return Ok(blog.ToBlogViewModel());
        }

        /// <summary>
        /// Add a new post to your blog
        /// </summary>
        /// <param name="blogId">The id of the blog to add a post to</param>
        /// <param name="model">The details of the blog post</param>
        /// <returns></returns>
        [HttpPost("{blogId:int}/Posts")]
        [Produces(typeof(PublicationViewModel))]
        public async Task<IActionResult> AddPost([FromRoute]int blogId, [FromBody] PublicationCreateViewModel model)
        {
            var blog = await CheckBlog(blogId);

            if (blog == null) 
                return Forbid();

            var sub = User.FindFirst("sub").Value;
            var entity = await _context.Publications.AddAsync(new Post
            {
                AuthorSub = sub,
                BlogId = blogId,
                Body = model.Body,
                Title = model.Title
            });

            await _context.SaveChangesAsync();

            await entity.Reference(x => x.AuthorProfile).LoadAsync();
            return Ok(entity.Entity.ToPublicationViewModel());
        }
        
        /// <summary>
        /// Add a new page to your blog
        /// </summary>
        /// <param name="blogId">The id of the blog to add a page to</param>
        /// <param name="model">The details of the blog page</param>
        /// <returns></returns>
        [HttpPost("{blogId:int}/Pages")]
        [Produces(typeof(PublicationViewModel))]
        public async Task<IActionResult> AddPage([FromRoute]int blogId, [FromBody] PublicationCreateViewModel model)
        {
            var blog = await CheckBlog(blogId);

            if (blog == null) 
                return Forbid();

            var sub = User.FindFirst("sub").Value;
            var entity = await _context.Publications.AddAsync(new Page
            {
                AuthorSub = sub,
                BlogId = blogId,
                Body = model.Body,
                Title = model.Title
            });

            await _context.SaveChangesAsync();
            
            await entity.Reference(x => x.AuthorProfile).LoadAsync();
            return Ok(entity.Entity.ToPublicationViewModel());
        }

        /// <summary>
        /// Update a blog post or page
        /// </summary>
        /// <param name="blogId">The blog to update the post or page on</param>
        /// <param name="publicationId">The Id of the post or page</param>
        /// <param name="model">The details of the item to update</param>
        /// <returns></returns>
        [HttpPost("{blogId:int}/Publications/{publicationId:int}")]
        [Produces(typeof(IEnumerable<PublicationViewModel>))]
        public async Task<IActionResult> UpdatePublication([FromRoute] int blogId, [FromRoute] int publicationId, [FromBody] PublicationUpdateViewModel model)
        {
            var blog = await CheckBlog(blogId);

            if (blog == null) 
                return Forbid();

            var pub = await _context.Publications
                .Where(p => p.BlogId == blogId && p.Id == publicationId)
                .IncludeForPublicationViewModel()
                .SingleOrDefaultAsync();

            if (pub == null)
                return NotFound();

            pub.Title = model.Title;
            pub.Body = model.Body;

            await _context.SaveChangesAsync();

            return Ok(pub.ToPublicationViewModel());
        }

        /// <summary>
        /// Delete a blog publication comment 
        /// </summary>
        /// <param name="blogId">The id of the blog to delete the comment from</param>
        /// <param name="publicationId">The publication the publication belongs to</param>
        /// <param name="commentId">The Id of the comment to delete</param>
        /// <returns></returns>
        [HttpDelete("{blogId:int}/Publications/{publicationId:int}/Comments/{commentId:int}")]
        [Produces(typeof(CommentViewModel))]
        public async Task<IActionResult> DeletePublicationComment([FromRoute] int blogId, [FromRoute] int publicationId, [FromRoute] int commentId)
        {
            var comment = await _context.Messages
                .SingleOrDefaultAsync(m => m.BlogId == blogId && m.Id == commentId && m.PublicationId == publicationId);
            
            if (comment == null)
                return NotFound();

            if (comment.SenderSub != User.FindFirst("sub").Value)
            {                
                var blog = await CheckBlog(blogId);

                if (blog == null)
                    return Forbid();
            }

            comment.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        private async Task<Blog> CheckBlog(int blogId)
        {
            var sub = User.FindFirst("sub").Value;
            var blog = await _context.Blogs.Where(b => b.Id == blogId).SingleOrDefaultAsync();

            return blog?.OwnerSub == sub ? blog : null;
        }
    }
}