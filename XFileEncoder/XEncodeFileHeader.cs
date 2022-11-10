using System;
using System.Collections.Generic;
using System.Text;

namespace XFileEncode
{

    [Serializable]
    public struct XEncodeFileHeader
    {
        public readonly uint sign;// Signature code.
        public readonly byte offset;// Rawdata offset form the file begin.
        public readonly byte encrypt_data_size; // Encrypt the length of data.
        public readonly byte encode_type; // The data encode type.0:default,do nothing. 1: zip compress.
        public readonly byte version; // The encode version
        public readonly uint length; // The data length.
        public readonly uint reserved; //Reserved for users. 
        public XEncodeFileHeader(uint sign, byte offset, byte encryptSize, byte encodeType, byte version, uint length, uint reserved = 0)
        {
            this.sign = sign;
            this.offset = offset;
            this.encrypt_data_size = encryptSize;
            this.encode_type = encodeType;
            this.version = version;
            this.length = length;
            this.reserved = reserved;
        }

        public static uint SignatureCode()
        {
            byte[] bpara = new byte[4] { (byte)'@', (byte)'X', (byte)'F', (byte)'E' };
            uint upara = System.BitConverter.ToUInt32(bpara, 0);
            return upara;
        }
    };
}
