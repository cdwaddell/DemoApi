using System.Collections.Generic;

namespace DemoApi.Data.Entities
{
    public class UserProfile
    {
        public UserProfile()
        {
            OwnedBlogs = new List<Blog>();
            SentMessages = new List<Message>();
            Publications = new List<Publication>();
        }
        
        public string Sub { get; set; }
        public string DisplayName { get; set; }
        public string ImageUrl { get; set; }

        public virtual ICollection<Blog> OwnedBlogs { get; set; }
        public virtual ICollection<Message> SentMessages { get; set; }
        public virtual ICollection<Publication> Publications { get; set; }
    }
}