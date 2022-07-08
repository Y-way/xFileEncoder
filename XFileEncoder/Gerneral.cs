using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;//二进制序列化，加上引用
using ComponentAce.Compression.Libs.zlib;
using swfFileParser;


namespace SwfFileParser
{
    class Gerneral
    {
        private string _outFile;
        private string _xSourceFile;
        private string _prefix;
        private string _version;
        private byte[] _byFile;
        private long lSize = 0;
        private FileStream _lSwfFile;

        [Serializable]
        class XEncodeFileHeader 
        {
            public uint sign;//文件头标志
            public uint offset;//文件偏移
            public uint length;//文件长度
            public uint version;//保留
            public XEncodeFileHeader()
            {
                sign = 0;
                sign = sign | 'F';
                sign = (sign << 8) | 'W';
                sign = (sign << 8) | 'S';
                sign = (sign << 8) | 'N';
            }
        };

        private XEncodeFileHeader _xHeader;
        public Gerneral(string source, string xFileName, string prefix = "", string version = "0")
        {
            _xSourceFile = source;
            _outFile = xFileName;
            _prefix = prefix;
            _version = version;

            _xHeader = new XEncodeFileHeader();
            _xHeader.version = uint.Parse(_version);
        }

        private bool ReBuildSwfFile()
        {
            byte[] resultOut = ZlibCompress.CompressBytes(_byFile);
            if (resultOut == null)
            {
                return false;
            }

            FileStream sw;
            try 
            {
                sw = new FileStream(_outFile, FileMode.Create);
            }
            catch(System.Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

            _xHeader.length = (uint)(resultOut.Length);
            _xHeader.offset = sizeof(uint) * 4;

            BinaryWriter bw = new BinaryWriter(sw);
            bw.Write((uint)_xHeader.sign);
            bw.Write((uint)_xHeader.offset);
            bw.Write((uint)_xHeader.length);
            bw.Write((uint)_xHeader.version);
            //TODO 加密数据
            bw.Write(resultOut, 0, (int)_xHeader.length);
            bw.Flush();

            bw.Close();
            sw.Close();
            Console.WriteLine("输出成功!" + _outFile);
            return true;
        }
        public enum SourceFileExtension
        {
            /// <summary>
            /// LuaJIT 编译后的乱文件
            /// </summary>
            LUAC=2776,
            /// <summary>
            /// JPG图片
            /// </summary>
            JPG = 255216,
            GIF = 7173,
            PNG = 13780,
            /// <summary>
            /// flash swf 文件
            /// </summary>
            SWF = 6787,
            /// <summary>
            /// RAR压缩文件
            /// </summary>
            RAR = 8297,
            /// <summary>
            /// Zip压缩文件
            /// </summary>
            ZIP = 8075,
            /// <summary>
            /// 7Zip压缩文件
            /// </summary>
            _7Z = 55122,
            /// <summary>
            /// .lua 文件
            /// </summary>
            LUA,
            /// <summary>
            /// 明文文件
            /// </summary>
            PLAIN,
            VALIDFILE = 9999999
        }
        public static SourceFileExtension CheckTextFile(string fileName)
        {
            String extStr = Path.GetExtension(fileName).ToLower();
            if (extStr == ".lua")
            {
                return SourceFileExtension.LUA;
            }
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            System.IO.BinaryReader br = new System.IO.BinaryReader(fs);
            string fileType = string.Empty; ;
            try
            {
                byte data = br.ReadByte();
                fileType += data.ToString();
                data = br.ReadByte();
                fileType += data.ToString();
                SourceFileExtension extension;
                try
                {
                    extension = (SourceFileExtension)Enum.Parse(typeof(SourceFileExtension), fileType);
                }
                catch 
                { 

                    extension=SourceFileExtension.VALIDFILE;
                }
                return extension;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    br.Close();
                }
            }
        }

        public void RunApp()
        {
            if (_xSourceFile == ""
                || _outFile == "")
            {
                return;
            }
            
            SourceFileExtension result = Gerneral.CheckTextFile(_xSourceFile);
            if (SourceFileExtension.JPG == result 
                || SourceFileExtension.PNG == result 
                || SourceFileExtension.LUAC == result
                || SourceFileExtension.LUA == result)
            {
                try
                {
                    _lSwfFile = new FileStream(_xSourceFile, FileMode.Open);
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _lSwfFile.Close();
                    return;
                }
                try
                {
                    lSize = _lSwfFile.Length;

                    _byFile = new byte[lSize];
                    _lSwfFile.Read(_byFile, 0, (int)lSize);
                    _lSwfFile.Close();
                }
                catch(System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _byFile = null;
                    _lSwfFile.Close();
                }
                ReBuildSwfFile();
            }
        }
    }
}
