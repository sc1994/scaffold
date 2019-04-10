using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace scaffold.Model
{
    public class DatabaseModel
    {
        private readonly string _path = "D:/Self/scaffold/scaffold" + "/wwwroot/data/databaselist.json";

        /// <summary>
        /// 命名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Host
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Port
        /// </summary>
        public string Port { get; set; }

        /// <summary>
        /// User
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// 链接字符串
        /// </summary>
        public string ConnString()
            => $"Server={Host};Port={Port};Database={Database};User={User};Password={Password};Min Pool Size=1;Max Pool Size=100;CharSet=utf8;SslMode=none;";

        public string ConnString(string name)
        {
            var list = GetDatabases();
            var flag = list.FirstOrDefault(x => x.Name == name);
            if (flag == null)
            {
                throw new Exception("错误的连接名称");
            }

            return flag.ConnString();
        }

        /// <summary>
        /// 保存
        /// </summary>
        public bool Save()
        {
            List<DatabaseModel> list;
            try
            {
                list = JsonConvert.DeserializeObject<List<DatabaseModel>>(File.ReadAllText(_path));
            }
            catch
            {
                list = new List<DatabaseModel>();
            }

            if (list == null)
            {
                list = new List<DatabaseModel>();
            }
            if (list.Any(x => x.Name == Name))
            {
                list.RemoveAll(x => x.Name == Name);
            }

            list.Add(this);
            File.WriteAllText(_path, JsonConvert.SerializeObject(list));
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool Save(IEnumerable<DatabaseModel> list)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(list));
            return true;
        }

        public IEnumerable<DatabaseModel> GetDatabases()
        {
            List<DatabaseModel> list;
            try
            {
                list = JsonConvert.DeserializeObject<List<DatabaseModel>>(File.ReadAllText(_path));
            }
            catch
            {
                list = null;
            }
            return list;
        }
    }
}
