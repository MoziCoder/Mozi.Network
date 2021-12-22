using System;
using System.Collections.Generic;
using System.Linq;
using Mozi.TLS.Extension;

namespace Mozi.TLS
{

    internal class TLSPackage
    {
        //1字节
        public byte ContentType { get; set; }
        //2字节 { major,minor }
        public ushort Version { get; set; }
        //2字节
        public ushort Length { get; set; }

        public byte[] PackageRawData { get; set; }

        public byte[] Pack()
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(ContentType);
            var arrVersion = BitConverter.GetBytes(Version);
            var arrLength = BitConverter.GetBytes((ushort)PackageRawData.Length);
            Array.Reverse(arrVersion);
            Array.Reverse(arrLength);
            bytes.AddRange(arrVersion);
            bytes.AddRange(arrLength);
            if (PackageRawData != null)
            {
                bytes.AddRange(PackageRawData);
            }
            return bytes.ToArray();
        }
        public static byte[] Pack(byte contentType, byte version, SessionPackage sp)
        {
            TLSPackage tp = new TLSPackage()
            {
                ContentType = contentType,
                Version = version,
                PackageRawData = sp.Pack()
            };
            return tp.Pack();
        }

        public static TLSPackage Parse(byte[] data)
        {
            TLSPackage tp = new TLSPackage();
            //ContentType
            tp.ContentType = data[0];
            //Version
            tp.Version = BitConverter.ToUInt16(data, 1);
            //Protocol Content Length
            var arrLength1 = new byte[2];
            Array.Copy(data, 3, arrLength1, 0, 2);
            Array.Reverse(arrLength1);
            tp.Length = BitConverter.ToUInt16(arrLength1, 0);
            //ShakeProtocol

            var blockContent = new byte[tp.Length];
            Array.Copy(data, 5, blockContent, 0, blockContent.Length);
            tp.PackageRawData = blockContent;
            return tp;
        }
        public TLSPackage()
        {

        }
    }

    internal abstract class SessionPackage
    {
        public abstract byte[] Pack();
        public abstract SessionPackage Parse(byte[] data);
        public byte[] PackageRawData { get; set; }
    }

    internal class HelloPackage : SessionPackage
    {
        public ShakePackage ShakeProtocol { get; set; }

        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    internal enum HandShakeType : byte
    {
        ClientHello = 1,
        ServerHello = 2,
        NewSessionTicket = 4,
        Certificate = 11,
        ServerKeyExchange = 12,
        ServerHelloDone = 14,
        ClientKeyExchange = 16
    }
    internal abstract class ShakePackage : SessionPackage
    {
        //1字节
        public byte HandShakeType { get; set; }
        //3字节 uint24
        public uint Length { get; set; }
        //2字节
        public ushort Version { get; set; }
        //32字节
        public RandomInfo Random { get; set; }
        //1字节
        public byte SessionIdLength { get; set; }
        public byte[] SessionId { get; set; }

    }
    internal class ClientHelloPackage : ShakePackage
    {
        //2字节
        public ushort CipherSuitesLength { get; set; }
        //每2个字节区分一个CipherSuite
        public List<CipherSuiteType> CipherSuites { get; set; }
        //1字节
        public byte CompressionMethodsLength { get; set; }
        //2字节一组
        public List<CompressMethodInfo> CompressionMethods { get; set; }
        //2字节
        public ushort ExtensionsLength { get; set; }
        public List<ExtensionInfo> Extensions { get; set; }

        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            ClientHelloPackage shake = new ClientHelloPackage();
            shake.HandShakeType = data[0];
            var arrLength2 = new byte[3];
            Array.Copy(data, 1, arrLength2, 0, 3);
            Array.Reverse(arrLength2);
            shake.Length = BitConverter.ToUInt16(arrLength2, 0);
            shake.Version = BitConverter.ToUInt16(data, 4);
            //Random
            var blockRandom = new byte[32];
            Array.Copy(data, 6, blockRandom, 0, 32);
            shake.Random = new RandomInfo();
            var random = shake.Random;

            var blockTime = new byte[4];
            var blockSead = new byte[28];
            Array.Copy(blockRandom, blockTime, 4);
            Array.Reverse(blockTime);
            Array.Copy(blockRandom, 0, blockSead, 0, 28);
            random.Timestamp = BitConverter.ToUInt32(blockTime, 0);
            random.Random = blockSead;

            //Seesion
            shake.SessionIdLength = data[38];
            if (shake.SessionIdLength > 0)
            {
                var arrSession = new byte[shake.SessionIdLength];
                Array.Copy(data, 39, arrSession, 0, shake.SessionIdLength);
            }
            //CipherSuites
            var arrLength3 = new byte[2];
            Array.Copy(data, 39 + shake.SessionIdLength, arrLength3, 0, 2);
            Array.Reverse(arrLength3);
            shake.CipherSuitesLength = BitConverter.ToUInt16(arrLength3, 0);
            var blockCipherSuites = new byte[shake.CipherSuitesLength];
            Array.Copy(data, 39 + shake.SessionIdLength + 2, blockCipherSuites, 0, shake.CipherSuitesLength);
            //解析
            shake.CipherSuites = new List<CipherSuiteType>();
            for (int i = 0; i < shake.CipherSuitesLength / 2; i++)
            {
                var arrCipher = new byte[2];
                Array.Copy(blockCipherSuites, i * 2, arrCipher, 0, 2);
                Array.Reverse(arrCipher);
                shake.CipherSuites.Add((CipherSuiteType)Enum.Parse(typeof(CipherSuiteType), BitConverter.ToUInt16(arrCipher, 0).ToString()));
            }
            //CompressionMethod
            shake.CompressionMethodsLength = data[39 + shake.SessionIdLength + 2 + shake.CipherSuitesLength];
            var blockCompressionMethod = new byte[shake.CompressionMethodsLength];
            Array.Copy(data, 39 + shake.SessionIdLength + 2 + shake.CipherSuitesLength, blockCompressionMethod, 0, shake.CompressionMethodsLength);
            shake.CompressionMethods = new List<CompressMethodInfo>();
            for (int i = 0; i < shake.CompressionMethodsLength; i++)
            {
                shake.CompressionMethods.Add(new CompressMethodInfo()
                {
                    Method = data[39 + shake.SessionIdLength + 2 + shake.CipherSuitesLength + 1 + i]
                });
            }
            //Extension
            var arrLenght4 = new byte[2];
            Array.Copy(data, 39 + shake.SessionIdLength + 2 + shake.CipherSuitesLength + shake.CompressionMethodsLength + 1, arrLenght4, 0, 2);
            Array.Reverse(arrLenght4);
            shake.ExtensionsLength = BitConverter.ToUInt16(arrLenght4, 0);
            if (shake.ExtensionsLength > 0)
            {
                shake.Extensions = new List<ExtensionInfo>();
                //忽略扩展
                shake.Extensions.Add(new ExtensionInfo()
                {
                    ExtensionLength = 1,
                    ExtensionType = 511
                });
            }
            return shake;
        }
    }
    internal class ServerHelloPackage : ShakePackage
    {
        public CipherSuiteType CipherSuite { get; set; }
        public CompressMethodInfo CompressMethod { get; set; }
        //2字节
        public ushort ExtensionsLength { get; set; }

        public List<ExtensionInfo> Extensions { get; set; }

        public override byte[] Pack()
        {
            List<byte> arrData = new List<byte>();
            //Random
            arrData.AddRange(DateTime.Now.ToTimestamp().ToBytes().Revert());
            //Sead
            arrData.AddRange(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x060, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x060, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 });
            //Session
            if (SessionId != null && SessionId.Length > 0)
            {
                arrData.AddRange(SessionId);
                arrData.Insert(0, (byte)SessionId.Length);
            }
            else
            {
                arrData.Insert(0, 0);
            }
            //Cipher Suite
            arrData.AddRange(BitConverter.GetBytes((ushort)CipherSuite).Revert());
            //Compress Method 
            arrData.Add(0);
            //Extension Info
            ushort lenExts = 0;
            if (Extensions != null && Extensions.Count > 0)
            {
                foreach (ExtensionInfo v in Extensions)
                {
                    arrData.AddRange(v.Pack());
                }
                lenExts = (ushort)(Extensions.Sum(x => x.ExtensionLength) + Extensions.Count * 4);
            }
            arrData.InsertRange(0, lenExts.ToBytes().Revert());

            //最后插入类型长度            
            arrData.InsertRange(0, TLSVersion.TLS10.Code.ToBytes().Revert());
            //uint24 3字节
            byte[] arrLen = BitConverter.GetBytes(arrData.Count).Revert();
            Array.Resize(ref arrLen, 3);
            arrData.InsertRange(0, arrLen);
            arrData.InsertRange(0, ((ushort)HandShakeType).ToBytes().Revert());
            return arrData.ToArray();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    internal class CertificatePackage : SessionPackage
    {
        public byte HandShakeType { get; set; }
        //3字节 uint24
        public uint Length { get; set; }
        //2字节
        public ushort Version { get; set; }
        //3字节 uint24
        public ulong CertificateLength { get; set; }
        //证书序列 每张证书都必须是 ASN.1Cert 结构
        public List<Certificate> Certificates { get; set; }

        public override byte[] Pack()
        {
            List<byte> arrData = new List<byte>();

            //证书

            //最后插入类型长度            
            arrData.InsertRange(0, TLSVersion.TLS10.Code.ToBytes().Revert());
            //uint24 3字节
            byte[] arrLen = BitConverter.GetBytes(arrData.Count).Revert();
            Array.Resize(ref arrLen, 3);
            arrData.InsertRange(0, arrLen);
            arrData.InsertRange(0, ((ushort)HandShakeType).ToBytes().Revert());
            return arrData.ToArray();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }

        public class Certificate
        {
            //3字节 uint24
            public ulong CertificateLength { get; set; }
            //1字节
            public byte version { get; set; }
            //9字节
            public byte[] serialnumber { get; set; }
            //9字节
            public byte[] signature { get; set; }
            public List<RDNSequence> issuer { get; set; }
            public Validity validity { get; set; }
            public List<RDNSequence> subject { get; set; }
        }

        public class RDNSequence
        {
            public ushort ItemType { get; set; }
            public ushort ItemLength { get; set; }
            //3字节
            public uint ItemId { get; set; }

            public string ItemContent { get; set; }
        }

        //证书有效期 格林威治时间UNIX TIMSTAMP
        public class Validity
        {
            public long notBefore { get; set; }
            public long notAfter { get; set; }
        }

        public class PublicKeyInfo
        {
            public ushort KeyType { get; set; }
            public ushort KeyLength { get; set; }
            public ushort Padding { get; set; }
            /**/
            //中间还有内容
            /**/
            public byte[] Key { get; set; }
        }
    }
    internal class ServerKeyExchangePackage : SessionPackage
    {
        public byte HandShakeType { get; set; }
        //uint24 3字节
        public ulong Length { get; set; }
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    internal class SeverHelloDonePackage : SessionPackage
    {
        public byte HandShakeType { get; set; }
        //uint24 3字节
        public ulong Length { get; set; }
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    internal class ApplicationData
    {

    }
    internal class ApplicationDataPackage : SessionPackage
    {
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    internal class EncryptedAlertPackage : SessionPackage
    {
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    internal class NewSessionPackage : SessionPackage
    {
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }

    internal class ChangeCipherSpecPackage : SessionPackage
    {
        public byte[] CipherMessage { get; set; }
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }

    internal class EncryptedHandShakePackage : SessionPackage
    {
        public override byte[] Pack()
        {
            throw new NotImplementedException();
        }

        public override SessionPackage Parse(byte[] data)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// TLS协议解析
    /// </summary>
    internal class TLSProtocol
    {

        public static object Parse(byte data)
        {
            object result = null;
            //循环提取消息
            return result;
        }
        /// <summary>
        /// 解析协议包
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static HelloPackage ParseClientHello(byte[] data)
        {
            TLSPackage tp = TLSPackage.Parse(data);
            HelloPackage proto = new HelloPackage();
            //ShakeProtocol
            proto.ShakeProtocol = new ClientHelloPackage();

            var blockContent = tp.PackageRawData;
            var shake = (ClientHelloPackage)proto.ShakeProtocol;
            proto.ShakeProtocol = (ClientHelloPackage)shake.Parse(blockContent);

            return proto;
        }

        public static TLSProtocol ParseClientKeyExchange(byte[] data)
        {
            throw new NotImplementedException();
        }

        public static TLSProtocol ParseApplicationData(byte[] data)
        {
            throw new NotImplementedException();
        }
        public static byte[] PackSessionTicket()
        {
            throw new NotImplementedException();
        }
        //ServerHello
        public static byte[] PackServerHello()
        {
            List<byte> bytes = new List<byte>();
            var contentType = (byte)TLSContentType.HANDSHAKE;
            var version = (byte)TLSVersion.TLS10.Code;
            //Server Hello
            ServerHelloPackage sh = new ServerHelloPackage();
            //Certificate
            CertificatePackage cert = new CertificatePackage();
            //Server Key Exchange
            ServerKeyExchangePackage sk = new ServerKeyExchangePackage();
            //Server Hello Done
            SeverHelloDonePackage sd = new SeverHelloDonePackage();

            bytes.AddRange(TLSPackage.Pack(contentType, version, sh));
            bytes.AddRange(TLSPackage.Pack(contentType, version, cert));
            bytes.AddRange(TLSPackage.Pack(contentType, version, sk));
            bytes.AddRange(TLSPackage.Pack(contentType, version, sd));

            return bytes.ToArray();
        }
    }
    internal enum TLSContentType : byte
    {
        CHANGE_CIPHER_SPEC = 20,
        ALERT = 21,
        HANDSHAKE = 22,
        APPLICATION_DATA = 23
    }

    //1字节
    internal class CompressMethodInfo
    {
        public byte Method { get; set; }
    }
    internal class ExtensionInfo
    {
        //2字节
        public ushort ExtensionType { get; set; }
        //2字节
        public ushort ExtensionLength { get; set; }

        public byte[] RawData { get; set; }

        public byte[] Pack()
        {
            List<byte> arrData = new List<byte>();
            arrData.AddRange(RawData);
            arrData.InsertRange(0, ((ushort)RawData.Length).ToBytes().Revert());
            arrData.InsertRange(0, ExtensionType.ToBytes().Revert());
            return arrData.ToArray();
        }
    }
    internal class RenegotiationInfo
    {
        public byte Renegotiation { get; set; }
    }
    /// <summary>
    /// 种子随机数 定长32字节
    /// </summary>
    internal class RandomInfo
    {
        //4字节
        public long Timestamp { get; set; }
        //28字节
        public byte[] Random { get; set; }
    }

    internal enum AlertLevel
    {
        warning = 1,
        fatal = 2
    }
    internal enum AlertDescription
    {
        close_notify = 0,
        unexpected_message = 10,
        bad_record_mac = 20,
        decryption_failed = 21,
        record_overflow = 22,
        decompression_failure = 30,
        handshake_failure = 40,
        bad_certificate = 42,
        unsupported_certificate = 43,
        certificate_revoked = 44,
        certificate_expired = 45,
        certificate_unknown = 46,
        illegal_parameter = 47,
        unknown_ca = 48,
        access_denied = 49,
        decode_error = 50,
        decrypt_error = 51,
        export_restriction = 60,
        protocol_version = 70,
        insufficient_security = 71,
        internal_error = 80,
        user_canceled = 90,
        no_renegotiation = 100
    }
    struct Alert
    {
        AlertLevel level;
        AlertDescription description;
    }

    //扩展类型
    enum ExtensionType
    {

    }
}
