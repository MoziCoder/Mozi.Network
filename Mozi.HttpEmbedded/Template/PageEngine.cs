using Mozi.HttpEmbedded.Encode;
using Mozi.HttpEmbedded.Generic;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Mozi.HttpEmbedded.Template
{
    //TODO 需要实现一个模板引擎或考虑通过Razor引擎提高通用性
    //2021/06/08 文档模板仅仅实现类建议参数注入功能
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
            Prepare();
            return this;
        }
        public PageEngine LoadFromText(string template)
        {
            _template = template;
            return this;
        }
        public PageEngine Prepare()
        {
            _page = _template;
            InflateGlobal();
            InflateValues();
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
                if (!param.Contains("."))
                {
                    _page = _page.Replace(m.ToString(), GetParameter(param).ToString());
                }
                else
                {
                    string[] target = param.Split(new char[] { '.' });
                    object pValue = _params[target[0]];

                    PropertyInfo props = pValue.GetType().GetProperty(target[1], BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    var targetValue = props.GetValue(pValue, null);
                    _page = _page.Replace(m.ToString(), targetValue.ToString());
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

        private PageEngine ParseExpressionMath()
        {
            return this;
        }
        private PageEngine ParseExpressionFormat()
        {
            return this;
        }
        /// <summary>
        /// == != <> &&　｜｜
        /// </summary>
        /// <returns></returns>
        private PageEngine ParseOperator()
        {
            return this;
        }

        private PageEngine ParseArithmeticOperator()
        {
            return this;
        }

        private PageEngine ParseLogicalOpeartor()
        {
            return this;
        }
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="paramName"></param>
        /// <returns></returns>
        public object GetParameter(string paramName)
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
        public PageEngine SetParameter(string paramName, object paramValue)
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
