using System.ComponentModel.DataAnnotations;

namespace WebBaseApi.Dtos
{
    public class OrgOutput
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class OrgQueryInput : IPageAndSortInputDto
    {
        public string Name { get; set; }
        public string Status { get; set; }
    }

    public class OrgCreateInput
    {
        [Required(ErrorMessage = "请输入角色编码")]
        public string Code { get; set; }
        [Required(ErrorMessage = "请输入角色名称")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class OrgUpdateInput
    {
        [Required(ErrorMessage = "请输入角色Id")]
        public int Id;
        [Required(ErrorMessage = "请输入角色编码")]
        public string Code { get; set; }
        [Required(ErrorMessage = "请输入角色名称")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }
}
