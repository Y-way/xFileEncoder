using System;
using System.IO;
using XEncryptNative;

namespace XFileEncode
{
    public static class Program
    {
        private static string _xFile;
        private static string _outFile;
        private static byte _encryptSize;
        private static byte _encodeType;
        private static int _cmd;

        //commandLineArgs:"encrypt -load App.Fight.dll.bytes -out App.Fight.xef.bytes -encode_type zip",
        //commandLineArgs:"decrypt -load App.Fight.xef.bytes -out App.Fight.xdf.bytes"
        static void Main(string[] args)
        {
            _xFile = "";
            _outFile = "out.xfe";
            _encodeType = 0;
            _encryptSize = 16;
            _cmd = 0;
            if (args.Length < 1)
            {
                Console.WriteLine("参数不对{0}", args.Length.ToString());
                PrintUsage();
                return;
            }
            if(ParseParameter(args))
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
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("XFileEncoder encrypt|decrypt -load FileName [-out OutputFileName] [-encrypt_size size] [-encode_type type]");
            Console.WriteLine("\tencrypt|decrypt:加密/解密文件,必须参数");
            Console.WriteLine("\t\t-load:加载欲加密文件,必须参数");
            Console.WriteLine("\t\t-out:输出文件名字.可选参数,默认文件名out.xfe");
            Console.WriteLine("\t\t-encrypt_size:加密数据长度,可选参数,默认16字节. 取值范围:Min(Clamp(encrypt_size, 16, 256), file_size)");
            Console.WriteLine("\t\t-encode_type:加密源数据方式,可选参数.none:不改变源文件内容, 默认;zip:zip压缩源文件内容");
            Console.WriteLine("\t-help:查看帮助");
            Console.WriteLine("例子:\nXFileEncoder -load test.png -out test.png -encrypt_size 32 -encode_type zip");
        }

        static bool ParseParameter(string[] args)
        {
            if (args.Length == 1
                && args[0] == "-help")
            {
                PrintUsage();
                return false;
            }

            if (args.Length < 3)
            {
                Console.WriteLine("参数错误{0}", args.Length.ToString());
                PrintUsage();
                return false;
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
                PrintUsage();
                return false;
            }

            for (int i = 1; i < args.Length; i++ )
            {
                if (args[i] == "-load")
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
                        Console.WriteLine($"{args[i]} 参数值无效");
                    }
                }

                if (args[i] == "-out")
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
                        Console.WriteLine($"{args[i]} 参数值无效");
                    }
                }

                if (args[i] == "-encrypt_size")
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
                        Console.WriteLine($"{args[i]} 参数值无效");
                    }

                    if(_encryptSize < 16)
                    {
                        _encryptSize = 16;
                    }
                }
                if (args[i] == "-encode_type")
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
                        Console.WriteLine($"{args[i]} 参数值无效");
                    }
                }
            }

            if (_xFile == "")
            {
                PrintUsage();
                return false;
            }

            if (_outFile == "")
            {
                _outFile = "out.xfe";
            }

            return true;
        }

        static bool IsValidVariate(string variate)
        {
            if(string.IsNullOrWhiteSpace(variate))
            {
                return false;
            }
            return !variate.StartsWith('-');
        }

        static bool IsXEncryptFile(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                if(fs.Length < sizeof(uint))
                {
                    return false;
                }
                using(BinaryReader br = new System.IO.BinaryReader(fs))
                {

                    uint data = br.ReadUInt32();
                    return XEncryptService.kSignature == data;
                }
            }
        }
        #region 解密
        public static void Decrypt(string source, string xFileName)
        {
            if(string.IsNullOrWhiteSpace(source) ||
                string.IsNullOrWhiteSpace(xFileName))
            {
                Console.WriteLine($"参数错误. 源文件:{source}=>输出文件:{xFileName}");
                return;
            }

            if(!IsXEncryptFile(source))
            {
                Console.WriteLine($"无效的文件:{source}");
                return;
            }

            if(xFileName.EndsWith("/") || xFileName.EndsWith("\\"))
            {
                Console.WriteLine($"输出文件无效:{xFileName}");
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
                Console.WriteLine(ex.Message);
                return;
            }

            ///Decrypt the source file.
            using(FileStream sourceStream = new FileStream(source, FileMode.Open))
            {
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
                    using(FileStream sw = new FileStream(xFileName, FileMode.Create))
                    {
                        using(BinaryWriter writer = new BinaryWriter(sw))
                        {
                            writer.Write(buffer);
                            writer.Flush();
                        }
                        Console.WriteLine("输出成功!" + xFileName);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public static bool DecryptData(Stream stream, out byte[] bytes)
        {
            bytes = null;
            if(stream == null)
            {
                Console.WriteLine("输出流为空");
                return false;
            }
            byte[] rawdata = new byte[stream.Length];
            stream.Read(rawdata, 0, rawdata.Length);

            using(XEncryptService.DecryptScope scope = new XEncryptService.DecryptScope())
            {
                scope.Begin();
                ResultCode code = scope.DecryptData(rawdata, out bytes);
                scope.End();
                return code == ResultCode.Ok;
            }
        }
        #endregion

        #region 加密
        public static void Encrypt(string source, string xFileName, byte encryptSize = 16, XEncodeType encodeType = XEncodeType.XNone)
        {
            if(string.IsNullOrWhiteSpace(source)
                || string.IsNullOrWhiteSpace(xFileName))
            {
                Console.WriteLine($"参数错误. 源文件:{source}=>输出文件:{xFileName}");
                return;
            }

            if(IsXEncryptFile(source))
            {
                Console.WriteLine($"源文件已经加密:{source}");
                return;
            }

            if(xFileName.EndsWith("/") || xFileName.EndsWith("\\"))
            {
                Console.WriteLine($"输出文件无效:{xFileName}");
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
                Console.WriteLine(ex.Message);
                return;
            }

            ///Encode the source file.
            using(FileStream sourceStream = new FileStream(source, FileMode.Open))
            {
                try
                {
                    long lSize = sourceStream.Length;
                    byte[] data = new byte[lSize];
                    sourceStream.Read(data, 0, (int)lSize);
                    sourceStream.Close();

                    using(FileStream sw = new FileStream(xFileName, FileMode.Create))
                    {
                        if(!EncryptData(data, sw, encryptSize, encodeType))
                        {
                            return;
                        }
                        Console.WriteLine("输出成功!" + xFileName);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static bool EncryptData(byte[] bytes, Stream stream, byte encryptSize = 16, XEncodeType encodeType = XEncodeType.XZip)
        {
            if(stream == null)
            {
                Console.WriteLine("输出流为空");
                return false;
            }

            if(bytes == null)
            {
                Console.WriteLine("加密数据为空");
                return false;
            }
            using(XEncryptService.EncryptScope scope = new XEncryptService.EncryptScope())
            {
                byte[] encodeData;
                scope.Begin();
                ResultCode code = scope.EncryptData(bytes, out encodeData, encryptSize, encodeType);
                scope.End();
                if(code == ResultCode.Ok)
                {
                    using(BinaryWriter bw = new BinaryWriter(stream))
                    {
                        bw.Write(encodeData, 0, encodeData.Length);
                        bw.Flush();
                    }
                }
                return code == ResultCode.Ok;
            }
        }
        #endregion
    }
}
