using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using scaffold.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace scaffold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // GET: api/Test
        [HttpGet]
        public IEnumerable<string> TestSaveApi()
        {
            var code = new CodeModel
            {
                ProjectName = "项目1",
                CheckedTables = new List<string>
                {
                    "TCDAILYSURPRISE_TEST.AIBSuspectSubject",
                    "TCDAILYSURPRISE_TEST.AIBGameInviteRecord"
                }
            };

            //code.SaveModel();
            //code.SaveDatabase();
            //code.SaveService();
            code.SaveApi();
            return new string[] { "value1", "value2" };
        }
    }
}
