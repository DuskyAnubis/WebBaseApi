using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using WebBaseApi.Data;
using WebBaseApi.Dtos;
using WebBaseApi.Filters;
using WebBaseApi.Models;

namespace WebBaseApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/Permissions")]
    //[Authorize]
    public class PermissionController : Controller
    {
        private readonly ApiContext dbContext;
        private readonly IMapper mapper;

        public PermissionController(ApiContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        #region 基本操作
        /// <summary>
        /// 获得权限列表
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<PermissionOutput>), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetPermissions(PermissionQueryInput input)
        {
            int pageIndex = input.Page - 1;
            int Per_Page = input.Per_Page;
            string sortBy = input.SortBy;

            IQueryable<Permission> query = dbContext.Permissions.AsQueryable<Permission>();

            query = query.Where(q => string.IsNullOrEmpty(input.Name) || q.Name.Contains(input.Name));
            query = query.Where(q => string.IsNullOrEmpty(input.Status) || q.Status.Contains(input.Status));
            query = query.OrderBy(sortBy);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / Per_Page);

            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages
            };
            HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            List<Permission> permissions = await query.Skip(pageIndex * Per_Page).Take(Per_Page).ToListAsync();
            List<PermissionOutput> list = mapper.Map<List<PermissionOutput>>(permissions);

            return Ok(list);
        }

        /// <summary>
        /// 获得权限列表
        /// </summary>
        /// <param name="permId"></param>
        /// <param name="input"></param>

        /// <returns></returns>
        [HttpGet("{permId}/Permissions")]
        [ProducesResponseType(typeof(List<PermissionOutput>), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetPermissions([FromRoute]int permId, PermissionQueryInput input)
        {
            int pageIndex = input.Page - 1;
            int Per_Page = input.Per_Page;
            string sortBy = input.SortBy;

            IQueryable<Permission> query = dbContext.Permissions.AsQueryable<Permission>();

            query = query.Where(q => q.Parent == permId);
            query = query.Where(q => string.IsNullOrEmpty(input.Name) || q.Name.Contains(input.Name));
            query = query.Where(q => string.IsNullOrEmpty(input.Status) || q.Status.Contains(input.Status));
            query = query.OrderBy(sortBy);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / Per_Page);

            var paginationHeader = new
            {
                TotalCount = totalCount,
                TotalPages = totalPages
            };
            HttpContext.Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));

            List<Permission> permissions = await query.Skip(pageIndex * Per_Page).Take(Per_Page).ToListAsync();
            List<PermissionOutput> list = mapper.Map<List<PermissionOutput>>(permissions);

            return Ok(list);
        }

        /// <summary>
        /// 获得权限信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetPermission")]
        [ProducesResponseType(typeof(PermissionOutput), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetPermission([FromRoute]int id)
        {
            var permission = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Id == id);

            if (permission == null)
            {
                return NotFound(Json(new { Error = "该权限不存在" }));
            }

            PermissionOutput permissionOutput = mapper.Map<PermissionOutput>(permission);

            return Ok(permissionOutput);
        }

        /// <summary>
        /// 创建权限信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(typeof(Permission), 201)]
        [ProducesResponseType(typeof(ValidationError), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> CreatePermission([FromBody]PermissionCreateInput input)
        {
            Permission permission = mapper.Map<Permission>(input);

            dbContext.Permissions.Add(permission);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetPermission", new { id = permission.Id }, mapper.Map<PermissionOutput>(permission));
        }

        /// <summary>
        /// 修改权限信息
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
        public async Task<IActionResult> UpdatePermission([FromRoute]int id, [FromBody]PermissionUpdateInput input)
        {
            if (input.Id != id)
            {
                return BadRequest(Json(new { Error = "请求参数错误" }));
            }

            var permission = await dbContext.Permissions.FirstOrDefaultAsync(p => p.Id == id);
            if (permission == null)
            {
                return NotFound(Json(new { Error = "该权限不存在" }));
            }

            dbContext.Entry(permission).CurrentValues.SetValues(input);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();

        }

        /// <summary>
        /// 更新权限信息
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patchDoc"></param>
        /// <returns></returns>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(PermissionOutput), 201)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(ValidationError), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> PatchPermission([FromRoute]int id, [FromBody]JsonPatchDocument<PermissionUpdateInput> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest(Json(new { Error = "请求参数错误" }));
            }

            var permission = await dbContext.Permissions.FirstOrDefaultAsync(o => o.Id == id);
            if (permission == null)
            {
                return NotFound(Json(new { Error = "该权限不存在" }));
            }

            var input = mapper.Map<PermissionUpdateInput>(permission);
            patchDoc.ApplyTo(input);

            TryValidateModel(input);
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            dbContext.Entry(permission).CurrentValues.SetValues(input);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetPermission", new { id = permission.Id }, mapper.Map<PermissionOutput>(permission));
        }

        /// <summary>
        /// 删除权限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> DeletePermission([FromRoute]int id)
        {
            var permission = await dbContext.Permissions.FirstOrDefaultAsync(o => o.Id == id);
            if (permission == null)
            {
                return NotFound(Json(new { Error = "该权限不存在" }));
            }

            int childCount = dbContext.Permissions.Count(p => p.Parent == id);
            int rolePermissionCount = dbContext.RolePermissions.Count(r => r.PermissionId == id);
            if (childCount != 0 || rolePermissionCount != 0)
            {
                return BadRequest(Json(new { Error = "该权限存在引用，不可删除" }));
            }

            dbContext.Permissions.Remove(permission);
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
        public async Task<IActionResult> BatchDelete([FromBody]int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                var permission = await dbContext.Permissions.FirstOrDefaultAsync(o => o.Id == ids[i]);
                int childCount = dbContext.Permissions.Count(p => p.Parent == ids[i]);
                int rolePermissionCount = dbContext.RolePermissions.Count(r => r.RoleId == ids[i]);
                if (permission != null && childCount == 0 && rolePermissionCount == 0)
                {
                    dbContext.Permissions.Remove(permission);
                }
            }
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }
        #endregion

        #region 角色权限操作
        /// <summary>
        /// 角色-权限列表
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet("~/api/v1/Role/{roleId}/Permissions")]
        [ProducesResponseType(typeof(int[]), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IEnumerable<int>> GetRolePermissions([FromRoute]int roleId)
        {
            var list = await dbContext.RolePermissions.Where(r => r.RoleId == roleId).ToListAsync();
            List<int> permissions = new List<int>();
            list.ForEach(perms => permissions.Add(perms.PermissionId));

            return permissions;
        }
        #endregion
    }
}