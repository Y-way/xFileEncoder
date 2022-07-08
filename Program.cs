using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwfFileParser
{
    class Program
    {
        private static string _xFile;
        private static string _outFile;
        private static string _prefix;
        private static string _version;
        
        static void Main(string[] args)
        {
            _xFile = "";
            _outFile = "out.xfe";
            _version = "0";
            if (args.Length < 1)
            {
                Console.WriteLine("参数不对{0}", args.Length.ToString());
                PrintUsage();
                return;
            }
            if(ParseParameter(args))
            {
                Gerneral pMyApp = new Gerneral(_xFile, _outFile, _prefix, _version);
                pMyApp.RunApp();
            }
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("[-load File Name [-out output File Name] [-_prefix _prefix]]");
            Console.WriteLine("\t-load:加载欲加密文件,必须参数");
            Console.WriteLine("\t-out:输出文件名字,输出内容为文本格式.可选参数,默认文件名out.xfe");
            Console.WriteLine("\t-prefix:每行行首前缀词.可选参数,默认为空");
            Console.WriteLine("\t-ver:文件加密版本号.可选参数,默认为空");
            Console.WriteLine("\t-help:查看帮助");
            Console.WriteLine("例子:\nParseRevision -load test.png -out test.png -_prefix test/");
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
                if (args[i] == "-prefix")
                {
                    i++;
                    if (i >= args.Length)
                    {
                        continue;
                    }
                    _prefix = args[i];
                }
                if (args[i] == "-ver")
                {
                    i++;
                    if (i >= args.Length)
                    {
                        continue;
                    }
                    _version = args[i];
                    if (_version == null)
                        _version = "0";

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
