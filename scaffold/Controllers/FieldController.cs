using Microsoft.AspNetCore.Mvc;
using scaffold.Model;
using System.Collections.Generic;
using System.Linq;

namespace scaffold.Controllers
{
    [Route("field")]
    [ApiController]
    public class FieldController : ControllerBase
    {
        /// <summary>
        /// 字段列表
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        [HttpGet("{databaseName}/{tableName}")]
        public IEnumerable<FieldModel> GetField(string databaseName, string tableName)
        {
            var model = new FieldModel
            {
                Table = new TableModel
                {
                    Database = new DatabaseModel().GetDatabases().FirstOrDefault(x => x.Name == databaseName)
                }.GetTables().FirstOrDefault(x => x.Name == tableName)
            };

            var flag = model.GetFields();
            return flag;
        }
    }
}
