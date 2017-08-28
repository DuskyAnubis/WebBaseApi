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

}
