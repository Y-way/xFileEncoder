using System;
using System.IO;
using System.IO.Compression;
using System.Reflection.Emit;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

namespace XFileEncode
{
    public class XEncryptGerneral
    {
        public XEncryptGerneral()
        {
        }

        public enum SourceFileExtension
        {
            /// <summary>
            /// LuaJIT 编译后的乱文件
            /// </summary>
            LUAC = 2776,
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
            if(extStr == ".lua")
            {
                return SourceFileExtension.LUA;
            }
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using(BinaryReader br = new System.IO.BinaryReader(fs))
                {
                    string fileType = string.Empty;
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
                        extension = SourceFileExtension.VALIDFILE;
                    }
                    return extension;
                }
            }
        }

        bool IsXEncryptFile(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using(BinaryReader br = new System.IO.BinaryReader(fs))
                {
                    uint data = br.ReadUInt32();
                    return XEncryptFile.kSignature == data;
                }
            }
        }

        public void Encrypt(string source, string xFileName, byte encryptSize = 16, byte encodeType = 0)
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
                        XEncryptFile _xHeader = new XEncryptFile(encryptSize: encryptSize, encodeType: encodeType, reserved: 0);
                        if(!XEncryptFile.EncryptData(_xHeader, data, sw))
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
    }
}
