using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Reflection.Emit;
using System.Text;
using XEncryptApi;

namespace XFileEncode
{
    public enum XEncodeType
    {
        None,
        GZip,
    }

    [Serializable]
    public struct XEncryptFile
    {
        public static readonly uint kSignature = System.BitConverter.ToUInt32(new byte[4] { (byte)'@', (byte)'X', (byte)'F', (byte)'E' }, 0);

        public readonly uint sign;// Signature code('@XFE')
        public readonly byte offset;// Rawdata offset form the file begin.
        public readonly uint reserved; //Reserved for users. 
        public readonly byte encode_type; // The data encode type.0:default,do nothing. 1: zip compress.
        public readonly byte version; // The encode version

        private byte encrypt_data_size; // Encrypt the length of data.
        private uint length; // The data length.
        
        public XEncryptFile(byte encryptSize, byte encodeType, uint reserved = 0)
        {
            this.sign = XEncryptFile.kSignature;
            this.offset = sizeof(uint) * 4;
            this.encrypt_data_size = encryptSize;
            this.encode_type = encodeType;
            this.version = 1;
            this.length = 0;
            this.reserved = reserved;
        }
        #region 加密
        public static bool EncryptData(XEncryptFile header, byte[] bytes, Stream stream)
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
                ResultCode code = scope.EncryptData(bytes, out encodeData, header.encrypt_data_size, (XEncryptApi.XEncodeType)header.encode_type);
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
#if false
            byte[] encodeData = bytes;
            if(header.encode_type == (byte)XEncodeType.GZip)
            {
                if(!GZipCompress(bytes, out encodeData))
                {
                    Console.WriteLine("Compress 数据失败");
                    return false;
                }
            }

            if(header.encrypt_data_size > encodeData.Length)
            {
                header.encrypt_data_size = (byte)encodeData.Length;
            }

            //加密数据
            if(!DoEncryptData(ref encodeData, header.encrypt_data_size))
            {
                return false;
            }

            header.length = (uint)encodeData.Length;
            using(BinaryWriter bw = new BinaryWriter(stream))
            {
                bw.Write(header.sign);
                bw.Write(header.offset);
                bw.Write(header.encrypt_data_size);
                bw.Write(header.encode_type);
                bw.Write(header.version);
                bw.Write(header.length);
                bw.Write(header.reserved);

                bw.Write(encodeData, 0, encodeData.Length);
                bw.Flush();
            }
            return true;
#endif
        }

        static bool GZipCompress(byte[] input, out byte[] output)
        {
            output = new byte[0];
            if(input == null)
            {
                return false;
            }
            using(MemoryStream memory = new MemoryStream())
            {
                using(GZipStream gZipStream = new GZipStream(memory, CompressionMode.Compress))
                {
                    gZipStream.Write(input, 0, input.Length);
                }
                output = memory.ToArray();
            }
            return true;
        }

        /// <summary>
        /// 数据加密处理
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        static bool DoEncryptData(ref byte[] bytes, byte size)
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
        #endregion

    };
}
