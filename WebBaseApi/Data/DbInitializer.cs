using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebBaseApi.Models;
using WebBaseApi.Common;

namespace WebBaseApi.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApiContext context)
        {
            context.Database.Migrate();
            if (context.Users.Any())
            {
                return;
            }

            var organazitions = new List<Organazition>
            {
                new Organazition{ Name="后台管理系统",Code="System",Parent=0,Description="组织机构根目录",Status="正常"},
                new Organazition{ Name="信息中心",Code="InfoCenter",Parent=1,Description="信息中心",Status="正常"}
            };
            organazitions.ForEach(organazition => context.Add(organazition));
            context.SaveChanges();

            var roles = new List<Role>
            {
                new Role{ Code="admin", Name="系统管理员",Description="拥有系统最高权限",Status="正常"},
                new Role{ Code="user", Name="普通用户",Description="普通用户权限",Status="正常"}
            };
            roles.ForEach(role => context.Add(role));
            context.SaveChanges();

            var permissions = new List<Permission>
            {
                new Permission{Name="后台管理系统",Code="System",Action="", Parent=0,Path="",Property="目录",Description="系统功能根目录",Icon="",Order=1,Status="正常" },
                new Permission{Name="系统设置",Code="SystemSetting",Action="",Parent=1,Path="",Property="目录",Description="系统设置",Icon="",Order=1,Status="正常" },
                new Permission{Name="基础设置",Code="BaseSetting",Action="",Parent=1,Path="",Property="目录",Description="基础设置",Icon="",Order=2,Status="正常" },
                new Permission{Name="部门管理",Code="OrganazitionManager",Action="Read",Parent=2,Path="",Property="菜单",Description="部门管理",Icon="",Order=1,Status="正常" },
                new Permission{Name="功能管理",Code="PowerManager",Action="Read",Parent=2,Path="",Property="菜单",Description="功能管理",Icon="",Order=2,Status="正常" },
                new Permission{Name="角色管理",Code="RoleManager",Action="Read",Parent=2,Path="",Property="菜单",Description="角色管理",Icon="",Order=3,Status="正常" },
                new Permission{Name="人员管理",Code="UserManager",Action="Read",Parent=2,Path="",Property="菜单",Description="人员管理",Icon="",Order=4,Status="正常" }
            };
            permissions.ForEach(power => context.Add(power));
            context.SaveChanges();

            var rolePermissions = new List<RolePermission>
            {
                new RolePermission{ RoleId=1,PermissionId=2},
                new RolePermission{ RoleId=1,PermissionId=3},
                new RolePermission{ RoleId=1,PermissionId=4},
                new RolePermission{ RoleId=1,PermissionId=5},
                new RolePermission{ RoleId=1,PermissionId=6},
                new RolePermission{ RoleId=1,PermissionId=7}
            };
            rolePermissions.ForEach(rolePermission => context.Add(rolePermission));
            context.SaveChanges();

            var users = new List<User>
            {
                new User{ Name="admin",PassWord=Encrypt.Md5Encrypt("123"),RoleId=1,OrganazitionId=2,Status="正常"},
                new User{ Name="anubis",PassWord=Encrypt.Md5Encrypt("123"),RoleId=2,OrganazitionId=2,Status="正常"}
            };
            users.ForEach(user => context.Add(user));
            context.SaveChanges();
        }
    }
}
