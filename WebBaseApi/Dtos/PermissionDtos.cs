using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebBaseApi.Dtos
{
    public class PermissionOutput
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Action { get; set; }
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public string Property { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string Status { get; set; }
    }

    public class PermissionQueryInput : IPageAndSortInputDto
    {
        public PermissionQueryInput()
        {
            base.SortBy = "Order Asc";
        }
        public string Name { get; set; }
        public string Status { get; set; }

    }

    public class PermissionCreateInput
    {
        [Required(ErrorMessage = "请输入权限编码")]
        public string Code { get; set; }
        public string Action { get; set; }
        [Required(ErrorMessage = "请输入权限名称")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public string Property { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string Status { get; set; }
    }

    public class PermissionUpdateInput
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "请输入权限编码")]
        public string Code { get; set; }
        public string Action { get; set; }
        [Required(ErrorMessage = "请输入权限名称")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Icon { get; set; }
        public string Path { get; set; }
        public string Property { get; set; }
        public string Description { get; set; }
        public int Order { get; set; }
        public string Status { get; set; }
    }

    public class PermissionTreeOutput
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public List<PermissionTreeOutput> Children { get; set; }
    }

    public class PermissionMenuOutput
    {
        public int PermissionId { get; set; }
        public string PermissionCode { get; set; }
        public string PermissionAction { get; set; }
        public string PermissionName { get; set; }
        public int PermissionParent { get; set; }
        public string PermissionIcon { get; set; }
        public string PermissionPath { get; set; }
        public string PermissionProperty { get; set; }
        public string PermissionDescription { get; set; }
        public int PermissionOrder { get; set; }
        public string PermissionStatus { get; set; }

        public List<PermissionMenuOutput> Children { get; set; }
    }

}
