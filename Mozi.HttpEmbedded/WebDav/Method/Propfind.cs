using System;
using System.Collections.Generic;
using System.Xml;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;
using Mozi.HttpEmbedded.WebDav.Exceptions;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>PROPFIND</c> WebDAV扩展方法
    /// </summary>
    internal class Propfind : MethodHandlerBase, IMethodHandler
    {
        private string _requestUri;
        private List<WebDavProperty> _reqProps;
        private List<IWebDavStoreItem> _webDavStoreItems;

        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="WebDAVServer" /> </param>
        /// <param name="context">
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="WebDAVServer" /></param>
        /// <exception cref="WebDavUnauthorizedException"></exception>
        public StatusCode Invoke(WebDAVServer server, HttpContext context, IWebDavStore store)
        {
            bool isPropname = false;
            int depth = GetDepthHeader(context.Request);
            _requestUri = context.Request.Path.ToString();

            try
            {
                _webDavStoreItems = GetWebDavStoreItems(WebDavExtensions.GetStoreItem(_requestUri, store), depth);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode.Unauthorized;
            }

            //获取请求文档
            XmlDocument requestDoc = GetXmlDocument(context.Request);

            //提取请求信息
            _reqProps = new List<WebDavProperty>();
            if (requestDoc.DocumentElement != null)
            {
                if (requestDoc.DocumentElement.LocalName != "propfind")
                    Log.Error("PROPFIND method without propfind in xml document");
                else
                {
                    XmlNode n = requestDoc.DocumentElement.FirstChild;
                    if (n == null)
                        Log.Error("propfind element without children");
                    else
                    {
                        switch (n.LocalName)
                        {
                            case "allprop":
                                _reqProps = GetAllProperties();
                                break;
                            case "propname":
                                isPropname = true;
                                _reqProps = GetAllProperties();
                                break;
                            case "prop":
                                foreach (XmlNode child in n.ChildNodes)
                                    _reqProps.Add(new WebDavProperty(child.LocalName, "", child.NamespaceURI));
                                break;
                            default:
                                _reqProps.Add(new WebDavProperty(n.LocalName, "", n.NamespaceURI));
                                break;
                        }
                    }
                }
            }
            else
                _reqProps = GetAllProperties();
            XmlDocument responseDoc = CreateResponseDocument(context, isPropname, StatusCode.Success);


            //转换成字节码
            byte[] responseBytes = StringEncoder.Encode(responseDoc.InnerXml);
            context.Response.Write(responseBytes);
            context.Response.AddHeader(HeaderProperty.ContentType.PropertyName, "text/xml");

            return StatusCode.MultiStatus;
        }

        private static List<IWebDavStoreItem> GetWebDavStoreItems(IWebDavStoreItem iWebDavStoreItem, int depth)
        {
            List<IWebDavStoreItem> list = new List<IWebDavStoreItem>();

            IWebDavStoreCollection collection = iWebDavStoreItem as IWebDavStoreCollection;
            if (collection != null)
            {
                list.Add(collection);
                if (depth == 0)
                    return list;
                foreach (IWebDavStoreItem item in collection.Items)
                {
                    try
                    {
                        list.Add(item);
                    }
                    catch (Exception)
                    {
                    }
                }
                return list;
            }
            if (!(iWebDavStoreItem is IWebDavStoreDocument))
                throw new WebDavConflictException();

            list.Add(iWebDavStoreItem);

            return list;
        }

        private XmlDocument GetXmlDocument(HttpRequest request)
        {
            try
            {

                string requestBody = StringEncoder.Decode(request.Body);

                if (!string.IsNullOrEmpty(requestBody))
                {
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(requestBody);
                    return xmlDocument;
                }
            }
            catch (Exception)
            {
                Log.Warn("XmlDocument has not been read correctly");
            }

            return new XmlDocument();
        }

        private List<WebDavProperty> GetAllProperties()
        {
            List<WebDavProperty> list = new List<WebDavProperty>
            {
                new WebDavProperty("creationdate"),
                new WebDavProperty("displayname"),
                new WebDavProperty("getcontentlength"),
                new WebDavProperty("getcontenttype"),
                new WebDavProperty("getetag"),
                new WebDavProperty("getlastmodified"),
                new WebDavProperty("resourcetype"),
                new WebDavProperty("supportedlock"),
                new WebDavProperty("ishidden")
            };
            //list.Add(new WebDAVProperty("getcontentlanguage"));
            //list.Add(new WebDAVProperty("lockdiscovery"));
            return list;
        }

        private XmlDocument CreateResponseDocument(HttpContext context, bool propname, StatusCode status)
        {
            // 实例XML文档
            XmlDocument responseDoc = new XmlDocument();
            const string responseXml = "<?xml version=\"1.0\"?><D:multistatus xmlns:D=\"DAV:\"></D:multistatus>";
            responseDoc.LoadXml(responseXml);

            XmlNamespaceManager manager = new XmlNamespaceManager(responseDoc.NameTable);
            manager.AddNamespace("D", "DAV:");
            manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
            manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
            manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

            int count = 0;

            foreach (IWebDavStoreItem webDavStoreItem in _webDavStoreItems)
            {
                WebDavProperty responseProperty = new WebDavProperty("response", "");
                XmlElement responseElement = responseProperty.ToXmlElement(responseDoc);

                string result;
                //TODO 取路径 此处需要测试
                if (count == 0)
                {
                    result = _requestUri;
                }
                else
                {
                    result = _requestUri + "/" + webDavStoreItem.Name + "/";
                }
                WebDavProperty hrefProperty = new WebDavProperty("href", result);
                responseElement.AppendChild(hrefProperty.ToXmlElement(responseDoc));
                count++;

                WebDavProperty propstatProperty = new WebDavProperty("propstat", "");
                XmlElement propstatElement = propstatProperty.ToXmlElement(responseDoc);

                WebDavProperty propProperty = new WebDavProperty("prop", "");
                XmlElement propElement = propProperty.ToXmlElement(responseDoc);

                foreach (WebDavProperty davProperty in _reqProps)
                {
                    propElement.AppendChild(PropChildElement(davProperty, responseDoc, webDavStoreItem, propname));
                }

                propstatElement.AppendChild(propElement);


                //TODO 此处响应状态码 检查是否正确
                WebDavProperty statusProperty = new WebDavProperty("status", "HTTP/1.1 " + status.Code.ToString() + " " + status.Text.ToString());
                propstatElement.AppendChild(statusProperty.ToXmlElement(responseDoc));


                responseElement.AppendChild(propstatElement);


                responseDoc.DocumentElement.AppendChild(responseElement);
            }

            return responseDoc;
        }

        /// <summary>
        /// <see cref="XmlElement" /> 
        /// <see cref="WebDavProperty" />
        /// </summary>
        /// <param name="webDavProperty"><see cref="WebDavProperty" /></param>
        /// <param name="xmlDocument"><see cref="XmlDocument" /></param>
        /// <param name="iWebDavStoreItem"><see cref="IWebDavStoreItem" /></param>
        /// <param name="isPropname"></param>
        /// <returns>
        /// <see cref="XmlElement" /> of <see cref="WebDavProperty" /> containing a value or child elements
        /// </returns>
        private XmlElement PropChildElement(WebDavProperty webDavProperty, XmlDocument xmlDocument, IWebDavStoreItem iWebDavStoreItem, bool isPropname)
        {

            if (isPropname)
            {
                webDavProperty.Value = string.Empty;
                return webDavProperty.ToXmlElement(xmlDocument);
            }

            webDavProperty.Value = GetWebDavPropertyValue(iWebDavStoreItem, webDavProperty);
            XmlElement xmlElement = webDavProperty.ToXmlElement(xmlDocument);

            if (webDavProperty.Name != "resourcetype" || !iWebDavStoreItem.IsCollection)
                return xmlElement;

            WebDavProperty collectionProperty = new WebDavProperty("collection", "");
            xmlElement.AppendChild(collectionProperty.ToXmlElement(xmlDocument));
            return xmlElement;
        }

        private string GetWebDavPropertyValue(IWebDavStoreItem webDavStoreItem, WebDavProperty davProperty)
        {
            switch (davProperty.Name)
            {
                case "creationdate":
                    return webDavStoreItem.CreateDate.ToUniversalTime().ToString("s") + "Z";
                case "displayname":
                    return webDavStoreItem.Name;
                case "getcontentlanguage":
                    return string.Empty;
                case "getcontentlength":
                    return !webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).Size : "";
                case "getcontenttype":
                    return !webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).MimeType : "";
                case "getetag":
                    return !webDavStoreItem.IsCollection ? "" + ((IWebDavStoreDocument)webDavStoreItem).Etag : "";
                case "getlastmodified":
                    return webDavStoreItem.ModifyDate.ToUniversalTime().ToString("R");
                case "lockdiscovery":
                    return string.Empty;
                case "resourcetype":
                    return "";
                case "supportedlock":
                    return "";
                //webDavProperty.Value = "<D:lockentry><D:lockscope><D:shared/></D:lockscope><D:locktype><D:write/></D:locktype></D:lockentry>";
                case "ishidden":
                    return "" + webDavStoreItem.Hidden;
                default:
                    return string.Empty;
            }
        }
    }
}
