﻿using System.Collections.Generic;

namespace Mozi.HttpEmbedded
{

    //TODO 实现全局调用委托
    public delegate bool Handler(HttpContext ctx);

    /// <summary>
    /// 全局对象
    /// </summary>
    /// <remarks>此处可实现匿名委托调用，控制反转等功能</remarks>
    internal class Global
    {

        //全局委托
        private readonly Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>(new Generic.StringCompareIgnoreCase());

        /// <summary>
        /// 注册委托
        /// </summary>
        /// <param name="name"></param>
        /// <param name="handler"></param>
        public void Register(string name, Handler handler)
        {
            if (_handlers.ContainsKey(name))
            {
                _handlers[name] = handler;
            }
            else
            {
                _handlers.Add(name, handler);
            }
        }

        /// <summary>
        /// 反注册委托
        /// </summary>
        /// <param name="name"></param>
        public void UnRegister(string name)
        {
            _handlers.Remove(name);
        }
        internal bool Exists(string name)
        {
            return _handlers.ContainsKey(name);
        }
        /// <summary>
        /// 查找
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal Handler Find(string name)
        {
            return _handlers.ContainsKey(name) ? _handlers[name] : null;
        }
        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ctx"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal bool Invoke(string name, HttpContext ctx)
        {
            Handler handler = Find(name);
            if (handler != null)
            {
                return handler.Invoke(ctx);
            }
            else
            {
                return false;
            }
        }
    }
}
