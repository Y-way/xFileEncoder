using System;
using XDecrypt;

namespace XFileEncode
{
    class Program
    {
        private static string _xFile;
        private static string _outFile;
        private static byte _encryptSize;
        private static byte _encodeType;
        private static int _cmd;

        //"commandLineArgs": "encrypt -load App.Fight.dll.bytes -out App.Fight.xfe.bytes -encode_type zip",
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
                    XEncryptGerneral pMyApp = new XEncryptGerneral();
                    pMyApp.Encrypt(_xFile, _outFile, _encryptSize, _encodeType);
                }
                else if(_cmd == 1)
                {
                    XDecryptGerneral decrypter = new XDecryptGerneral();
                    decrypter.Decrypt(_xFile, _outFile);
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

    }
}
