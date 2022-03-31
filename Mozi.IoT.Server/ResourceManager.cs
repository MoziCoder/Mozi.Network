using Mozi.IoT.Serialize;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Mozi.IoT
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager
    {
        public static ResourceManager _rm;

        public static ResourceManager Default
        {
            get
            {
                return _rm ?? (_rm = new ResourceManager());
            }
        }

        private List<Assembly> _assemblies = new List<Assembly>();
        private ISerializer _dataserializer;

        private readonly List<ResourceInfo> _apis = new List<ResourceInfo>();

        private ResourceManager()
        {
            //提供一个默认数据序列化接口
            //载入内部接口API
            LoadInternalApi();
        }

        public List<ResourceInfo> GetAll()
        {
            return _apis;
        }
        //TODO 注册时将Method也一并缓存
        /// <summary>
        /// 从程序集载入接口
        /// </summary>
        /// <param name="ass"></param>
        private void LoadApiFromAssembly(Assembly ass)
        {
            Type[] types = ass.GetExportedTypes();
            foreach (var type in types)
            {
                Register(type);
            }
        }
        /// <summary>
        /// 载入内部接口
        /// </summary>
        private void LoadInternalApi()
        {
            Assembly ass = Assembly.GetExecutingAssembly();
            LoadApiFromAssembly(ass);
        }
        //TODO 是否需要注入Server对象

        /// <summary>
        /// 调起
        /// </summary>
        /// <param name="ctx"></param>
        internal CoAPPackage Invoke(CoAPContext ctx)
        {
            string path = ctx.Request.Path;

            //确定路径映射关系
            string ns = "", name = "";
            var paths = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (paths.Length > 0)
            {
                name = paths[paths.Length - 1];
                if (paths.Length > 1)
                {
                    ns = string.Join("/", paths, 0, paths.Length - 1);
                }
            }
            var ri = _apis.Find(x => x.Namespace.Equals(ns, StringComparison.OrdinalIgnoreCase) && x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)&&x.Online);
            Type cls = null;
            cls = ri.ClsType;

            //TODO 将Method缓存
            //TODO 介入Size2查询
            //TODO 介入Block2查询
            //TODO 介入分块传输
            //TODO 介入查询协商


            //实例化对象
            CoAPResource instance = (CoAPResource)Activator.CreateInstance(cls);
            CoAPPackage result = null;

            if (!instance.HandleSize2Query(ctx))
            {

                MethodInfo method = cls.GetMethod("On" + ctx.Request.Code.Name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

                ParameterInfo[] pms = method.GetParameters();

                //开始装配参数
                object[] args = new object[] { ctx };

                ////TODO 注入上下文变量
                //((CoAPResource)instance).Server = ctx;

                //调用方法
                result = (CoAPPackage)method.Invoke(instance, BindingFlags.IgnoreCase, null, args, CultureInfo.CurrentCulture);
            }
            else
            {
                result = ctx.Response;
            }
            //对象置空
            instance = null;

            return result;
        }

        //TODO 加入域控制
        /// <summary>
        /// 载入模块
        /// <para>自动扫描程序集中的接口模块</para>
        /// </summary>
        /// <returns></returns>
        public ResourceManager Register(string filePath)
        {
            Assembly ass = Assembly.LoadFrom(filePath);
            LoadApiFromAssembly(ass);
            return this;
        }
        //TODO 加入域控制
        /// <summary>
        /// 载入模块
        /// <para>自动扫描程序集中继承自<see cref="T:BaseApi"/>的类，或者类标记为<see cref="T:BasicApiAttribute"/></para>
        /// </summary>
        /// <param name="ass"></param>
        /// <returns></returns>
        public ResourceManager Register(Assembly ass)
        {
            LoadApiFromAssembly(ass);
            return this;
        }
        /// <summary>
        /// 单独注册某个接口模块
        /// </summary>
        /// <param name="type">参数需继承自<see cref="T:CoAPResource"/>的类，其他类型无法注册</param>
        /// <returns></returns>
        public ResourceManager Register(Type type)
        {
            if (type.IsSubclassOf(typeof(CoAPResource)))
            {
                var attDesc = type.GetCustomAttributes(typeof(ResourceDescriptionAttribute), false);
                string ns = "", name = type.Name,desc=null,href="",resType="";
                if (attDesc.Length > 0)
                {
                    var att = (ResourceDescriptionAttribute)attDesc[0];
                    ns = att.Namespace ?? "";
                    name = att.Name ?? type.Name;
                    desc = att.Description;
                    href = string.IsNullOrEmpty(ns) ? $"/{name}" : $"/{ns}/{name}";
                    resType = att.ResourceType;
                }
                if (!_apis.Exists(x => x.Namespace.Equals(ns) && x.Name.Equals(name)))
                {
                    _apis.Add(new ResourceInfo() { 
                        Namespace = ns, 
                        Name = name,
                        Description = desc, 
                        ClsType = type,
                        Online=true,
                        Href=href,
                        InterfaceDescription= string.IsNullOrEmpty(desc)?null:new string[] { desc },
                        ResourceType= string.IsNullOrEmpty(resType) ? null : new string[] { resType },
                    });
                }
            }
            return this;
        }
        //TODO 资源上线
        internal void SetOnline(string ns,string name){

        }
        //TODO 资源下线
        internal void SetOffline(string ns,string name)
        {

        }

        /// <summary>
        /// 配置数据序列化组件,宿主程序需要实现一个序列化/反序列化功能，然后调用本方法注册
        /// </summary>
        /// <param name="ser"></param>
        internal void SetDataSerializer(ISerializer ser)
        {
            _dataserializer = ser;
        }
    }
}
