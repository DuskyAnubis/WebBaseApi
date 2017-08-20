using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using WebBaseApi.Filters;
using WebBaseApi.Data;

namespace WebBaseApi.Dtos
{
    public class UserOutput
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrganazitionId { get; set; }
        public string OrganazitionCode { get; set; }
        public string OrganazitionName { get; set; }
        public int RoleId { get; set; }
        public string RoleCode { get; set; }
        public string RoleName { get; set; }
        public string Status { get; set; }
    }

    public class UserQueryInput : IPageAndSortInputDto
    {
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class UserCreateInput
    {
        [Required(ErrorMessage = "请输入用户名")]
        [StringLength(14, MinimumLength = 4, ErrorMessage = "用户名必须为4到14位")]
        public string Name { get; set; }
        [Required(ErrorMessage = "请输入密码")]
        [MinLength(6, ErrorMessage = "密码长度不能少于6位")]
        public string PassWord { get; set; }
        [RegularExpression("^[1-9]\\d*$", ErrorMessage = "组织机构ID为正整数")]
        public int OrganazitionId { get; set; }
        [RegularExpression("^[1-9]\\d*$", ErrorMessage = "角色ID为正整数")]
        public int RoleId { get; set; }
        public string Status { get; set; }
    }

    public class UserUpdateInput
    {
        [Required(ErrorMessage = "Id不能为空")]
        public int Id { get; set; }
        [RegularExpression("^[1-9]\\d*$", ErrorMessage = "组织机构ID为正整数")]
        public int OrganazitionId { get; set; }
        [RegularExpression("^[1-9]\\d*$", ErrorMessage = "角色ID为正整数")]
        public int RoleId { get; set; }
        public string Status { get; set; }
    }

}
