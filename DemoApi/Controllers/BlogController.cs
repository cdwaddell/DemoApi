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
    public class BlogController: Controller
    {
        private readonly DemoAppDbContext _context;

        public BlogController(DemoAppDbContext context)
        {
            _context = context;
        }
        
        /// <summary>
        /// Get a list of blogs hosted here
        /// </summary>
        /// <returns>A list of blogs</returns>
        [HttpGet]
        [Produces(typeof(IEnumerable<BlogViewModel>))]
        public async Task<IActionResult> GetBlogs()
        {
            return Ok(await _context.Blogs.ToBlogViewModel().ToListAsync());
        }
        
        /// <summary>
        /// Get the details of a blog by Id
        /// </summary>
        /// <param name="blogId">The Id of the blog to get</param>
        /// <returns>The details of the blog</returns>
        [HttpGet("{blogId:int}")]
        [Produces(typeof(IEnumerable<BlogViewModel>))]
        public async Task<IActionResult> GetBlog([FromRoute]int blogId)
        {
            var blog = await _context.Blogs
                .Where(b => b.Id == blogId)
                .ToBlogViewModel()
                .SingleOrDefaultAsync();

            if (blog == null)
                return NotFound();

            return Ok(blog);
        }
        
        /// <summary>
        /// Get the posts belonging to a blog
        /// </summary>
        /// <param name="blogId">The Id of the blog</param>
        /// <returns>A list of posts belinging to the blog</returns>
        [HttpGet("{blogId:int}/Posts")]
        [Produces(typeof(IEnumerable<PublicationViewModel>))]
        public async Task<IActionResult> GetPosts([FromRoute]int blogId)
        {
            var blogPosts = await _context.Blogs.Where(b => b.Id == blogId).GroupJoin(
                _context.Posts,
                b => b.Id,
                p => p.BlogId,
                (b, p) => new
                {
                    Blog = b,
                    Posts = p
                }
            ).SingleOrDefaultAsync();

            if (blogPosts?.Blog == null)
                return NotFound();

            return Ok(blogPosts.Posts.ToPublicationViewModel());
        }

        /// <summary>
        /// Get the pages belonging to a blog
        /// </summary>
        /// <param name="blogId">The Id of the blog</param>
        /// <returns>A list of pages belinging to the blog</returns>
        [HttpGet("{blogId:int}/Pages")]
        [Produces(typeof(IEnumerable<PublicationViewModel>))]
        public async Task<IActionResult> GetPages([FromRoute]int blogId)
        {
            var blogPages = await _context.Blogs.Where(b => b.Id == blogId).GroupJoin(
                _context.Pages,
                b => b.Id,
                p => p.BlogId,
                (b, p) => new
                {
                    Blog = b,
                    Pages = p
                }
            ).SingleOrDefaultAsync();

            if (blogPages?.Blog == null)
                return NotFound();

            return Ok(blogPages.Pages.ToPublicationViewModel());
        }

        /// <summary>
        /// Get the details of a page or post
        /// </summary>
        /// <param name="blogId">The id of the blog</param>
        /// <param name="publicationId">The id of the post of page</param>
        /// <returns>The details of the page or post</returns>
        [HttpGet("{blogId:int}/Publications/{publicationId:int}")]
        [Produces(typeof(IEnumerable<PublicationViewModel>))]
        public async Task<IActionResult> GetPublication([FromRoute]int blogId, [FromRoute] int publicationId)
        {
            var blogPosts = await _context.Blogs.Where(b => b.Id == blogId).GroupJoin(
                _context.Posts.Where(p => p.Id == publicationId),
                b => b.Id,
                p => p.BlogId,
                (b, p) => new
                {
                    Blog = b,
                    Posts = p.ToList()
                }
            ).SingleOrDefaultAsync();

            if (blogPosts.Blog == null)
                return NotFound();

            return Ok(blogPosts.Posts.SingleOrDefault());
        }

        /// <summary>
        /// Get a post or a page by Id
        /// </summary>
        /// <param name="blogId">The blog the page or post belongs to</param>
        /// <param name="publicationId">The Id of the page or post</param>
        /// <returns></returns>
        [HttpGet("{blogId:int}/Publications/{publicationId:int}/Comments")]
        [Produces(typeof(IEnumerable<CommentViewModel>))]
        public async Task<IActionResult> GetPublicationComments([FromRoute]int blogId, [FromRoute]int publicationId)
        {
            var messages = await _context.Messages
                .Where(m => m.BlogId == blogId && m.PublicationId == publicationId)
                .ToCommentViewModel()
                .ToListAsync();

            return Ok(messages);
        }

        /// <summary>
        /// Add a comment to a blog page or post
        /// </summary>
        /// <param name="blogId">The Id of the blog that has the page or post</param>
        /// <param name="publicationId">The Id of the page or post</param>
        /// <param name="model">The details of the comment</param>
        /// <returns>The newly created comment</returns>
        [HttpPost("{blogId:int}/Publications/{publicationId:int}/Comments")]
        [Produces(typeof(CommentViewModel))]
        public async Task<IActionResult> AddPublicationComments([FromRoute]int blogId, [FromRoute]int publicationId, [FromBody] CommentCreateViewModel model)
        {
            var publication = await _context.Publications.SingleOrDefaultAsync(p => p.BlogId == blogId && p.Id == publicationId);

            if (publication == null)
                return NotFound();

            var entity = await _context.Messages.AddAsync(new Message
            {
                BlogId = blogId,
                PublicationId = publicationId,
                Body = model.Body,
                Subject = model.Subject
            });

            entity.Reference(m => m.SenderProfile).Load();

            await _context.SaveChangesAsync();

            return Ok(entity.Entity.ToCommentViewModel());
        }

        /// <summary>
        /// Get the details of a comment
        /// </summary>
        /// <param name="blogId">The id of the blog that has the post or page</param>
        /// <param name="publicationId">The Id of the post or page the comment belongs to</param>
        /// <param name="commentId">The Id of the comment</param>
        /// <returns>The details of the comment</returns>
        [HttpGet("{blogId:int}/Publications/{publicationId:int}/Comments/{commentId:int}")]
        [Produces(typeof(CommentViewModel))]
        public async Task<IActionResult> GetPublicationComment([FromRoute]int blogId, [FromRoute]int publicationId, [FromRoute] int commentId)
        {
            var message = await _context.Messages.Where(m =>
                m.BlogId == blogId && m.PublicationId == publicationId && m.Id == commentId)
                .ToCommentViewModel()
                .SingleOrDefaultAsync();

            if (message == null)
                return NotFound();

            return Ok(message);
        }

        /// <summary>
        /// Get the replies to a comment
        /// </summary>
        /// <param name="blogId">The Id of the blog the comment belongs to</param>
        /// <param name="publicationId">The Id of the publication the comment belongs to</param>
        /// <param name="commentId">The id of the comment</param>
        /// <returns>A list of comment replies</returns>
        [HttpGet("{blogId:int}/Publications/{publicationId:int}/Comments/{commentId:int}/Replies")]
        [Produces(typeof(IEnumerable<CommentViewModel>))]
        public async Task<IActionResult> GetPublicationCommentReplies([FromRoute]int blogId, [FromRoute]int publicationId, [FromRoute] int commentId)
        {
            var message = await _context.Messages.Include(m => m.ChildMessages).ThenInclude(cm => cm.SenderProfile).SingleOrDefaultAsync(m =>
                m.BlogId == blogId && m.PublicationId == publicationId && m.Id == commentId);

            if (message == null)
                return NotFound();

            return Ok(message.ChildMessages.ToCommentViewModel());
        }

        /// <summary>
        /// Update a comment
        /// </summary>
        /// <param name="blogId">The Id of the blog the comment belongs to</param>
        /// <param name="publicationId">The Id of hte publication with the comment</param>
        /// <param name="commentId">The Id of the comment</param>
        /// <param name="model">The details of the comment</param>
        /// <returns></returns>
        [HttpPost("{blogId:int}/Publications/{publicationId:int}/Comments/{commentId:int}")]
        [Produces(typeof(CommentViewModel))]
        public async Task<IActionResult> UpdatePublicationComment([FromRoute]int blogId, [FromRoute]int publicationId, [FromRoute] int commentId, [FromBody] CommentUpdateViewModel model)
        {
            var message = await _context.Messages.Include(m => m.ChildMessages).Include(m => m.SenderProfile).SingleOrDefaultAsync(m =>
                m.BlogId == blogId && m.PublicationId == publicationId && m.Id == commentId);

            if (message == null)
                return NotFound();

            if (message.SenderSub == User.FindFirst("sub").Value)
                return Forbid();

            message.Subject = model.Subject;
            message.Body = model.Body;

            await _context.SaveChangesAsync();

            return Ok(message.ToCommentViewModel());
        }
    }
}
