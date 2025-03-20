using System;
using System.IO;
using XEncryptAPI;
using static XEncryptAPI.XEFPlugin;

namespace XFileEncode
{
    public static class Program
    {
        private static string _xFile;
        private static string _outFile;
        private static byte _encryptSize;
        private static byte _encodeType;
        private static int _cmd;

        //commandLineArgs:"encrypt -load App.Fight.dll.bytes -out App.Fight.xef.bytes -encrypt-size 16 -encode-type zip",
        //commandLineArgs:"decrypt -load App.Fight.xef.bytes -out App.Fight.xdf.bytes"
        static void Main(string[] args)
        {
            _xFile = "";
            _outFile = "out.xfe";
            _encodeType = 0;
            _encryptSize = 16;
            _cmd = 0;
            if(ParseParameter(args))
            {
                try
                {
                    if(_cmd == 0)
                    {
                        Encrypt(_xFile, _outFile, _encryptSize, (XEncodeType)_encodeType);
                    }
                    else if(_cmd == 1)
                    {
                        Decrypt(_xFile, _outFile);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

        static void PrintUsage()
        {
            XService.DebugLog("Usage:");
            XService.DebugLog("XFileEncoder encrypt|decrypt -load FileName [-out OutputFileName] [-encrypt-size size] [-encode-type type]");
            XService.DebugLog("\tencrypt|decrypt:加密/解密文件,必须参数");
            XService.DebugLog("\t\t-load:加载欲加密文件,必须参数");
            XService.DebugLog("\t\t-out:输出文件名字.可选参数,默认文件名out.xfe");
            XService.DebugLog("\t\t-encrypt-size:加密数据长度,可选参数,默认16字节. 取值范围:Min(Clamp(encrypt-size, 16, 256), file_size)");
            XService.DebugLog("\t\t-encode-type:加密源数据方式,可选参数.none:不改变源文件内容, 默认;zip:zip压缩源文件内容");
            XService.DebugLog("\t-help:查看帮助");
            XService.DebugLog("例:\n加密:\n\tXFileEncoder encrypt -load test.png -out test.png -encrypt-size 32 -encode-type zip");
            XService.DebugLog("解密:\n\tXFileEncoder edcrypt -load test.png -out test.png");
        }

        static bool ParseParameter(string[] args)
        {
            bool result = true;
            do
            {
                if(args.Length < 1)
                {
                    XService.DebugLog("参数不对{0}", args.Length.ToString());
                    result = false;
                    break;
                }
                if(args.Length == 1
                    && args[0] == "-help")
                {
                    result = false;
                    break;
                }

                if(args.Length < 3)
                {
                    XService.DebugLog("参数错误{0}", args.Length.ToString());
                    result = false;
                    break;
                }
                _cmd = -1;
                if(args[0] == "encrypt")
                {
                    _cmd = 0;
                }
                else if(args[0] == "decrypt")
                {
                    _cmd = 1;
                }
                else
                {
                    result = false;
                    break;
                }

                for(int i = 1; i < args.Length; i++)
                {
                    if(args[i] == "-load")
                    {
                        if(i + 1 >= args.Length)
                        {
                            break;
                        }

                        if(IsValidVariate(args[i + 1]))
                        {
                            i++;
                            _xFile = args[i];
                        }
                        else
                        {
                            XService.DebugLog($"{args[i]} 参数值无效");
                        }
                    }

                    if(args[i] == "-out")
                    {
                        if(i + 1 >= args.Length)
                        {
                            break;
                        }

                        if(IsValidVariate(args[i + 1]))
                        {
                            i++;
                            _outFile = args[i];
                        }
                        else
                        {
                            XService.DebugLog($"{args[i]} 参数值无效");
                        }
                    }

                    if(args[i] == "-encrypt-size")
                    {
                        _encryptSize = 16;

                        if(i + 1 >= args.Length)
                        {
                            break;
                        }

                        if(IsValidVariate(args[i + 1]))
                        {
                            i++;
                            if(!byte.TryParse(args[i], out _encryptSize))
                            {
                                _encryptSize = 16;
                            }
                        }
                        else
                        {
                            XService.DebugLog($"{args[i]} 参数值无效");
                        }

                        if(_encryptSize < 16)
                        {
                            _encryptSize = 16;
                        }
                    }
                    if(args[i] == "-encode-type")
                    {
                        _encodeType = 0;
                        if(i + 1 >= args.Length)
                        {
                            break;
                        }
                        if(IsValidVariate(args[i + 1]))
                        {
                            i++;
                            if(args[i] == "zip")
                            {
                                _encodeType = 1;
                            }
                        }
                        else
                        {
                            XService.DebugLog($"{args[i]} 参数值无效");
                        }
                    }
                }

                if(string.IsNullOrWhiteSpace(_xFile))
                {
                    result = false;
                    break;
                }

                if(string.IsNullOrWhiteSpace(_outFile))
                {
                    _outFile = "out.xfe";
                }
            } while(false);

            if(!result)
            {
                PrintUsage();
            }
            return result;
        }

        static bool IsValidVariate(string variate)
        {
            if(string.IsNullOrWhiteSpace(variate))
            {
                return false;
            }
            return !variate.StartsWith("-");
        }

        static bool IsXEncryptFile(string fileName)
        {
            using FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            if(fs.Length < sizeof(uint))
            {
                return false;
            }
            using BinaryReader br = new System.IO.BinaryReader(fs);

            uint data = br.ReadUInt32();
            return XService.kSignature == data;
        }
        #region 解密
        /// <summary>
        /// 文件解密.输入文件/输出文件可同名,同名文件解密后,源文件将丢失.
        /// </summary>
        /// <param name="source">加密文件名</param>
        /// <param name="xFileName">解密输出文件名</param>
        public static void Decrypt(string source, string xFileName)
        {
            if(string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(xFileName))
            {
                XService.DebugLog($"参数错误. 源文件:{source}=>输出文件:{xFileName}");
                return;
            }

            if(!IsXEncryptFile(source))
            {
                XService.DebugLog($"无效的文件:{source}");
                return;
            }

            if(xFileName.EndsWith("/") || xFileName.EndsWith("\\"))
            {
                XService.DebugLog($"输出文件无效:{xFileName}");
                return;
            }
            try
            {
                string path = Path.GetDirectoryName(xFileName);
                if(!string.IsNullOrWhiteSpace(path))
                {
                    if(!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch(Exception ex)
            {
                XService.DebugLog(ex.Message);
                return;
            }

            //Decrypt the source file.
            using FileStream sourceStream = new FileStream(source, FileMode.Open);
            try
            {
                long lSize = sourceStream.Length;
                byte[] data = new byte[lSize];
                sourceStream.Read(data, 0, (int)lSize);
                sourceStream.Close();

                MemoryStream memory = new MemoryStream(data);

                if(!DecryptData(memory, out var buffer))
                {
                    return;
                }
                using FileStream sw = new FileStream(xFileName, FileMode.Create);
                using(BinaryWriter writer = new BinaryWriter(sw))
                {
                    writer.Write(buffer);
                    writer.Flush();
                }
                XService.DebugLog("输出成功!" + xFileName);
            }
            catch(Exception ex)
            {
                XService.DebugLog(ex.Message);
            }
        }
        /// <summary>
        /// 解密数据流,输出字节数组
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="bytes"></param>
        /// <returns>解密成功,返回true,否则返回false</returns>
        public static bool DecryptData(Stream stream, out byte[] bytes)
        {
            bytes = null;
            if(stream == null)
            {
                XService.DebugLog("输出流为空");
                return false;
            }
            byte[] rawdata = new byte[stream.Length];
            stream.Read(rawdata, 0, rawdata.Length);

            using DecryptScope scope = new DecryptScope((XEncodeType)_encodeType, _encryptSize);
            scope.Begin();
            ResultCode code = scope.DecryptData(rawdata, out bytes);
            scope.End();
            XService.DebugLog($"State:{code}");
            return code == ResultCode.Ok;
        }
        #endregion

        #region 加密
        /// <summary>
        /// 文件加密.输入文件/输出文件可同名,同名文件加密后,源文件将丢失.
        /// </summary>
        /// <param name="source">源文件名</param>
        /// <param name="xFileName">加密输出文件名</param>
        /// <param name="encryptSize">加密数据长度</param>
        /// <param name="encodeType">源文件数据加密格式</param>
        public static void Encrypt(string source, string xFileName, byte encryptSize = 16, XEFPlugin.XEncodeType encodeType = XEncodeType.XNone)
        {
            if(string.IsNullOrWhiteSpace(source)
                || string.IsNullOrWhiteSpace(xFileName))
            {
                XService.DebugLog($"参数错误. 源文件:{source}=>输出文件:{xFileName}");
                return;
            }

            if(IsXEncryptFile(source))
            {
                XService.DebugLog($"源文件已经加密:{source}");
                return;
            }

            if(xFileName.EndsWith("/") || xFileName.EndsWith("\\"))
            {
                XService.DebugLog($"输出文件无效:{xFileName}");
                return;
            }
            try
            {
                string path = Path.GetDirectoryName(xFileName);
                if(!string.IsNullOrWhiteSpace(path))
                {
                    if(!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
            catch(Exception ex)
            {
                XService.DebugLog(ex.Message);
                return;
            }

            ///Encode the source file.
            using FileStream sourceStream = File.Open(source, FileMode.Open);
            try
            {
                long lSize = sourceStream.Length;
                byte[] data = new byte[lSize];
                sourceStream.Read(data, 0, (int)lSize);
                sourceStream.Close();
                using FileStream sw = new FileStream(xFileName, FileMode.Create);
                if(!EncryptData(data, sw, encryptSize, encodeType))
                {
                    XService.DebugLog("失败!" + xFileName);
                    return;
                }
                XService.DebugLog("输出成功!" + xFileName);
            }
            catch(Exception ex)
            {
                XService.DebugLog(ex.Message);
            }
        }
        /// <summary>
        /// 数据加密,输出数据流.
        /// </summary>
        /// <param name="bytes">待加密数据</param>
        /// <param name="stream">加密输出</param>
        /// <param name="encryptSize">加密数据长度</param>
        /// <param name="encodeType">源文件数据加密格式</param>
        /// <returns>加密成功,返回true,否则返回false</returns>
        public static bool EncryptData(byte[] bytes, Stream stream, byte encryptSize = 16, XEFPlugin.XEncodeType encodeType = XEncodeType.XGZip)
        {
            if(stream == null)
            {
                XService.DebugLog("输出流为空");
                return false;
            }

            if(bytes == null || bytes.Length <= 0)
            {
                XService.DebugLog("加密数据为空");
                return false;
            }
            using EncryptScope scope = new EncryptScope((XEncodeType)_encodeType, _encryptSize);
            scope.Begin();
            ResultCode code = scope.EncryptData(bytes, out var encodeData, encryptSize, encodeType);
            scope.End();
            XService.DebugLog($"State:{code}");
            if(code == ResultCode.Ok)
            {
                using BinaryWriter bw = new BinaryWriter(stream);
                bw.Write(encodeData, 0, encodeData.Length);
                bw.Flush();
            }
            return code == ResultCode.Ok;
        }
        #endregion
    }
}
