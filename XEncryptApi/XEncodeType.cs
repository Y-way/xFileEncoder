namespace XEncryptAPI
{
    /// <summary>
    /// 加密服务时,源数据加密编码格式
    /// </summary>
    public enum XEncodeType
    {
        /// <summary>
        /// 源文件不作处理,只增加加密文件头部
        /// </summary>
        XNone,
        /// <summary>
        /// 源数据重新GZip压缩编码
        /// </summary>
        XGZip,
    };

}
