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
    }
}
