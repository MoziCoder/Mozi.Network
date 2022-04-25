using System.Collections;
using System;
using System.Collections.Generic;
using Mozi.IoT.Generic;
using System.Linq;

namespace Mozi.IoT.Encode
{

    //link-format item format 
    //
    //   <{link1-value}[{;link-param1}={param1-value}...{;link-paramN=paramN-value}],
    //   <{link2-value}[{;link-param1}={param1-value}...{;link-paramN=paramN-value}]
    //
    //link-attribute: 
    //
    //    "rel"      = relation-types 
    //    "anchor"   = DQUOTE URI-Reference DQUOTE
    //    "rev"      = relation-types 
    //    "hreflang" = Language-Tag 
    //    "media"    = MediaDesc / (DQUOTE MediaDesc DQUOTE   
    //    "title"    = {Title}
    //    "title*"   = ext-value 
    //    "type"     = (media-type / quoted-mt ) 
    //    "rt"       = relation-types
    //    "if"       = relation-types
    //    "sz"       = [0-9]+
    //
    //link-extension:
    //

    //    Link            = link-value-list
    //link-value-list = [link-value *[ "," link-value]]
    //link-value     = "<" URI-Reference ">" * ( ";" link-param )
    //link-param     = (( "rel" "=" relation-types )
    //               /  ( "anchor" "=" DQUOTE URI-Reference DQUOTE )
    //               /  ( "rev" "=" relation-types )
    //               /  ( "hreflang" "=" Language-Tag )
    //               /  ( "media" "=" (MediaDesc / (DQUOTE MediaDesc DQUOTE ) ) )
    //               /  ( "title" "=" quoted-string )
    //               /  ( "title*" "=" ext-value )
    //               /  ( "type" "=" (media-type / quoted-mt ) )
    //               /  ( "rt" "=" relation-types )
    //               /  ( "if" "=" relation-types )
    //               /  ( "sz" "=" cardinal )
    //               / (link-extension ) )
    //link-extension = (parmname["="(ptoken / quoted - string)] )

    //               / (ext-name-star "=" ext-value )
    //ext-name-star  = parmname "*" ; reserved for RFC-2231-profiled
    //                              ; extensions.Whitespace NOT
    //                              ; allowed in between.
    //ptoken         = 1* ptokenchar
    //ptokenchar     = "!" / "#" / "$" / "%" / "&" / "'" / "("
    //               / ")" / "*" / "+" / "-" / "." / "/" / DIGIT
    //               / ":" / "<" / "=" / ">" / "?" / "@" / ALPHA
    //               / "[" / "]" / "^" / "_" / "`" / "{" / "|"
    //               / "}" / "~"
    //media-type     = type-name "/" subtype-name
    //quoted-mt      = DQUOTE media-type DQUOTE
    //relation-types = relation-type
    //               / DQUOTE relation-type* ( 1* SP relation-type ) DQUOTE
    //relation-type  = reg-rel-type / ext-rel-type
    //reg-rel-type   = LOALPHA* (LOALPHA / DIGIT / "." / "-" )
    //ext-rel-type   = URI
    //cardinal = "0" / ( % x31 - 39 * DIGIT)
    //LOALPHA        = %x61-7A   ; a-z
    //quoted-string  = <defined in [RFC2616]>
    //URI            = <defined in [RFC3986]>
    //URI-Reference  = <defined in [RFC3986]>
    //type-name      = <defined in [RFC4288]>
    //subtype-name   = <defined in [RFC4288]>
    //MediaDesc      = <defined in [W3C.HTML.4.01]>
    //Language-Tag   = <defined in [RFC5646]>
    //ext-value      = <defined in [RFC5987]>
    //parmname       = <defined in [RFC5987]>


