using System.Collections.Generic;
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
        [Required(ErrorMessage = "请输入部门编码")]
        public string Code { get; set; }
        [Required(ErrorMessage = "请输入部门名称")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class OrgUpdateInput
    {
        [Required(ErrorMessage = "请输入部门Id")]
        public int Id;
        [Required(ErrorMessage = "请输入部门编码")]
        public string Code { get; set; }
        [Required(ErrorMessage = "请输入部门名称")]
        public string Name { get; set; }
        public int Parent { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class OrgTreeOutput
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public List<OrgTreeOutput> Children { get; set; }
    }
}
