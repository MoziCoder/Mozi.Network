using System;
using System.Collections.Generic;

namespace Mozi.IoT
{
    //Main Reference:RFC7252
    //Patial Reference:
    //RFC7959 分块传输
    //RFC8613 对象安全
    //RFC8974 扩展凭据和无状态客户端 

    //内容采用UTF-8编码
    //头部截断使用0xFF填充

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
        /// Token长度 4bits
        /// 0-8bytes取值范围
        /// 9-15为保留使用，收到此消息时直接消息报错
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
                foreach (var op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriHost)
                    {
                        domain = (string)(op.Value.Value);
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
                foreach(var op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriPath)
                    {
                        path+="/"+(string)(op.Value.Value);
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
                foreach (var op in Options)
                {
                    if (op.Option == CoAPOptionDefine.UriQuery)
                    {
                        query.Add((string)(op.Value.Value));
                    }
                }
                return string.Join("&",query);
            }
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
            data.AddRange(Token);
            uint delta = 0;
            foreach (var op in Options)
            {
                op.DeltaValue = op.Option.OptionNumber - delta;
                data.AddRange(op.Pack);
                delta += op.DeltaValue;
            }
            if (Payload != null)
            {
                data.Add(CoAPProtocol.HeaderEnd);
                data.AddRange(Payload);
            }
            return data.ToArray();
        }

        /// <summary>
        /// 转为HTTP包,ASCII字符串数据包
        /// </summary>
        /// <returns></returns>
        //public byte[] ToHttp()
        //{
        //List<string> data = new List<string>();
        //string head = string.Format("{0} ");
        //}

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
            var optGreater = Options.FindIndex(x => x.DeltaValue > option.DeltaValue);
            if (optGreater < 0)
            {
                optGreater = Options.Count;
            }
            Options.Insert(optGreater, option);
            return this;
        }
        /// <summary>
        /// 设置字节流选项值
        /// </summary>
        /// <param name="define"></param>
        /// <param name="optionValue"></param>
        /// <returns></returns>
        public CoAPPackage SetOption(CoAPOptionDefine define, byte[] optionValue)
        {
            CoAPOption option = new CoAPOption()
            {
                Option = define,
                Value = new ArrayByteOptionValue() { Value = optionValue }
            };
            Options.Add(option);
            return this;
        }
        /// <summary>
        /// 设置uint(32)选项值
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
                var opt = Options.Find(x => x.Option == define);
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
        /// <summary>
        /// 解析数据包
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isRequest"></param>
        /// <returns></returns>
        public static CoAPPackage Parse(byte[] data, bool isRequest)
        {
            CoAPPackage pack = new CoAPPackage();
            byte head = data[0];
            pack.Version = (byte)(head >> 6);
            pack.MessageType = AbsClassEnum.Get<CoAPMessageType>(((byte)(head << 2) >> 4).ToString());
            pack.TokenLength = (byte)((byte)(head << 4) >> 4);

            pack.Code = isRequest ? AbsClassEnum.Get<CoAPRequestMethod>(data[1].ToString()) : (CoAPCode)AbsClassEnum.Get<CoAPResponseCode>(data[1].ToString());

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
                if (option.Delta <= 12)
                {

                }
                else if (option.Delta == 13)
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
                if (ReferenceEquals(null, option.Option))
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
                pack.Options.Add(option);
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
}
