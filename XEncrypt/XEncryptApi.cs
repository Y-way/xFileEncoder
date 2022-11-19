using System;
using System.Runtime.InteropServices;
namespace XEncryptApi
{
    /// <summary>
    /// 加密/解密结果状态码
    /// </summary>
    public enum ResultCode
    {
        /// <summary>
        /// 正常
        /// </summary>
        Ok,
        /// <summary>
        /// 未知错误
        /// </summary>
        Unknown,
        /// <summary>
        /// 加密/解密服务未初始化
        /// </summary>
        UnInitialize,
        /// <summary>
        /// 无效的输入数据
        /// </summary>
        InvalidInputData,
        /// <summary>
        /// 输入数据长度不正确
        /// </summary>
        InvalidInputDataSize,
        /// <summary>
        /// 无效的解密器
        /// </summary>
        InvalidDecoder,
        /// <summary>
        /// 加密文件不是GZip压缩
        /// </summary>
        InvalidUnzip,
        /// <summary>
        /// 无效的加密器
        /// </summary>
        InvalidEncoder,
        /// <summary>
        /// 文件加密GZip压缩失败
        /// </summary>
        InvalidZip,
        /// <summary>
        /// 文件已加密
        /// </summary>
        EncryptedData,
        /// <summary>
        /// 内存不足
        /// </summary>
        OutMemory,
        /// <summary>
        /// 加密/解密上下文类型不匹配
        /// </summary>
        ContextTypeError,
        /// <summary>
        /// 不支持解密服务
        /// </summary>
        NotSupportDecrypt,
        /// <summary>
        /// 不支持加密服务
        /// </summary>
        NotSupportEncrypt,
    };
    /// <summary>
    /// 加密/解密服务器上下文类型
    /// </summary>
    public enum XContextType
    {
        /// <summary>
        /// 未知
        /// </summary>
        XUnknown,
        /// <summary>
        /// 加密服务上下文
        /// </summary>
        Ecrypt,
        /// <summary>
        /// 解密服务上下文
        /// </summary>
        Decrypt,
    };
    /// <summary>
    /// 明文加密类型
    /// </summary>
    public enum XEncodeType
    {
        /// <summary>
        /// 源文件不作处理,只增加加密文件头部
        /// </summary>
        XNone,
        /// <summary>
        /// 源文件GZip压缩
        /// </summary>
        XGZip,
    };

    public static class XEncryptService
    {
        private const string dllName = "XEncrypt.dll";
        /// <summary>
        /// 加密文件标记
        /// </summary>
        public static readonly uint kSignature = System.BitConverter.ToUInt32(new byte[4] { (byte)'@', (byte)'X', (byte)'F', (byte)'E' }, 0);