    /// <summary>
    /// Link-Format序列化
    /// </summary>
    public class LinkFormator
    {
        /// <summary>
        /// 解析link-format字符串
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static LinkInfoCollection Parse(string text)
        {
            LinkInfoCollection info = new LinkInfoCollection();
            string[] items = text.Split(new char[] { ',' });
            foreach(var term in items)
            {
                LinkInfo link = new LinkInfo();
                string[] atts = term.Split(new char[] { ';' });
                link.Href = atts[0].Trim(new char[] { '<', '>' });
                for(int i = 1; i < atts.Length; i++)
                {
                    string[] kp = atts[i].Split(new char[] { '=' });
                    string key = null, v = null;
                    if (kp.Length > 0)
                    {
                        key = kp[0];
                        if (kp.Length > 1)
                        {
                            v = kp[1];
                        }

                        switch (key)
                        {
                            case "rel":
                                {
                                    link.RelationType = v.Trim(new char[] { '"' }).Split(new char[] { ' ' });
                                }
                                break;
                            case "anchor":
                                {
                                    link.Anchor = v.Trim(new char[] { '"' });
                                }
                                break;
                            case "rev":
                                {
                                    link.Rev = v.Trim(new char[] { '"' }).Split(new char[] { ' ' });
                                }
                                break;
                            case "hreflang":
                                {
                                    link.HrefLang = v.Trim(new char[] { '"' });
                                }
                                break;
                            case "media":
                                {
                                    link.Media = v.Trim(new char[] { '"' });
                                }
                                break;
                            case "title":
                                {
                                    link.Title = v.Trim(new char[] { '"' });
                                }
                                break;
                            case "title*":
                                {
                                    link.Title2 = v.Trim(new char[] { '"' });
                                }
                                break;
                            case "type":
                                {
                                    link.Type = v.Trim(new char[] { '"' });
                                }
                                break;
                            case "rt":
                                {
                                    link.ResourceType = v.Trim(new char[] { '"' }).Split(new char[] { ' ' });
                                }
                                break;
                            case "if":
                                {
                                    link.InterfaceDescription = v.Trim(new char[] { '"' }).Split(new char[] { ' ' });
                                }
                                break;
                            case "sz":
                                {
                                    link.ResourceSize = long.Parse(v);
                                }
                                break;
                            case "ct":
                                {
                                    link.ContentType = AbsClassEnum.Get<ContentFormat>(v);
                                }
                                break;
                            default:
                                {
                                    link.Extensions.Add(new LinkAttribute() { AttributeName = key, AttributeValue = v });
                                }
                                break;
                        }

                        
                    }
                }
                info.Add(link);
            }
            return info;
        }
        /// <summary>
        /// link-format集合转为字符串
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string ToString(IEnumerable infos)
        {
            List<string> items = new List<string>();
            foreach (LinkInfo link in infos)
            {
                string item = $"<{link.Href}>";

                if (link.RelationType != null && link.RelationType.Length > 0)
                {
                    item += $";rel=\"{string.Join(" ", link.RelationType)}\"";
                }

                if (!string.IsNullOrEmpty(link.Anchor))
                {
                    item += $";anchor=\"{link.Anchor}\"";
                }

                if (link.Rev != null && link.Rev.Length > 0)
                {
                    item += $";rev=\"{string.Join(" ", link.Rev)}\"";
                }

                if (!string.IsNullOrEmpty(link.HrefLang))
                {
                    item += $";hreflang=\"{link.HrefLang}\"";
                }

                if (!string.IsNullOrEmpty(link.Media))
                {
                    item += $";media=\"{link.Media}\"";
                }

                if (!string.IsNullOrEmpty(link.Title))
                {
                    item += $";title=\"{link.Title}\"";
                }

                if (!string.IsNullOrEmpty(link.Title2))
                {
                    item += $";title*=\"{link.Title2}\"";
                }

                if (!string.IsNullOrEmpty(link.Type))
                {
                    item += $";type=\"{link.Type}\"";
                }

                if (link.ResourceType != null && link.ResourceType.Length > 0)
                {
                    item += $";rt=\"{string.Join(" ", link.ResourceType)}\"";
                }

                if (link.InterfaceDescription != null && link.InterfaceDescription.Length > 0)
                {
                    item += $";if=\"{string.Join(" ", link.InterfaceDescription)}\"";
                }

                if (link.ResourceSize > 0)
                {
                    item += $";sz={link.ResourceSize}";
                }

                if (!(link.ContentType is null))
                {
                    item += $";ct={link.ContentType.Num}";
                }

                foreach (var att in link.Extensions)
                {
                    var key = att.AttributeName;
                    var v = att.AttributeValue;
                    if (v != null)
                    {
                        item += ";key=";
                        if (v is string)
                        {
                            item += $"\"{v}\"";
                        }
                        else
                        {
                            item += v.ToString();
                        }
                    }
                }
                items.Add(item);
            }

            return string.Join(",", items);
        }
    }

    //"rel"     
    //"anchor"  
    //"rev"     
    //"hreflang"
    //"media"   
    //"title"   
    //"title*"  
    //"type"    
    //"rt"      
    //"if"      
    //"sz"      

