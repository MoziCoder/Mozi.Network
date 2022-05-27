# 更新历史
[2022/05/05]
[新增]
验证了项目在Linux下运行的可行性
[变更]
HttpClient增加了PUT,DELETE方法和超时时间
HttpRequest中增加Content-Encoding，Content-Type的解析
HttpEmbedded中增加了Digest摘要认证，提高安全性
SSDPHost中改变事件的实现方式
IoT.CoAP中命令调用超时，使用SemaphoreSlim(信号量)代替Action.BegionInvoke

[2022/05/05]
[新增]
HttpEmbedded中新增断点续传
HttpEmbedded中增加API简易注册接口， 无需继承BaseApi即可实现接口
HttpEmbedded中增加Transfer-Encoding:chunked,分块传输

[2022/04/21]
[变更]
1，移除IoT.CoAP中的部分命令行参数，包括-locationquery -locationpath -maxage
2，IoT.CoAP中增加了 -round参数，用于重复发起请求
3，将iot4c项目独立出来
[2022/04/06]
[优化]
解决IoT.CoAP参数不能重复的问题，增加了-file -dump参数

[2022/03/31]
[新增]
IoT增加LinkFormat序列化器
CoAPServer增加数据序列化接口
HttpEmbedded增加40x报错页面

[优化]
CoAPServer数据接收的处理逻辑

[2022/03/09]
[新增]
IoT.Server中增加CoAPResource,ResourceManager用于管理后端资源API
IoT.OptionValue增加ToString方法
IoT.CoAPPackage增加转为HTTP样式字符串的功能
IoT.CoAP CoapClient客户端调试工具
[修正]
CoAPPackage的Path,Query,Domain取值错误

[2022/02/15]
[新增]
HttpEmbedded模块中增加HttpClient

[2022/01/05]
[新增]
发布iot4j项目

[修正]
修正CoAPPackage.Parse中MessageType解析错误的问题
优化UdpSocket的数据包接收逻辑

[2021/12/22] 
[新增]
发布IoT项目,CoAPServer,CoAPClient

[2021/12/15] 
[新增]
公开IoT项目

[2021/12/02] 
[优化]优化部分已知BUG
[新增]增加NTP授时服务器模块

[2021/11/17] 
[新增]Telnet服务器组件
[优化]HttpEmbedded组件中，对SocketServer传递参数进行丰富

[2021/07/13] 
[修复]
Mozi.StateService解决心跳包解析错误的问题
[新增]
Mozi.StateService增加订阅者功能
Mozi.StateService抛弃BeginInvoke调用方式
Mozi.SSDP抛弃BeginInvoke调用方式
Mozi.HttpEmbedded开始着手实现模板引擎

[2021/06/23] 
[优化]
HeartBeatService功能丰富
HttpResponse增加SetCookie方法
HttpResponse增加Parse方法
Runtime中增加Cache方法