using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DemoApi.Data.Entities
{
    public class File
    {
        public int Id { get; set; }
        public byte[] Data { get; set; }
        public string Name { get; set; }
        public string MimeType { get; set; }
        public string OwnerSub { get; set; }
        public UserProfile OwnerProfile { get; set; }
    }
}
