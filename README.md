# Mozi.Network

Mozi.Network 是基于.Net 开发的网络应用协议基础组件套装。项目实现了几个常见的网络通讯协议。所有的模块在实现时均以应用场景为出发点进行开发，保证模块精巧，功能完备，调用简单，可扩展性强，且调用不破坏宿主程序的逻辑结构。

## 功能模块

### [HttpEmbedded][httpembedded]　　
	Http服务器

### [IoT][iot]　　
	IoT物联网标准通讯套件 网关，客户端，调试工具
#### [IoT.Server][iotserver]
	IoT服务端
#### [IoT.Client][iotclient]
	IoT客户端
#### [IoT.CoAP][iotcoap]
	IoT命令行调试工具
### [iot4j][iot4j]
    IoT(CoAP)的Java客户端实现
### [iot4c][iot4c]
	IoT(CoAP)的c/c++客户端实现(目前还在规划中)

### [SSDP][ssdp] 　　
	SSDP/UPNP实现

### [StateService][stateservice]　　

	自行设计的心跳服务组件，服务端,观察者及客户端

### [Telnet][telnet] 　　

	Telnet服务器及客户端实现

### [NTP][ntp]

	NTP授时服务器，目前仅有SNTP功能

## [规划与展望][roadmap]

	规划中的项目待总体设计完成后再进行开发，项目规划详情请查阅[Roadmap.md]。

## 项目地址

- [Gitee][gitee]

- [Github][github]

- [CSDN][codechina]

## 项目下载

	所有可用子项目均会发布到Nuget,并同步发布到Gitee发行版，同时提供可用的编译结果

## 版本迭代

	不定期对Mozi.Network的功能进行完善,解决各种BUG。应用中如果遇到无法解决的问题，请联系软件作者。如果期望作者在下一版本中加入某些协议的解析实现，请提交ISSUE。

## 版权说明

	整个工程采用MIT开源协议，子项目如无特殊说明则默认采用MIT协议，如有说明则请仔细查看证书及说明文件。欢迎复制，引用和修改。复制请注明出处，引用请附带证书。意见建议疑问请联系软件作者，或提交ISSUE。

## 联系
	QQ群：539784553	

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