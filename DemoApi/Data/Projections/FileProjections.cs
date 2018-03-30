using System;
using System.Linq;
using System.Linq.Expressions;
using DemoApi.Controllers;
using DemoApi.Data.Entities;
using DemoApi.Models;

namespace DemoApi.Data.Projections
{
    public static class FileProjections
    {
        private static readonly Expression<Func<File, FileViewModel>> Expression = f => new FileViewModel
        {
            Name = f.Name,
            MimeType = f.MimeType,
            Content = new FileContentViewModel
            {
                RouteValues = new { sub = f.OwnerSub, fileId = f.Id }
            },
            RouteValues = new { FileId = f.Id },
            Owner = new UserViewModel
            {
                RouteValues = new { sub = f.OwnerSub },
                Name = f.OwnerProfile.DisplayName
            }
        };

        private static readonly Func<File, FileViewModel> Func = Expression.Compile();

        public static IQueryable<FileViewModel> ToFileViewModel(this IQueryable<File> Files)
        {
            return Files.Select(Expression);
        }

        public static FileViewModel ToFileViewModel(this File File)
        {
            return Func.Invoke(File);
        }
    }
}
