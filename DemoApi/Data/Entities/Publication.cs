using System.Collections.Generic;

namespace DemoApi.Data
{
    public class Publication : AuditableEntity
    {
        public Publication()
        {
            Messages = new List<Message>();
        }

        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string AuthorSub { get; set; }

        public virtual UserProfile AuthorProfile { get; set; }
        public virtual Blog Blog { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
    }
}