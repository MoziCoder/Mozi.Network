# Mozi.HttpEmbedded 嵌入式Web服务器

Mozi.HttpEmbedded是一个基于.Net构建的轻量型HTTP服务器，为.Net应用程序提供HTTP服务功能。项目实现了HTTP/1.1的大部分规范，但并不会实现非常丰富的大型Web服务器功能。项目基于.Net Framework 4.0开发,也可转换为.Net Core/.Net Standard项目,亦可作为.Net5.0项目的引用库。

## 特点

Mozi.HttpEmbedded在Socket之上使用异步单线程模型，构建了一个HTTP服务器，项目自行开发了Http协议包解析器，使HTTP协议解析和TCP通讯高度分离。该项目针对的是不能使用Kestrel/IIS的部署环境，比如中间件，服务程序等。

1. 嵌入式  
	本项目可作为.Net应用的内嵌Web服务器，亦可作为单独Web服务器
2. 轻量化  
	编译结果小，部署程序占用系统资源少
3. 可用性  
	开箱即用，配置少，可控性高。同时遵从.Net平台Web项目开发的典型规范。
4. 低耦合  
	不改变现有业务逻辑，无需对现有代码结构进行改造，即可实现现有功能面向HTTP提供服务器
5. 可控性  
	宿主程序对Web服务高度可控


## 典型应用场景

业务体系或功能已开发完成，期望在项目中加入一个Web服务器功能，同时不对现有代码进行大规模改动。

1. APP内嵌WEB服务  
	通过HTTP方式对应用终端应用程序内存数据观察，终端管理，文件上传下载，API调用
2. 简易WEB服务器  
	仅用很少的系统资源即可快速搭建WEB服务器

> 在经典的Asp.Net开发中，Web服务的部署高度依赖于IIS,.Net Core项目则可基于部署。基于KESTREL或IIS部署的WEB项目，都基于Asp.Net体系。

## 原理及功能

1. HTTP协议  
	实现HTTP/1.1

2. 通讯认证  
	实现基本认证(Basic)

3. Cookie管理  
	支持标准Cookie

4. HTTP请求方法  
	GET POST

5. 路由  
	实现了URL管理,参见Router模块

6. 引用与依赖关系  
	依赖于.Net Framework

7. 传输压缩  
	支持GZip，Deflate算法

8. 字符编码  
	字符编码使用UTF-8 

## 项目地址

- [Github][github]

- [Gitee][gitee]

- [CSDN][codechina]

## 程序下载

~~~shell

	dotnet add package Mozi.HttpEmbedded --version 1.3.8

~~~

## 项目依赖  

无

## 功能与版本迭代

	不定期对Mozi.HttpEmbedded的功能进行完善,解决各种BUG。HTTP标准功能繁多，需要一步步实现。

## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

## 用例说明

### 服务器初始化用例

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
### API实现用例

~~~csharp
    
    public class App:BaseApi
    {
        /// <summary>
        /// 获取应用程序信息
        /// </summary>
        /// <returns></returns>
        public AppInfo GetAppInfo()
        {
            var ass = Assembly.GetEntryAssembly();
            var assName = ass.GetName();
            return new AppInfo()
            {
                AppName = assName.Name,
                Version = assName.Version.ToString(),
                BinName = (new FileInfo((new Uri(ass.CodeBase)).LocalPath)).Name
            };
        }
    }

~~~
### By [Jason][1] on Feb. 5,2020

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[github]:https://github.com/MoziCoder/Mozi.Network.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git