    /// <summary>
    /// link-format链接描述信息
    /// </summary>
    public class LinkInfo
    {
        public string Href { get; set; }
        /// <summary>
        /// 资源类型 rt
        /// </summary>
        public string[] ResourceType { get; set; }
        /// <summary>
        /// 内容MIME类型 ct
        /// </summary>
        public ContentFormat ContentType { get; set; }
        /// <summary>
        /// 资源大小 sz
        /// </summary>
        public long ResourceSize { get; set; }
        /// <summary>
        /// 描述 if
        /// </summary>
        public string[] InterfaceDescription { get; set; }
        /// <summary>
        /// rel
        /// </summary>
        public string[] RelationType { get; set; }
        /// <summary>
        /// rev
        /// </summary>
        public string[] Rev { get; set; }
        /// <summary>
        /// anchor
        /// </summary>
        public string Anchor { get; set; }
        /// <summary>
        /// hreflang
        /// </summary>
        public string HrefLang { get; set; }
        /// <summary>
        /// media
        /// </summary>
        public string Media { get; set; }
        /// <summary>
        /// title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// title*
        /// </summary>
        public string Title2 { get; set; }
        /// <summary>
        /// type
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 扩展属性值
        /// </summary>
        public List<LinkAttribute> Extensions = new List<LinkAttribute>();
    }
    /// <summary>
    /// link-format attribute
    /// </summary>
    public class LinkAttribute
    {
        public string AttributeName { get; set; }
        public object AttributeValue { get; set; }
    }
    /// <summary>
    /// Link-Format查询过滤
    /// </summary>
    public interface ILinkInfoFilter
    {
        List<LinkInfo> Find(string pattern);
    }

    /// <summary>
    /// LinkInfo集合
    /// </summary>
    public class LinkInfoCollection : IEnumerable,ILinkInfoFilter
    {
        private readonly List<LinkInfo> _items = new List<LinkInfo>();

        public LinkInfo this[string name] { get { return GetItem(name); } }
        public LinkInfo this[int ind] { get { return _items[ind]; } }

        /// <summary>
        /// LinkInfo集合
        /// </summary>
        public List<LinkInfo> Items { get { return _items; } }
        /// <summary>
        /// 集合计数
        /// </summary>
        public int Count { get { return _items.Count; } }

