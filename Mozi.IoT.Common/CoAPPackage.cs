using Mozi.IoT.Encode;
using Mozi.IoT.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mozi.IoT
{
    // Main Reference:RFC7252
    // Patial Reference:
    // RFC7959 分块传输
    // RFC8613 对象安全
    // RFC8974 扩展凭据和无状态客户端 

    // 内容采用UTF-8编码
    // 头部截断使用0xFF填充

    /// <summary>
    /// CoAP协议包
    /// </summary>
    public class CoAPPackage
    {
        private byte _version = 1;
        /// <summary>
        /// 版本 2bits 目前版本为1
        /// </summary>
        public byte Version { get => _version; set => _version = value; }
        /// <summary>
        /// 消息类型 2bits  
        /// </summary>
        public CoAPMessageType MessageType { get; set; }
        /// <summary>
        /// 
        /// RFC7252定义：
        /// 
        /// Token长度 4bits
        /// 0-8bytes取值范围
        /// 9-15为保留使用 
        /// 
        /// RFC8974定义
        /// 
        /// 13-指示TokenLength>8  Token=4+8bits  TKL-Ext=Token.Lenght-13
        /// 14-指示TokenLength>269 Token=4+16bits TKL-Ext=Token.Length-269
        /// 15-报错 
        /// 
        /// </summary>
        public byte TokenLength
        {
            get
            {
                return (byte)(Token == null ? 0 : Token.Length);
            }
            set
            {
                if (value == 0)
                {
                    Token = null;
                }
                else
                {
                    Token = new byte[value];
                }
            }
        }
        /// <summary>
        /// 8bits Lengths 9-15 reserved
        /// </summary>
        public CoAPCode Code { get; set; }
        /// <summary>
        /// 用于消息确认防重，消息确认-重置 16bits
        /// </summary>
        public ushort MesssageId { get; set; }
        /// <summary>
        /// 凭据
        ///  0-8bytes 典型应用场景需>=4bytes。本地和远程终结点不变的情况下可以使用同一Token,一般建议每请求重新生成Token
        /// </summary>
        public byte[] Token { get; set; }
        /// <summary>
        /// 选项 类似HTTP头属性
        /// </summary>
        public List<CoAPOption> Options = new List<CoAPOption>();
        /// <summary>
        /// 包体
        /// </summary>
        public byte[] Payload { get; set; }
        /// <summary>
        /// 域名，该域名为包内域名，无法判定真实性，有被篡改的可能性
        /// </summary>
        public string Domain
        {
            get
            {
                string domain = "";
                foreach (CoAPOption op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriHost)
                    {
                        domain = (string)new StringOptionValue() { Pack = op.Value.Pack }.Value;
                    }
                }
                return domain;
            }
        }
        /// <summary>
        /// 链接地址
        /// </summary>
        public string Path
        {
            get
            {
                string path = "";
                foreach(CoAPOption op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriPath)
                    {
                        path+="/"+ (string)new StringOptionValue() { Pack = op.Value.Pack }.Value;
                    }
                }
                return path;
            }
        }
        /// <summary>
        /// 查询字符串
        /// </summary>
        public string Query
        {
            get
            {
                List<string> query = new List<string>();
                foreach (CoAPOption op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriQuery)
                    {
                        query.Add((string)new StringOptionValue() { Pack = op.Value.Pack }.Value);
                    }
                }
                return string.Join("&",query);
            }
        }
        /// <summary>
        /// 查询参数集合
        /// </summary>
        public Dictionary<string,string> QueryCollection 
        { 
            get 
            {
                Dictionary<string, string> queries = new Dictionary<string, string>();
                foreach (CoAPOption op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriQuery)
                    {
                        string kp=(string)new StringOptionValue() { Pack = op.Value.Pack }.Value;
                        string[] kps = kp.Split(new char[] { '=' });
                        if (kps.Length == 2 && !queries.ContainsKey(kps[0]))
                        {
                            queries.Add(kps[0], kps[1]);
                        }
                    }
                }
                return queries;
            }
        }

        public CoAPPackageType PackageType
        {
            get;private set;
        }
        /// <summary>
        /// 打包|转为字节流
        /// </summary>
        /// <returns></returns>
        public byte[] Pack()
        {
            List<byte> data = new List<byte>();
            byte head = 0b00000000;
            head = (byte)(head | (Version << 6));
            head = (byte)(head | (MessageType.Value << 4));
            head = (byte)(head | TokenLength);

            data.Add(head);
            data.Add((byte)(((byte)Code.Category << 5) | ((byte)(Code.Detail << 3) >> 3)));
            data.AddRange(BitConverter.GetBytes(MesssageId).Revert());

            if (TokenLength > 0)
            {
                data.AddRange(Token);
            }

            uint delta = 0;
            foreach (CoAPOption op in Options)
            {
                op.DeltaValue = op.Option.OptionNumber - delta;
                data.AddRange(op.Pack);
                delta += op.DeltaValue;
            }
            //如果没有Payload就不加截断标识
            if (Payload != null)
            {
                data.Add(CoAPProtocol.HeaderEnd);
                data.AddRange(Payload);
            }
            return data.ToArray();
        }
        /// <summary>
        /// 设置空选项值
        /// </summary>
        /// <param name="define"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define)
        {
            return SetOption(define, new EmptyOptionValue());
        }
        /// <summary>
        /// 设置选项值
        /// </summary>
        /// <param name="opt"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOption opt)
        {
            int optGreater = Options.FindIndex(x => x.Option.OptionNumber > opt.Option.OptionNumber);
            if (optGreater < 0)
            {
                optGreater = Options.Count;
            }
            Options.Insert(optGreater, opt);
            return this;
        }
        /// <summary>
        /// 设置选项值，此方法可以设置自定义的选项值类型
        /// </summary>
        /// <param name="define"></param>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define, OptionValue optionValue)
        {
            CoAPOption option = new CoAPOption()
            {
                Option = define,
                Value = optionValue
            };
            return SetOption(option);
        }
        /// <summary>
        /// 设置选项值 字节流
        /// </summary>
        /// <param name="define"></param>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define, byte[] optionValue)
        {
            ArrayByteOptionValue v = new ArrayByteOptionValue() { Value = optionValue };
            SetOption(define, v);
            return this;
        }
        /// <summary>
        /// 设置选项值(uint(32))
        /// </summary>
        /// <param name="define"></param>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define, uint optionValue)
        {
            UnsignedIntegerOptionValue v = new UnsignedIntegerOptionValue() { Value = optionValue };
            return SetOption(define, v);
        }
        /// <summary>
        /// 设置字符串选项值
        /// </summary>
        /// <param name="define"></param>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define, string optionValue)
        {
            StringOptionValue v = new StringOptionValue() { Value = optionValue };
            return SetOption(define, v);
        }
        /// <summary>
        /// 设置Block1|Block2选项值，此处会作去重处理。设置非Block1|Block2会被忽略掉
        /// </summary>
        /// <param name="define"></param>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define, BlockOptionValue optionValue)
        {
            if (define == CoAPOptionDefine.Block1 || define == CoAPOptionDefine.Block2)
            {
                CoAPOption opt = Options.Find(x => x.Option == define);
                StringOptionValue v = new StringOptionValue() { Value = optionValue };
                if (opt == null)
                {
                    opt = new CoAPOption() { Option = define, Value = v };
                }
                else
                {
                    opt.Value = v;
                }

                return SetOption(define, v);
            }
            else
            {
                return this;
            }
        }
        /// <summary>
        /// 设置Block1
        /// </summary>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetBlock1(BlockOptionValue optionValue)
        {
           return SetOption(CoAPOptionDefine.Block1, optionValue);
        }
        /// <summary>
        /// 设置Block2
        /// </summary>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetBlock2(BlockOptionValue optionValue)
        {
            return SetOption(CoAPOptionDefine.Block2, optionValue);
        }
        /// <summary>
        /// 设置内容格式类型Content-Format,Http中的Content-Type
        /// </summary>
        /// <param name="ft"></param>
        /// <returns></returns>
        public CoAPPackage SetContentType(ContentFormat ft)
        {
            return SetOption(CoAPOptionDefine.ContentFormat, ft.Num);
        }
        //TODO 如果包集中到达，解析会出现问题
        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="packType"></param>
        /// <returns></returns>
        public static CoAPPackage Parse(byte[] data, CoAPPackageType packType)
        {
            CoAPPackage pack = new CoAPPackage();
            byte head = data[0];
            pack.Version = (byte)(head >> 6);
            pack.MessageType = AbsClassEnum.Get<CoAPMessageType>(((byte)(head << 2) >> 6).ToString());
            pack.TokenLength = (byte)((byte)(head << 4) >> 4);

            pack.Code = packType==CoAPPackageType.Request ? AbsClassEnum.Get<CoAPRequestMethod>(data[1].ToString()) : (CoAPCode)AbsClassEnum.Get<CoAPResponseCode>(data[1].ToString());
            if (pack.Code is null)
            {
                pack.Code = CoAPCode.Empty;
            }
            pack.PackageType = packType;
            byte[] arrMsgId = new byte[2], arrToken = new byte[pack.TokenLength];
            Array.Copy(data, 2, arrMsgId, 0, 2);
            Array.Copy(data, 2 + 2, arrToken, 0, arrToken.Length);
            pack.Token = arrToken;
            pack.MesssageId = BitConverter.ToUInt16(arrMsgId.Revert(), 0);
            //3+2+arrToken.Length+1开始是Option部分
            int bodySplitterPos = 2 + 2 + arrToken.Length;
            uint deltaSum = 0;

            while (bodySplitterPos < data.Length && data[bodySplitterPos] != CoAPProtocol.HeaderEnd)
            {

                CoAPOption option = new CoAPOption();
                option.OptionHead = data[bodySplitterPos];
                //byte len=(byte)(option.OptionHead)
                int lenDeltaExt = 0, lenLengthExt = 0;
                //if (option.Delta <= 12)
                //{

                //}
                //else 
                if (option.Delta == 13)
                {
                    lenDeltaExt = 1;
                }
                else if (option.Delta == 14)
                {
                    lenDeltaExt = 2;
                }
                if (lenDeltaExt > 0)
                {
                    byte[] arrDeltaExt = new byte[2];
                    Array.Copy(data, bodySplitterPos + 1, arrDeltaExt, arrDeltaExt.Length - lenDeltaExt, lenDeltaExt);
                    option.DeltaExtend = BitConverter.ToUInt16(arrDeltaExt.Revert(), 0);
                }
                //赋默认值
                option.Option = AbsClassEnum.Get<CoAPOptionDefine>((option.DeltaValue + deltaSum).ToString());
                if (option.Option is null)
                {
                    option.Option = CoAPOptionDefine.Unknown;
                }

                if (option.Length <= 12)
                {

                }
                else if (option.Length == 13)
                {
                    lenLengthExt = 1;
                }
                else if (option.Length == 14)
                {
                    lenLengthExt = 2;
                }
                if (lenLengthExt > 0)
                {
                    byte[] arrLengthExt = new byte[2];
                    Array.Copy(data, bodySplitterPos + 1 + lenDeltaExt, arrLengthExt, arrLengthExt.Length - lenLengthExt, lenLengthExt);
                    option.LengthExtend = BitConverter.ToUInt16(arrLengthExt.Revert(), 0);
                }

                option.Value.Pack = new byte[option.LengthValue];
                Array.Copy(data, bodySplitterPos + 1 + lenDeltaExt + lenLengthExt, option.Value.Pack, 0, option.Value.Length);

                pack.SetOption(option);
                deltaSum += option.Delta;
                //头长度+delta扩展长度+len
                bodySplitterPos += 1 + lenDeltaExt + lenLengthExt + option.Value.Length;

            }
            //有效荷载
            if (data.Length > bodySplitterPos && data[bodySplitterPos] == CoAPProtocol.HeaderEnd)
            {
                pack.Payload = new byte[data.Length - bodySplitterPos - 1];

                Array.Copy(data, bodySplitterPos + 1, pack.Payload, 0, pack.Payload.Length);
            }
            return pack;

        }
        /// <summary>
        /// 带参数实例化，最小参数量实例化
        /// </summary>
        /// <param name="method"></param>
        /// <param name="token"></param>
        /// <param name="msgId"></param>
        /// <param name="msgType"></param>
        public CoAPPackage(CoAPRequestMethod method, byte[] token, ushort msgId, CoAPMessageType msgType)
        {
            Code = method;
            Token = token;
            MesssageId = msgId;
            MessageType = msgType;
        }

        public CoAPPackage()
        {

        }
        /// <summary>

        /// <summary>
        /// 将URI信息配置到包中，即domain,port,paths,queries注入到"Options"中
        /// <list type="bullet">
        ///     <listheader>自动注入的Option</listheader>
        ///     <item><term><see cref="CoAPOptionDefine.UriHost"/></term>如果URL中的主机地址为域名，则注入此Option</item>
        ///     <item><term><see cref="CoAPOptionDefine.UriPort"/></term></item>
        ///     <item><term><see cref="CoAPOptionDefine.UriPath"/></term>以'/'分割Option</item>
        ///     <item><term><see cref="CoAPOptionDefine.UriQuery"/></term>以'&'分割Option</item>
        /// </list>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public CoAPPackage SetUri(UriInfo uri)
        {
            //注入域名
            if (!string.IsNullOrEmpty(uri.Domain))
            {
                SetOption(CoAPOptionDefine.UriHost, uri.Domain);
            }
            //注入端口号
            if (uri.Port > 0 && !(uri.Port == CoAPProtocol.Port || uri.Port == CoAPProtocol.SecurePort))
            {
                SetOption(CoAPOptionDefine.UriPort, (uint)uri.Port);
            }

            //注入路径
            for (int i = 0; i < uri.Paths.Length; i++)
            {
                SetOption(CoAPOptionDefine.UriPath, uri.Paths[i]);
            }
            //注入查询参数
            for (int i = 0; i < uri.Queries.Length; i++)
            {
                SetOption(CoAPOptionDefine.UriQuery, uri.Queries[i]);
            }
            return this;
        }
        /// <summary>
        /// 设置荷载
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public CoAPPackage SetPayload(string text)
        {
            Payload = StringEncoder.Encode(text);
            return this;
        }
        /// <summary>
        /// 转为HTTP样式的字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(CoAPPackageToStringType.HttpStyle);
        }
        /// <summary>
        /// 转为HTTP样式的字符串
        /// </summary>
        /// <param name="tp"></param>
        /// <returns></returns>
        public string ToString(CoAPPackageToStringType tp)
        {
            StringBuilder pack = new StringBuilder();
            bool isReq = false;
            
            List<KeyValuePair<string, string>> arrHeaders = new List<KeyValuePair<string, string>>();

            if (Code == CoAPRequestMethod.Get || Code == CoAPRequestMethod.Post || Code == CoAPRequestMethod.Put || Code == CoAPRequestMethod.Delete)
            {
                isReq = true;
            }
            else
            {

            }
            if (tp == CoAPPackageToStringType.HttpStyle)
            {
                //REQUEST
                if (isReq) {
                    pack.AppendLine(string.Format("{0} {1} COAP/{2}", Code.Name,  (!string.IsNullOrEmpty(Domain) ?( "coap://" + Domain):"")+Path+(!String.IsNullOrEmpty(Query)?("?"+Query):""), Version));
                }
                //RESPONSE
                else
                {
                    pack.AppendLine(string.Format("COAP/{0} {1} {2}", Version, string.Format("{0}{1:00}", Code.Category, Code.Detail), Code.Description));
                }

                arrHeaders.Add(new KeyValuePair<string, string>("Message-Id", MesssageId.ToString()));
                arrHeaders.Add(new KeyValuePair<string, string>("Message-Type", MessageType.Name));

                if (Token != null && Token.Length > 0)
                {
                    arrHeaders.Add(new KeyValuePair<string, string>("Token", "0x"+Hex.To(Token)));
                }

                foreach(var opt in Options)
                {
                    var sOptValue = "";
                    if (opt.Option == CoAPOptionDefine.Block1 || opt.Option == CoAPOptionDefine.Block2)
                    {
                        sOptValue = new BlockOptionValue() { Pack = opt.Value.Pack }.ToString();
                    }
                    else if (opt.Option == CoAPOptionDefine.ContentFormat)
                    {
                        var optValue = new UnsignedIntegerOptionValue() { Pack = opt.Value.Pack };
                        uint contentType = (uint)optValue.Value;
                        //int contentType = BitConvert.ToUint32(opt.Value.Pack);
                        ContentFormat ct =AbsClassEnum.Get<ContentFormat>(contentType.ToString());
                        sOptValue = ct.ContentType;
                    }else if (opt.Option == CoAPOptionDefine.Size1 || opt.Option == CoAPOptionDefine.Size2){
                        var optValue = new UnsignedIntegerOptionValue() { Pack = opt.Value.Pack };
                        sOptValue = optValue.ToString();
                    }
                    else if (opt.Option == CoAPOptionDefine.UriHost || opt.Option == CoAPOptionDefine.UriPath || opt.Option == CoAPOptionDefine.UriQuery || opt.Option == CoAPOptionDefine.LocationPath || opt.Option == CoAPOptionDefine.LocationQuery || opt.Option == CoAPOptionDefine.ProxyUri || opt.Option == CoAPOptionDefine.ProxyScheme)
                    {
                        var optValue = new StringOptionValue() { Pack = opt.Value.Pack };
                        sOptValue = optValue.ToString();
                    }
                    else
                    {
                        sOptValue = opt.Value.ToString();
                    }
                    arrHeaders.Add(new KeyValuePair<string, string>(opt.Option.Name, sOptValue));
                }

                foreach(var hd in arrHeaders)
                {
                    pack.AppendLine(string.Format("{0}: {1}", hd.Key, hd.Value));
                }
                pack.AppendLine();
                if (Payload != null)
                {
                    pack.AppendLine(StringEncoder.Decode(Payload));
                }
            //TODO HTTP mapping
            }else{

            }
            return pack.ToString();
        }
    }
    /// <summary>
    /// CoAP包转字符串的格式类型
    /// </summary>
    public enum CoAPPackageToStringType
    {
        HttpStyle,
        HttpMapping
    }
    /// <summary>
    /// 消息类型
    /// <list type="table">
    ///     <listheader>取值范围</listheader>
    ///     <item><term>0</term><see cref="Confirmable"/></item>
    ///     <item><term>1</term><see cref="NonConfirmable"/></item>
    ///     <item><term>2</term><see cref="Acknowledgement"/></item>
    ///     <item><term>3</term><see cref="Reset"/></item>
    /// </list>
    /// </summary>
    public class CoAPMessageType : AbsClassEnum
    {
        private string _name = "";
        private byte _typeValue;

        public static CoAPMessageType Confirmable = new CoAPMessageType("Confirmable", 0);
        public static CoAPMessageType NonConfirmable = new CoAPMessageType("NonConfirmable", 1);
        public static CoAPMessageType Acknowledgement = new CoAPMessageType("Acknowledgement", 2);
        public static CoAPMessageType Reset = new CoAPMessageType("Reset", 3);

        public byte Value
        {
            get
            {
                return _typeValue;
            }
        }

        public string Name
        {
            get { return _name; }
        }
        protected override string Tag
        {
            get
            {
                return _typeValue.ToString();
            }
        }

        internal CoAPMessageType(string name, byte typeValue)
        {
            _name = name;
            _typeValue = typeValue;
        }

        internal static object Get<T>(int v)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// 数据包类型，是请求还是响应
    /// </summary>
    public enum CoAPPackageType
    {
        Request,
        Response
    }
}
