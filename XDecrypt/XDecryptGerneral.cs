using System;
using System.IO;
using System.IO.Compression;

namespace XDecrypt
{
    public class XDecryptGerneral
    {
        public XDecryptGerneral()
        {
        }

        bool IsXEncryptFile(string fileName)
        {
            using(FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using(BinaryReader br = new System.IO.BinaryReader(fs))
                {
                    uint data = br.ReadUInt32();
                    return XDecryptFile.kSignature == data;
                }
            }
        }

        public void Decrypt(string source, string xFileName)
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
                    
                    if(!XDecryptFile.DecryptData(memory, out var buffer))
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


    }
}
