# Mozi.IoT.CoAP 

Mozi.IoT.CoAP为Iot.Client调试工具，使用命令行方式对CoAPClient进行调试。

## 命令调用方式
~~~shell
    用法：coap command url [options] [body]
         
      command 可选值：
        get
        post
        put
        delete

      url 格式
        coap://{host}[:{port}]/{path}[?{query}]

      options 请求选项参数如下：

        -dump                    值为文件路径，将编码好的数据包转储到文件，不发起请求
        -file                    需要上传的文件的路径
        -time                    阻塞式监听若干秒，参数值为整数，单位为秒
        -round                   重复发起请求的次数（最高100次），需-time参数进行配合

        -type                    消息类型,取值
                                    con   --Confirmable
                                    non   --NonConfirmable
                                    ack   --Acknowledgement
                                    rst   --Reset

        -token                   格式：0x0f0e
        -ifmatch                 
        -etag                    
        -ifnonematch             
        -extendedtokenlength     
        -locationpath            
        -contentformat           
        -maxage                  
        -accept                  
        -locationquery           
        -block2                  Block2设置，格式：Num/MoreFlag/Size
        -block1                  Block1设置，格式：Num/MoreFlag/Size
                                 Num:0~1045785,MoreFlag:[0|1],Size:[16|32|64|128|256|512|1024|2048]
        -size2                   
        -proxyuri                
        -proxyscheme             
        -size1  
        
        注：
            1.字符串变量值用""包裹
            2.整型变量值用，直接输入整数即可，如 -size 1024

        body 说明：
            1.0x开始的字符串被识别为HEX字符串并被转为字节流
            2.其它识别为普通字符串同时被编码成字节流，编码方式为UTF-8
            3.带空格的字符串请用""进行包裹" 

      示例：
         coap get coap://127.0.0.1:5683/core/time?type=1 -block2 0/0/128

~~~

## 命令调用图示
![][example1]

![][example2]

[example1]:./coap_202203100001.png
[example2]:./coap_202203100002.png
