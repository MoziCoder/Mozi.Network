using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Mozi.HttpEmbedded.Template
{
    //TODO 2021/07/06 考虑将这个模板引擎单独剥离成一个项目
    /// <summary>
    /// 页面生成器
    /// </summary>
    public class PageEngine
    {

        private readonly Dictionary<string, object> _params = new Dictionary<string, object>(new StringCompareIgnoreCase());

        private readonly Dictionary<string, IEnumerable<object>> _datas = new Dictionary<string, IEnumerable<object>>(new StringCompareIgnoreCase());

        private string _template = "";

        private string _page = "";

        public PageEngine()
        {

        }

        internal PageEngine Load(string filePath)
        {
            return this;
        }

        internal PageEngine LoadFromStream(Stream stream)
        {
            return this;
        }
        /// <summary>
        /// 从文本载入模板
        /// </summary>
        /// <param name="template"></param>
        /// <returns></returns>
        public PageEngine LoadFromText(string template)
        {
            _template = template;
            return this;
        }
        public PageEngine Prepare()
        {
            _page = _template;
            ApplyAll();
            return this;
        }
        /// <summary>
        /// 应用所有规则
        /// </summary>
        /// <returns></returns>
        private PageEngine ApplyAll()
        {
            //首先解析语句

            //填充全局变量
            InflateGlobal();
            //填充变量
            InflateValues();
            //填充format指令执行结果
            InflateExpressionFormat();
            return this;
        }
        /// <summary>
        /// 注入全局数据
        /// </summary>
        /// <returns></returns>
        private PageEngine InflateGlobal()
        {
            return this;
        }
        /// <summary>
        /// 注入临时数据
        /// </summary>
        /// <returns></returns>
        private PageEngine InflateValues()
        {
            Regex regParam = new Regex("\\${[A-Za-z0-9_]+(\\.[A-Za-z0-9_]+)?}");
            MatchCollection matchesParam = regParam.Matches(_page);
            foreach (var m in matchesParam)
            {
                var param = m.ToString().Trim(new char[] { '$', '{', '}' });
                var paramValue = GetPatternValue(param);
                if (paramValue != null)
                {
                    _page = _page.Replace(m.ToString(), paramValue);
                }
            }
            return this;
        }
        /// <summary>
        /// 解析if
        /// </summary>
        /// <returns></returns>
        private PageEngine ParseStatementIf()
        {
            return this;
        }
        private PageEngine ParseStatementForeach()
        {
            return this;
        }
        private PageEngine ParseStatementSwitch()
        {
            return this;
        }
        private PageEngine ParseStatementFor()
        {
            return this;
        }
        private string GetPatternValue(string pattern)
        {
            string result =null;
            if (!pattern.Contains("."))
            {
                result=Get(pattern).ToString();
            }
            else
            {
                string[] target = pattern.Split(new char[] { '.' });
                object pValue = _params[target[0]];

                PropertyInfo props = pValue.GetType().GetProperty(target[1], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var targetValue = props.GetValue(pValue, null);
                result=targetValue.ToString();
            }
            return result;
        }
        /// <summary>
        /// 内置变量
        /// </summary>
        /// <returns></returns>
        private string InflateVariableSystem()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// $set表达式
        /// </summary>
        /// <returns></returns>
        private PageEngine InflateExpressionSet()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// $math表达式
        /// </summary>
        /// <returns></returns>
        private PageEngine InflateExpressionMath()
        {
            Regex regParam = new Regex("\\$math\\.\\d+\\(.*\\)");
            MatchCollection matchesParam = regParam.Matches(_page);
            foreach (var m in matchesParam)
            {
                var pattern = m.ToString().Trim(new char[] { '$', '{', '}' });
                var paramValue = GetPatternValue(pattern);
                if (paramValue != null)
                {
                    _page = _page.Replace(m.ToString(), paramValue);
                }
            }
            return this;
        }
        /// <summary>
        /// $format表达式
        /// </summary>
        /// <returns></returns>
        private PageEngine InflateExpressionFormat()
        {
            Regex regParam = new Regex("\\$format\\(.*\\)");
            MatchCollection matchesParam = regParam.Matches(_page);
            foreach (var m in matchesParam)
            {
                var pattern = m.ToString().Replace("$format", "").Trim(new char[] {  '(', ')' });
                var splitIndex = pattern.IndexOf("\",");
                var format = pattern.Substring(1, splitIndex-1);
                var paramexp = pattern.Substring(splitIndex + 2);
                var pms = paramexp.Split(new char[] { ',' });
                var pmvs = new object[pms.Length];
                for (int i = 0; i < pms.Length; i++)
                {
                    pmvs[i] = GetPatternValue(pms[i]);
                }
                _page = _page.Replace(m.ToString(),string.Format(format, pmvs));
            }
            return this;
        }
        /// <summary>
        /// IIF表达式
        /// </summary>
        /// <returns></returns>
        private PageEngine InflateExpressionIIF()
        {
            Regex regParam = new Regex("\\$iif\\(.*\\)");
            MatchCollection matchesParam = regParam.Matches(_page);
            foreach (var m in matchesParam)
            {
                var pattern = m.ToString().Replace("$iif", "").Trim(new char[] { '(', ')' });
                var splitIndex = pattern.IndexOf("\",");
                var format = pattern.Substring(1, splitIndex - 1);
                var paramexp = pattern.Substring(splitIndex + 2);
                var pms = paramexp.Split(new char[] { ',' });
                var pmvs = new object[pms.Length];
                for (int i = 0; i < pms.Length; i++)
                {
                    pmvs[i] = GetPatternValue(pms[i]);
                }
                _page = _page.Replace(m.ToString(), string.Format(format, pmvs));
            }
            return this;
        }
        /// <summary>
        /// == != <> &&　｜｜
        /// </summary>
        /// <returns></returns>
        private PageEngine ParseOperator()
        {
            throw new NotImplementedException();
        }

        private PageEngine ParseArithmeticOperator()
        {
            throw new NotImplementedException();
        }

        private PageEngine ParseLogicalOpeartor()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public object Get(string paramName)
        {
            if (_params.ContainsKey(paramName))
            {
                return _params[paramName];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// 注入参数
        /// </summary>
        /// <param name="paramName">参数名</param>
        /// <param name="paramValue">参数值</param>
        /// <returns></returns>
        public PageEngine Set(string paramName, object paramValue)
        {
            if (_params.ContainsKey(paramName))
            {
                _params[paramName] = paramValue;
            }
            else
            {
                _params.Add(paramName, paramValue);
            }
            return this;
        }
        /// <summary>
        /// 注入可枚举集合
        /// </summary>
        /// <param name="dataName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        internal PageEngine RegData(string dataName, IEnumerable<object> data)
        {
            if (_datas.ContainsKey(dataName))
            {
                _datas[dataName] = data;
            }
            else
            {
                _datas.Add(dataName, data);
            }
            return this;
        }
        /// <summary>
        /// 取出缓冲区数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBuffer()
        {
            return StringEncoder.Encode(_page);
        }
    }
}
