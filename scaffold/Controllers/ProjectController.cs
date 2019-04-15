using Microsoft.AspNetCore.Mvc;
using scaffold.Model;
using System.Collections.Generic;
using System.Linq;

namespace scaffold.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        // GET: api/Project
        [HttpGet]
        public IEnumerable<ProjectModel> GetProjects()
        {
            var a = new ProjectModel().GetProjects().ToList() ?? new List<ProjectModel>();
            return a;
        }

        [HttpPost]
        public (bool, string) SaveProjects([FromBody]ProjectModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name)
                || string.IsNullOrWhiteSpace(model.Path))
            {
                return (false, "参数不全");
            }
            model.Save();
            return (true, "");
        }

        [HttpDelete("{name}")]
        public bool DeleteProjects(string name)
        {
            var list = new ProjectModel().GetProjects()?.ToList();

            var model = list?.FirstOrDefault(x => x.Name == name);
            if (model == null)
            {
                return false;
            }

            list.RemoveAll(x => x.Name == model.Name);
            model.Save(list);

            return true;
        }
    }
}
