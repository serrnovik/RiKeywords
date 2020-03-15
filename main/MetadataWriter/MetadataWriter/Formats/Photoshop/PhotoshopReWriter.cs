using System;
using System.Collections.Generic;
using System.Text;
using DirectoryList = System.Collections.Generic.IReadOnlyList<MetadataExtractor.Directory>;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Icc;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor.Formats.Xmp;
using MetadataExtractor.IO;
using MetadataExtractor;
using MetadataExtractor.Formats.Photoshop;
using RiMetadataWriter.MetadataWriter.Formats.Jpeg;
using RiMetadataWriter.IO;
using RiMetadataWriter.MetadataWriter.IO;

namespace RiMetadataWriter.MetadataWriter.Formats.Photoshop
{
    class PhotoshopReWriter : IJpegSementMetadataReWriter
    {
        public const string JpegSegmentPreamble = "Photoshop 3.0";

        ICollection<JpegSegmentType> IJpegSegmentMetadataReader.SegmentTypes => new[] { JpegSegmentType.AppD };


        public DirectoryList ReadJpegSegments(IEnumerable<JpegSegment> segments)
        {
            throw new Exception("Will be removed");
            //return ReadJpegSegments(segments, null);
        }
        public DirectoryList ReadJpegSegments(IEnumerable<JpegSegment> segments, System.IO.Stream writer)
        {
            var preambleLength = JpegSegmentPreamble.Length;
            return segments
                .Where(segment => segment.Bytes.Length >= preambleLength + 1 && JpegSegmentPreamble == Encoding.UTF8.GetString(segment.Bytes, 0, preambleLength))
                .SelectMany(segment => Extract(new SequentialByteArrayExtendedReader(segment.Bytes, preambleLength + 1), writer, segment.Bytes.Length - preambleLength - 1))
                .ToList();
        }
        private static void Write(System.IO.Stream writer, byte[] input)
        {
            writer.Write(input);
        }

        private static void Write(System.IO.Stream writer, byte input)
        {
            writer.WriteByte(input);
        }
        public DirectoryList Extract(SequentialReWriter reader, System.IO.Stream writer, int length)
        {
            var directory = new PhotoshopDirectory();

            var directories = new List<Directory> { directory };

            // Data contains a sequence of Image Resource Blocks (IRBs):
            //
            // 4 bytes - Signature; mostly "8BIM" but "PHUT", "AgHg" and "DCSR" are also found
            // 2 bytes - Resource identifier
            // String  - Pascal string, padded to make length even
            // 4 bytes - Size of resource data which follows
            // Data    - The resource data, padded to make size even
            //
            // http://www.adobe.com/devnet-apps/photoshop/fileformatashtml/#50577409_pgfId-1037504
            var pos = 0;
            int clippingPathCount = 0;
            while (pos < length)
            {
                try
                {
                    // 4 bytes for the signature ("8BIM", "PHUT", etc.)   
                    var signatureData = reader.GetBytes(4);
                    var signature = reader.GetString(signatureData, Encoding.UTF8);
                    Write(writer, signatureData);
                    pos += 4;

                    // 2 bytes for the resource identifier (tag type).
                    var tagTypeData = reader.GetBytes(2);
                    var tagType = reader.GetUInt16(tagTypeData) ;
                    Write(writer, tagTypeData);
                    pos += 2;

                    // A variable number of bytes holding a pascal string (two leading bytes for length).
                    var descriptionLength = reader.GetByte();
                    Write(writer,descriptionLength);
                    pos += 1;

                    // Some basic bounds checking
                    if (descriptionLength + pos > length)
                        throw new ImageProcessingException("Invalid string length");

                    // Get name (important for paths)
                    var description = new StringBuilder();
                    // Loop through each byte and append to string
                    while (descriptionLength > 0)
                    {
                        var charData = reader.GetByte();
                        Write(writer, charData);
                        description.Append((char)charData);
                        pos++;
                        descriptionLength--;
                    }

                    // The number of bytes is padded with a trailing zero, if needed, to make the size even.
                    if (pos % 2 != 0)
                    {
                        var skipData = reader.GetByte();
                        Write(writer, skipData);
                        //reader.Skip(1);
                        pos++;
                    }

                    // 4 bytes for the size of the resource data that follows.
                    var byteCountData = reader.GetBytes(4);
                    var byteCount = reader.GetInt32(byteCountData);
                    Write(writer, byteCountData);
                    pos += 4;

                    // The resource data.
                    var tagBytes = reader.GetBytes(byteCount);
                    Write(writer, tagBytes);
                    pos += byteCount;

                    // The number of bytes is padded with a trailing zero, if needed, to make the size even.
                    if (pos % 2 != 0)
                    {
                        var skipData = reader.GetByte();
                        Write(writer, skipData);
                        //reader.Skip(1);
                        pos++;
                    }

                    // Skip any unsupported IRBs
                    if (signature != "8BIM")
                        continue;

                    switch (tagType)
                    {
                        case PhotoshopDirectory.TagIptc:
                            var iptcDirectory = new IptcReader().Extract(new SequentialByteArrayReader(tagBytes), tagBytes.Length);
                            //iptcDirectory.Parent = directory;
                            directories.Add(iptcDirectory);
                            break;
                        case PhotoshopDirectory.TagIccProfileBytes:
                            var iccDirectory = new IccReader().Extract(new ByteArrayReader(tagBytes));
                            //iccDirectory.Parent = directory;
                            directories.Add(iccDirectory);
                            break;
                        case PhotoshopDirectory.TagExifData1:
                        case PhotoshopDirectory.TagExifData3:
                            var exifDirectories = new ExifReader().Extract(new ByteArrayReader(tagBytes));
                            foreach (var exifDirectory in exifDirectories.Where(d => d.Parent == null))
                                //exifDirectory.Parent = directory;
                                directories.AddRange(exifDirectories);
                            break;
                        case PhotoshopDirectory.TagXmpData:
                            var xmpDirectory = new XmpReader().Extract(tagBytes);
                            //xmpDirectory.Parent = directory;
                            directories.Add(xmpDirectory);
                            break;
                        default:
                            if (tagType >= PhotoshopDirectory.TagClippingPathBlockStart && tagType <= PhotoshopDirectory.TagClippingPathBlockEnd)
                            {
                                clippingPathCount++;
                                Array.Resize(ref tagBytes, tagBytes.Length + description.Length + 1);
                                // Append description(name) to end of byte array with 1 byte before the description representing the length
                                for (int i = tagBytes.Length - description.Length - 1; i < tagBytes.Length; i++)
                                {
                                    if (i % (tagBytes.Length - description.Length - 1 + description.Length) == 0)
                                        tagBytes[i] = (byte)description.Length;
                                    else
                                        tagBytes[i] = (byte)description[i - (tagBytes.Length - description.Length - 1)];
                                }
                                //PhotoshopDirectory.TagNameMap[PhotoshopDirectory.TagClippingPathBlockStart + clippingPathCount - 1] = "Path Info " + clippingPathCount;
                                directory.Set(PhotoshopDirectory.TagClippingPathBlockStart + clippingPathCount - 1, tagBytes);
                            }
                            else
                                directory.Set(tagType, tagBytes);
                            break;
                    }

                    //if (tagType >= 0x0fa0 && tagType <= 0x1387)
                    //PhotoshopDirectory.TagNameMap[tagType] = $"Plug-in {tagType - 0x0fa0 + 1} Data";
                }
                catch (Exception ex)
                {
                    directory.AddError(ex.Message);
                    break;
                }
            }

            return directories;
        }
    }
}
