using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.ComponentModel.DataAnnotations;
using WebBaseApi.Data;

namespace WebBaseApi.Filters
{
    /// <summary>
    /// 验证唯一性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class UniqueAttribute : ValidationAttribute
    {
        protected string tableName;
        protected string filedName;
        private static ApiContext dbContext;

        public UniqueAttribute(string tableName, string filedName)
        {
            this.tableName = tableName;
            this.filedName = filedName;
        }

        public static void Initialize(ApiContext context)
        {
            dbContext = context;
        }

        public override Boolean IsValid(Object value)
        {
            bool validResult = false;

            string sql = $"select count(1) from {tableName} where {filedName}='{value}'";
            DbConnection conn = dbContext.Database.GetDbConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            conn.Open();
            int result = Convert.ToInt32(cmd.ExecuteScalar());
            validResult = (result == 0);
            conn.Close();
            return validResult;
        }

    }
}
