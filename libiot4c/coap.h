
// 默认服务端口UDP
#define  Port  5683
// 默认DTLS服务端口
#define SecurePort  5684
// 默认服务端口TCP
#define TcpPort  5685
// 头部和内容分割数据
#define HeaderEnd  0xFF

/**
 * CoAP包 
 */
typedef struct CoAPPackage {

} coappackage;
/*
* 
*/
typedef struct CoAPOption {

} coapoption;

typedef struct CoAPOptionValue {

} coapoptionvalue;

CoAPPackage parse(byte[] * data) {

}

byte[] pack(CoAPPackage *pack){

}