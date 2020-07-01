using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CodeGenerator.Common;
using CodeGenerator.Models;
using Microsoft.AspNetCore.Mvc;

namespace CodeGenerator.Controllers
{
    public class TemplateConfigController : Controller
    {

        private ISqliteFreeSql _sqliteFreeSql;
        public TemplateConfigController(ISqliteFreeSql sqliteFreeSql)
        {
            _sqliteFreeSql = sqliteFreeSql;
        }




        public async Task<IActionResult> Create()
        {
            return View();
        }

        public async Task<IActionResult> Edit(string Id)
        {
            var model = _sqliteFreeSql.Select<TemplateConfig>().Where(m => m.Id.Equals(Id)).ToOne();
            _sqliteFreeSql.Dispose();
            return View(model);
        }


        /// <summary>
        /// 添加模板
        /// </summary>
        /// <param name="sqlconnect"></param>
        /// <returns></returns>
        public async Task<JsonResult> AddTemp(TemplateConfig temp)
        {
            PageResponse reponse = new PageResponse();
            try
            {
                //判断模板是否存在
                var path = AppContext.BaseDirectory + temp.TempatePath;
                if (System.IO.File.Exists(path))
                {
                    reponse.code = "500";
                    reponse.status = -1;
                    reponse.msg = "模板已存在,创建失败!";
                    return Json(reponse);
                }
                else
                {
                    FileHelp fileHelp = new FileHelp();
                    await fileHelp.WriteViewAsync(temp.TempatePath, "");
                }
                var insert = _sqliteFreeSql.Insert<TemplateConfig>();
                temp.Id = Guid.NewGuid().ToString();
                var res = insert.AppendData(temp);
                var i = insert.ExecuteAffrows();
                _sqliteFreeSql.Dispose();
                if (i > 0)
                {
                    reponse.code = "200";
                    reponse.status = 0;
                    reponse.msg = "添加成功!";
                    reponse.data = temp;
                }
                else
                {
                    reponse.code = "500";
                    reponse.status = -1;
                    reponse.msg = "添加失败!";
                }
                return Json(reponse);

            }
            catch (Exception ex)
            {
                reponse.code = "500";
                reponse.status = -1;
                reponse.msg = "添加失败!";
                return Json(reponse);
            }
        }

        /// <summary>
        /// 修改服务
        /// </summary>
        /// <param name="sqlconnect"></param>
        /// <returns></returns>
        public async Task<JsonResult> UpdateTemp(TemplateConfig temp)
        {
            PageResponse reponse = new PageResponse();
            var update = _sqliteFreeSql.Update<TemplateConfig>();
            var i = update.SetSource(temp).ExecuteAffrows();
            _sqliteFreeSql.Dispose();
            if (i > 0)
            {
                reponse.code = "200";
                reponse.status = 0;
                reponse.msg = "修改成功!";
                reponse.data = temp;
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
        public async Task<JsonResult> DeleteTemp(string Id)
        {
            PageResponse reponse = new PageResponse();
            var i = _sqliteFreeSql.Delete<TemplateConfig>().Where(m => m.Id.Equals(Id)).ExecuteAffrows();
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
        /// 获取所有的配置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetTempList()
        {
            PageResponse reponse = new PageResponse();
            List<zTree> list_ztree = new List<zTree>();
            var list = _sqliteFreeSql.Select<TemplateConfig>().ToList();
            _sqliteFreeSql.Dispose();
            //根节点
            zTree ztree = new zTree()
            {
                id = "temp0",
                pId = "#",
                name = "模板配置",
                noEditBtn = true,
                noRemoveBtn = true,
                open = true
            };
            list_ztree.Add(ztree);
            foreach (var temp in list)
            {
                ztree = new zTree()
                {
                    id = temp.Id,
                    pId = "temp0",
                    name = temp.Name,
                    open = true
                };
                list_ztree.Add(ztree);
            }
            reponse.code = "200";
            reponse.data = list_ztree;
            reponse.status = 0;
            reponse.total = list_ztree.Count();
            return Json(reponse);
        }


        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetTempContent(string Id)
        {
            PageResponse reponse = new PageResponse();

            try
            {
                reponse.status = -1;
                reponse.code = "200";
                if (string.IsNullOrEmpty(Id))
                {
                    reponse.msg = "未选择模板";
                    return Json(reponse);
                }
                var model = _sqliteFreeSql.Select<TemplateConfig>().Where(m => m.Id.Equals(Id)).ToOne();
                _sqliteFreeSql.Dispose();
                if (model == null)
                {
                    reponse.msg = "模板不存在";
                    return Json(reponse);
                }
                var path = $"{AppContext.BaseDirectory}{model.TempatePath}";
                var sourceTemplate = System.IO.File.ReadAllText(path);
                reponse.code = "200";
                reponse.data = sourceTemplate;
                reponse.status = 0;
                return Json(reponse);
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> UpdateTempContent(string Id, string context)
        {
            PageResponse reponse = new PageResponse();
            try
            {
                reponse.status = -1;
                reponse.code = "200";
                if (string.IsNullOrEmpty(Id))
                {
                    reponse.msg = "未选择模板";
                    return Json(reponse);
                }
                var model = _sqliteFreeSql.Select<TemplateConfig>().Where(m => m.Id.Equals(Id)).ToOne();
                _sqliteFreeSql.Dispose();
                if (model == null)
                {
                    reponse.msg = "模板不存在";
                    return Json(reponse);
                }
                FileHelp fileHelp = new FileHelp();
                await fileHelp.WriteViewAsync(model.TempatePath, context);

                reponse.msg = "修改成功!";
                reponse.status = 0;
                return Json(reponse);
            }
            catch (Exception ex)
            {
                reponse.msg = ex.Message;
                reponse.status = -1;
                reponse.code = "500";
                return Json(reponse);
            }
        }
    }
}