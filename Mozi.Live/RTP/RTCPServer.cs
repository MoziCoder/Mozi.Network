namespace Mozi.Live.RTP
{
    class RTCPServer
    {

    }
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
    /// RTCP请求方法
    /// </summary>
    public class RTCPMethod
    {
        public static HttpEmbedded.RequestMethod DESCRIBE = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod GET_PARAMETER = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod OPTIONS = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod PAUSE = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod PLAY = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod PLAY_NOTIFY = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod REDIRECT = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod SETUP = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod SET_PARAMETER = new HttpEmbedded.RequestMethod("DESCRIBE");
        public static HttpEmbedded.RequestMethod TEARDOWN = new HttpEmbedded.RequestMethod("DESCRIBE");
    }
}
