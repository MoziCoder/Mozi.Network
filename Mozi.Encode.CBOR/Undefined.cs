namespace Mozi.Encode
{
    /// <summary>
    /// Undefined类型
    /// </summary>
    public class Undefined: object
    {
        private static Undefined _value;
        public static Undefined Value
        {
            get
            {
                return _value??(_value=new Undefined());
            }
        }
        public override string ToString()
        {
            return "undefined";
        }
    }
    /// <summary>
    /// 正无穷大
    /// </summary>
    public class Infinity : object
    {
        private static Infinity _value;
        public static Infinity Value
        {
            get
            {
                return _value ?? (_value = new Infinity());
            }
        }
        public override string ToString()
        {
            return "Infinity";
        }
    }
    /// <summary>
    /// 负无穷小
    /// </summary>
    public class NegInfinity : object
    {
        private static NegInfinity _value;
        public static NegInfinity Value
        {
            get
            {
                return _value ?? (_value = new NegInfinity());
            }
        }
        public override string ToString()
        {
            return "-Infinity";
        }
    }
}
