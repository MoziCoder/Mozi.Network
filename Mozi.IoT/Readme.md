# Mozi.IoT物联网

Mozi.IoT是一个物联网标准通讯组件(CoAP协议)，包含网关服务器和客户端。该项目并不是一个应用框架,而是CoAP的标准通协议实现，项目目前是Mozi.Network的子项目，有可能发展成为一个独立的项目。精力有限，当前仅实现和充实CoAP相关功能。

## CoAP协议简介
Coap（Constrained Application Protocol）受限应用协议，是一种在物联网世界的类web协议，RFC规范定义为RFC7252,7959,8613,8974。该协议使用在资源受限的物联网设备上。物联网设备的ram，rom都通常非常小，不能运行消耗资源比较多的TCP和HTTP。

## 功能模块
### 网关服务器
	CoAPServer是CoAP协议的服务端,其主要职能是承载CoAP网关功能。

### 客户端
	CoAPClient是CoAP协议的客户端，鉴于.Net项目基本不用于嵌入式场景，此客户端实现可作为测试工具。

## CoAP协议实现完成度
	实现了RFC7252中约定的大部分功能，目前可以应对基本的CoAP协议调试。
	
## 总体规划

预期实现如下功能模块：

1. IoT服务端/网关 
2. .Net客户端调用类库
3. .Java客户端调用包
4. C/C++可移植客户端项目

## 待完成的功能
- 拥塞算法
- 安全认证
- 消息缓存
- 分块传输 RFC 7959
- 对象安全

## 版权说明
	本项目采用MIT开源协议，引用请注明出处。欢迎复制，引用和修改。意见建议疑问请联系软件作者，或提交ISSUE。

## 用例说明
~~~csharp

	//服务端
    CoAPServer cs = new CoAPServer();
    cs.Start();
    Console.ReadLine();

    //客户端
    CoAPClient cc = new CoAPClient();
    cc.SetPort(12341);
    cc.Start();
    cc.Get("coap://127.0.0.1/id/1");
    Console.ReadLine();

~~~
### By [Jason][1] on Dec. 5,2021

[1]:mailto:brotherqian@163.com