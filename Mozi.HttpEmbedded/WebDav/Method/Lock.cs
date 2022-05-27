using System;
using System.Xml;
using Mozi.HttpEmbedded.Common;
using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.WebDav.Storage;

namespace Mozi.HttpEmbedded.WebDav.Method
{
    /// <summary>
    ///  <c>LOCK</c> WebDAV扩展方法
    /// </summary>
    internal class Lock : MethodHandlerBase, IMethodHandler
    {
        /// <summary>
        /// 响应请求
        /// </summary>
        /// <param name="server"><see cref="WebDAVServer" /> </param>
        /// <param name="context">
        /// <see cref="HttpContext" /> 
        ///  </param>
        /// <param name="store"><see cref="IWebDavStore" /> <see cref="WebDAVServer" /></param>
        /// <exception cref="WebDavPreconditionFailedException"></exception>
        public StatusCode Invoke(WebDAVServer server, HttpContext context, IWebDavStore store)
        {
            //头部信息
            int depth = GetDepthHeader(context.Request);
            string timeout = GetTimeoutHeader(context.Request);

            XmlNamespaceManager manager = null;
            XmlNode lockscopeNode = null, locktypeNode = null, ownerNode = null;

            //读取包体
            try
            {
                string requestBody = StringEncoder.Decode(context.Request.Body);

                if (!requestBody.Equals("") && requestBody.Length != 0)
                {
                    XmlDocument requestDocument = new XmlDocument();
                    requestDocument.LoadXml(requestBody);

                    if (requestDocument.DocumentElement != null && requestDocument.DocumentElement.LocalName != "prop" &&
                        requestDocument.DocumentElement.LocalName != "lockinfo")
                    {
                        Log.Debug("LOCK method without prop or lockinfo element in xml document");
                    }

                    manager = new XmlNamespaceManager(requestDocument.NameTable);
                    manager.AddNamespace("D", "DAV:");
                    manager.AddNamespace("Office", "schemas-microsoft-com:office:office");
                    manager.AddNamespace("Repl", "http://schemas.microsoft.com/repl/");
                    manager.AddNamespace("Z", "urn:schemas-microsoft-com:");

                    // Get the lockscope, locktype and owner as XmlNodes from the XML document
                    lockscopeNode = requestDocument.DocumentElement.SelectSingleNode("D:lockscope", manager);
                    locktypeNode = requestDocument.DocumentElement.SelectSingleNode("D:locktype", manager);
                    ownerNode = requestDocument.DocumentElement.SelectSingleNode("D:owner", manager);
                }
                else
                {
                    return StatusCode.PreconditionFailed;
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex.Message);
                throw;
            }

            /***************************************************************************************************
             * Lock the file or folder
             ***************************************************************************************************/

            bool isNew = false;

            //父目录资源清单
            IWebDavStoreCollection collection = GetParentCollection(store, context.Request.Path);

            try
            {
                //子目录资源清单
                IWebDavStoreItem item = GetItemFromCollection(collection, context.Request.Path);
            }
            catch (Exception)
            {
                UrlTree ut = new UrlTree(context.Request.Path);
                collection.CreateDocument(ut.Last().TrimEnd('/', '\\'));
                isNew = true;
            }



            /***************************************************************************************************
             * Create the body for the response
             ***************************************************************************************************/

            //实例化XML文档
            XmlDocument responseDoc = new XmlDocument();
            string responseXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><D:prop " + "xmlns:D=\"DAV:\"><D:lockdiscovery><D:activelock/></D:lockdiscovery></D:prop>";
            responseDoc.LoadXml(responseXml);

            //锁定节点
            XmlNode activelock = responseDoc.DocumentElement.SelectSingleNode("D:lockdiscovery/D:activelock", manager);

            //导入节点
            activelock.AppendChild(responseDoc.ImportNode(lockscopeNode, true));
            activelock.AppendChild(responseDoc.ImportNode(locktypeNode, true));
            activelock.AppendChild(responseDoc.ImportNode(ownerNode, true));

            // timeout头属性
            WebDavProperty timeoutProperty = new WebDavProperty("timeout", timeout);
            activelock.AppendChild(timeoutProperty.ToXmlElement(responseDoc));

            // depth头属性
            WebDavProperty depthProperty = new WebDavProperty("depth", depth == 0 ? "0" : "Infinity");
            activelock.AppendChild(depthProperty.ToXmlElement(responseDoc));

            // locktoken头属性
            WebDavProperty locktokenProperty = new WebDavProperty("locktoken", "");
            XmlElement locktokenElement = locktokenProperty.ToXmlElement(responseDoc);
            WebDavProperty hrefProperty = new WebDavProperty("href", "opaquelocktoken:e71d4fae-5dec-22df-fea5-00a0c93bd5eb1");
            locktokenElement.AppendChild(hrefProperty.ToXmlElement(responseDoc));
            activelock.AppendChild(locktokenElement);

            string resp = responseDoc.InnerXml;
            byte[] responseBytes = StringEncoder.Encode(resp);
            context.Response.AddHeader(HeaderProperty.ContentType.PropertyName, "text/xml");
            return isNew ? StatusCode.Created : StatusCode.Success;
        }
    }
}