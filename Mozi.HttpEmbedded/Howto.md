# Mozi.HttpEmbedded应用范例

## 接口封装
URL地址经路由解析器解析后，后转发给类中的方法进行处理。实践中只需要继承BaseApi并注册类库就可以了,注册方法为Router.Default.Register($"{dllpath}");。
下面的例子访问路径为 http://{ip}:{port}/Test/Hello
~~~csharp

	public class Test:BaseApi
    {
        public string Hello()
        {
            return "Hello world";
        }
    }

~~~

## 服务器实例化和初始化
~~~csharp

    HttpServer hs = new HttpServer();
    //配置端口并启动服务器
    hs.SetPort(9000).Start();

    //开启认证
    hs.UseAuth(AuthorizationType.Basic).SetUser("admin", "admin");

    //开启文件压缩
    hs.UseGzip(new Compress.CompressOption() { 
        MinContentLength=1024,
        CompressLevel=2
    });

    //开启静态文件支持
    hs.UseStaticFiles("");
	//配置虚拟目录 虚拟目录下的文件可以随意访问
	hs.SetVirtualDirectory("config", AppDomain.CurrentDomain.BaseDirectory + @"Config\");
	
	Router router = Router.Default;
	//注入API
    //1,此方法会扫描程序集内继承自BaseApi或属性标记为[BasicApi]的类
    //2,Http通讯数据标准默认为xml,使用Router.Default.SetDataSerializer(ISerializer ser)更改序列化类型
    router.Register($"{dllpath}");
	router.SetDataSerializer(new JSONSerializer());

    //路由映射
    router.Map("services/{controller}/{action}");

    Console.ReadLine();

	//路径信息
    //    首页
    //      @ip 服务器地址，本机调试使用127.0.0.1
    //      @port 服务器端口 
    //      http://{ip}:{port}
    //    静态文件
    //      @ip 服务器地址，本机调试使用127.0.0.1
    //      @port 服务器端口 
    //      @path 路径名，为静态目录的子级目录名
    //      @filename 文件名
    //      http://{ip}:{port}/{path}/{filename} 
    //    API
    //      @ip 服务器地址，本机调试使用127.0.0.1
    //      @port 服务器端口 
    //      @controller 注册的类名，暂不支持改名
    //      @action 方法名
    //      http://{ip}:{port}/{controller}/{action}

    //内置API 请参看Runtime.cs文件

~~~