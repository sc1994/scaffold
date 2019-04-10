using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;


namespace scaffold.Model
{
    public class FieldModel
    {
        /// <summary>
        /// 表
        /// </summary>
        [JsonIgnore]
        public TableModel Table { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// IsNull
        /// </summary>
        public string IsNull { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Length
        /// </summary>
        public string Length { get; set; }

        /// <summary>
        /// ColumnKey
        /// </summary>
        public string ColumnKey { get; set; }

        /// <summary>
        /// Increment
        /// </summary>
        public string Increment { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 完整类型
        /// </summary>
        public string FullType { get; set; }

        public IEnumerable<FieldModel> GetFields()
        {
            if (Table == null)
            {
                throw new Exception("表信息不能为空");
            }

            var sql = @"SELECT 
                          COLUMN_NAME `Name`,
                          COLUMN_DEFAULT `Default`,
                          IS_NULLABLE `IsNull`,
                          DATA_TYPE `Type`,
                          CHARACTER_MAXIMUM_LENGTH `Length`,
                          COLUMN_KEY ColumnKey,
                          EXTRA Increment,
                          COLUMN_COMMENT `Comment`,
                          COLUMN_TYPE FullType
                        FROM INFORMATION_SCHEMA.Columns 
                        WHERE 
                          table_name=@name";
            using (var conn = new MySqlConnection(Table.Database.ConnString()))
            {
                var flag = conn.Query<FieldModel>(sql, new { name = Table.Name });
                foreach (var fieldModel in flag)
                {
                    fieldModel.Table = Table;
                }

                return flag;
            }
        }
    }
}
