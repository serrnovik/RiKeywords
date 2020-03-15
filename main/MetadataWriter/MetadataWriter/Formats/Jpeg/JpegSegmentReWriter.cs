using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MetadataExtractor.IO;
using MetadataExtractor.Formats.Jpeg;
using RiMetadataWriter.IO;
using RiMetadataWriter.MetadataWriter.IO;
using RiMetadataWriter.MetadataWriter.Formats.Jpeg;

namespace RiMetadataWriter.Formats.Jpeg.Writer
{
    public class JpegSegmentReWriter
    {

        private static void Write(Stream writer, byte[] input)
        {
            writer.Write(input);
        }

        private static void Write(Stream writer, byte input)
        {
            writer.WriteByte(input);
        }

        public static IEnumerable<JpegSegment> ReadSegments(SequentialStreamReWriter reader, Stream writer
            , ICollection<IJpegSementMetadataReWriter> segmentProcessors )
        {
            if (segmentProcessors == null || segmentProcessors.Count() <= 0)
            {
                throw new ArgumentException(nameof(segmentProcessors));
            }
            // Build the union of segment types desired by all readers
            var segmentTypes = new HashSet<JpegSegmentType>(segmentProcessors.SelectMany(reader => reader.SegmentTypes));


            if (!reader.IsMotorolaByteOrder)
                throw new JpegProcessingException("Must be big-endian/Motorola byte order.");

            // first two bytes should be JPEG magic number
            var magicTupleData = reader.GetBytes(2);
            var magicNumber = reader.GetUInt16(magicTupleData);
            Write(writer, magicTupleData);

            if (magicNumber != 0xFFD8)
                throw new JpegProcessingException($"JPEG data is expected to begin with 0xFFD8 (ÿØ) not 0x{magicNumber:X4}");

            do
            {
                // Find the segment marker. Markers are zero or more 0xFF bytes, followed
                // by a 0xFF and then a byte not equal to 0x00 or 0xFF.
                var segmentIdentifier = reader.GetByte();
                Write(writer, segmentIdentifier);
                var segmentTypeByte = reader.GetByte();
                Write(writer, segmentTypeByte);

                // Read until we have a 0xFF byte followed by a byte that is not 0xFF or 0x00
                while (segmentIdentifier != 0xFF || segmentTypeByte == 0xFF || segmentTypeByte == 0)
                {
                    segmentIdentifier = segmentTypeByte;
                    segmentTypeByte = reader.GetByte();
                    Write(writer, segmentTypeByte);
                }

                var segmentType = (JpegSegmentType)segmentTypeByte;



                if (segmentType == JpegSegmentType.Eoi)
                {
                    // the 'End-Of-Image' segment                   
                    yield break;
                }

                // next 2-bytes are <segment-size>: [high-byte] [low-byte]
                var segmentLenghtDataData = reader.GetBytes(2);
                var segmentLenghtData = reader.GetUInt16(segmentLenghtDataData);
                Write(writer, segmentLenghtDataData);

                var segmentLength = (int)segmentLenghtData;
                // segment length includes size bytes, so subtract two
                segmentLength -= 2;

                if (segmentLength < 0)
                    throw new JpegProcessingException("JPEG segment size would be less than zero");

                if (segmentType == JpegSegmentType.Sos)
                {
                    // The 'Start-Of-Scan' segment's length doesn't include the image data, instead would
                    // have to search for the two bytes: 0xFF 0xD9 (EOI).
                    // It comes last so simply return at this point
                    reader.CopyRemainingBytes(writer);

                    yield break;
                }

                // Check whether we are interested in this segment
                if ( segmentTypes.Contains(segmentType))
                {
                    var segmentOffset = reader.Position;
                    var segmentBytes = reader.GetBytes(segmentLength);
                    //Write(writer, segmentBytes); // components are responsable for writing themselfs
                    Debug.Assert(segmentLength == segmentBytes.Length);

                    var orignialSegmentContent = new JpegSegment(segmentType, segmentBytes, segmentOffset);
                    var segmentProcessor = segmentProcessors.FirstOrDefault(x => x.SegmentTypes.Contains(segmentType));

                    // should not happen
                    if (segmentProcessor == null) throw new InvalidOperationException("Segment processor for {segmentType} not found");

                    var result = segmentProcessor.ReadJpegSegments(new JpegSegment[] { orignialSegmentContent }, writer);

                    yield return orignialSegmentContent; // [sno] todo - remove return here
                }
                else
                {
                    // Any other segment
                    var segmentBytes = reader.GetBytes(segmentLength);
                    Write(writer, segmentBytes);
                }
            }
            while (true);
        }
    }
}