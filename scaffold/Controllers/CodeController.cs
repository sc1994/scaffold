using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace scaffold.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        // GET: api/Code
        [HttpGet]
        public IEnumerable<string> GetCode()
        {
            return new string[]
            {
                Environment.CurrentDirectory,
                Directory.GetCurrentDirectory(),
                Path.GetDirectoryName(typeof(Program).Assembly.Location),
                AppContext.BaseDirectory
            };
        }
    }
}
