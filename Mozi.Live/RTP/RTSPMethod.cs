using Mozi.HttpEmbedded;

namespace Mozi.Live.RTP
{
    /// +---------------+-----------+--------+-------------+-------------+
    /// | method        | direction | object | Server req. | Client req. |
    /// +---------------+-----------+--------+-------------+-------------+
    /// | DESCRIBE      | C -> S    | P, S    | recommended | recommended |
    /// |               |           |        |             |             |
    /// | GET_PARAMETER | C -> S    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// | OPTIONS       | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// | PAUSE         | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// | PLAY          | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// | PLAY_NOTIFY   | S -> C    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// | REDIRECT      | S -> C    | P, S    | optional    | required    |
    /// |               |           |        |             |             |
    /// | SETUP         | C -> S    | S      | required    | required    |
    /// |               |           |        |             |             |
    /// | SET_PARAMETER | C -> S    | P, S    | required    | optional    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P, S    | optional    | optional    |
    /// |               |           |        |             |             |
    /// | TEARDOWN      | C -> S    | P, S    | required    | required    |
    /// |               |           |        |             |             |
    /// |               | S -> C    | P      | required    | required    |
    /// +---------------+-----------+--------+-------------+-------------+
    /// <summary>
    /// RTSP请求方法
    /// </summary>
    public class RTSPMethod
    {
        public static RequestMethod DESCRIBE        = new RequestMethod("DESCRIBE");
        public static RequestMethod GET_PARAMETER   = new RequestMethod("GET_PARAMETER");
        public static RequestMethod OPTIONS         = new RequestMethod("OPTIONS");
        public static RequestMethod PAUSE           = new RequestMethod("PAUSE");
        public static RequestMethod PLAY            = new RequestMethod("PLAY");
        public static RequestMethod PLAY_NOTIFY     = new RequestMethod("PLAY_NOTIFY");
        public static RequestMethod REDIRECT        = new RequestMethod("REDIRECT");
        public static RequestMethod SETUP           = new RequestMethod("SETUP");
        public static RequestMethod SET_PARAMETER   = new RequestMethod("SET_PARAMETER");
        public static RequestMethod TEARDOWN        = new RequestMethod("TEARDOWN");
    }
}
