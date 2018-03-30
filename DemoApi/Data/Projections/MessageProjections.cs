using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DemoApi.Controllers;

namespace DemoApi.Data.Projections
{
    public static class MessageProjections
    {
        private static readonly Expression<Func<Message, CommentViewModel>> Expression = c => new CommentViewModel
        {
            ReplyToCommentId = c.ParentMessageId,
            RouteValues = new { blogId = c.BlogId, publicationId = c.Id, commentId = c.Id },
            Body = c.Body,
            Subject = c.Subject,
            Replies = new ReplySummaryViewModel
            {
                RouteValues = new { blogId = c.BlogId, publicationId = c.Id, commentId = c.Id },
                Count = c.ChildMessages.Count,
            },
            Sender = new UserViewModel
            {
                RouteValues = new { sub = c.SenderSub},
                Name = c.SenderProfile.DisplayName
            }
        };

        private static readonly Func<Message, CommentViewModel> Func = Expression.Compile();

        public static IQueryable<CommentViewModel> ToCommentViewModel(this IQueryable<Message> posts)
        {
            return posts.Select(Expression);
        }

        public static IEnumerable<CommentViewModel> ToCommentViewModel(this IEnumerable<Message> posts)
        {
            return posts.Select(Func);
        }

        public static CommentViewModel ToCommentViewModel(this Message post)
        {
            return Func.Invoke(post);
        }
    }
}