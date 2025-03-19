using System;
using System.Runtime.InteropServices;
namespace XEncryptAPI
{
    public static class XService
    {
        private const string LIBNAME = "XEncrypt";
        /// <summary>
        /// 加密文件标记
        /// </summary>
        public static readonly uint kSignature = System.BitConverter.ToUInt32(new byte[4] {
            (byte)'@', (byte)'X', (byte)'F', (byte)'E'
        }, 0);

        public static void DebugLog(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        /// <summary>
        /// 加密/解密结果
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct XResult
        {
            /// <summary>
            /// 结果状态码
            /// </summary>
            public ResultCode code;
            /// <summary>
            /// 结果数据大小
            /// </summary>
            public long size;
            /// <summary>
            /// 结果数据
            /// </summary>
            public IntPtr data;
            /// <summary>
            /// XReult指针, 用于释放内存
            /// </summary>
            private IntPtr result;
        }
        /// <summary>
        /// 创建XEF格式加密/解密器插件实例 <br />
        /// void* xef_plugin_create(int type, uint8_t encryptSize)
        /// </summary>
        /// <param name="type">数据加密编码类型</param>
        /// <param name="encryptSize">数据加密长度</param>
        /// <returns>插件实例指针</returns>
        [DllImport(LIBNAME, EntryPoint = "xef_plugin_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr XEFPluginCreate(int type, byte encryptSize);

        /// <summary>
        /// 销毁XEF格式 加密/解密插件实例 <br />
        /// void xef_plugin_destroy(void* plugin)
        /// </summary>
        /// <param name="plugin">已创建的插件实例</param>
        [DllImport(LIBNAME, EntryPoint = "xef_plugin_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void XEFPluginDestroy(IntPtr plugin);

        /// <summary>
        /// 初始化服务 <br />
        /// void* xencrypt_service_initialize(void* plugin)
        /// </summary>
        /// <param name="plugin">加密/解密插件实例</param>
        /// <returns>加密/解密服务实例</returns>
        [DllImport(LIBNAME, EntryPoint = "xencrypt_service_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Initialize(IntPtr plugin);

        /// <summary>
        /// 检查数据是否已加密 <br />
        /// bool xencrypt_service_is_encrypted(void* service, const byte* data, int64_t size)
        /// </summary>
        /// <param name="service">加密/解密服务实例</param>
        /// <param name="data">内存数据地址</param>
        /// <param name="size">数据长度</param>
        /// <returns>数据是否已加密.返回true,数据已加密,否则,未加密.</returns>
        [DllImport(LIBNAME, EntryPoint = "xencrypt_service_is_encrypted", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe bool IsEncrypted(IntPtr service, byte* data, long size);
        /// <summary>
        /// 加密数据 <br />
        /// void* xencrypt_service_encrypt(void* service, const byte* in, int64_t in_size)
        /// <param name="service">加密/解密服务实例</param>
        /// <param name="inData">待加密数据</param>
        /// <param name="size">待加密数据长度</param>
        /// <returns>解密结果实例指针</returns>
        [DllImport(LIBNAME, EntryPoint = "xencrypt_service_encrypt", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe XResult Encrypt(IntPtr service, byte* inData, long size);
        /// <summary>
        /// 解密数据 <br />
        /// void* xencrypt_service_decrypt(void* service, const byte* in, int64_t in_size)
        /// </summary>
        /// <param name="service">加密/解密服务实例</param>
        /// <param name="inData">待解密数据</param>
        /// <param name="size">密数据长度</param>
        /// <returns>解密结果实例指针</returns>
        [DllImport(LIBNAME, EntryPoint = "xencrypt_service_decrypt", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe XResult Decrypt(IntPtr service, byte* inData, long size);
        /// <summary>
        /// 销毁结果 <br />
        /// void xencrypt_service_release_result(void* service, void* result)
        /// </summary>
        /// <param name="service">加密/解密服务实例</param>
        /// <param name="result">加/解密结果指针</param>
        /// <returns></returns>
        [DllImport(LIBNAME, EntryPoint = "xencrypt_service_release_result", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void ReleaseResult(IntPtr service, ref XResult result);
        /// <summary>
        /// 注销服务 <br />
        /// void xencrypt_service_deinitialize(void* service)
        /// </summary>
        /// <param name="service">加密/解密服务实例</param>
        [DllImport(LIBNAME, EntryPoint = "xencrypt_service_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern unsafe void Deinitialize(IntPtr service);
    }
}
