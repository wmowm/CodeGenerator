﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CodeGenerator.Models;
using CodeGenerator.Common;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using System.Dynamic;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CodeGenerator.Controllers
{
    public class HomeController : Controller
    {

        private IViewRenderService _viewRenderService;
        private ISqliteFreeSql _sqliteFreeSql;
        private readonly IMemoryCache _Cache;

        public HomeController(IViewRenderService viewSendeRenderService, ISqliteFreeSql sqliteFreeSql, IMemoryCache Cache)
        {
            _viewRenderService = viewSendeRenderService;
            _sqliteFreeSql = sqliteFreeSql;
            _Cache = Cache;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        /// <summary>
        /// 创建连接
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Create()
        {
            await SetSqlTypeList();
            return View();
        }

        /// <summary>
        /// 修改连接
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Edit(string Id)
        {
            await SetSqlTypeList();
            var model = _sqliteFreeSql.Select<SqlConnect>().Where(m => m.Id.Equals(Id)).ToOne();
            _sqliteFreeSql.Dispose();
            return View(model);
        }

        /// <summary>
        /// 获取所有的服务
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetTreeDBList()
        {
            PageResponse reponse = new PageResponse();
            List<zTree> list_ztree = new List<zTree>();
            var list_sqlconnect = _sqliteFreeSql.Select<SqlConnect>().ToList();
            _sqliteFreeSql.Dispose();
            list_ztree = await GetTreeList(list_sqlconnect);
            reponse.code = "200";
            reponse.data = list_ztree;
            reponse.status = 0;
            reponse.total = list_ztree.Count();
            return Json(reponse);
        }

        /// <summary>
        /// 获取服务下指定的库全部信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetServerByID(string id)
        {
            PageResponse reponse = new PageResponse();
            var list_ztree = new List<zTree>();
            var sqlconnect = _sqliteFreeSql.Select<SqlConnect>().Where(p => p.Id == id).ToOne();
            _sqliteFreeSql.Dispose();
            try
            {
                IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                              .UseConnectionString(sqlconnect.SqlType, sqlconnect.Address)
                              .Build();
                var dbs = fsql.DbFirst.GetDatabases();
                if (!string.IsNullOrEmpty(sqlconnect.DbName))
                {
                    dbs = dbs.Where(p => p == sqlconnect.DbName).ToList();
                }
                foreach (var db in dbs)//数据库
                {
                    var dbId = Guid.NewGuid().ToString();
                    var ztree = new zTree()
                    {
                        id = dbId,
                        pId = id,
                        name = db,
                        noEditBtn = true,
                        noRemoveBtn = true
                    };
                    list_ztree.Add(ztree);
                    var tables = fsql.DbFirst.GetTablesByDatabase(db);
                    foreach (var table in tables)//表
                    {
                        var tableid = Guid.NewGuid().ToString();
                        ztree = new zTree()
                        {
                            id = tableid,
                            pId = dbId,
                            name = table.Name,
                            noEditBtn = true,
                            noRemoveBtn = true
                        };
                        list_ztree.Add(ztree);
                        //将table信息缓存

                        TableConfig tableConfig = new TableConfig()
                        {
                            Id = tableid,
                            TableName = table.Name,
                            DbName = db,
                            ColumnConfig = new List<ColumnConfig>()
                        };
                        foreach (var column in table.Columns)
                        {
                            tableConfig.ColumnConfig.Add(new ColumnConfig()
                            {
                                ColumnName = column.Name,
                                CsType = column.CsType.FullName,
                                Remark = column.Coment
                            });
                        }
                        _Cache.Set(tableid, tableConfig);
                    }

                }
                reponse.code = "200";
                reponse.data = list_ztree;
                reponse.status = 0;
                reponse.total = list_ztree.Count();

            }
            catch (Exception ex)
            {
                reponse.code = "500";
                reponse.status = -1;
                return Json(reponse);
            }
            return Json(reponse);
        }

        /// <summary>
        /// 获取库里的表(暂时无用)
        /// </summary>
        /// <param name="id">服务器id</param>
        /// <param name="dbName">库名</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetDbTable(string id,string dbName)
        {
            PageResponse reponse = new PageResponse();
            var sqlconnect = _sqliteFreeSql.Select<SqlConnect>().Where(p => p.Id == id).ToOne();

            IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                .UseConnectionString(sqlconnect.SqlType, sqlconnect.Address)
                .Build();
            var db = fsql.DbFirst.GetTablesByDatabase(dbName);
            fsql.Dispose();
            List<TableConfig> list_table = new List<TableConfig>();
            foreach (var table in db)
            {
                TableConfig tableConfig = new TableConfig()
                {
                    Id = Guid.NewGuid().ToString(),
                    TableName = table.Name,
                    ColumnConfig = new List<ColumnConfig>()
                };
                foreach (var column in table.Columns)
                {
                    tableConfig.ColumnConfig.Add(new ColumnConfig()
                    {
                        ColumnName = column.Name,
                        CsType = column.CsType.ToString(),
                        Remark = column.Coment
                    });
                }
                list_table.Add(tableConfig);
            }
            reponse.code = "200";
            reponse.data = list_table;
            reponse.status = 0;
            reponse.total = list_table.Count();
            return Json(reponse);
        }


        /// <summary>
        /// 获取服务下的库
        /// </summary>
        /// <param name="sqlconnect"></param>
        /// <returns></returns>
        public async Task<JsonResult> GetServerDbList(SqlConnect sqlconnect)
        {
            sqlconnect.DbName = "";
            return ServerDbList(sqlconnect);
        }


        /// <summary>
        /// 添加服务
        /// </summary>
        /// <param name="sqlconnect"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddServer(SqlConnect sqlconnect)
        {
            PageResponse reponse = new PageResponse();
            var insert = _sqliteFreeSql.Insert<SqlConnect>();
            sqlconnect.Id = Guid.NewGuid().ToString();
            var res = insert.AppendData(sqlconnect);
            var i = insert.ExecuteAffrows();
            _sqliteFreeSql.Dispose();
            if (i > 0)
            {
                reponse.code = "200";
                reponse.status = 0;
                reponse.msg = "添加成功!";
                reponse.data = sqlconnect;
            }
            else
            {
                reponse.code = "500";
                reponse.status = -1;
                reponse.msg = "添加失败!";
            }
            return Json(reponse);
        }

        /// <summary>
        /// 修改服务
        /// </summary>
        /// <param name="sqlconnect"></param>
        /// <returns></returns>
        public async Task<JsonResult> UpdateServer(SqlConnect sqlconnect)
        {
            PageResponse reponse = new PageResponse();
            var update = _sqliteFreeSql.Update<SqlConnect>();
            var i = update.SetSource(sqlconnect).ExecuteAffrows();
            _sqliteFreeSql.Dispose();
            if (i > 0)
            {
                reponse.code = "200";
                reponse.status = 0;
                reponse.msg = "修改成功!";
                reponse.data = sqlconnect;
            }
            else
            {
                reponse.code = "500";
                reponse.status = -1;
                reponse.msg = "修改失败!";
            }
            return Json(reponse);
        }


        /// <summary>
        /// 删除服务
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> DeleteServer(string Id)
        {
            PageResponse reponse = new PageResponse();
            var i = _sqliteFreeSql.Delete<SqlConnect>().Where(m => m.Id.Equals(Id)).ExecuteAffrows();
            _sqliteFreeSql.Dispose();
            if (i > 0)
            {
                reponse.code = "200";
                reponse.status = 0;
                reponse.msg = "删除成功!";
            }
            else
            {
                reponse.code = "500";
                reponse.status = -1;
                reponse.msg = "删除失败!";
            }
            return Json(reponse);
        }




        /// <summary>
        /// 生成
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Generator(string[] tables, string[] temps)
        {
            PageResponse reponse = new PageResponse();
            try
            {
                //查询所有需要生成的模板信息
                var list_temp = _sqliteFreeSql.Select<TemplateConfig>().Where(p => temps.Contains(p.Id)).ToList();

                //dbset
                var dbset = "";

                //automap
                var automap = "";

                FileHelp fileHelp = new FileHelp();

                foreach (var item in tables)
                {
                    var table = _Cache.Get<TableConfig>(item);
                    string functionStr = table.TableName.Substring(0, 1).ToUpper() + table.TableName.Substring(1);
                    table.TableName = functionStr;
                    var dbset_temp = "public DbSet<@@@> @@@ { get; set; }\r\n";
                    dbset += dbset_temp.Replace("@@@", table.TableName);

                    var automap_temp = "CreateMap<@@@, @@@Dto>();\r\n";
                    automap += automap_temp.Replace("@@@", table.TableName);

                    foreach (var temp in list_temp)
                    {
                        if (!string.IsNullOrEmpty(temp.FileName))
                        {
                            table.FullName = temp.FileName.Replace("{TableName}", table.TableName);
                        }
                        else
                        {
                            table.FullName = table.TableName;
                        }
                        //获取模板
                       // var path = $"{Directory.GetCurrentDirectory()}/Template/test.html";
                        var sourceTemplate = System.IO.File.ReadAllText($"{AppContext.BaseDirectory}{temp.TempatePath}");
                        var template = Mustachio.Parser.Parse(sourceTemplate);
                        var result = template(table.Map);
                        var name = $"/{table.FullName}{temp.FileSuffix}";
                        var url = temp.FilePath;
                        await fileHelp.WriteViewAsync(url, name, result);
                    }
                }
                await fileHelp.WriteViewAsync("temp/", "dbset.cs", dbset);
                await fileHelp.WriteViewAsync("temp/", "automap.cs", automap);
                reponse.code = "200";
                reponse.status = 0;
                reponse.msg = "生成成功!";
            }
            catch (Exception ex)
            {

                reponse.code = "500";
                reponse.status = -1;
                reponse.msg = "生成失败!";
            }
            return Json(reponse);
        }


        private async Task<List<zTree>> GetTreeList(List<SqlConnect> list_SqlConnect)
        {
            List<zTree> list_ztree = new List<zTree>();
            //根节点
            zTree ztree = new zTree()
            {
                id = "0",
                pId = "#",
                name = "服务器",
                noEditBtn = true,
                noRemoveBtn = true,
                open = true
            };
            list_ztree.Add(ztree);

            foreach (var SqlConnect in list_SqlConnect)
            {
                ztree = new zTree()
                {
                    id = SqlConnect.Id,
                    pId = "0",
                    name = SqlConnect.FullName,
                    open = true
                };
                list_ztree.Add(ztree);
            }
            return list_ztree;
        }



        private JsonResult ServerDbList(SqlConnect sqlconnect)
        {
            PageResponse reponse = new PageResponse();
            try
            {
                IFreeSql fsql = new FreeSql.FreeSqlBuilder()
                              .UseConnectionString(sqlconnect.SqlType, sqlconnect.Address)
                              .Build();
                var dbs = fsql.DbFirst.GetDatabases();
                fsql.Dispose();
                if (!string.IsNullOrEmpty(sqlconnect.DbName))
                {
                    dbs = dbs.Where(p => p == sqlconnect.DbName).ToList();
                }
                List<TableConfig> list_table = new List<TableConfig>();
                foreach (var name in dbs)
                {
                    TableConfig tableConfig = new TableConfig()
                    {
                        Id = Guid.NewGuid().ToString(),
                        TableName = name
                    };
                    list_table.Add(tableConfig);
                }
                reponse.code = "200";
                reponse.data = list_table;
                reponse.status = 0;
                reponse.total = list_table.Count();
                return Json(reponse);
            }
            catch (Exception ex)
            {
                reponse.code = "500";
                reponse.status = -1;
                return Json(reponse);
            }
        }

        private async Task SetSqlTypeList()
        {
            var list = new List<SqlConnect>();
            list.Add(new SqlConnect() { Name = "0",DbName = "MySql" });
            list.Add(new SqlConnect() { Name = "1", DbName = "SqlServer" });
            ViewBag.sqlTypeList = new SelectList(list, "Name", "DbName");

        }

    }
}
