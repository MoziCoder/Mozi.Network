using Mozi.HttpEmbedded;

namespace Mozi.Live.RTSP
{
    /// +---------------+-----------+--------+-------------+-------------+
    /// | method        | direction | object | Server req. | Client req. |
    /// +---------------+-----------+--------+-------------+-------------+
    /// | DESCRIBE      | C -> S    | P, S   | recommended | recommended |
    /// |               |           |        |             |             |
    /// | GET_PARAMETER | C -> S    | P, S   | optional    | optional    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S   | optional    | optional    |
    /// |               |           |        |             |             |
    /// | OPTIONS       | C -> S    | P, S   | required    | required    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S   | optional    | optional    |
    /// |               |           |        |             |             |
    /// | PAUSE         | C -> S    | P, S   | required    | required    |
    /// |               |           |        |             |             |
    /// | PLAY          | C -> S    | P, S   | required    | required    |
    /// |               |           |        |             |             |
    /// | PLAY_NOTIFY   | S -> C    | P, S   | required    | required    |
    /// |               |           |        |             |             |
    /// | REDIRECT      | S -> C    | P, S   | optional    | required    |
    /// |               |           |        |             |             |
    /// | SETUP         | C -> S    | S      | required    | required    |
    /// |               |           |        |             |             |
    /// | SET_PARAMETER | C -> S    | P, S   | required    | optional    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S   | optional    | optional    |
    /// |               |           |        |             |             |
    /// | TEARDOWN      | C -> S    | P, S   | required    | required    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P      | required    | required    |
    /// +---------------+-----------+--------+-------------+-------------+
    /// <summary>
    /// RTSP请求方法
    /// </summary>
    public class RTSPMethod
    {
        /// <summary>
        /// DESCRIBE
        /// </summary>
        public static RequestMethod DESCRIBE = new RequestMethod("DESCRIBE");
        /// <summary>
        /// GET_PARAMETER
        /// </summary>
        public static RequestMethod GET_PARAMETER = new RequestMethod("GET_PARAMETER");
        /// <summary>
        /// OPTIONS
        /// </summary>
        public static RequestMethod OPTIONS = new RequestMethod("OPTIONS");
        /// <summary>
        /// PAUSE
        /// </summary>
        public static RequestMethod PAUSE = new RequestMethod("PAUSE");
        /// <summary>
        /// PLAY
        /// </summary>
        public static RequestMethod PLAY = new RequestMethod("PLAY");
        /// <summary>
        /// PLAY_NOTIFY
        /// </summary>
        public static RequestMethod PLAY_NOTIFY = new RequestMethod("PLAY_NOTIFY");
        /// <summary>
        /// REDIRECT
        /// </summary>
        public static RequestMethod REDIRECT = new RequestMethod("REDIRECT");
        /// <summary>
        /// SETUP
        /// </summary>
        public static RequestMethod SETUP = new RequestMethod("SETUP");
        /// <summary>
        /// SET_PARAMETER
        /// </summary>
        public static RequestMethod SET_PARAMETER = new RequestMethod("SET_PARAMETER");
        /// <summary>
        /// TEARDOWN
        /// </summary>
        public static RequestMethod TEARDOWN = new RequestMethod("TEARDOWN");
    }
}
