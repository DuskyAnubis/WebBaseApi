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
    [Route("api/v1/Users")]
    //[Authorize]
    public class UserController : Controller
    {
        private readonly ApiContext dbContext;
        private readonly IMapper mapper;

        public UserController(ApiContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }


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
            int pageIndex = input.PageIndex;
            int pageSize = input.PageSize == 0 ? 10 : input.PageSize;
            pageSize = pageSize > 500 ? 500 : pageSize;
            string sortBy = string.IsNullOrEmpty(input.SortBy) ? "Id" : input.SortBy;

            IQueryable<User> query = dbContext.Users
                 .Include(q => q.Organazition)
                 .Include(q => q.Role)
                 .AsQueryable<User>();

            query = query.Where(q => input.Id == 0 || q.Id == input.Id);
            query = query.Where(q => string.IsNullOrEmpty(input.Name) || q.Name.Contains(input.Name));
            query = query.Where(q => input.OrganazitionId == 0 || q.OrganazitionId == input.OrganazitionId);
            query = query.Where(q => input.RoleId == 0 || q.RoleId == input.RoleId);
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

            UserOutput userOpt = mapper.Map<UserOutput>(user);

            return new ObjectResult(userOpt);
        }

        /// <summary>
        /// �����û�
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateModel]
        [ProducesResponseType(typeof(UserOutput), 201)]
        [ProducesResponseType(typeof(IDictionary<string, string>), 422)]
        [ProducesResponseType(typeof(void), 500)]
        public async Task<IActionResult> CreateUser([FromBody]UserCreateInput input)
        {
            User user = mapper.Map<User>(input);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return CreatedAtRoute("GetUser", new { id = user.Id }, user);
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

            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound(Json(new { Error = "���û�������" }));
            }

            user.Name = input.Name;
            user.PassWord = Encrypt.Md5Encrypt(input.PassWord);
            user.OrganazitionId = input.OrganazitionId;
            user.RoleId = input.RoleId;
            user.Status = input.Status;

            dbContext.Users.Update(user);
            await dbContext.SaveChangesAsync();

            return new NoContentResult();
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
            var user = dbContext.Users.FirstOrDefault(u => u.Id == id);
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
                var user = dbContext.Users.FirstOrDefault(u => u.Id == ids[i]);
                if (user != null)
                {
                    dbContext.Users.Remove(user);
                }
            }

            await dbContext.SaveChangesAsync();

            return new NoContentResult();
        }
    }
}