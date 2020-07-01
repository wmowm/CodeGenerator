using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGenerator.Models
{
    public class TableConfig
    {



        public string Id { get; set; }
        /// <summary>
        /// 表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; set; }


        /// <summary>
        /// 库名
        /// </summary>
        public string DbName { get; set; }

        /// <summary>
        /// 列
        /// </summary>
        public List<ColumnConfig> ColumnConfig { get; set; }


        public Dictionary<string, object> Map
        {
            get 
            {
                Dictionary<string, object> dict = new Dictionary<string, object>();
                
                //列
                List<Dictionary<string, object>> map = new List<Dictionary<string, object>>();
                if (ColumnConfig == null) return dict;
                foreach (var item in ColumnConfig)
                {
                    dict = new Dictionary<string, object>();
                    dict.Add("ColumnName", item.ColumnName);
                    dict.Add("ColumnNamePrefix", item.ColumnNamePrefix);
                    dict.Add("CsType", item.CsType);
                    dict.Add("PropName", item.PropName);
                    dict.Add("Remark", item.Remark);
                    map.Add(dict);
                }
                dict.Add("Id", Id);
                dict.Add("TableName", TableName);
                dict.Add("FullName", FullName);
                dict.Add("DbName", DbName);
                dict.Add("ColumnConfig", map);
                return dict;
            }
        }

    }


}
