using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Mozi.Encode.CBOR
{
    //TODO 进一步实现CBOR编码解码

    /// <summary>
    /// CBOR编码解码器
    /// </summary>
    /// <remarks>
    /// <list type="table">
    ///     <item>1,CBOR压缩空间效率比较高，但从编码复杂度看并不适宜作为嵌入式使用，更像是人为设计的高难度编码格式。</item>
    ///     <item>2,本解码器反对复杂数据类型嵌套</item>
    /// </list>
    /// </remarks>
    public class CBOREncoder
    {
        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static CBORData Decode(byte[] data)
        {
            CBORDataType cb = CBORDataType.Parse(data[0]);
            CBORData di = cb.Serializer.Parse(data);
            return di;
        }
        /// <summary>
        /// 编码数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encode(CBORData data)
        {
            return data.DataType.Serializer.Pack(data);
        }
        /// <summary>
        /// 从字符串解析数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <remarks>Parse方法还在测试中，暂时不能实用</remarks>
        public static CBORData Parse(string data)
        {
            Regex regUInt = new Regex("^(\\+)?\\d+$");
            Regex regNegInt = new Regex("^-\\d+$");
            Regex regTagItem=new Regex("\\d+\\(\\w+\\)") ;
            data = data.Trim();
            CBORData info=null;
            //unsigned integer
            ulong value;
            if (ulong.TryParse(data, out value))
            {
                info = new CBORData
                {
                    DataType = CBORDataType.UnsignedInteger,
                    Value = value
                };

            }//negative integer
            else if (regNegInt.IsMatch(data))
            {
                info = new CBORData
                {
                    DataType = CBORDataType.NegativeInteger,
                    Value = long.Parse(data)
                };
            }
            //hex array
            else if (data.StartsWith("h'")|data.StartsWith("(_ h")|data.StartsWith("(_h"))
            {
                info = new CBORData
                {
                    DataType = CBORDataType.StringArray
                };
                if (data.StartsWith("("))
                {
                    if (data.StartsWith("(_"))
                    {
                        info.IsIndefinite = true;
                    }

                    data = data.Trim(new char[] { '(', ')', '_'}).Trim();

                    string[] list = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<CBORData> items = new List<CBORData>();
                    foreach(var l in list)
                    {
                        items.Add(new CBORData(CBORDataType.StringArray, l.Trim(new char[] { 'h', '\'' })));
                    }
                    info.Value = items.ToArray();
                }
                else
                {
                    info.Value = data.Trim(new char[] { 'h', '\'' });
                }
            } 
            //string text
            else if (data[0] == '\"'|data.StartsWith("(_ \"") | data.StartsWith("(_\""))
            {
                info = new CBORData
                {
                    DataType = CBORDataType.StringText
                };
                if (data.StartsWith("("))
                {
                    if (data.StartsWith("(_"))
                    {
                        info.IsIndefinite = true;
                    }

                    data = data.Trim(new char[] { '(', ')', '_' }).Trim();

                    string[] list = data.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    List<CBORData> items = new List<CBORData>();
                    foreach (var l in list)
                    {
                        items.Add(new CBORData(CBORDataType.StringText, l.Trim(new char[] { '"' })));
                    }
                    info.Value = items;
                }
                else
                {
                    info.Value = data.Trim(new char[] { '"' });
                }
            }
            //array
            else if (data[0] == '[')
            {
                //TODO 进一步实现数组的解析
                //[_ "

                info = new CBORData { DataType = CBORDataType.DataArray };
            }
            //keypair
            else if (data[0] == '{')
            {
                //TODO 进一步实现键值对的解析
                //"a":"b"|1:"b"
                info = new CBORData { DataType = CBORDataType.KeyPair };
                if (data.StartsWith("{_"))
                {
                    data = data.Trim(new char[] { '{', '}', '_' }).Trim();
                }
                int offset = 0;
                Dictionary<CBORData, CBORData> list = new Dictionary<CBORData, CBORData>();
                while (offset < data.Length)
                {

                    CBORData key = new CBORData();
                    CBORData kv = new CBORData();
                    char indicator=data[offset];
                    if (indicator == '"')
                    {
                        string itemName=data.Substring(offset,data.IndexOf('"')-offset+1);
                    }
                    else if (indicator == '{') { 
                        
                    }
                    else
                    {
                       
                    }

                    //键名

                    //键值

                    //下一层级

                    list.Add(key, kv);
                }
                info.Value = list;
            }
            //tagitem
            else if (regTagItem.IsMatch(data))
            {
                byte indicator = byte.Parse(data.Substring(0, data.IndexOf("(")));
                string item = data.Substring(data.IndexOf("(") + 1).Trim(new char[] { ')' });
                
                info = new CBORData() { DataType = CBORDataType.TagItem };
                info.Indicator = indicator;
                info.Value = CBOREncoder.Parse(item);
            }
            //simplefloat
            else
            {
                info = new CBORData() { DataType = CBORDataType.SimpleFloat };
                bool bValue;
                string simpleText = "";
                if (data.StartsWith("simple"))
                {
                    simpleText = data.Substring(data.IndexOf("(") + 1).Trim(new char[] { ')' });
                }
                byte simpleValue;
                //null value
                if (data == null || data == "null")
                {
                    info.Value = null;
                }
                else if (data.StartsWith("simple") && byte.TryParse(simpleText, out simpleValue))
                {
                    info.Value = simpleValue;
                }//true false
                else if (bool.TryParse(data, out bValue))
                {
                    info.Value = bValue;
                }
                else if (data == "undefined")
                {
                    info.Value = Undefined.Value;
                }
                else if (data == "NaN")
                {
                    info.Value = float.NaN;
                }
                else if (data == "Infinity")
                {
                    info.Value = float.PositiveInfinity;
                }
                else if (data == "-Infinity")
                {
                    info.Value = float.NegativeInfinity;
                }
                else
                {
                    double dValue = double.Parse(data);
                    if (value <= HalfFloat.MaxValue && value >= HalfFloat.MinValue)
                    {
                        info.Value = new HalfFloat((float)dValue);
                    }
                    else if (value <= float.MaxValue && value >= float.MaxValue)
                    {
                        info.Value = (float)dValue;
                    }
                    else
                    {
                        info.Value = dValue;
                    }
                }
            }
            return info;
        }
    }
}
