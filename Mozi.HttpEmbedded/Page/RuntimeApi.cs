using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace Mozi.HttpEmbedded.Page
{
    //TODO 多增加一些常见的方法接口，减少调用方的开发工作量

    //TODO 如何实现DLL热覆盖？
    /// <summary>
    /// 内置接口 API接口使用示范 
    /// </summary>
    public class Runtime : BaseApi
    {
        /// <summary>
        /// 无参方法
        /// </summary>
        /// <returns></returns>
        public string Hello()
        {
            return "Welcome to Mozi.HttpEmbedded";
        }
        [Description("回应")]
        public string Echo(string info)
        {
            return info;
        }
        /// <summary>
        /// 取服务器当前时间
        /// </summary>
        /// <returns></returns>
        [Description("获取服务器时间 UTC")]
        public string GetTime()
        {
            return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }
        [Description("获取运行环境")]
        public RuntimeInfo Info()
        {
            RuntimeInfo info = new RuntimeInfo();
            var ass = Assembly.GetExecutingAssembly();
            info.Name = ass.GetName().Name;
            info.VersionName = ass.GetName().Version.ToString();
            info.PlatformName = ass.ImageRuntimeVersion;
            info.StartupTime = Context.Server.StartTime.ToUniversalTime().ToString("r");
            info.StartupPath = ass.Location;
            Module[] modules = ass.GetLoadedModules();
            foreach (Module m in modules)
            {
                info.LoadedModules.Add(new LoadedModuleInfo()
                {
                    Name = m.Name,
                    VersionName = m.Assembly.GetName().Version.ToString()
                });
            }
            return info;
        }
        /// <summary>
        /// 验证用户
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [Description("验证用户")]
        public ResponseMessage AuthUser(string username, string password)
        {
            ResponseMessage rm = new ResponseMessage();
            var server = Context.Server;
            if (server != null && server.Auth != null && server.Auth.IsValidUser(username, password))
            {
                rm.success = true;
                rm.message = "验证成功";
            }
            return rm;
        }
        /// <summary>
        /// 列出所有用户
        /// </summary>
        /// <returns></returns>
        internal ResponseMessage GetUsers()
        {
            throw new NotImplementedException();
        }
        //DONE 此处重传同名文件有问题
        /// <summary>
        /// 上传文件 支持多文件上传
        /// </summary>
        /// <returns></returns>
        [Description("上传文件")]
        public ResponseMessage UploadFile()
        {
            ResponseMessage rm = new ResponseMessage();
            bool success = false;
            if (Context.Request.Files.Count > 0)
            {
                try
                {
                    for (int i = 0; i < Context.Request.Files.Count; i++)
                    {
                        File f = Context.Request.Files[i];
                        string filePath = AppDomain.CurrentDomain.BaseDirectory + f.FileName;
                        System.IO.File.Delete(filePath);
                        using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                        {
                            fs.Write(f.FileData, 0, f.FileData.Length);
                            fs.Flush();
                            fs.Close();
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    rm.message = ex.Message;
                }
            }
            rm.success = success;
            return rm;
        }
        /// <summary>
        /// 将文件存放到指定的路径，以程序启动路径为根目录<see cref="F:AppDomain.CurrentDomain.BaseDirectory"/>
        /// </summary>
        /// <param name="dir">路径名以“.”分割,例如dir1.dir2.dir，不要在路径名中包含不符合路径命名的特殊字符</param>
        /// <returns></returns>
        [Description("上传文件到指定的路径，路径为相对路径")]
        public ResponseMessage PutFile(string dir)
        {
            ResponseMessage rm = new ResponseMessage();
            bool success = false;
            if (!string.IsNullOrEmpty(dir))
            {
                dir = Context.Server.ServerRoot + dir.Replace('.', '/');
            }
            else
            {
                dir = Context.Server.ServerRoot;
            }
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            if (Context.Request.Files.Count > 0)
            {
                try
                {
                    for (int i = 0; i < Context.Request.Files.Count; i++)
                    {
                        File f = Context.Request.Files[i];
                        string filePath = dir + "/" + f.FileName;
                        System.IO.File.Delete(filePath);
                        using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                        {
                            fs.Write(f.FileData, 0, f.FileData.Length);
                            fs.Flush();
                            fs.Close();
                        }
                    }
                    success = true;
                }
                catch (Exception ex)
                {
                    rm.message = ex.Message;
                }
            }
            rm.success = success;
            return rm;
        }
        [Description("列出指定目录文件")]
        private ResponseMessage ListFiles(string dir)
        {
            throw new NotImplementedException();
        }
        private ResponseMessage DeleteFile(string dir, string fileName)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 列出所有API
        /// </summary>
        /// <returns></returns>
        public ResponseMessage ListApi()
        {
            ResponseMessage rm = new ResponseMessage();
            List<Type> types = Router.Default.GetTypes();
            List<ApiInfo> apis = new List<ApiInfo>();
            foreach (var type in types)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var m in methods)
                {
                    ApiInfo api = new ApiInfo
                    {
                        domain = type.Namespace,
                        controller = type.Name,
                        methodname = m.Name
                    };
                    var attrs = m.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    if (attrs.Length > 0)
                    {
                        api.description = ((DescriptionAttribute)attrs[0]).Description;
                    }

                    ParameterInfo[] pms = m.GetParameters();
                    if (pms.Length > 0)
                    {
                        api.args = new List<ApiParam>();
                        foreach (var p in pms)
                        {
                            api.args.Add(new ApiParam()
                            {
                                paramname = p.Name,
                                paramtype = p.ParameterType.Name,
                            });
                        }
                    }
                    apis.Add(api);
                }
            }
            rm.data = apis;
            rm.success = true;
            return rm;
        }
        /// <summary>
        /// 测试Get方法
        /// </summary>
        /// <returns></returns>
        public ResponseMessage Get()
        {
            ResponseMessage rm = new ResponseMessage
            {
                data = Context.Request.Query,
                success = true
            };
            if (Context.Request.Method == RequestMethod.GET)
            {
                rm.success = true;
            }
            else
            {
                rm.success = false;
                rm.message = "Not a GET request";
            }
            return rm;
        }
        /// <summary>
        /// 测试POST方法
        /// </summary>
        /// <returns></returns>
        public ResponseMessage Post()
        {
            ResponseMessage rm = new ResponseMessage
            {
                data = Context.Request.Query,
                success = true
            };
            if (Context.Request.Method == RequestMethod.POST)
            {
                rm.success = true;
            }
            else
            {
                rm.success = false;
                rm.message = "Not a POST request";
            }
            return rm;
        }
        /// <summary>
        /// 主要用于判断头信息是否被正常解析
        /// </summary>
        /// <returns></returns>
        public ResponseMessage GetHeaders()
        {
            ResponseMessage rm = new ResponseMessage
            {
                data = Context.Request.Headers.GetAll(),
                success = true
            };
            return rm;
        }

        //TODO 支持增删改查，缓存过期
        /// <summary>
        /// 全局缓存-内存型
        /// </summary>
        /// <param name="action">query|add|remove|clear</param>
        /// <param name="name"></param>
        /// <param name="param"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ResponseMessage Cache(string action, string name, string param, string data)
        {
            ResponseMessage rm = new ResponseMessage();
            if (action == "query")
            {
                rm.data = Context.Server.Cache.Find(name, param);
            }
            else if (action == "add")
            {
                Context.Server.Cache.Add(name, param, data);
            }
            else if (action == "remove")
            {
                Context.Server.Cache.Remove(name, param);
            }
            else if (action == "clear")
            {
                Context.Server.Cache.ClearExpired();
            }
            else
            {
                rm.success = false;
                rm.message = "未给定正确的操作方法";
            }
            rm.success = true;
            return rm;
        }
        //TODO 2021-06-28 实现程序集的上传与注册
        //TODO 2021-06-28 实现程序集热更新
        /// <summary>
        /// 上传程序集，并实现自动注册
        /// </summary>
        /// <returns></returns>
        internal ResponseMessage UploadAssembly()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 这个方法暂时不用，还有问题
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public string Soap(string action)
        {
            if (action == "example")
            {
                WebService.SoapEnvelope envelope = new WebService.SoapEnvelope();
                envelope.Body.Method = "GetGoodsPrice";
                envelope.Body.Items.Add("GoodsCode", "123456789");
                envelope.Body.Items.Add("Price", "1");
                Context.Response.SetContentType(envelope.Version == WebService.SoapVersion.Ver11 ? "text/xml" : "application/soap+xml");
                return envelope.CreateDocument();
            }
            else if (action == "wsdl")
            {
                WebService.WSDL envelope = new WebService.WSDL();
                envelope.ServiceName = GetType().Name;
                envelope.ApiTypes.Methods.AddRange(this.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                Context.Response.SetContentType("text/xml");
                return envelope.CreateDocument();
            }
            return "";
        }
    }
    /// <summary>
    /// 标准消息封装
    /// </summary>
    [Serializable]
    public class ResponseMessage
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public override string ToString()
        {
            return string.Format("success:{0},code:{1},message:{2},data:{3}", success, code, message, data);
        }

    }
    /// <summary>
    /// API信息
    /// </summary>
    [Serializable]
    public class ApiInfo
    {
        public string domain { get; set; }
        public string controller { get; set; }
        public string methodname { get; set; }
        public string description { get; set; }
        public List<ApiParam> args { get; set; }
        public string returntype { get; set; }
    }
    /// <summary>
    /// API参数信息
    /// </summary>
    [Serializable]
    public class ApiParam
    {
        public string paramname { get; set; }
        public string paramtype { get; set; }
        public string constraint { get; set; }

    }
    /// <summary>
    /// 服务器运行环境
    /// </summary>
    [Serializable]
    public class RuntimeInfo
    {
        public string Name { get; set; }
        public string VersionName { get; set; }
        public string PlatformName { get; set; }
        public string StartupTime { get; set; }
        public string StartupPath { get; set; }
        public List<LoadedModuleInfo> LoadedModules { get; set; }
        public RuntimeInfo()
        {
            LoadedModules = new List<LoadedModuleInfo>();
        }
    }
    /// <summary>
    /// 装载到服务器的模块信息
    /// </summary>
    public class LoadedModuleInfo
    {
        public string Name { get; set; }
        public string VersionName { get; set; }
    }
}
