# Mozi.Telnet简介

## 项目介绍

Mozi.Telnet是一个基于.Net开发的Telnet组件，基于UDP Socket开发，主要面向终端远程控制场景，为基于.Net开发的应用程序提供高度可控制的Telnet实现方案。

## 特点
1. 精巧
2. 高度可控

## 功能模块

- TelnetServer
    Telnet服务端

- TelnetClient
    Telnet客户端

## 项目地址

- [Gitee][gitee]

- [CSDN][codechina]

## 程序下载

~~~shell

	dotnet add package Mozi.Telnet --version 1.2.9

~~~
## 使用说明

~~~csharp

    /// <summary>
    /// Telnet调用范例
    /// </summary>
    static class Program
    {
        static void Main(string[] args)
        {
            TelnetServer ts = new TelnetServer();
            //设置用户
            ts.AddUser("admin", "admin");
            //调用命令注册
            ts.AddCommand<Shell>();
            //配置端口及启动服务
            ts.SetPort(23).Start();
            Console.ReadLine();
        }
    }

    /// <summary>
    /// 调用命令必须继承<see cref="ITelnetShellCommand"/>
    /// </summary>
    public class Shell : ITelnetShellCommand
    {
        public string Name => this.GetType().Name;

        public string Title => "系统指令调用入口";

        public string Descript()
        {
            return "";
        }

        public bool Invoke(ref string message,params string[] args)
        {
            return false;   
        }
    }

~~~

## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

### By [Jason][1] on Jun. 5,2021

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git