using Microsoft.AspNetCore.Mvc;
using scaffold.Model;
using System.Linq;

namespace scaffold.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        /// <summary>
        /// 开始生成代码
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost("{projectName}")]
        public (bool, string) StartCode(string projectName, [FromBody] (string[] tables, string[] types) info)
        {
            if (string.IsNullOrWhiteSpace(projectName)
            || projectName == "undefined"
            || info.tables == null
            || info.types == null
            || info.tables.Length < 1
            || info.types.Length < 1)
            {
                return (false, "参数不全");
            }

            var code = new CodeModel
            {
                ProjectName = projectName,
                CheckedTables = info.tables.ToList(),
            };

            if (info.types.Contains("Model"))
            {
                code.SaveModel();
            }
            if (info.types.Contains("Database"))
            {
                code.SaveDatabase();
            }
            if (info.types.Contains("Service"))
            {
                code.SaveService();
            }

            return (true, "");
        }
    }
}
