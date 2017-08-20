using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBaseApi.Models;

namespace WebBaseApi.Data
{
    public class ApiContext : DbContext
    {
        public ApiContext(DbContextOptions<ApiContext> options) : base(options)
        {

        }

        public DbSet<Organazition> Organazitions { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
    }
}
