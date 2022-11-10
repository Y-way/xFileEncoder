using System;

namespace XFileEncode
{
    class Program
    {
        private static string _xFile;
        private static string _outFile;
        private static byte _encryptSize;
        private static byte _encodeType;
        
        static void Main(string[] args)
        {
            _xFile = "";
            _outFile = "out.xfe";
            _encodeType = 0;
            if (args.Length < 1)
            {
                Console.WriteLine("参数不对{0}", args.Length.ToString());
                PrintUsage();
                return;
            }
            if(ParseParameter(args))
            {
                Gerneral pMyApp = new Gerneral(_xFile, _outFile, _encryptSize, _encodeType);
                pMyApp.RunApp();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("XFileEncoder -load FileName [-out OutputFileName] [-encrypt_size size] [-encode_type type]");
            Console.WriteLine("\t-load:加载欲加密文件,必须参数");
            Console.WriteLine("\t-out:输出文件名字.可选参数,默认文件名out.xfe");
            Console.WriteLine("\t-encrypt_size:加密数据长度,可选参数,默认16字节. 取值范围:Min(Clamp(encrypt_size, 16, 256), file_size)");
            Console.WriteLine("\t-encode_type:源数据加密方式,可选参数.none:不改变源文件内容, 默认;zip:zip压缩源文件内容");
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
            if (args.Length < 2)
            {
                Console.WriteLine("参数错误{0}", args.Length.ToString());
                PrintUsage();
                return false;
            }

            for (int i = 0; i < args.Length; i++ )
            {
                if (args[i] == "-load")
                {
                    i++;
                    _xFile = args[i];
                }
                if (args[i] == "-out")
                {
                    i++;
                    if (i >= args.Length)
                    {
                        continue;
                    }
                    _outFile = args[i];
                }
                if (args[i] == "-encrypt_size")
                {
                    i++;
                    _encryptSize = 16;
                    if (i >= args.Length)
                    {
                        continue;
                    }

                    if(string.IsNullOrWhiteSpace(args[i]))
                        _encryptSize = 16;
                    else
                    {
                        if(!byte.TryParse(args[i], out _encryptSize))
                        {
                            _encryptSize = 16;
                        }
                    }
                    if(_encryptSize < 16)
                    {
                        _encryptSize = 16;
                    }
                }
                if (args[i] == "-encode_type")
                {
                    i++;
                    _encodeType = 0;
                    if (i >= args.Length)
                    {
                        continue;
                    }
                    if(args[i] == "zip")
                    {
                        _encodeType = 1;
                    }
                }
            }
            if (_xFile == "")
            {
                return false;
            }
            if (_outFile == "")
            {
                _outFile = "out.xfe";
            }
            return true;
        }
    }
}