        public static void DebugLog(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
        /// <summary>
        /// 解密Scope
        /// </summary>
        public class DecryptScope : IDisposable
        {
            IntPtr _context;
            public DecryptScope()
            {
                DebugLog("DecryptScope Initialize");
                XEncryptService.Initialize();
            }

            public void Begin()
            {
                DebugLog("DecryptScope Begin");
                _context = XEncryptService.CreateXContext(XContextType.Decrypt);
            }

            public ResultCode DecryptData(byte[] rawdata, out byte[] data)
            {
                DebugLog("DecryptScope DecryptData");
                data = rawdata;
                unsafe
                {
                    fixed(byte* rawdataPtr = rawdata)
                    {
                        if(!XEncryptService.IsEncrypted(rawdataPtr, rawdata.LongLength))
                        {
                            return ResultCode.InvalidInputData;
                        }
                        IntPtr decodeData = IntPtr.Zero;
                        long decodeSize = 0;
                        ResultCode resultCode = XEncryptService.Decrypt(_context, rawdataPtr, rawdata.LongLength, out decodeData, out decodeSize);
                        DebugLog($"Decrypt data state({resultCode})");
                        if(resultCode == ResultCode.Ok)
                        {
                            data = new byte[decodeSize];
                            Marshal.Copy(decodeData, data, 0, (int)decodeSize);
                        }
                        return resultCode;
                    }
                }
            }

            public void End()
            {
                DebugLog("DecryptScope End");
                if(_context != null)
                {
                    XEncryptService.ReleaseXContext(_context);
                }
            }

            public void Dispose()
            {
                DebugLog("DecryptScope UnInitialize");
                XEncryptService.Deinitialize();
            }
        }
        /// <summary>
        /// 加密Scope
        /// </summary>
        public class EncryptScope : IDisposable
        {
            IntPtr _context;
            public EncryptScope()
            {
                DebugLog("EncryptScope Initialize");
                XEncryptService.Initialize();
            }

            public void Begin()
            {
                DebugLog("EncryptScope Begin");
                _context = XEncryptService.CreateXContext(XContextType.Ecrypt);
            }

            public ResultCode EncryptData(byte[] rawdata, out byte[] data, byte encryptSize = 16, XEncodeType type = XEncodeType.XNone)
            {
                DebugLog("EncryptScope EncryptData");
                data = rawdata;
                unsafe
                {
                    fixed(byte* rawdataPtr = rawdata)
                    {
                        if(XEncryptService.IsEncrypted(rawdataPtr, rawdata.LongLength))
                        {
                            return ResultCode.EncryptedData;
                        }
                        IntPtr encodeData = IntPtr.Zero;
                        ulong decodeSize = 0;
                        ResultCode resultCode = XEncryptService.Encrypt(_context, rawdataPtr, rawdata.LongLength, out encodeData, out decodeSize, encryptSize, type);
                        DebugLog($"Encrypt data state({resultCode})");
                        if(resultCode == ResultCode.Ok)
                        {
                            data = new byte[decodeSize];
                            Marshal.Copy(encodeData, data, 0, (int)decodeSize);
                        }
                        return resultCode;
                    }
                }
            }

            public void End()
            {
                DebugLog("EncryptScope End");
                if(_context != null)
                {
                    XEncryptService.ReleaseXContext(_context);
                }
            }

            public void Dispose()
            {
                DebugLog("EncryptScope UnInitialize");
                XEncryptService.Deinitialize();
            }
        }

        /// <summary>
        /// 解密服务初始化
        /// </summary>
        [DllImport(dllName, EntryPoint = "decrypt_service_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Initialize();

        /// <summary>
        /// 解密服务反初始化
        /// </summary>
        [DllImport(dllName, EntryPoint = "decrypt_service_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Deinitialize();

        /// <summary>
        /// 数据是否加密
        /// </summary>
        [DllImport(dllName, EntryPoint = "decrypt_service_is_encrypted", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe bool IsEncrypted(byte* data, long size);
        /// <summary>
        /// 创建解密服务上下文
        /// </summary>
        /// <param name="type">服务上下文类型</param>
        /// <returns></returns>
        [DllImport(dllName, EntryPoint = "decrypt_create_xcontext", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateXContext(XContextType type);

        /// <summary>
        /// 数据加密
        /// </summary>
        /// <param name="context">加密服务上下文</param>
        /// <param name="input">输入源数据</param>
        /// <param name="inputSize">源数据长度</param>
        /// <param name="encryptSize">加密数据长度:</param>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="dataSize"></param>
        /// <returns></returns>
        [DllImport(dllName, EntryPoint = "decrypt_service_encrypt", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode Encrypt(IntPtr context, byte* input, long inputSize, out IntPtr data, out ulong dataSize, byte encryptSize = 16, XEncodeType type = XEncodeType.XGZip);

        /// <summary>
        /// 解密数据
        /// </summary>
        /// <param name="context">解密服务上下文</param>
        /// <param name="input">加密数据输入</param>
        /// <param name="inputSize">加密数据长度</param>
        /// <param name="data">解密结果数据输出</param>
        /// <param name="dataSize">解密数据长度</param>
        /// <returns></returns>
        [DllImport(dllName, EntryPoint = "decrypt_service_decrypt", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe ResultCode Decrypt(IntPtr context, byte* input, long inputSize, out IntPtr data, out long dataSize);

        /// <summary>
        /// 是否解密服务上下文
        /// </summary>
        /// <param name="context"></param>
        [DllImport(dllName, EntryPoint = "decrypt_release_xcontext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReleaseXContext(IntPtr context);
    }
}
