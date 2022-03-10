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
        -ifmatch                 
        -etag                    
        -ifnonematch             
        -extendedtokenlength     
        -locationpath            
        -contentformat           
        -maxage                  
        -accept                  
        -locationquery           
        -block2                  Block2大小，格式：Num/MoreFlag/Size
        -block1                  Block1,格式：Num/MoreFlag/Size
        -size2                   
        -proxyuri                
        -proxyscheme             
        -size1  
        
      body 说明：
         1：0x开始的字符串被识别为HEX字符串并被转为字节流
         2：其它识别为普通字符串同时被编码成字节流，编码方式为UTF-8

      示例：
         coap get coap://127.0.0.1:5683/core/time?type=1 -block1 0/0/128


~~~

## 命令调用图示
![][example1]

![][example2]

[example1]:./coap_202203100001.png
[example2]:./coap_202203100002.png
