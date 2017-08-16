using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebBaseApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PassWord { get; set; }
        public int OrganazitionId { get; set; }
        public int RoleId { get; set; }
        public string Status { get; set; }

        public Organazition Organazition { get; set; }
        public Role Role { get; set; }
    }
}
