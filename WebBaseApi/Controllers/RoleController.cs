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
using Microsoft.AspNetCore.JsonPatch;

namespace WebBaseApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/Roles")]
    [Authorize]
    public class RoleController : Controller
    {
        private readonly ApiContext dbContext;
        private readonly IMapper mapper;

        public RoleController(ApiContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        #region 角色基本操作
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
            int pageIndex = input.Page - 1;
            int Per_Page = input.Per_Page;
            string sortBy = input.SortBy;

            IQueryable<Role> query = dbContext.Roles.AsQueryable<Role>();

            query = query.Where(q => string.IsNullOrEmpty(input.Code) || q.Code.Contains(input.Code));
            query = query.Where(q => string.IsNullOrEmpty(input.Name) || q.Name.Contains(input.Name));
            query = query.Where(q => string.IsNullOrEmpty(input.Status) || q.Status.Contains(input.Status));
            query = query.OrderBy(sortBy);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / Per_Page);

            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages
            };

            HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            query = query.Skip(pageIndex * Per_Page).Take(Per_Page);

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
        [ProducesResponseType(typeof(ValidationError), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> CreateRole([FromBody] RoleCreateInput input)
        {
            Role role = mapper.Map<Role>(input);

            dbContext.Roles.Add(role);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetRole", new { id = role.Id }, mapper.Map<RoleOutput>(role));
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

            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);
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

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(void), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(ValidationError), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> PatchRole(int id, [FromBody]JsonPatchDocument patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest(Json(new { Error = "请求参数错误" }));
            }

            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return NotFound(Json(new { Error = "该角色不存在" }));
            }

            var input = mapper.Map<RoleUpdateInput>(role);
            patchDoc.ApplyTo(input);

            TryValidateModel(input);
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            role.Code = input.Code;
            role.Name = input.Name;
            role.Description = input.Description;
            role.Status = input.Status;

            dbContext.Roles.Update(role);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetRole", new { id = role.Id }, mapper.Map<RoleOutput>(role));
        }

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null)
            {
                return NotFound(Json(new { Error = "该角色不存在" }));
            }

            int userCount = await dbContext.Users.CountAsync(u => u.RoleId == id);
            int rolePowerCount = await dbContext.RolePermissions.CountAsync(rp => rp.RoleId == id);
            if (userCount != 0 || rolePowerCount != 0)
            {
                return BadRequest(Json(new { Error = "该角色存在引用，不可删除" }));
            }

            dbContext.Roles.Remove(role);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> BatchDelete([FromBody] int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == ids[i]);
                if (role == null)
                {
                    dbContext.Roles.Remove(role);
                }
            }
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }
        #endregion

        #region 角色-权限操作
        /// <summary>
        /// 角色-权限列表
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("~/api/v1/Role/{roleId}/Permissions")]
        [ProducesResponseType(typeof(int[]), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IEnumerable<int>> GetRolePermissions(int roleId)
        {
            var list = await dbContext.RolePermissions.Where(r => r.RoleId == roleId).ToListAsync();
            List<int> permissions =new List<int>();
            list.ForEach(perms => permissions.Add(perms.PermissionId));

            return permissions;
        }
        #endregion

    }
}