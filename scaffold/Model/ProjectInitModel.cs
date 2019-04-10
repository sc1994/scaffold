using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace scaffold.Model
{
    public class ProjectInitModel
    {
        private readonly string _path = "D:/Self/scaffold/scaffold" + "/wwwroot/data/projectlist.json"; // todo 考虑直接请求http地址

        public string Name { get; set; }

        public string Path { get; set; }

        /// <summary>
        /// GetProjects
        /// </summary>
        public IEnumerable<ProjectInitModel> GetProjects()
        {
            try
            {
                return JsonConvert.DeserializeObject<IEnumerable<ProjectInitModel>>(File.ReadAllText(_path));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        /// <param name="list"></param>
        public void Save(IEnumerable<ProjectInitModel> list)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(list));
        }

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            var list = GetProjects()?.ToList();
            if (list == null)
            {
                list = new List<ProjectInitModel>
                {
                    this
                };
            }
            else
            {
                if (list.Any(x => x.Name == Name))
                {
                    list.RemoveAll(x => x.Name == Name);
                }
                list.Add(this);
            }
            Save(list);
        }
    }
}
