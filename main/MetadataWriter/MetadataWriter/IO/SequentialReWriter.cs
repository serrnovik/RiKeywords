using MetadataExtractor.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RiMetadataWriter.IO
{
    public abstract class SequentialReWriter : SequentialReader
    {
        protected SequentialReWriter(bool isMotorolaByteOrder) : base(isMotorolaByteOrder)
        {
        }
       
        public virtual ushort GetUInt16(byte[] bytes)
        {
            if (bytes.Length != 2) throw new ArgumentException(nameof(bytes));
            if (IsMotorolaByteOrder)
            {
                // Motorola - MSB first
                return (ushort)(bytes[0] << 8 | bytes[1]);
            }
            // Intel ordering - LSB first
            return (ushort)(bytes[0] | bytes[1] << 8);
        }
        public virtual int GetInt32(byte[] bytes)
        {
            if (bytes.Length != 4) throw new ArgumentException(nameof(bytes));
            if (IsMotorolaByteOrder)
            {
                // Motorola - MSB first (big endian)
                return
                    bytes[0] << 24 |
                    bytes[1] << 16 |
                    bytes[2] << 8 |
                    bytes[3];
            }
            else
            {
                // Intel ordering - LSB first (little endian)
                return
                    bytes[0] |
                    bytes[1] << 8 |
                    bytes[2] << 16 |
                    bytes[3] << 24;
            }
        }
        public virtual string GetString(byte[] bytes, Encoding encoding)
        {
            return encoding.GetString(bytes, 0, bytes.Length);
        }
   
        public abstract void CopyRemainingBytes(Stream writeStream);
    }
}
