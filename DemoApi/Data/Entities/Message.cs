using System.Collections.Generic;

namespace DemoApi.Data
{
    public class Message : AuditableEntity
    {
        public Message()
        {
            ChildMessages = new List<Message>();
        }
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int? PublicationId { get; set; }
        public int? ParentMessageId { get; set; }
        public string SenderSub { get; set; }

        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Message> ChildMessages { get; set; }
        public virtual Blog Blog { get; set; }
        public virtual Publication Publication { get; set; }
        public virtual UserProfile SenderProfile { get; set; }
        public virtual Message ParentMessage { get; set; }
    }
}