        public IEnumerator GetEnumerator()
        {
            return new LinkInfoCollectionEnumerator(_items);
        }
        /// <summary>
        /// 获取项
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public LinkInfo GetItem(string name)
        {
            return _items.Find(x => x.Href.Equals(name));
        }
        /// <summary>
        /// 增加项
        /// </summary>
        /// <param name="f"></param>
        public void Add(LinkInfo item)
        {
            _items.Add(item);
        }
        /// <summary>
        /// 批量增加项目
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<LinkInfo> items)
        {
            _items.AddRange(items);
        }
        /// <summary>
        /// 清空集合
        /// </summary>
        public void Clear()
        {
            _items.Clear();
        }
        /// <summary>
        /// 移除项
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(LinkInfo item)
        {
            return _items.Remove(item);
        }
        /// <summary>
        /// 移除指定位置的项
        /// </summary>
        /// <param name="ind"></param>
        public void RemoveAt(int ind)
        {
            _items.RemoveAt(ind);
        }
        /// <summary>
        /// 获取项的索引位置
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(LinkInfo item)
        {
            return _items.IndexOf(item);
        }
        /// <summary>
        /// 查询过滤
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<LinkInfo> Find(string pattern)
        {
                string[] kp = pattern.Split(new char[] { '=' });
                if (kp.Length >= 2)
                {
                    var key = kp[0];
                    var v = kp[1];
                    switch (key)
                    {
                        case "rel":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => x.RelationType != null);
                                }else if (v.EndsWith("*")){
                                    return _items.FindAll(x => x.RelationType != null && x.RelationType.Any(b => !String.IsNullOrEmpty(b)&&b.StartsWith(v.TrimEnd(new char[] { '*' }))));
                                }
                                else
                                {
                                    return _items.FindAll(x => x.RelationType != null && x.RelationType.Any(b => b == v));
                                }
                            }
                        case "anchor":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => !string.IsNullOrEmpty(x.Anchor));
                                }
                                else if (v.EndsWith("*"))
                                {
                                    return _items.FindAll(x =>!string.IsNullOrEmpty(x.Anchor)&& x.Anchor.StartsWith(v.TrimEnd(new char[] { '*' })));
                                }
                                else
                                {
                                    return _items.FindAll(x => !string.IsNullOrEmpty(x.Anchor) && x.Anchor==v);
                                }
                            }
                        case "rev":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => x.Rev != null);
                                }
                                else if (v.EndsWith("*"))
                                {
                                    return _items.FindAll(x => x.Rev != null && x.Rev.Any(b => b.StartsWith(v.TrimEnd(new char[] { '*' }))));
                                }
                                else
                                {
                                    return _items.FindAll(x => x.Rev != null && x.Rev.Any(b => b == v));
                                }
                            }
                        case "hreflang":
                            {
                            if (v == "*")
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.HrefLang));
                            }
                            else if (v.EndsWith("*"))
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.HrefLang) && x.HrefLang.StartsWith(v.TrimEnd(new char[] { '*' })));
                            }
                            else
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.HrefLang) && x.HrefLang == v);
                            }
                        }
                        case "media":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => !string.IsNullOrEmpty(x.Media));
                                }
                                else if (v.EndsWith("*"))
                                {
                                    return _items.FindAll(x => !string.IsNullOrEmpty(x.Media) && x.Media.StartsWith(v.TrimEnd(new char[] { '*' })));
                                }
                                else
                                {
                                    return _items.FindAll(x => !string.IsNullOrEmpty(x.Media) && x.Media == v);
                                }
                        }
                        case "title":
                            {
                            if (v == "*")
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Title));
                            }
                            else if (v.EndsWith("*"))
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Title) && x.Title.StartsWith(v.TrimEnd(new char[] { '*' })));
                            }
                            else
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Title) && x.Title == v);
                            }
                        }
                        case "title*":
                            {
                            if (v == "*")
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Title2));
                            }
                            else if (v.EndsWith("*"))
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Title2) && x.Title2.StartsWith(v.TrimEnd(new char[] { '*' })));
                            }
                            else
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Title2) && x.Title2 == v);
                            }
                        }
                        case "type":
                            {
                            if (v == "*")
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Type));
                            }
                            else if (v.EndsWith("*"))
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Type) && x.Type.StartsWith(v.TrimEnd(new char[] { '*' })));
                            }
                            else
                            {
                                return _items.FindAll(x => !string.IsNullOrEmpty(x.Type) && x.Type == v);
                            }
                        }
                        case "rt":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => x.ResourceType != null);
                                }
                                else if (v.EndsWith("*"))
                                {
                                    return _items.FindAll(x => x.ResourceType != null && x.ResourceType.Any(b => !String.IsNullOrEmpty(b) && b.StartsWith(v.TrimEnd(new char[] { '*' }))));
                                }
                                else
                                {
                                    return _items.FindAll(x => x.ResourceType != null && x.ResourceType.Any(b => b == v));
                                }
                            }
                        case "if":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => x.InterfaceDescription != null);
                                }
                                else if (v.EndsWith("*"))
                                {
                                    return _items.FindAll(x => x.InterfaceDescription != null && x.InterfaceDescription.Any(b => !String.IsNullOrEmpty(b)&&b.StartsWith(v.TrimEnd(new char[] { '*' }))));
                                }
                                else
                                {
                                    return _items.FindAll(x => x.InterfaceDescription != null && x.InterfaceDescription.Any(b => b==v));
                                }
                            }
                        case "sz":
                            {
                                return null;
                            }
                        case "ct":
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => !(x.ContentType is null));
                                }
                                else
                                {
                                    return _items.FindAll(x => !(x.ContentType is null) && x.ContentType.Num==int.Parse(v));
                                }
                            }
                        default:
                            {
                                if (v == "*")
                                {
                                    return _items.FindAll(x => x.Extensions.Any(b => b.AttributeName == key));
                                }
                                else if (v.EndsWith("*"))
                                {
                                    return _items.FindAll(x => x.Extensions.Any(b => b.AttributeName == key&&(b.AttributeValue is string)&& string.IsNullOrEmpty(b.AttributeValue.ToString())&&b.AttributeValue.ToString().StartsWith(v.TrimEnd(new char[] { '*' }))));
                                }
                                else
                                {
                                return _items.FindAll(x => x.Extensions.Any(b => b.AttributeName == key && (b.AttributeValue is string) && string.IsNullOrEmpty(b.AttributeValue.ToString()) && b.AttributeValue.ToString()==v));
                            }
                            }
                    }
            }
            else
            {
                return null;
            }
        }
    }
    /// <summary>
    /// LinkInfo集合迭代器
    /// </summary>
    public class LinkInfoCollectionEnumerator : IEnumerator
    {
        private int _index;
        private List<LinkInfo> _collection;
        private LinkInfo value;
        public LinkInfoCollectionEnumerator(List<LinkInfo> colletion)
        {
            _collection = colletion;
            _index = -1;
        }
        object IEnumerator.Current
        {
            get { return value; }
        }

        public bool MoveNext()
        {
            _index++;
            if (_index >= _collection.Count)
            {
                return false;
            }
            else
            {
                value = _collection[_index];
            }
            return true;
        }
        public void Reset()
        {
            _index = -1;
        }
    }
}
