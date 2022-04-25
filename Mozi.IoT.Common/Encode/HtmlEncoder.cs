using System.Text.RegularExpressions;

namespace Mozi.IoT.Encode
{
    public class HtmlEncoder
    {
        /// <summary>
        /// HTML实体字符串转普通字符串
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EntityCodeToString(string data)
        {
            var regEntity = new Regex("&#\\d+;");
            MatchCollection matches = regEntity.Matches(data);
            foreach (var match in matches)
            {
                var matchstr = match.ToString().Replace("&#", "").Replace(";", "");
                var result = char.ConvertFromUtf32(int.Parse(matchstr));
                data = data.Replace(match.ToString(), result);
            }
            return data;
        }
    }
}
