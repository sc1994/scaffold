using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace scaffold.Model
{
    public class ProjectModel
    {
        private readonly string _path = Environment.CurrentDirectory + "/wwwroot/data/projectlist.json";

        public string Name { get; set; }

        public string Path { get; set; }

        public List<KeyValue> ItemDictionary { get; set; }

        /// <summary>
        /// GetProjects
        /// </summary>
        public IEnumerable<ProjectModel> GetProjects()
        {
            try
            {
                return JsonConvert.DeserializeObject<IEnumerable<ProjectModel>>(File.ReadAllText(_path));
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
        public void Save(IEnumerable<ProjectModel> list)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(list));
        }

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            var that = this;
            that.ItemDictionary.ForEach(x =>
            {
                if (string.IsNullOrWhiteSpace(x.Value))
                {
                    x.Value = $"{Name}.{x.Key}";
                }
            });

            var list = GetProjects()?.ToList();
            if (list == null)
            {
                list = new List<ProjectModel>
                {
                    that
                };
            }
            else
            {
                if (list.Any(x => x.Name == Name))
                {
                    list.RemoveAll(x => x.Name == Name);
                }
                list.Add(that);
            }
            Save(list);
        }

        public class KeyValue
        {
            public string Key { get; set; }

            public string Value { get; set; }
        }
    }
}
