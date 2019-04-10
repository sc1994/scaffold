using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using scaffold.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace scaffold.Controllers
{
    [Route("database")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        /// <summary>
        /// 数据库链接列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IEnumerable<DatabaseModel> GetDatabase()
        {
            return new DatabaseModel().GetDatabases();
        }

        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        [HttpGet("{databaseName}")]
        public bool DeleteDatabase(string databaseName)
        {
            var db = new DatabaseModel();
            var list = db.GetDatabases().ToList();
            if (list.RemoveAll(x => x.Name == databaseName) == 1)
            {
                return db.Save(list);
            }
            return false;
        }

        /// <summary>
        /// 添加数据库
        /// </summary>
        /// <param name="value"></param>
        [HttpPost]
        public (bool done, string msg) PostDatabase([FromBody] DatabaseModel value)
        {
            if (string.IsNullOrWhiteSpace(value.Database)
               || string.IsNullOrWhiteSpace(value.Host)
               || string.IsNullOrWhiteSpace(value.Name)
               || string.IsNullOrWhiteSpace(value.Password)
               || string.IsNullOrWhiteSpace(value.Port)
               || string.IsNullOrWhiteSpace(value.User))
            {
                return (false, "请求参数不全");
            }

            try
            {
                using (var conn = new MySqlConnection(value.ConnString()))
                {
                    conn.Open();
                    if (conn.State == ConnectionState.Open)
                    {
                        if (value.Save())
                        {
                            return (true, "保存成功");
                        }
                    }
                    return (false, "连接失败");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.ToString());
            }
        }
    }
}
