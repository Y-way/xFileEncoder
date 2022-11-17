using System;
using System.Runtime.InteropServices;
namespace XEncryptApi
{
    public enum ResultCode
    {
        Ok,
        Unknown,
        UnInitialize,
        InvalidInputData,
        InvalidInputDataSize,
        InvalidDecoder,
        InvalidUnzip,
        InvalidEncoder,
        InvalidZip,
        EncryptedData,
        OutMemory,
        ContextTypeError,
        NotSupportDecrypt,
        NotSupportEncrypt,
    };

    public enum XContextType
    {
        XUnknown,
        Ecrypt,
        Decrypt,
    };

    public enum XEncodeType
    {
        XNone,
        XZip,
    };

    public static class XEncryptService
    {
        private const string dllName = "XEncrypt.dll";

        public static readonly uint kSignature = System.BitConverter.ToUInt32(new byte[4] { (byte)'@', (byte)'X', (byte)'F', (byte)'E' }, 0);
        public class DecryptScope : IDisposable
        {
            IntPtr _context;
            public DecryptScope()
            {
                XEncryptService.Initialize();
            }

            public void Begin()
            {
                _context = XEncryptService.CreateXContext(XContextType.Decrypt);
            }

            public ResultCode DecryptData(byte[] rawdata, out byte[] data)
            {
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
                        Console.WriteLine($"Decrypt data state({resultCode})");
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
                if(_context != null)
                {
                    XEncryptService.ReleaseXContext(_context);
                }
            }

            public void Dispose()
            {
                XEncryptService.Denitialize();
            }
        }

        public class EncryptScope : IDisposable
        {
            IntPtr _context;
            public EncryptScope()
            {
                XEncryptService.Initialize();
            }

            public void Begin()
            {
                _context = XEncryptService.CreateXContext(XContextType.Ecrypt);
            }

            public ResultCode EncryptData(byte[] rawdata, out byte[] data, byte encryptSize = 16, XEncodeType type = XEncodeType.XNone)
            {
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
                        Console.WriteLine($"Encrypt data state({resultCode})");
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
                if(_context != null)
                {
                    XEncryptService.ReleaseXContext(_context);
                }
            }

            public void Dispose()
            {
                XEncryptService.Denitialize();
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
        public static extern void Denitialize();

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
        public static extern unsafe ResultCode Encrypt(IntPtr context, byte* input, long inputSize, out IntPtr data, out ulong dataSize, byte encryptSize = 16, XEncodeType type = XEncodeType.XZip);

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
