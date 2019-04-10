using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace scaffold.Model
{
    public class TableModel
    {
        /// <summary>
        /// 数据库
        /// </summary>
        [JsonIgnore]
        public DatabaseModel Database { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// DatabaseName
        /// </summary>
        public string DatabaseName { get; set; }

        public IEnumerable<TableModel> GetTables()
        {
            if (Database == null)
            {
                throw new Exception("库信息不能为空");
            }

            var sql = "SELECT TABLE_NAME `Name`,TABLE_COMMENT `Comment` FROM information_schema.TABLES WHERE TABLE_SCHEMA=@name;";
            using (var conn = new MySqlConnection(Database.ConnString()))
            {
                var flag = conn.Query<TableModel>(sql, new
                {
                    name = Database.Database
                });
                foreach (var item in flag)
                {
                    item.Database = Database;
                    item.DatabaseName = Database.Name;
                }
                return flag;
            }
        }
    }
}
