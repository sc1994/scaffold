using Microsoft.AspNetCore.Mvc;
using scaffold.Model;
using System.Collections.Generic;
using System.Linq;

namespace scaffold.Controllers
{
    [Route("table")]
    [ApiController]
    public class TableController : ControllerBase
    {
        /// <summary>
        /// 获取表列表
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        [HttpGet("{databaseName}")]
        public IEnumerable<TableModel> GetTable(string databaseName)
        {
            var model = new TableModel
            {
                Database = new DatabaseModel().GetDatabases().FirstOrDefault(x => x.Name == databaseName)
            };
            var flag = model.GetTables();
            return flag;
        }

        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        [HttpPost]
        public IEnumerable<TableModel> Search([FromBody]string[] match)
        {
            var dbs = new DatabaseModel().GetDatabases();

            var list = new List<TableModel>();
            foreach (var databaseModel in dbs)
            {
                list.AddRange(new TableModel
                {
                    Database = new DatabaseModel().GetDatabases().FirstOrDefault(x => x.Name == databaseModel.Name)
                }.GetTables());
            }

            foreach (var c in match)
            {
                if (string.IsNullOrWhiteSpace(c)) continue;
                list = list.Where(x => x.Name.ToLower().Contains(c.ToLower()) 
                                       || x.Comment.ToLower().Contains(c.ToLower())
                                       || x.DatabaseName.ToLower().Contains(c.ToLower())).ToList();
            }

            return list;
        }
    }
}
