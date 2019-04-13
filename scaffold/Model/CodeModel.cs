using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace scaffold.Model
{
    public class CodeModel
    {
        private IEnumerable<DatabaseModel> Databases { get; } = new DatabaseModel().GetDatabases();

        /// <summary>
        /// 已选中的表
        /// </summary>
        public IEnumerable<string> CheckedTables { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// 项目信息
        /// </summary>
        [JsonIgnore]
        public ProjectInitModel Project
                => new ProjectInitModel().GetProjects()?.FirstOrDefault(x => x.Name == ProjectName);

        public void SaveModel()
        {
            foreach (var checkedTable in CheckedTables)
            {
                var (database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any()) continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1)) continue;

                var code = new StringBuilder();
                code.AppendLine("//=============系统自动生成=============");
                code.AppendLine($"//时间：{DateTime.Now:g}");
                code.AppendLine("//备注：表字段对应的数据模型。请勿在此文件中变动代码。");
                code.AppendLine("//=============系统自动生成=============");
                code.AppendLine("using System;");
                code.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                code.AppendLine("// ReSharper disable InconsistentNaming");
                code.AppendLine();
                code.AppendLine($"namespace {ProjectName}.Models.{database.Database}");
                code.AppendLine("{");
                code.AppendLine($"    /// <summary>{table.Comment}</summary>");
                code.AppendLine($"    [Table(\"{table.Name}\")]");
                code.AppendLine($"    public class {table.Name}Model");
                code.AppendLine("    {");
                foreach (var field in fields)
                {
                    code.AppendLine($"        /// <summary>{field.Comment}</summary>");
                    code.AppendLine($"        public {MapDataType(field.Type)} {field.Name} {{ get; set; }}");
                    if (fields.IndexOf(field) != fields.Count - 1)
                        code.AppendLine();
                }
                code.AppendLine("    }");
                code.AppendLine("}");
                var path = $"{Project.Path}/{ProjectName}.Models/{database.Database}Model";
                Directory.CreateDirectory(path);
                path += $"/{table.Name}.cs";
                File.WriteAllText(path, code.ToString());
            }
        }

        public void SaveDatabase()
        {
            foreach (var checkedTable in CheckedTables)
            {
                var (database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any()) continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1)) continue;

                var path = $"{Project.Path}/{ProjectName}.Database/{database.Database}";
                Directory.CreateDirectory(path);
                var contextPath = $"{path}/1_{database.Database}Context.cs";
                StringBuilder code;

                if (File.Exists(contextPath))
                {
                    #region 添加数据结构声明主方法
                    var lines = File.ReadAllLines(contextPath).ToList();
                    var lineFlag = false;
                    var contextIndex = 0;
                    var oldContext = new List<string>();
                    foreach (var line in lines)
                    {
                        if (line.Trim().Replace("  ", " ").Replace("  ", " ")
                            == "protected override void OnModelCreating(ModelBuilder modelBuilder)")
                        {
                            lineFlag = true;
                        }
                        if (lineFlag && line.Trim() == "}") // 第一个结束后符视为这个方法的结尾
                        {
                            break;
                        }
                        if (lineFlag)
                        {
                            oldContext.Add(line);
                        }
                        contextIndex++;
                    }
                    if (oldContext.All(x => !x.Contains($"OnModelCreating{table.Name}(modelBuilder);")))
                    {
                        lines.Insert(contextIndex, $"            OnModelCreating{table.Name}(modelBuilder); // {table.Comment}");
                    }
                    code = new StringBuilder();
                    foreach (var line in lines)
                    {
                        code.AppendLine(line);
                    }
                    #endregion
                }
                else
                {
                    #region 初始化数据结构声明主方法
                    code = new StringBuilder();
                    code.AppendLine("//=============系统自动生成=============");
                    code.AppendLine($"//时间：{DateTime.Now:g}");
                    code.AppendLine("//备注：依次注入表结构到EF框架中。请勿在此文件中变动代码。");
                    code.AppendLine("//=============系统自动生成=============");
                    code.AppendLine("using Microsoft.EntityFrameworkCore;");
                    code.AppendLine();
                    code.AppendLine($"namespace {ProjectName}.Database.{database.Database}");
                    code.AppendLine("{");
                    code.AppendLine("    /// <inheritdoc />");
                    code.AppendLine("    /// <summary>");
                    code.AppendLine($"    /// {database.Name}");
                    code.AppendLine("    /// </summary>");
                    code.AppendLine($"    public partial class {database.Database}Context : BaseContext<{database.Database}Context>");
                    code.AppendLine("    {");
                    code.AppendLine("        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
                    code.AppendLine("        {");
                    code.AppendLine("            if (!optionsBuilder.IsConfigured)");
                    code.AppendLine($"                optionsBuilder.UseMySql(\"{database.ConnString()}\");");
                    code.AppendLine("        }");
                    code.AppendLine();
                    code.AppendLine("        protected override void OnModelCreating(ModelBuilder modelBuilder)");
                    code.AppendLine("        {");
                    code.AppendLine($"            OnModelCreating{table.Name}(modelBuilder);");
                    code.AppendLine("        }");
                    code.AppendLine("    }");
                    code.AppendLine("}");
                    #endregion
                }
                File.WriteAllText(contextPath, code.ToString());

                #region 生成数据结构扩展代码
                code = new StringBuilder();
                code.AppendLine("//=============系统自动生成=============");
                code.AppendLine($"//时间：{DateTime.Now:g}");
                code.AppendLine("//备注：简单的数据库操作方法，以及声明表结构。请勿在此文件中变动代码。");
                code.AppendLine("//=============系统自动生成=============");
                code.AppendLine("using System;");
                code.AppendLine("using System.Linq;");
                code.AppendLine("using System.Threading.Tasks;");
                code.AppendLine("using System.Linq.Expressions;");
                code.AppendLine("using System.Collections.Generic;");
                code.AppendLine("using Microsoft.EntityFrameworkCore;");
                code.AppendLine($"using {ProjectName}.Models.{database.Database};");
                code.AppendLine();
                code.AppendLine($"namespace {ProjectName}.Database.{database.Database}");
                code.AppendLine("{");
                code.AppendLine($"    /// <summary>{table.Comment}</summary>");
                code.AppendLine($"    public partial class {table.Name}Storage : BaseStorage<{database.Database}Context, {table.Name}Model>");
                code.AppendLine("    {");
                code.AppendLine("        /// <summary>");
                code.AppendLine("        /// 第一个或者默认值");
                code.AppendLine("        /// </summary>");
                code.AppendLine("        /// <param name=\"predicate\">搜索条件</param>");
                code.AppendLine("        /// <returns></returns>");
                code.AppendLine($"        public override async Task<{table.Name}Model> FirstOrDefaultAsync(Expression<Func<{table.Name}Model, bool>> predicate)");
                code.AppendLine("        {");
                code.AppendLine($"            using (var context = new {database.Database}Context())");
                code.AppendLine($"                return await context.{table.Name}Model.FirstOrDefaultAsync(predicate);");
                code.AppendLine("        }");
                code.AppendLine();
                code.AppendLine("        /// <summary>");
                code.AppendLine("        /// 简单查询");
                code.AppendLine("        /// </summary>");
                code.AppendLine("        /// <param name=\"predicate\">搜索条件</param>");
                code.AppendLine("        /// <returns></returns>");
                code.AppendLine($"        public override async Task<List<{table.Name}Model>> FindAsync(Expression<Func<{table.Name}Model, bool>> predicate)");
                code.AppendLine("        {");
                code.AppendLine($"            using (var context = new {database.Database}Context())");
                code.AppendLine($"                return await context.{table.Name}Model.Where(predicate).ToListAsync();");
                code.AppendLine("        }");
                code.AppendLine("    }");
                code.AppendLine();
                code.AppendLine($"    public partial class {database.Database}Context");
                code.AppendLine("    {");
                code.AppendLine($"        public virtual DbSet<{table.Name}Model> {table.Name}Model {{ get; set; }}");
                code.AppendLine();
                code.AppendLine($"        protected void OnModelCreating{table.Name}(ModelBuilder modelBuilder)");
                code.AppendLine("        {");
                code.AppendLine($"            modelBuilder.Entity<{table.Name}Model>(entity =>");
                code.AppendLine("            {");
                var isKey = true;
                foreach (var field in fields)
                {
                    if (field.ColumnKey.Trim().Length > 0 && isKey)
                    {
                        code.AppendLine($"                entity.HasKey(e => e.{field.Name})");
                        code.AppendLine("                      .HasName(\"PRIMARY\");");
                        code.AppendLine();
                        isKey = false;
                    }
                    code.AppendLine($"                entity.Property(e => e.{field.Name})");
                    if (field.IsNull == "NO" && field.ColumnKey.Trim().Length <= 0) // 
                    {
                        code.AppendLine("                      .IsRequired()");
                    }
                    code.AppendLine($"                      .HasColumnName(\"{field.Name}\")");
                    code.Append($"                      .HasColumnType(\"{field.FullType}\")");
                    if (field.Default?.Trim() == "")
                        code.AppendLine("\r\n                      .HasDefaultValueSql(\"''\");");
                    else if (field.Default != null)
                        code.AppendLine($"\r\n                      .HasDefaultValueSql(\"'{field.Default}'\");");
                    else
                        code.Append(";\r\n");
                    if (fields.IndexOf(field) != fields.Count - 1)
                        code.AppendLine();
                }
                code.AppendLine("            });");
                code.AppendLine("        }");
                code.AppendLine("    }");
                code.AppendLine("}");
                File.WriteAllText($"{path}/{table.Name}StorageExtend.cs", code.ToString());
                #endregion

                #region 生成主文件
                if (!File.Exists($"{path}/{table.Name}Storage.cs"))
                {
                    code = new StringBuilder();
                    code.AppendLine($"namespace {ProjectName}.Database.{database.Database}");
                    code.AppendLine("{");
                    code.AppendLine($"    /// <summary>{table.Comment}</summary>");
                    code.AppendLine($"    public partial class {table.Name}Storage");
                    code.AppendLine("    {");
                    code.AppendLine("    }");
                    code.AppendLine("}");
                    File.WriteAllText($"{path}/{table.Name}Storage.cs", code.ToString());
                }
                #endregion

                #region 生成BaseContext
                var pathBase = $"{Project.Path}/{ProjectName}.Database/BaseContext.cs";
                if (!File.Exists(path))
                {
                    var codeStr =
@"using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace {0}.Database
{
    public class BaseContext<TContext> : DbContext
        where TContext : DbContext
    {
        protected readonly DbContextOptions<TContext> Options;

        public BaseContext()
        { }

        public BaseContext(DbContextOptions<TContext> options)
            : base(options)
        {
            Options = options;
        }
    }

    public interface IBaseStorage<TModel>
        where TModel : class
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<(bool done, TModel after)> AddAsync(TModel model);

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> AddRangeAsync(IEnumerable<TModel> list);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        (bool done, TModel after) Update(TModel model);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        int UpdateRange(IEnumerable<TModel> list);

        /// <summary>
        /// 单个查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate);

        /// <summary>
        /// 简单查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate);
    }

    public class BaseStorage<TContext, TModel> : IBaseStorage<TModel>
        where TContext : DbContext, new()
        where TModel : class
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<(bool done, TModel after)> AddAsync(TModel model)
        {
            using (var context = new TContext())
            {
                var after = await context.AddAsync(model);
                var change = context.SaveChanges();
                return (change > 0, after.Entity);
            }
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> AddRangeAsync(IEnumerable<TModel> list)
        {
            using (var context = new TContext())
            {
                await context.AddRangeAsync(list);
                var change = context.SaveChanges();
                return change;
            }
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public (bool done, TModel after) Update(TModel model)
        {
            using (var context = new TContext())
            {
                var after = context.Update(model);
                var change = context.SaveChanges();
                return (change > 0, after.Entity);
            }
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public int UpdateRange(IEnumerable<TModel> list)
        {
            using (var context = new TContext())
            {
                context.UpdateRange(list);
                var change = context.SaveChanges();
                return change;
            }
        }

        /// <summary>
        /// 单个查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        public virtual Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 简单查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        public virtual Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate)
        {
            throw new NotImplementedException();
        }
    }
}";
                    codeStr = codeStr.Replace("{0}", ProjectName);
                    File.WriteAllText(pathBase, codeStr);
                }
                #endregion
            }
        }

        public void SaveService()
        {
            foreach (var checkedTable in CheckedTables)
            {
                var (database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any()) continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1)) continue;

                #region BaseService
                Directory.CreateDirectory($"{Project.Path}/{ProjectName}.Services");
                var pathBase = $"{Project.Path}/{ProjectName}.Services/BaseService.cs";
                if (!File.Exists(pathBase))
                {
                    var codeStr =
@"using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using {0}.Database;

namespace {0}.Services
{
    public interface IBaseService<TModel>
        where TModel : class
    {
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<(bool done, TModel after)> AddAsync(TModel model);

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> AddRangeAsync(IEnumerable<TModel> list);

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        (bool done, TModel after) Update(TModel model);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        int UpdateRange(IEnumerable<TModel> list);

        /// <summary>
        /// 单个查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate);

        /// <summary>
        /// 简单查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate);
    }

    public class BaseService<TModel> : IBaseService<TModel>
        where TModel : class
    {
        private readonly IBaseStorage<TModel> _storage;

        protected BaseService(IBaseStorage<TModel> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<(bool done, TModel after)> AddAsync(TModel model)
        {
            return await _storage.AddAsync(model);
        }

        /// <summary>
        /// 批量新增
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> AddRangeAsync(IEnumerable<TModel> list)
        {
            return await _storage.AddRangeAsync(list);
        }

        /// <summary>
        /// 简单查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        public async Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate)
        {
            return await _storage.FindAsync(predicate);
        }

        /// <summary>
        /// 单个查询
        /// </summary>
        /// <param name=""predicate""></param>
        /// <returns></returns>
        public async Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate)
        {
            return await _storage.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public (bool done, TModel after) Update(TModel model)
        {
            return _storage.Update(model);
        }

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public int UpdateRange(IEnumerable<TModel> list)
        {
            return _storage.UpdateRange(list);
        }
    }
}";
                    File.WriteAllText(pathBase, codeStr.Replace("{0}", ProjectName));
                }
                #endregion
                Directory.CreateDirectory($"{Project.Path}/{ProjectName}.Services/{database.Database}");
                var path = $"{Project.Path}/{ProjectName}.Services/{database.Database}/{table.Name}Service.cs";
                if (!File.Exists(path))
                {
                    var codeStr =
$@"using {ProjectName}.Database;
using {ProjectName}.Models.{database.Database};
using {ProjectName}.Database.{database.Database};

namespace {ProjectName}.Services.{database.Database}
{{
    /// <summary>{table.Comment}接口</summary>
    public interface I{table.Name}Service : IBaseStorage<{table.Name}Model>
    {{
        
    }}

    /// <summary>{table.Comment}服务</summary>
    public class {table.Name}Service : BaseService<{table.Name}Model>, I{table.Name}Service
    {{
        private static readonly IBaseStorage<{table.Name}Model> Storage = new {table.Name}Storage();

        public {table.Name}Service() : base(Storage) {{ }}
    }}
}}";
                    File.WriteAllText(path, codeStr);
                }

            }
        }

        private (DatabaseModel database, TableModel table, List<FieldModel> fields) InitInfo(string checkedTable)
        {
            var databaseName = checkedTable.Split('.')[0];
            var tableName = checkedTable.Split('.')[1];

            var database = Databases?.FirstOrDefault(x => x.Name == databaseName);
            if (database == null) throw new NullReferenceException(nameof(database));
            var table = new TableModel
            {
                Database = database
            }.GetTables()?.FirstOrDefault(x => x.Name == tableName);
            if (table == null) throw new NullReferenceException(nameof(table));
            table.Comment = table.Comment.Replace("\r\n", "");
            var fields = new FieldModel
            {
                Table = table
            }.GetFields()?.ToList();
            if (fields == null) throw new NullReferenceException(nameof(fields));
            return (database, table, fields);
        }

        private string MapDataType(string sqlDataType)
        {
            switch (sqlDataType.ToLower())
            {
                case "bigint": return "long";
                case "datetime": return "DateTime";

                case "text":
                case "varchar": return "string";

                case "int":
                case "decimal":
                case "double": return sqlDataType.ToLower();
                default: return "object";
            }
        }
    }
}

