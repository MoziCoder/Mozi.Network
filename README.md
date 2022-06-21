# Mozi.Network

Mozi.Network 是基于.Net Socket开发的网络应用协议基础组件套装。项目实现了几个常见的网络通讯协议，开发重点集中在传输层和应用层。所有子项目均没有引用Windows下的特殊程序集，可作为跨平台工具的一部分，目前已在Linux进行可用性测试。


所有的模块在开发时均以应用场景为出发点进行开发，保证模块精巧，功能完备，调用简单，可扩展性强，对宿主程序的入侵性小。本项目基于最宽松的MIT开源，没有任何外部支援。由于精力有限，暂时没有丰富的应用示例和演示文档,但演示项目中都包含了使用的基本范式。如果对本项目感兴趣，请进QQ群进行讨论。

## 功能模块

### [HttpEmbedded][httpembedded]　　

Http服务器及HttpClient

### [IoT][iot]　　

IoT物联网组件核心

#### [IoT.Server][iotserver]

IoT服务端

#### [IoT.Client][iotclient]

IoT客户端

#### [IoT.CoAP][iotcoap]

CoAP协议命令行调试工具

#### [iot4j][iot4j]

IoT(CoAP)的Java客户端实现

#### [iot4c][iot4c]

IoT(CoAP)的c/c++客户端实现(目前还在规划中)

### [Live.RTSP][rtsp]
RTSP流媒体服务端及客户端

### [SSDP][ssdp] 　　

SSDP/UPNP实现

### [StateService][stateservice]

自行设计的心跳服务组件，服务端,观察者及客户端

### [Telnet][telnet] 　　

Telnet服务器及客户端实现

### [NTP][ntp]

NTP授时服务器，目前仅有SNTP功能

## [规划与展望][roadmap]

规划中的项目待总体设计完成后再进行开发，项目规划详情请查阅 [Roadmap.md][roadmap]。

## 项目地址

- [Gitee][gitee]

- [Github][github]

- [CSDN][codechina]

## 开发平台
为什么项目基于.NET4进行开发？因为要保证最大兼容性。.NET4是一个成熟的平台，大量的企业项目都是运行在.NET4这个平台上的，而且新的.NET6/.NET7项目完全可以调用.NET4开发的类库。

## 项目下载

所有可用子项目均会发布到Nuget,并同步发布到Gitee发行版，同时提供可用的编译结果。

## 版本迭代

不定期对Mozi.Network的功能进行完善,解决各种BUG。应用中如果遇到无法解决的问题，请联系软件作者。如果期望作者在下一版本中加入某些协议的解析实现，请提交ISSUE。

## 版权说明

整个工程采用最宽松的MIT开源协议，子项目如无特殊说明则默认采用**MIT**协议，如有说明则请仔细查看证书及说明文件。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

## 联系

QQ群：539784553	
[Blog][blog]

### By [Jason][1] on Feb. 5,2020

[1]:mailto:brotherqian@163.com
[gitee]:https://gitee.com/myui_admin/mozi.git
[github]:https://github.com/MoziCoder/Mozi.HttpEmbedded.git
[codechina]:https://codechina.csdn.net/mozi/mozi.httpembedded.git
[httpembedded]:./Mozi.HttpEmbedded
[ssdp]:./Mozi.SSDP
[stateservice]:./Mozi.StateService
[telnet]:./Mozi.Telnet
[ntp]:./Mozi.NTP
[iot]:./Mozi.IoT
[iot4j]:https://gitee.com/myui/mozi.iot4j
[iot4c]:https://gitee.com/myui/iot4c
[iotserver]:./Mozi.IoT.Server
[iotclient]:./Mozi.IoT.Client
[iotcoap]:./Mozi.IoT.CoAP
[roadmap]:./RoadMap.md
[rtsp]:./Mozi.Live
[blog]:https://blog.csdn.net/wangchixiao