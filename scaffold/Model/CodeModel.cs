﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

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
        public ProjectModel Project
            => new ProjectModel().GetProjects()?.FirstOrDefault(x => x.Name == ProjectName);

        private(DatabaseModel database, TableModel table, List<FieldModel> fields)InitInfo(string checkedTable)
        {
            var databaseName = checkedTable.Split('.')[0];
            var tableName = checkedTable.Split('.')[1];

            var database = Databases?.FirstOrDefault(x => x.Name == databaseName);
            if (database == null)throw new NullReferenceException(nameof(database));
            var table = new TableModel
            {
                Database = database
            }.GetTables()?.FirstOrDefault(x => x.Name == tableName);
            if (table == null)throw new NullReferenceException(nameof(table));
            table.Comment = table.Comment.Replace("\r\n", "");
            var fields = new FieldModel
            {
                Table = table
            }.GetFields()?.ToList();
            if (fields == null)throw new NullReferenceException(nameof(fields));
            return (database, table, fields);
        }

        private string MapDataType(string sqlDataType)
        {
            switch (sqlDataType.ToLower())
            {
                case "bigint":
                    return "long";
                case "datetime":
                    return "DateTime";

                case "text":
                case "varchar":
                    return "string";

                case "int":
                case "decimal":
                case "double":
                    return sqlDataType.ToLower();
                default:
                    return "object";
            }
        }

        public void SaveModel()
        {
            foreach (var checkedTable in CheckedTables)
            {
                var(database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any())continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1))continue;

                var code = new StringBuilder();
                code.AppendLine("// =============系统自动生成=============");
                code.AppendLine($"// 时间：{DateTime.Now:g}");
                code.AppendLine("// 备注：表字段对应的数据模型。请勿在此文件中变动代码。");
                code.AppendLine("// =============系统自动生成=============");
                code.AppendLine("// ReSharper disable InconsistentNaming");
                code.AppendLine();
                code.AppendLine("using System;");
                code.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
                code.AppendLine();
                code.AppendLine($"namespace {Project["Entities"]}.{database.Database}");
                code.AppendLine("{");
                code.AppendLine($"    /// <summary>{table.Comment}</summary>");
                code.AppendLine($"    [Table(\"{table.Name}\")]");
                code.AppendLine($"    public class {table.Name}Entity");
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
                var path = $"{Project.Path}/{Project["Entities"]}/{database.Database}";
                Directory.CreateDirectory(path);
                path += $"/{table.Name}Entity.cs";
                File.WriteAllText(path, code.ToString());
            }
        }

        public void SaveDatabase()
        {
            //var csprojPath = $"{Project.Path}/{Project["Repositories"]}/{Project["Repositories"]}.csproj";
            //ProjectInitModel.AddNuget(csprojPath, "Pomelo.EntityFrameworkCore.MySql", "2.2.0");

            foreach (var checkedTable in CheckedTables)
            {
                var(database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any())continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1))continue;

                var path = $"{Project.Path}/{Project["Repositories"]}/{database.Database}";
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
                        if (line.Trim().Replace("  ", " ").Replace("  ", " ") ==
                            "protected override void OnModelCreating(ModelBuilder modelBuilder)")
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
                    code.AppendLine("// =============系统自动生成=============");
                    code.AppendLine($"// 时间：{DateTime.Now:g}");
                    code.AppendLine("// 备注：依次注入表结构到EF框架中。请勿在此文件中变动代码。");
                    code.AppendLine("// =============系统自动生成=============");
                    code.AppendLine();
                    code.AppendLine("using Microsoft.EntityFrameworkCore;");
                    code.AppendLine();
                    code.AppendLine($"namespace {Project["Repositories"]}.{database.Database}");
                    code.AppendLine("{");
                    code.AppendLine("    /// <inheritdoc />");
                    code.AppendLine($"    /// <summary>{database.Name}</summary>");
                    code.AppendLine($"    public partial class {database.Database}Context : BaseContext<{database.Database}Context>");
                    code.AppendLine("    {");
                    code.AppendLine("        /// <summary>配置数据库地址</summary>");
                    code.AppendLine("        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
                    code.AppendLine("        {");
                    code.AppendLine("            if (!optionsBuilder.IsConfigured)");
                    code.AppendLine($"                optionsBuilder.UseMySql(\"{database.ConnString()}\");");
                    code.AppendLine("        }");
                    code.AppendLine();
                    code.AppendLine("        /// <summary>创建表结构</summary>");
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
                code.AppendLine("// =============系统自动生成=============");
                code.AppendLine($"// 时间：{DateTime.Now:g}");
                code.AppendLine("// 备注：简单的数据库操作方法，以及声明表结构。请勿在此文件中变动代码。");
                code.AppendLine("// =============系统自动生成=============");
                code.AppendLine();
                code.AppendLine("using System;");
                code.AppendLine("using System.Linq;");
                code.AppendLine("using System.Threading.Tasks;");
                code.AppendLine("using System.Linq.Expressions;");
                code.AppendLine("using System.Collections.Generic;");
                code.AppendLine("using Microsoft.EntityFrameworkCore;");
                code.AppendLine($"using {Project["Entities"]}.{database.Database};");
                code.AppendLine();
                code.AppendLine($"namespace {Project["Repositories"]}.{database.Database}");
                code.AppendLine("{");
                code.AppendLine($"    /// <summary>{table.Comment}</summary>");
                code.AppendLine($"    public partial class {table.Name}Storage : BaseStorage<{database.Database}Context, {table.Name}Entity>");
                code.AppendLine("    {");
                code.AppendLine("        /// <summary>单个查询</summary>");
                code.AppendLine("        /// <param name=\"predicate\">搜索条件</param>");
                code.AppendLine("        /// <returns></returns>");
                code.AppendLine($"        public override async Task<{table.Name}Entity> FirstOrDefaultAsync(Expression<Func<{table.Name}Entity, bool>> predicate)");
                code.AppendLine("        {");
                code.AppendLine($"            using (var context = new {database.Database}Context())");
                code.AppendLine($"                return await context.{table.Name}Entity.FirstOrDefaultAsync(predicate);");
                code.AppendLine("        }");
                code.AppendLine();
                code.AppendLine("        /// <summary>简单查询</summary>");
                code.AppendLine("        /// <param name=\"predicate\">搜索条件</param>");
                code.AppendLine("        /// <param name=\"index\"></param>");
                code.AppendLine("        /// <param name=\"size\"></param>");
                code.AppendLine("        /// <returns></returns>");
                code.AppendLine($"        public override async Task<List<{table.Name}Entity>> FindAsync(Expression<Func<{table.Name}Entity, bool>> predicate, int index = 0, int size = 0)");
                code.AppendLine("        {");
                code.AppendLine($"            using (var context = new {database.Database}Context())");
                code.AppendLine("            {");
                code.AppendLine($"                var query = context.{table.Name}Entity.Where(predicate);");
                code.AppendLine("                if (index > 0 && size > 0)");
                code.AppendLine("                    query = query.Skip((index - 1) * size).Take(size);");
                code.AppendLine("                return await query.ToListAsync();");
                code.AppendLine("            }");
                code.AppendLine("        }");
                code.AppendLine();
                code.AppendLine("        /// <summary>简单查询</summary>");
                code.AppendLine("        /// <param name=\"predicate\">搜索条件</param>");
                code.AppendLine("        /// <param name=\"index\"></param>");
                code.AppendLine("        /// <param name=\"size\"></param>");
                code.AppendLine("        /// <returns></returns>");
                code.AppendLine($"        public override async Task<List<{table.Name}Entity>> FindAsync({table.Name}Entity predicate, int index = 0, int size = 0)");
                code.AppendLine("        {");
                code.AppendLine($"            Expression<Func<{table.Name}Entity, bool>> search = null;");
                code.AppendLine($"            var defaultModel = new {table.Name}Entity();");
                code.AppendLine();
                foreach (var field in fields)
                {
                    if (fields.IndexOf(field) == 0)
                    {
                        code.AppendLine($"            if (defaultModel.{field.Name} != predicate.{field.Name})");
                        code.AppendLine($"                search = x => x.{field.Name} == predicate.{field.Name};");
                        continue;
                    }
                    code.AppendLine($"            if (defaultModel.{field.Name} != predicate.{field.Name})");
                    code.AppendLine("            {");
                    code.AppendLine("                if (search == null)");
                    code.AppendLine($"                    search = x => x.{field.Name} == predicate.{field.Name};");
                    code.AppendLine($"                else search = search.And(x => x.{field.Name} == predicate.{field.Name});");
                    code.AppendLine("            }");
                }
                var key = fields.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ColumnKey));
                if (key != null)
                {
                    code.AppendLine("            if (search == null)");
                    code.AppendLine($"                search = x => x.{key.Name}.ToString() != \"\"; // 添加默认条件，不推荐，务必在查询时加上条件");
                }
                code.AppendLine();
                code.AppendLine("            return await FindAsync(search, index, size);");
                code.AppendLine("        }");
                code.AppendLine("    }");
                code.AppendLine();
                code.AppendLine($"    /// <summary>初始化 {table.Comment} 结构</summary>");
                code.AppendLine($"    public partial class {database.Database}Context");
                code.AppendLine("    {");
                code.AppendLine($"        /// <summary>{table.Comment}</summary>");
                code.AppendLine($"        public virtual DbSet<{table.Name}Entity> {table.Name}Entity {{ get; set; }}");
                code.AppendLine();
                code.AppendLine($"        /// <summary>{table.Comment}</summary>");
                code.AppendLine($"        protected void OnModelCreating{table.Name}(ModelBuilder modelBuilder)");
                code.AppendLine("        {");
                code.AppendLine($"            modelBuilder.Entity<{table.Name}Entity>(entity =>");
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
                    code.AppendLine($"using {Project["Entities"]}.{database.Database};");
                    code.AppendLine();
                    code.AppendLine($"namespace {Project["Repositories"]}.{database.Database}");
                    code.AppendLine("{");
                    code.AppendLine($"    /// <summary>{table.Comment}接口</summary>");
                    code.AppendLine($"    public interface I{table.Name}Storage : IBaseStorage<{table.Name}Entity>");
                    code.AppendLine("    {");
                    code.AppendLine("    }");
                    code.AppendLine();
                    code.AppendLine($"    /// <summary>{table.Comment}</summary>");
                    code.AppendLine($"    public partial class {table.Name}Storage : I{table.Name}Storage");
                    code.AppendLine("    {");
                    code.AppendLine("    }");
                    code.AppendLine("}");
                    File.WriteAllText($"{path}/{table.Name}Storage.cs", code.ToString());
                }
                #endregion                
            }
        }


        public void SaveBaseRepositories()
        {
            var pathBase = $"{Project.Path}/{Project["Repositories"]}/BaseContext.cs";
            if (!File.Exists(pathBase))
            {
                var codeStr =
                    @"using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace {0}
{
    /// <summary>数据库上下文基类</summary>
    public class BaseContext<TContext> : DbContext
        where TContext : DbContext
    {
        /// <summary>数据库上下文</summary>
        protected readonly DbContextOptions<TContext> Options;

        /// <summary>空参构</summary>
        public BaseContext()
        { }

        /// <summary>设置数据库</summary>
        public BaseContext(DbContextOptions<TContext> options)
            : base(options)
        {
            Options = options;
        }
    }

    /// <summary>数据基类接口</summary>
    public interface IBaseStorage<TModel>
        where TModel : class
    {
        /// <summary>新增</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<(bool done, TModel after)> AddAsync(TModel model);

        /// <summary>批量新增</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> AddRangeAsync(IEnumerable<TModel> list);

        /// <summary>删除</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<int> RemoveAsync(TModel model);

        /// <summary>批量删除</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> RemoveRangeAsync(IEnumerable<TModel> list);

        /// <summary>更新</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<(bool done, TModel after)> UpdateAsync(TModel model);

        /// <summary>批量更新</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> UpdateRangeAsync(IEnumerable<TModel> list);

        /// <summary>单个查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <returns></returns>
        Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate);

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate, int index = 0, int size = 0);

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        Task<List<TModel>> FindAsync(TModel predicate, int index = 0, int size = 0);
    }

    /// <summary>数据基类</summary>
    public class BaseStorage<TContext, TModel> : IBaseStorage<TModel>
        where TContext : DbContext, new()
        where TModel : class
    {
        /// <summary>新增</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<(bool done, TModel after)> AddAsync(TModel model)
        {
            using (var context = new TContext())
            {
                var after = await context.AddAsync(model);
                var change = await context.SaveChangesAsync();
                return (change > 0, after.Entity);
            }
        }

        /// <summary>批量新增</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> AddRangeAsync(IEnumerable<TModel> list)
        {
            using (var context = new TContext())
            {
                await context.AddRangeAsync(list);
                var change = await context.SaveChangesAsync();
                return change;
            }
        }

        /// <summary>删除</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<int> RemoveAsync(TModel model)
        {
            using (var context = new TContext())
            {
                context.Remove(model);
                return await context.SaveChangesAsync();
            }
        }

        /// <summary>批量删除</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> RemoveRangeAsync(IEnumerable<TModel> list)
        {
            using (var context = new TContext())
            {
                context.RemoveRange(list);
                return await context.SaveChangesAsync();
            }
        }

        /// <summary>更新</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<(bool done, TModel after)> UpdateAsync(TModel model)
        {
            using (var context = new TContext())
            {
                var after = context.Update(model);
                var change = await context.SaveChangesAsync();
                return (change > 0, after.Entity);
            }
        }

        /// <summary>批量更新</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> UpdateRangeAsync(IEnumerable<TModel> list)
        {
            using (var context = new TContext())
            {
                context.UpdateRange(list);
                var change = await context.SaveChangesAsync();
                return change;
            }
        }

        /// <summary>单个查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <returns></returns>
        public virtual Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate)
            => throw new NotImplementedException();

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        public virtual Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate, int index = 0, int size = 0)
            => throw new NotImplementedException();

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        public virtual Task<List<TModel>> FindAsync(TModel predicate, int index = 0, int size = 0)
            => throw new NotImplementedException();
    }

    /// <summary>Enables the efficient, dynamic composition of query predicates.</summary>
    public static class PredicateBuilder
    {
        /// <summary>Creates a predicate that evaluates to true.</summary>
        public static Expression<Func<T, bool>> True<T>()
            =>  param => true;

        /// <summary>Creates a predicate that evaluates to false.</summary>
        public static Expression<Func<T, bool>> False<T>()
            =>  param => false;

        /// <summary>Creates a predicate expression from the specified lambda expression.</summary>
        public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> predicate)
            =>  predicate;

        /// <summary>Combines the first predicate with the second using the logical ""and"".</summary>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
            => first.Compose(second, Expression.AndAlso);

        /// <summary>Combines the first predicate with the second using the logical ""or"".</summary>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
            =>  first.Compose(second, Expression.OrElse);

        /// <summary>Negates the predicate.</summary>
        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            var negated = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(negated, expression.Parameters);
        }

        /// <summary>Combines the first expression with the second using the specified merge function.</summary>
        private static Expression<T> Compose<T>(this Expression<T> first, Expression<T> second, Func<Expression, Expression, Expression> merge)
        {
            var map = first.Parameters
                .Select((f, i) => new { f, s = second.Parameters[i] })
                .ToDictionary(p => p.s, p => p.f);

            var secondBody = ParameterRebinder.ReplaceParameters(map, second.Body);
            return Expression.Lambda<T>(merge(first.Body, secondBody), first.Parameters);
        }
    }

    /// <summary></summary>
    public class ParameterRebinder : ExpressionVisitor
    {
        private readonly Dictionary<ParameterExpression, ParameterExpression> _map;

        /// <summary></summary>
        public ParameterRebinder(Dictionary<ParameterExpression, ParameterExpression> map)
        {
            _map = map ?? new Dictionary<ParameterExpression, ParameterExpression>();
        }

        /// <summary></summary>
        public static Expression ReplaceParameters(Dictionary<ParameterExpression, ParameterExpression> map, Expression exp)
            => new ParameterRebinder(map).Visit(exp);

        /// <summary></summary>
        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (_map.TryGetValue(p, out var replacement))
                p = replacement;
            return base.VisitParameter(p);
        }
    }
}";
                codeStr = codeStr.Replace("{0}", Project["Repositories"]);
                File.WriteAllText(pathBase, codeStr);
            }
        }

        public void SaveService()
        {
            foreach (var checkedTable in CheckedTables)
            {
                var(database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any())continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1))continue;

                #region BaseService
                Directory.CreateDirectory($"{Project.Path}/{Project["Services"]}");
                var pathBase = $"{Project.Path}/{Project["Services"]}/BaseService.cs";
                if (!File.Exists(pathBase))
                {
                    var codeStr =
                        @"using System;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;
using {0};

namespace {1}
{
    /// <summary>数据基类接口</summary>
    public interface IBaseService<TModel>
        where TModel : class
    {
        /// <summary>新增</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<(bool done, TModel after)> AddAsync(TModel model);

        /// <summary>批量新增</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> AddRangeAsync(IEnumerable<TModel> list);

        /// <summary>删除</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<int> RemoveAsync(TModel model);

        /// <summary>批量删除</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> RemoveRangeAsync(IEnumerable<TModel> list);

        /// <summary>更新</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        Task<(bool done, TModel after)> UpdateAsync(TModel model);

        /// <summary>批量更新</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        Task<int> UpdateRangeAsync(IEnumerable<TModel> list);

        /// <summary>单个查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <returns></returns>
        Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate);

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate, int index = 0, int size = 0);

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        Task<List<TModel>> FindAsync(TModel predicate, int index = 0, int size = 0);
    }

    /// <summary>数据基类</summary>
    public class BaseService<TModel> : IBaseService<TModel>
        where TModel : class
    {
        private readonly IBaseStorage<TModel> _storage;

        /// <summary>数据基类</summary>
        protected BaseService(IBaseStorage<TModel> storage)
        {
            _storage = storage;
        }

        /// <summary>新增</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<(bool done, TModel after)> AddAsync(TModel model)
            => await _storage.AddAsync(model);

        /// <summary>批量新增</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> AddRangeAsync(IEnumerable<TModel> list)
            => await _storage.AddRangeAsync(list);

        /// <summary>删除</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<int> RemoveAsync(TModel model)
            => await _storage.RemoveAsync(model);

        /// <summary>批量删除</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> RemoveRangeAsync(IEnumerable<TModel> list)
            => await _storage.RemoveRangeAsync(list);

        /// <summary>更新</summary>
        /// <param name=""model""></param>
        /// <returns></returns>
        public async Task<(bool done, TModel after)> UpdateAsync(TModel model)
            => await _storage.UpdateAsync(model);

        /// <summary>批量更新</summary>
        /// <param name=""list""></param>
        /// <returns></returns>
        public async Task<int> UpdateRangeAsync(IEnumerable<TModel> list)
            => await _storage.UpdateRangeAsync(list);

        /// <summary>单个查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <returns></returns>
        public async Task<TModel> FirstOrDefaultAsync(Expression<Func<TModel, bool>> predicate)
            => await _storage.FirstOrDefaultAsync(predicate);

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        public async Task<List<TModel>> FindAsync(Expression<Func<TModel, bool>> predicate, int index = 0, int size = 0)
            => await _storage.FindAsync(predicate, index, size);

        /// <summary>简单查询</summary>
        /// <param name=""predicate"">搜索条件</param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>
        public async Task<List<TModel>> FindAsync(TModel predicate, int index = 0, int size = 0)
            => await _storage.FindAsync(predicate, index, size);
    }
}";
                    File.WriteAllText(pathBase, codeStr.Replace("{0}", Project["Repositories"]).Replace("{1}", Project["Services"]));
                }
                #endregion

                Directory.CreateDirectory($"{Project.Path}/{Project["Services"]}/{database.Database}");
                var path = $"{Project.Path}/{Project["Services"]}/{database.Database}/{table.Name}Service.cs";
                if (!File.Exists(path))
                {
                    var codeStr =
                        $@"using {Project["Entities"]}.{database.Database};
using {Project["Repositories"]}.{database.Database};

namespace {Project["Services"]}.{database.Database}
{{
    /// <summary>{table.Comment}接口</summary>
    public interface I{table.Name}Service : IBaseService<{table.Name}Model>
    {{
    }}

    /// <summary>{table.Comment}服务</summary>
    public class {table.Name}Service : BaseService<{table.Name}Model>, I{table.Name}Service
    {{
        /// <summary>{table.Comment}服务</summary>
        public {table.Name}Service(I{table.Name}Storage storage) : base(storage)
        {{ }}
    }}
}}";
                    File.WriteAllText(path, codeStr);
                }

            }
        }

        public void SaveApi()
        {
            #region 注入
            var codeStr =
                $@"        
        /// <summary>
        /// 注入
        /// </summary>
        /// <param name=""services""></param>
        public static void Transfuse(IServiceCollection services)
        {{
            // 单例（Singleton）生命周期服务在它们第一次被请求时创建，或者如果你在 ConfigureServices运行时指定一个实例）
            // 并且每个后续请求将使用相同的实例。如果你的应用程序需要单例行为，
            // 建议让服务容器管理服务的生命周期而不是在自己的类中实现单例模式和管理对象的生命周期。
            // 参考文章 http://www.cnblogs.com/dotNETCoreSG/p/aspnetcore-3_10-dependency-injection.html

            var allTypes = Assembly.Load(""{Project["Services"]}"").GetTypes().ToList(); 
            allTypes.AddRange(Assembly.Load(""{Project["Repositories"]}"").GetTypes()); // 获取命名空间下的全部实例

            foreach (var type in allTypes)
            {{
                if (type.GetTypeInfo().IsInterface)
                {{
                    var instance = allTypes.FirstOrDefault(x => x.Name == type.Name.Substring(1, type.Name.Length - 1));
                    if (instance != null)
                    {{
                        services.AddSingleton(type, instance); // 注入
                    }}
                }}
            }}
        }}";
            var basePath = $"{Project.Path}/{Project["Api"]}";
            var startupPath = basePath + "/Startup.cs";
            if (File.Exists(startupPath))
            {
                var codeLines = File.ReadAllLines(startupPath).ToList();
                for (var configureServicesIndex = codeLines.FindIndex(x => x.Trim() == "public void ConfigureServices(IServiceCollection services)"); configureServicesIndex < codeLines.Count; configureServicesIndex++)
                {
                    var line = codeLines[configureServicesIndex];

                    if (line.Trim() == "Transfuse(services);")
                    {
                        break;
                    }
                    if (line.Trim() == "}")
                    {
                        codeLines.Insert(configureServicesIndex, "            Transfuse(services);");
                        break;
                    }
                }

                if (codeLines.All(x => x.Trim() != "public static void Transfuse(IServiceCollection services)"))
                {
                    var lastFirst = codeLines.FindLastIndex(x => x.Trim() == "}");
                    codeLines.RemoveAt(lastFirst);
                    var lastSecond = codeLines.FindLastIndex(x => x.Trim() == "}");
                    codeLines.Insert(lastSecond, codeStr);
                    codeLines.Add("}");
                    File.WriteAllLines(startupPath, codeLines);
                }
            }
            #endregion

            #region 接口
            foreach (var checkedTable in CheckedTables)
            {
                var(database, table, fields) = InitInfo(checkedTable);
                if (!fields.Any())continue;
                if (fields.All(x => x.ColumnKey.Trim().Length < 1))continue;

                var code = new StringBuilder();
                code.AppendLine("// =============系统自动生成=============");
                code.AppendLine($"// 时间：{DateTime.Now:g}");
                code.AppendLine("// 备注：单表增删改查");
                code.AppendLine("// =============系统自动生成=============");
                code.AppendLine();
                code.AppendLine("using System.Threading.Tasks;");
                code.AppendLine("using Microsoft.AspNetCore.Mvc;");
                code.AppendLine("using System.Collections.Generic;");
                code.AppendLine($"using {Project["Entities"]}.{database.Database};");
                code.AppendLine();
                code.AppendLine($"namespace {Project["Api"]}.Controllers.{database.Database}");
                code.AppendLine("{");
                code.AppendLine($"    [Route(\"{database.Database}/[controller]\")]");
                code.AppendLine("    [ApiController]");
                code.AppendLine($"    public partial class {table.Name}Controller : ControllerBase");
                code.AppendLine("    {");

                code.AppendLine(
                    @"        /// <summary>查询</summary>
        /// <param name=""predicate""></param>
        /// <param name=""index""></param>
        /// <param name=""size""></param>
        /// <returns></returns>");

                code.AppendLine("        [HttpPost(\"{index}/{size}\")]");
                code.AppendLine($"        public async Task<IEnumerable<{table.Name}Model>> Get{table.Name}List([FromBody]{table.Name}Model predicate, int index, int size)");
                code.AppendLine("            => await _service.FindAsync(predicate, index, size);");

                var key = fields.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ColumnKey));
                if (key != null)
                {
                    code.AppendLine($@"        
        /// <summary>获取单个数据</summary>
        /// <param name=""id""></param>
        /// <returns></returns>
        [HttpGet(""{{id}}"")]
        public async Task<{table.Name}Model> Get{table.Name}Model({MapDataType(key.Type)} id)
            => await _service.FirstOrDefaultAsync(x => x.{key.Name} == id);");
                }

                code.AppendLine($@"        
        /// <summary>添加</summary>
        /// <param name=""value""></param>
        [HttpPost]
        public async Task<bool> Add{table.Name}([FromBody] {table.Name}Model value)
            => (await _service.AddAsync(value)).done;");
                if (key != null)
                {
                    code.AppendLine($@"
        /// <summary>修改</summary>
        /// <param name=""value""></param>
        [HttpPut]
        public async Task<bool> Edit{table.Name}([FromBody] {table.Name}Model value)
        {{
            var model = await Get{table.Name}Model(value.{key.Name});
            if (model == null) return false;
            return (await _service.UpdateAsync(value)).done;
        }}");
                    code.AppendLine($@"
        /// <summary>删除</summary>
        /// <param name=""id""></param>
        [HttpDelete(""{{id}}"")]
        public async Task<bool> Delete{table.Name}({MapDataType(key.Type)} id)
        {{
            var model = await Get{table.Name}Model(id);
            if (model == null) return false;
            return await _service.RemoveAsync(model) > 0;
        }}");
                }
                code.AppendLine("    }");
                code.AppendLine("}");
                var controllerPath = $"{basePath}/Controllers/{database.Database}";
                Directory.CreateDirectory(controllerPath);
                File.WriteAllText($"{controllerPath}/{table.Name}ControllerExtend.cs", code.ToString());

                // 主文件
                if (!File.Exists($"{controllerPath}//{table.Name}Controller.cs"))
                {
                    code = new StringBuilder();
                    code.AppendLine(
                        $@"using {Project["Services"]}.{database.Database};

namespace {Project["Api"]}.Controllers.{database.Database}
{{
    /// <summary>{table.Comment}</summary>
    public partial class {table.Name}Controller
    {{
        private readonly I{table.Name}Service _service;

        /// <summary>{table.Comment}</summary>
        public {table.Name}Controller(I{table.Name}Service service)
        {{
            _service = service;
        }}
    }}
}}");
                    File.WriteAllText($"{controllerPath}/{table.Name}Controller.cs", code.ToString());
                }
            }



            #endregion
        }

        public void SaveWeb()
        {

        }
    }
}