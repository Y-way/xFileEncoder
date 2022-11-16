using System;
using System.IO.Compression;
using System.IO;
using System.Text;
using XEncryptNative;
using System.Runtime.InteropServices;

namespace XDecrypt
{
    public enum XEncodeType
    {
        None,
        GZip,
    }
    public class XDecryptFile
    {
        public static readonly uint kSignature = System.BitConverter.ToUInt32(new byte[4] { (byte)'@', (byte)'X', (byte)'F', (byte)'E' }, 0);
        #region 解密
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
#if false
            long position = stream.Position;
            using(BinaryReader reader = new BinaryReader(stream))
            {
                uint sign = reader.ReadUInt32();
                if(kSignature != sign)
                {
                    stream.Position = position;
                    Console.WriteLine("无效的文件");
                    return false;
                }
                byte offset = reader.ReadByte();
                byte encryptDataSize = reader.ReadByte();
                byte encodeType = reader.ReadByte();
                byte version = reader.ReadByte();
                uint length = reader.ReadUInt32();
                uint reserved = reader.ReadUInt32();

                try
                {
                    byte[] data = reader.ReadBytes((int)length);
                    if(!DoDecryptData(ref data, encryptDataSize))
                    {
                        return false;
                    }
                    bytes = data;
                    if(encodeType == (byte)XEncodeType.GZip)
                    {
                        if(!GZipDecompress(data, out var output))
                        {
                            return false;
                        }
                        bytes = output;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return true;
#endif
        }
        /// <summary>
        /// 数据解密处理
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        static bool DoDecryptData(ref byte[] bytes, byte size)
        {
            if(bytes == null)
            {
                Console.WriteLine("解密数据为空");
                return false;
            }

            if(bytes.Length < size)
            {
                Console.WriteLine($"解密数据长度不足.{bytes.Length} < {size}");
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

        static bool GZipDecompress(byte[] input, out byte[] output)
        {
            output = new byte[0];
            if(input == null)
            {
                return false;
            }

            using(MemoryStream memory = new MemoryStream(input))
            {
                using(GZipStream gZipStream = new GZipStream(memory, CompressionMode.Decompress))
                {
                    MemoryStream outBuffer = new MemoryStream();
                    byte[] block = new byte[1024];
                    while(true)
                    {
                        int bytesRead = gZipStream.Read(block, 0, block.Length);
                        if(bytesRead <= 0)
                            break;
                        else
                            outBuffer.Write(block, 0, bytesRead);
                    }
                    output = outBuffer.ToArray();
                }

            }
            return true;
        }

        #endregion
    }
}
