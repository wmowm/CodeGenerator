using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CodeGenerator.Common
{
    public class FileHelp
    {

        /// <summary>
        /// 将视图写入文件
        /// </summary>
        /// <param name="url">相对路径</param>
        /// <param name="name">文件名(包括后缀)</param>
        /// <param name="html">内容</param>
        /// <returns></returns>
        public async Task WriteViewAsync(string url, string name, string html,string tk = "StaticFile/")
        {
            //获取根目录
            var path = AppContext.BaseDirectory;
            //文件夹路径
            var dirpath = path + tk + url;
            //文件完整路径
            path = dirpath + name;

            //判断文件夹是否存在
            if (!System.IO.Directory.Exists(dirpath))//如果不存在就创建file文件夹
            {
                System.IO.Directory.CreateDirectory(dirpath);
            }
            //创建文件流  
            FileStream myfs = new FileStream(path, FileMode.Create);
            //打开方式  
            //1:Create  用指定的名称创建一个新文件,如果文件已经存在则改写旧文件  
            //2:CreateNew 创建一个文件,如果文件存在会发生异常,提示文件已经存在  
            //3:Open 打开一个文件 指定的文件必须存在,否则会发生异常  
            //4:OpenOrCreate 打开一个文件,如果文件不存在则用指定的名称新建一个文件并打开它.  
            //5:Append 打开现有文件,并在文件尾部追加内容.  

            //创建写入器  
            StreamWriter mySw = new StreamWriter(myfs);//将文件流给写入器  
            //将录入的内容写入文件  
            mySw.Write(html);
            //关闭写入器  
            mySw.Close();
            //关闭文件流  
            myfs.Close();
        }


        /// <summary>
        /// 将视图写入文件
        /// </summary>
        /// <param name="url">相对路径</param>
        /// <param name="html">内容</param>
        /// <returns></returns>
        public async Task WriteViewAsync(string url, string html)
        {
            //获取根目录
            var path = AppContext.BaseDirectory;
            //文件完整路径
            path = path + url;
            //创建文件流  
            FileStream myfs = new FileStream(path, FileMode.Create);
            //打开方式  
            //1:Create  用指定的名称创建一个新文件,如果文件已经存在则改写旧文件  
            //2:CreateNew 创建一个文件,如果文件存在会发生异常,提示文件已经存在  
            //3:Open 打开一个文件 指定的文件必须存在,否则会发生异常  
            //4:OpenOrCreate 打开一个文件,如果文件不存在则用指定的名称新建一个文件并打开它.  
            //5:Append 打开现有文件,并在文件尾部追加内容.  

            //创建写入器  
            StreamWriter mySw = new StreamWriter(myfs);//将文件流给写入器  
            //将录入的内容写入文件  
            mySw.Write(html);
            //关闭写入器  
            mySw.Close();
            //关闭文件流  
            myfs.Close();
        }
    }
}
