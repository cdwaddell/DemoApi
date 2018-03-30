using System;
using System.Linq;
using System.Linq.Expressions;
using DemoApi.Controllers;

namespace DemoApi.Data.Projections
{
    public static class BlogProjections
    {
        private static readonly Expression<Func<Blog, BlogViewModel>> Expression = b => new BlogViewModel
        {
            Description = b.Description,
            Name = b.DisplayName,
            RouteValues = new { blogId = b.Id },
            Owner = new UserViewModel
            {
                RouteValues = new { sub = b.OwnerSub },
                Name = b.OwnerProfile.DisplayName
            },
            Pages = new PagesSummaryViewModel
            {
                RouteValues = new { blogId = b.Id },
                Count = b.Publications.OfType<Page>().Count()
            },
            Posts = new PostsSummaryViewModel
            {
                RouteValues = new { blogId = b.Id },
                Count = b.Publications.OfType<Post>().Count()
            }
        };

        private static readonly Func<Blog, BlogViewModel> Func = Expression.Compile();

        public static IQueryable<BlogViewModel> ToBlogViewModel(this IQueryable<Blog> blogs)
        {
            return blogs.Select(Expression);
        }

        public static BlogViewModel ToBlogViewModel(this Blog blog)
        {
            return Func.Invoke(blog);
        }
    }
}
