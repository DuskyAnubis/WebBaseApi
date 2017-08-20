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
using System.Linq.Dynamic.Core;
using Newtonsoft.Json;
using WebBaseApi.Filters;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Authorization;

namespace WebBaseApi.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/Users")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApiContext dbContext;
        private readonly IMapper mapper;

        public UserController(ApiContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }

        #region �û���������
        /// <summary>
        /// ��ȡ�û��б�
        /// </summary>
        /// <param name="input">�������</param>
        /// <returns>�û��б�</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserOutput>), 200)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IEnumerable<UserOutput>> GetUsers(UserQueryInput input)
        {
            int pageIndex = input.Page - 1;
            int Per_Page = input.Per_Page;
            string sortBy = input.SortBy;

            IQueryable<User> query = dbContext.Users
                 .Include(q => q.Organazition)
                 .Include(q => q.Role)
                 .AsQueryable<User>();

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

            List<User> users = await query.ToListAsync();
            List<UserOutput> list = mapper.Map<List<UserOutput>>(users);

            return list;
        }

        /// <summary>
        /// ����û���Ϣ
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetUser")]
        [ProducesResponseType(typeof(UserOutput), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> GetUser(int id)
        {
            User user = await dbContext.Users
               .Include(q => q.Organazition)
               .Include(q => q.Role)
               .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(Json(new { Error = "���û�������" }));
            }

            UserOutput output = mapper.Map<UserOutput>(user);

            return new ObjectResult(output);
        }

        /// <summary>
        /// �����û�
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(typeof(UserOutput), 201)]
        [ProducesResponseType(typeof(ValidationError), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> CreateUser([FromBody]UserCreateInput input)
        {
            if (dbContext.Users.Count(u => u.Name == input.Name) > 0)
            {
                return BadRequest(Json(new { Error = "�û����Ѵ���" }));
            }
            if (await dbContext.Organazitions.CountAsync(o => o.Id == input.OrganazitionId) == 0)
            {
                return BadRequest(Json(new { Error = "�����ڸò���" }));
            }
            if (await dbContext.Roles.CountAsync(r => r.Id == input.RoleId) == 0)
            {
                return BadRequest(Json(new { Error = "�����ڸý�ɫ" }));
            }

            var user = mapper.Map<User>(input);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetUser", new { id = user.Id }, mapper.Map<UserOutput>(user));
        }

        /// <summary>
        /// �޸��û���Ϣ
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
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateInput input)
        {
            if (input.Id != id)
            {
                return BadRequest(Json(new { Error = "�����������" }));
            }
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(Json(new { Error = "���û�������" }));
            }
            if (await dbContext.Organazitions.CountAsync(o => o.Id == input.OrganazitionId) == 0)
            {
                return BadRequest(Json(new { Error = "�����ڸò���" }));
            }
            if (await dbContext.Roles.CountAsync(r => r.Id == input.RoleId) == 0)
            {
                return BadRequest(Json(new { Error = "�����ڸý�ɫ" }));
            }

            user.OrganazitionId = input.OrganazitionId;
            user.RoleId = input.RoleId;
            user.Status = input.Status;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }

        /// <summary>
        /// �����û���Ϣ
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
        public async Task<IActionResult> PatchUserAsync(int id, [FromBody] JsonPatchDocument<UserUpdateInput> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest(Json(new { Error = "�����������" }));
            }

            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(Json(new { Error = "���û�������" }));
            }

            var input = mapper.Map<UserUpdateInput>(user);
            patchDoc.ApplyTo(input);

            TryValidateModel(input);
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            if (await dbContext.Organazitions.CountAsync(o => o.Id == input.OrganazitionId) == 0)
            {
                return BadRequest(Json(new { Error = "�����ڸò���" }));
            }
            if (await dbContext.Roles.CountAsync(r => r.Id == input.RoleId) == 0)
            {
                return BadRequest(Json(new { Error = "�����ڸý�ɫ" }));
            }

            user.OrganazitionId = input.OrganazitionId;
            user.RoleId = input.RoleId;
            user.Status = input.Status;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetUser", new { id = user.Id }, mapper.Map<UserOutput>(user));
        }

        /// <summary>
        /// ɾ���û�
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound(Json(new { Error = "���û�������" }));
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }

        /// <summary>
        /// ����ɾ��
        /// </summary>
        /// <param name="ids">ID����</param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(void), 204)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> BatchDelete([FromBody] int[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == ids[i]);
                if (user != null)
                {
                    dbContext.Users.Remove(user);
                }
            }

            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }

    }
    #endregion
}