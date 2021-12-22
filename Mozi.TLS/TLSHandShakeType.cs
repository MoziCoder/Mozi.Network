namespace Mozi.TLS
{
    //握手类型
    internal enum TLSHandShakeType : byte
    {
        HelloRequest = 0x00,
        ClientHello = 0x01,
        ServerHello = 0x02,
        NewSessionTicke = 0x04,
        Certificate = 0x0b,
        ServerKeyExchange = 0x0c,
        ServerHelloDone = 0x0e,
        CertificateVerify = 0x0f,
        ClientKeyExchange = 0x10,
        Finished = 0x20
    }
}
