using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace WebBaseApi.Models
{
    public class Organazition
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public ICollection<User> Users { get; set; }

    }
}
