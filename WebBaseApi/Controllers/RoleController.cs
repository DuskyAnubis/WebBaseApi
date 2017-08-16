using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebBaseApi.Models;
using WebBaseApi.Data;
using WebBaseApi.Dtos;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using WebBaseApi.Filters;
using WebBaseApi.Common;

namespace WebBaseApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/Roles")]
    public class RoleController : Controller
    {
        private readonly ApiContext dbContext;
        private readonly IMapper mapper;

        public RoleController(ApiContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }


        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleOutput>), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IEnumerable<RoleOutput>> GetRoles(RoleQueryInput input)
        {
            int pageIndex = input.PageIndex;
            int pageSize = input.PageSize == 0 ? 10 : input.PageSize;
            pageSize = pageSize > 500 ? 500 : pageSize;
            string sortBy = string.IsNullOrEmpty(input.SortBy) ? "Id" : input.SortBy;

            IQueryable<Role> query = dbContext.Roles.AsQueryable<Role>();

            query = query.Where(q => input.Id == 0 || q.Id == input.Id);
            query = query.Where(q => string.IsNullOrEmpty(input.Code) || q.Code.Contains(input.Code));
            query = query.Where(q => string.IsNullOrEmpty(input.Name) || q.Name.Contains(input.Name));
            query = query.Where(q => string.IsNullOrEmpty(input.Status) || q.Status.Contains(input.Status));
            query = query.OrderBy(sortBy);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            //query = query.Skip(pageIndex * pageSize).Take(pageSize);

            List<Role> roles = await query.ToListAsync();
            List<RoleOutput> list = mapper.Map<List<RoleOutput>>(roles);

            return list;
        }

        /// <summary>
        /// 获取角色信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetRole")]
        [ProducesResponseType(typeof(RoleOutput), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetRole(int id)
        {
            Role role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return NotFound(Json(new { Error = "该角色不存在" }));
            }

            RoleOutput roleOpt = mapper.Map<RoleOutput>(role);

            return new ObjectResult(roleOpt);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(typeof(UserOutput), 201)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateInput input)
        {
            Role role = mapper.Map<Role>(input);

            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetRole", new { id = role.Id }, role);
        }

        /// <summary>
        /// 修改角色信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ValidateModel]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(ValidationError), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> UpdateRole(int id, [FromBody]RoleUpdateInput input)
        {
            if (input.Id != id)
            {
                return BadRequest(Json(new { Error = "请求参数错误" }));
            }

            var role = dbContext.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
            {
                return NotFound(Json(new { Error = "该角色不存在" }));
            }

            role.Code = input.Code;
            role.Name = input.Name;
            role.Description = input.Description;
            role.Status = input.Status;

            dbContext.Roles.Update(role);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = dbContext.Roles.FirstOrDefault(r => r.Id == id);
            if (role == null)
            {
                return NotFound(Json(new { Error = "该角色不存在" }));
            }

            int userCount = dbContext.Users.Count(u => u.RoleId == id);
            int rolePowerCount = dbContext.RolePowers.Count(rp => rp.RoleId == id);
            if (userCount != 0 || rolePowerCount != 0)
            {
                return BadRequest(Json(new { Error = "该角色存在外键引用，不可删除" }));
            }

            dbContext.Roles.Remove(role);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}