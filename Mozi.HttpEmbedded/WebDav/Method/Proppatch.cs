using System;
using System.IO;
using System.Linq;
using System.Xml;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>PROPPATCH</c> WebDAV扩展方法
    /// </summary>
    internal class Proppatch : MethodHandlerBase, IMethodHandler
    {

        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="WebDAVServer" /> </param>
        /// <param name="context"> 
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /><see cref="WebDAVServer" /></param>
        public StatusCode Invoke(WebDAVServer server, HttpContext context, IWebDavStore store)
        {

            string requestUri = context.Request.Path;

            XmlNamespaceManager manager = null;
            XmlNode propNode = null;

            //读取包体
            try
            {
                string requestBody = StringEncoder.Decode(context.Request.Body);

                if (!string.IsNullOrEmpty(requestBody))
                {
                    XmlDocument requestDocument = new XmlDocument();
                    requestDocument.LoadXml(requestBody);

                    if (requestDocument.DocumentElement != null)
                    {
                        if (requestDocument.DocumentElement.LocalName != "propertyupdate")
                        {
                            Log.Debug("PROPPATCH method without propertyupdate element in xml document");
                        }

                        manager = new XmlNamespaceManager(requestDocument.NameTable);
                        manager.AddNamespace("D", "DAV:");
                        manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
                        manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
                        manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

                        propNode = requestDocument.DocumentElement.SelectSingleNode("D:set/D:prop", manager);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
            }

            //父目录资源清单
            IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path);

            //子目录资源清单
            IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Path);

            FileInfo fileInfo = new FileInfo(item.ItemPath);

            if (propNode != null && fileInfo.Exists)
            {
                foreach (XmlNode node in propNode.ChildNodes)
                {
                    switch (node.LocalName)
                    {
                        case "Win32CreationTime":
                            fileInfo.CreationTime = Convert.ToDateTime(node.InnerText).ToUniversalTime();
                            break;
                        case "Win32LastAccessTime":
                            fileInfo.LastAccessTime = Convert.ToDateTime(node.InnerText).ToUniversalTime();
                            break;
                        case "Win32LastModifiedTime":
                            fileInfo.LastWriteTime = Convert.ToDateTime(node.InnerText).ToUniversalTime();
                            break;
                        case "Win32FileAttributes":
                            //fileInfo.Attributes = 
                            //fileInfo.Attributes = Convert.ToDateTime(node.InnerText);
                            break;
                    }
                }
            }


            // 实例XML文档
            XmlDocument responseDoc = new XmlDocument();
            const string responseXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:multistatus " +
                                       "xmlns:Z=\"urn:schemas-microsoft-com:\" xmlns:D=\"DAV:\">" +
                                       "<D:response></D:response></D:multistatus>";
            responseDoc.LoadXml(responseXml);

            //响应节点
            XmlNode responseNode = responseDoc.DocumentElement.SelectSingleNode("D:response", manager);

            WebDavProperty hrefProperty = new WebDavProperty("href", requestUri.ToString());
            responseNode.AppendChild(hrefProperty.ToXmlElement(responseDoc));

            WebDavProperty propstatProperty = new WebDavProperty("propstat", "");
            XmlElement propstatElement = propstatProperty.ToXmlElement(responseDoc);

            WebDavProperty statusProperty = new WebDavProperty("status", "HTTP/1.1 " + context.Response.Status.Code + " " + context.Response.Status.Text);
            propstatElement.AppendChild(statusProperty.ToXmlElement(responseDoc));


            foreach (WebDavProperty property in from XmlNode child in propNode.ChildNodes
                                                where child.Name.ToLower()
                                                    .Contains("creationtime") || child.Name.ToLower()
                                                        .Contains("fileattributes") || child.Name.ToLower()
                                                            .Contains("lastaccesstime") || child.Name.ToLower()
                                                                .Contains("lastmodifiedtime")
                                                let node = propNode.SelectSingleNode(child.Name, manager)
                                                select node != null
                                                    ? new WebDavProperty(child.LocalName, "", node.NamespaceURI)
                                                    : new WebDavProperty(child.LocalName, "", ""))
                propstatElement.AppendChild(property.ToXmlElement(responseDoc));

            responseNode.AppendChild(propstatElement);

            string resp = responseDoc.InnerXml;
            byte[] responseBytes = StringEncoder.Encode(resp);

            context.Response.AddHeader(HeaderProperty.ContentType.PropertyName, "text/xml");

            context.Response.Write(responseBytes);

            return StatusCode.MultiStatus;
        }
    }
}