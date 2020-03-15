﻿using MetadataExtractor.IO;
using RiMetadataWriter.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RiMetadataWriter.MetadataWriter.IO
{
    class SequentialByteArrayExtendedReader : SequentialReWriter
    {

        private readonly byte[] _bytes;

        private int _index;

        public override long Position => _index;

        public SequentialByteArrayExtendedReader(byte[] bytes, int baseIndex = 0, bool isMotorolaByteOrder = true)
            : base(isMotorolaByteOrder)
        {
            _bytes = bytes ?? throw new ArgumentNullException(nameof(bytes));
            _index = baseIndex;
        }

        public override byte GetByte()
        {
            if (_index >= _bytes.Length)
                throw new IOException("End of data reached.");

            return _bytes[_index++];
        }

        public override SequentialReader WithByteOrder(bool isMotorolaByteOrder) =>
            isMotorolaByteOrder == IsMotorolaByteOrder ? this : new SequentialByteArrayExtendedReader(_bytes, _index, isMotorolaByteOrder);

        public override byte[] GetBytes(int count)
        {
            if (_index + count > _bytes.Length)
                throw new IOException("End of data reached.");

            var bytes = new byte[count];
            Array.Copy(_bytes, _index, bytes, 0, count);
            _index += count;
            return bytes;
        }

        public override void GetBytes(byte[] buffer, int offset, int count)
        {
            if (_index + count > _bytes.Length)
                throw new IOException("End of data reached.");

            Array.Copy(_bytes, _index, buffer, offset, count);
            _index += count;
        }

        public override void Skip(long n)
        {
            if (n < 0)
                throw new ArgumentException("n must be zero or greater.");

            if (_index + n > _bytes.Length)
                throw new IOException("End of data reached.");

            _index += unchecked((int)n);
        }

        public override bool TrySkip(long n)
        {
            if (n < 0)
                throw new ArgumentException("n must be zero or greater.");

            _index += unchecked((int)n);

            if (_index > _bytes.Length)
            {
                _index = _bytes.Length;
                return false;
            }

            return true;
        }

        public override int Available()
        {
            return _bytes.Length - _index;
        }

        public override bool IsCloserToEnd(long numberOfBytes)
        {
            return _index + numberOfBytes > _bytes.Length;
        }

        public override void CopyRemainingBytes(Stream writeStream)
        {
            throw new NotImplementedException();
        }
    }

}
