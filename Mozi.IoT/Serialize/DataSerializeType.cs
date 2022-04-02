namespace Mozi.IoT.Serialize
{
    /// <summary>
    /// 接口数据序列化格式
    /// </summary>
    public enum DataSerializeType
    {
        Customize = -1,
        XML = 0,
        JSON = 1,
        LinkFormat=2,
        EXI = 3,
        CBOR =4 
    }
}
