using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace WebBaseApi.Dtos
{
    public class IPageAndSortInputDto
    {
        /// <summary>
        /// 默认为0，查询第一页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 默认每页10条，最大每页500条
        /// </summary>
        public int PageSize { get; set; } 
        /// <summary>
        /// 默认为Id Asc,多个排序字段用逗号分隔
        /// </summary>
        public string SortBy { get; set; }
    }
}
