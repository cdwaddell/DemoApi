using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DemoApi.Controllers;
using DemoApi.Data.Entities;
using DemoApi.Models;
using DemoApi.Models.Summary;
using Microsoft.EntityFrameworkCore;

namespace DemoApi.Data.Projections
{
    public static class PublicationProjections
    {
        public static IQueryable<Publication> IncludeForPublicationViewModel(this IQueryable<Publication> pub)
        {
            return pub.Include(p => p.AuthorProfile).Include(p => p.Messages);
        }

        private static readonly Expression<Func<Publication, PublicationViewModel>> Expression = p => new PublicationViewModel
        {
            Body = p.Body,
            Title = p.Title,
            RouteValues = new { blogId = p.BlogId, publicationId = p.Id },
            Author = new UserViewModel
            {
                RouteValues = new { sub = p.AuthorSub },
                Name = p.AuthorProfile.DisplayName
            },
            Comments = new CommentSummaryViewModel
            {
                RouteValues = new { blogId = p.BlogId, publicationId = p.Id },
                Count = p.Messages.Count
            }
        };

        private static readonly Func<Publication, PublicationViewModel> Func = Expression.Compile();

        public static IQueryable<PublicationViewModel> ToPublicationViewModel(this IQueryable<Publication> posts)
        {
            return posts.Select(Expression);
        }

        public static IEnumerable<PublicationViewModel> ToPublicationViewModel(this IEnumerable<Publication> posts)
        {
            return posts.Select(Func);
        }

        public static PublicationViewModel ToPublicationViewModel(this Publication post)
        {
            return Func.Invoke(post);
        }
    }
}