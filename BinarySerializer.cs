using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;//二进制序列化，加上引用
using System.IO;

namespace swfFileParser
{
    [Serializable]
    class BinarySerializer<T> 
    { 
        public void Serialize(T obj, string filePath) 
        { 
            BinaryFormatter transfer = new BinaryFormatter(); 
            MemoryStream ms = new MemoryStream(); 
            transfer.Serialize(ms, obj); 
            byte[] b = new byte[ms.Length]; 
            b = ms.GetBuffer(); 
            FileStream fs = File.Create(filePath); 
            fs.Write(b, 0, b.Length); 
            ms.Close(); 
            fs.Close(); 
        } 
        public T Deserialize(string filePath) 
        { 
            FileStream fs = File.OpenRead(filePath); 
            byte[] b = new byte[fs.Length]; 
            fs.Read(b, 0, b.Length); 
            fs.Close(); 
            BinaryFormatter transfer = new BinaryFormatter(); 
            MemoryStream ms = new MemoryStream(b); 
            T t = (T)transfer.Deserialize(ms); 
            ms.Close(); 
            return t; 
        } 
    }
}
