using System.Collections.Generic;

namespace DemoApi.Data
{
    public class Blog : AuditableEntity
    {
        public Blog()
        {
            Publications = new List<Publication>();
            Messages = new List<Message>();
        }

        public int Id { get; set; }
        public string Key { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }
        public string OwnerSub { get; set; }

        public virtual UserProfile OwnerProfile { get; set; }
        public virtual ICollection<Message> Messages { get; set; }
        public virtual ICollection<Publication> Publications { get; set; }
    }
}