using System;
using System.IO;
using System.IO.Compression;

namespace XFileEncode
{
    class Gerneral
    {
        private string _outFile;
        private string _xSourceFile;
        
        private byte _encryptDataSize;
        private byte _encodeType;

        public Gerneral(string source, string xFileName, byte encryptSize = 16, byte encodeType = 0)
        {
            _xSourceFile = source;
            _outFile = xFileName;
            _encryptDataSize = encryptSize;
            _encodeType = encodeType;
        }

        private bool EncodeFile(byte[] bytes)
        {
            byte[] resultOut = bytes;
            if(_encodeType == 1)
            {
                using(MemoryStream memory = new MemoryStream())
                {
                    using(GZipStream gZipStream = new GZipStream(memory, CompressionMode.Compress))
                    {
                        gZipStream.Write(bytes, 0, bytes.Length);
                    }
                    resultOut = memory.ToArray();
                }
                if(resultOut == null)
                {
                    return false;
                }
            }
            if(_encryptDataSize > resultOut.Length)
            {
                _encryptDataSize = (byte)resultOut.Length;
            }

            //加密数据
            if(!Encrypte(ref resultOut, _encryptDataSize))
            {
                return false;
            }

            using(FileStream sw = new FileStream(_outFile, FileMode.Create))
            {
                XEncodeFileHeader _xHeader = new XEncodeFileHeader(
                    sign:XEncodeFileHeader.SignatureCode(), 
                    offset:sizeof(uint) * 4,
                    encryptSize: _encryptDataSize,
                    encodeType: _encodeType,
                    version:1,
                    length: (uint)(resultOut.Length),
                    reserved:0);

                using(BinaryWriter bw = new BinaryWriter(sw))
                {
                    //public uint sign;// Signature code.
                    //public byte offset;// Rawdata offset form the file begin.
                    //public byte encrypt_data_size; // Encrypt the length of data.
                    //public byte encode_type; // The data encode type.0:default,do nothing. 1: zip compress.
                    //public byte version; // The encode version
                    //public uint length; // The data length.
                    //public uint reserved; //Reserved for users.
                    
                    bw.Write(_xHeader.sign);
                    bw.Write(_xHeader.offset);
                    bw.Write(_xHeader.encrypt_data_size);
                    bw.Write(_xHeader.encode_type);
                    bw.Write(_xHeader.version);
                    bw.Write(_xHeader.length);
                    bw.Write(_xHeader.reserved);
                    
                    bw.Write(resultOut, 0, (int)_xHeader.length);
                    bw.Flush();
                }
                Console.WriteLine("输出成功!" + _outFile);
            }
            return true;
        }
        /// <summary>
        /// 数据加密处理
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        bool Encrypte(ref byte[] bytes, byte size)
        {
            if(bytes == null)
            {
                Console.WriteLine("加密数据为空");
                return false;
            }

            if(bytes.Length < size)
            {
                Console.WriteLine($"加密数据长度不足.{bytes.Length} < {size}");
                return false;
            }
            byte data;
            for(int i = 0; i < size; i++)
            {
                data = bytes[i];
                data = ((byte)((data << 0x04) | (data >> 0x04)));
                bytes[i] = data;
            }
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

        bool IsXEncodedFile(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using(BinaryReader br = new System.IO.BinaryReader(fs))
                {
                    uint data = br.ReadUInt32();
                    return XEncodeFileHeader.SignatureCode() == data;
                }
            }
        }

        public void RunApp()
        {
            if (string.IsNullOrWhiteSpace(_xSourceFile)
                || string.IsNullOrWhiteSpace(_outFile))
            {
                Console.WriteLine($"参数错误. 源文件:{_xSourceFile}=>输出文件:{_outFile}");
                return;
            }

            //SourceFileExtension result = Gerneral.CheckTextFile(_xSourceFile);
            //if (SourceFileExtension.JPG == result 
            //    || SourceFileExtension.PNG == result 
            //    || SourceFileExtension.LUAC == result
            //    || SourceFileExtension.LUA == result)
            //{

            //}

            if(IsXEncodedFile(_xSourceFile))
            {
                Console.WriteLine($"源文件:{_xSourceFile} 已经加密.");
                return;
            }

            ///Encode the source file.
            using(FileStream source = new FileStream(_xSourceFile, FileMode.Open))
            {
                try
                {
                    long lSize = source.Length;
                    byte[] byFile = new byte[lSize];
                    source.Read(byFile, 0, (int)lSize);
                    source.Close();
                    EncodeFile(byFile);
                }
                catch(System.Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
            
        }
    }
}
