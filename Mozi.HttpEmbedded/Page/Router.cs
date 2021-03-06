using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Mozi.HttpEmbedded.Attributes;
using Mozi.HttpEmbedded.Serialize;

namespace Mozi.HttpEmbedded.Page
{
    //TODO 增加API下载的功能，允许客户端提取所有API，同时加入鉴权机制

    /// <summary>
    /// 全局路由 单例模式
    /// </summary>
    /// <remarks>
    /// 实例化Router时会自动扫描此程序集内部的API
    /// </remarks>
    public sealed class Router
    {
        private static Router _r;

        private List<Assembly> _assemblies = new List<Assembly>();

        private readonly List<Type> _apis = new List<Type>();

        //数据序列化对象
        private ISerializer _dataserializer;

        /// <summary>
        /// 路由配置，内置2个默认
        /// </summary>
        private readonly List<RouteMapper> _mappers = new List<RouteMapper>() {

            new RouteMapper() { Pattern = "/{controller}/{action}" },
            new RouteMapper() { Pattern = "/{controller}.{action}" }

        };

        private Global _global = new Global();
        /// <summary>
        /// 默认实例
        /// </summary>
        public static Router Default
        {
            get { return _r ?? (_r = new Router()); }
        }

        private Router()
        {
            //提供一个默认数据序列化接口
            //载入内部接口API
            LoadInternalApi();
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
        /// <summary>
        /// 调起
        /// </summary>
        /// <param name="ctx"></param>
        internal object Invoke(HttpContext ctx)
        {
           
            string path = ctx.Request.Path;

            //确定路径映射关系
            AccessPoint ap = Match(path);
            string name = ctx.Request.Method.Name+"::"+ap.Action;

            if (_global.Exists(name))
            {
                return InvokeFromGlobal(ref ctx, ap);
            }
            else
            {
                return InvokeFromModule(ref ctx, ap);
            }

        }
        /// <summary>
        /// 简易调用
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="ap"></param>
        /// <returns></returns>
        internal object InvokeFromGlobal(ref HttpContext ctx,AccessPoint ap)
        {
            ApiHandler handler = _global.Find(ctx.Request.Method.Name + "::" + ap.Action);
            return handler.Invoke(ctx);
        }

        private object InvokeFromModule(ref HttpContext ctx, AccessPoint ap)
        {
            //TODO 此处路由有问题，需要改进
            //TODO 此处考虑加入域控制，否则会出现api重复
            Type cls = _apis.Find(x => x.Name.Equals(ap.Controller, StringComparison.OrdinalIgnoreCase));

            //TODO 将Method缓存
            MethodInfo method = cls.GetMethod(ap.Action, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);

            ParameterInfo[] pms = method.GetParameters();

            //开始装配参数
            object[] args = new object[pms.Length];

            for (int i = 0; i < pms.Length; i++)
            {
                var argname = pms[i].Name;
                if (ctx.Request.Query.ContainsKey(argname))
                {
                    args[i] = ctx.Request.Query[argname];
                }
                if (ctx.Request.Post.ContainsKey(argname))
                {
                    args[i] = ctx.Request.Post[argname];
                }
            }

            //实例化对象
            object instance = Activator.CreateInstance(cls);
            //注入上下文变量
            ((BaseApi)instance).Context = ctx;

            //调用方法
            object result = method.Invoke(instance, BindingFlags.IgnoreCase, null, args, CultureInfo.CurrentCulture);
            var ctTypes = method.GetCustomAttributes(typeof(ContentTypeAttribute), false);
            //方法自定义属性中的返回类型
            if (ctTypes.Length > 0)
            {
                ctx.Response.SetContentType(((ContentTypeAttribute)ctTypes[0]).ContentType);
            //数据序列化器中的返回类型
            }else if (_dataserializer!=null&& string.IsNullOrEmpty(_dataserializer.ContentType)){
                ctx.Response.SetContentType(_dataserializer.ContentType);
            }

            //对象置空
            instance = null;

            if (method.ReturnType != typeof(void))
            {
                if (_dataserializer != null)
                {

                    return _dataserializer.Encode(result);
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return null;
            }
        }

        internal AccessObject GetMethodInfo(HttpContext ctx)
        {
            string path = ctx.Request.Path;
            //确定路径映射关系
            AccessPoint ap = Match(path);

            //TODO 此处路由有问题，需要改进
            //TODO 此处考虑加入域控制
            Type cls = _apis.Find(x => x.Name.Equals(ap.Controller, StringComparison.OrdinalIgnoreCase));
            MethodInfo method = cls.GetMethod(ap.Action, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
            AccessObject target = new AccessObject
            {
                Target = cls,
                Method = method,
                Params = method != null ? method.GetParameters() : null
            };
            return target;
        }
        //TODO 加入域控制
        /// <summary>
        /// 载入模块
        /// <para>自动扫描程序集中的接口模块</para>
        /// </summary>
        /// <returns></returns>
        public Router Register(string filePath)
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
        public Router Register(Assembly ass)
        {
            LoadApiFromAssembly(ass);
            return this;
        }
        /// <summary>
        /// 单独注册某个接口模块
        /// </summary>
        /// <param name="type">参数需继承自<see cref="T:BaseApi"/>的类，或者类标记为<see cref="T:BasicApiAttribute"/>,其他类型无法注册</param>
        /// <returns></returns>
        public Router Register(Type type)
        {
            var attribute = type.GetCustomAttributes(typeof(BasicApiAttribute), false);
            if (type.IsSubclassOf(typeof(BaseApi)) || attribute.Length > 0)
            {
                _apis.Add(type);
            }
            return this;
        }

        /// <summary>
        /// 路由注入
        /// </summary>
        /// <param name="pattern">
        /// 匹配范式        
        /// <para>
        ///     范式:<code>api/{controller}/{action}</code> {controller}和{action}为固定参数       
        ///     <list type="table">
        ///         <item><term><c>{controller}</c></term><description>表示继承自<see cref="C:BaseApi"/>的类</description></item>
        ///         <item><term><c>{action}</c></term><description>类中的非静态方法名</description></item>
        ///     </list>        
        ///     <example>
        ///         范式1
        ///         api/{controller}/{action}
        ///         范式2
        ///         api/on{controller}/get{action}
        ///         范式3
        ///         api.{controller}.{action}
        ///         范式4
        ///         api.on{controller}.get{action}
        ///     </example>
        /// 
        /// </para>
        /// </param>
        /// <returns></returns>
        public Router Map(string pattern)
        {
            _mappers.Add(new RouteMapper() { Pattern = pattern });
            return this;
        }
        /// <summary>
        /// 匹配路由
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public AccessPoint Match(string path)
        {
            //TODO 此处要对接Global
            foreach (var mapper in _mappers)
            {
                if (mapper.Match(path))
                {
                    return mapper.Parse(path);
                }
            }
           
            if (_global.Exists(RequestMethod.GET.Name + "::" + path) || _global.Exists(RequestMethod.POST.Name + "::" + path)){
                return new AccessPoint() { Domain = "", Controller = "", Action = path };
            }
            return null;
        }

        /// <summary>
        /// 配置数据序列化组件,宿主程序需要实现一个序列化/反序列化功能，然后调用本方法注册
        /// </summary>
        /// <param name="ser"></param>
        /// <remarks>
        /// 设置序列化器后，返回的内容格式会跟随<see cref="ISerializer.ContentType"/>变化
        /// </remarks>
        public void SetDataSerializer(ISerializer ser)
        {
            _dataserializer = ser;
        }
        /// <summary>
        /// 注册GET API
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void Get(string name,ApiHandler handler)
        {
            RegisterGlobalMethod(name, RequestMethod.GET, handler);
        }
        /// <summary>
        /// 注册POST API
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void Post(string name,ApiHandler handler)
        {
            RegisterGlobalMethod(name, RequestMethod.POST, handler);
        }
        /// <summary>
        /// 注册POST API
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void Put(string name,ApiHandler handler)
        {
            RegisterGlobalMethod(name, RequestMethod.PUT, handler);
        }
        /// <summary>
        /// 注册POST API
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void Delete(string name,ApiHandler handler)
        {
            RegisterGlobalMethod(name, RequestMethod.DELETE, handler);
        }
        internal void RegisterGlobalMethod(string name, RequestMethod method,ApiHandler hadler)
        {
            if (name.IndexOf((char)ASCIICode.DIVIDE) != 0)
            {
                name = "/" + name;
            }
            _global.Register(method.Name+"::"+name, hadler);
        }

        internal void UnRegisterGlobalMethod(string name)
        {
            _global.UnRegister(name);
        }
        /// <summary>
        /// 返回所有的API类
        /// </summary>
        /// <returns></returns>
        internal List<Type> GetTypes()
        {
            return _apis;
        }

        /// <summary>
        /// 路由映射
        /// </summary>
        private class RouteMapper
        {
            private string _pattern = "";

            public string Pattern
            {
                get
                {
                    return _pattern;

                }
                set
                {
                    _pattern = value;
                    ApplyPattern(value);
                }
            }
            private string Prefix { get; set; }
            private string Suffix { get; set; }
            private string Link { get; set; }
            private string IdName { get; set; }
            private Regex Matcher { get; set; }

            public RouteMapper()
            {

            }
            /// <summary>
            /// 应用匹配字符串
            /// </summary>
            /// <param name="pattern"></param>
            private void ApplyPattern(string pattern)
            {
                //修正
                if (pattern.IndexOf("/", StringComparison.CurrentCulture) != 0)
                {
                    pattern = "/" + pattern;
                }
                //替换特殊字符
                int indCrl = pattern.IndexOf("{controller}", StringComparison.CurrentCulture);
                int indAction = pattern.IndexOf("{action}", StringComparison.CurrentCulture);

                Prefix = pattern.Substring(0, indCrl);
                Link = pattern.Substring(indCrl + 12, indAction - indCrl - 12);
                Suffix = "";
                Matcher = new Regex(string.Format("^{0}[a-zA-Z]\\w+{1}[a-zA-Z]\\w+{2}$", Regex.Escape(Prefix), Regex.Escape(Link), Regex.Escape(Suffix)), RegexOptions.IgnoreCase);

            }
            /// <summary>
            /// 判断是否匹配路由
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public bool Match(string path)
            {
                var m = Matcher.Match(path);
                return m != null && m.Success && m.Index == 0;
            }
            /// <summary>
            /// 解析入口点
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public AccessPoint Parse(string path)
            {
                AccessPoint ap = null;
                if (Match(path))
                {
                    ap = new AccessPoint();
                    int indCrl = Prefix.Length;
                    int indLink = path.IndexOf(Link, indCrl + 1, StringComparison.CurrentCulture);
                    ap.Domain = Prefix.Replace("/", ".");
                    ap.Controller = path.Substring(indCrl, indLink - indCrl);
                    ap.Action = path.Substring(indLink + Link.Length);
                }
                return ap;
            }
        }
        /// <summary>
        /// 访问域
        /// </summary>
        public class AccessPoint
        {
            /// <summary>
            /// 域
            /// </summary>
            public string Domain { get; set; }
            /// <summary>
            /// 控制器
            /// </summary>
            public string Controller { get; set; }
            /// <summary>
            /// 动作
            /// </summary>
            public string Action { get; set; }
        }
        /// <summary>
        /// 访问对象
        /// </summary>
        public class AccessObject
        {
            /// <summary>
            /// 目标类型
            /// </summary>
            public Type Target { get; set; }
            /// <summary>
            /// 方法
            /// </summary>
            public MethodInfo Method { get; set; }
            /// <summary>
            /// 参数
            /// </summary>
            public ParameterInfo[] Params { get; set; }
        }
    }
}
