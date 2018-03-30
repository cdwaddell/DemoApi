using System;

namespace DemoApi.Data
{
    public abstract class AuditableEntity
    {
        public DateTime? CreatedDate { get; set; }
        public string CreatedBySub { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public string LastModifiedBySub { get; set; }
    }
}