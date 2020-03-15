using System.Collections.Generic;
//using System.IO;
using System.Linq;
using MetadataExtractor.Formats.Adobe;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Icc;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jfif;
using MetadataExtractor.Formats.Jfxx;
using MetadataExtractor.Formats.Photoshop;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.Xmp;
using MetadataExtractor.IO;
using System.Text;
using DirectoryList = System.Collections.Generic.IReadOnlyList<MetadataExtractor.Directory>;
using MetadataExtractor.Formats.Jpeg;
using MetadataExtractor;
using System;
using RiMetadataWriter.IO;
using RiMetadataWriter.MetadataWriter.Formats.Photoshop;
using RiMetadataWriter.MetadataWriter.IO;
using RiMetadataWriter.MetadataWriter.Formats.Jpeg;

namespace RiMetadataWriter.Formats.Jpeg.Writer
{
    public class JpegMetadataReWriter
    //public static class JpegMetadataReader
    {
        private static readonly ICollection<IJpegSementMetadataReWriter> _allReaders = new IJpegSementMetadataReWriter[]
        {
            //new JpegReader(),
            //new JpegCommentReader(),
            //new JfifReader(),
            //new JfxxReader(),
            //new ExifReader(),
            //new XmpReader(),
            //new IccReader(),
            //new PhotoshopReader(),
            //new PhotoshopReWriter(),
            //new DuckyReader(),
            //new IptcReader(),
            //new IptcReWriter(),
            //new AdobeJpegReader(),
            //new JpegDhtReader(),
            //new JpegDnlReader()
        };

        public static ICollection<IJpegSementMetadataReWriter> AllReaders => _allReaders;

        /// <exception cref="JpegProcessingException"/>
        /// <exception cref="System.IO.IOException"/>
        public static DirectoryList ReadMetadata(System.IO.Stream stream, System.IO.Stream writeStream, ICollection<IJpegSementMetadataReWriter>? readers = null)
        {
            return Process(stream, writeStream, readers);
        }

        /// <exception cref="JpegProcessingException"/>
        /// <exception cref="System.IO.IOException"/>
        public static DirectoryList ReadMetadata(string filePath, ICollection<IJpegSementMetadataReWriter>? readers = null)
        {
            throw new NotImplementedException();
        }

        /// <exception cref="JpegProcessingException"/>
        /// <exception cref="System.IO.IOException"/>
        public static DirectoryList Process(System.IO.Stream stream, System.IO.Stream writeStream, ICollection<IJpegSementMetadataReWriter>? readers = null)
        {
            if (readers == null)
                readers = AllReaders;


            var sequentialStreamReWriter = new SequentialStreamReWriter(stream);
            // Read out those segments
            var segments = JpegSegmentReWriter.ReadSegments(sequentialStreamReWriter, writeStream, readers);

            // Process them
            //return ProcessJpegSegments(readers, segments.ToList());
            return null;
        }

        //public static DirectoryList ProcessJpegSegments(IEnumerable<IJpegSementMetadataReWriter> readers, ICollection<JpegSegment> segments)
        //{
        //    var directories = new List<Directory>();

        //    foreach (var reader in readers)
        //    {
        //        var readerSegmentTypes = reader.SegmentTypes;
        //        var readerSegments = segments.Where(s => readerSegmentTypes.Contains(s.Type));
        //        directories.AddRange(reader.ReadJpegSegments(readerSegments));
        //    }

        //    return directories;
        //}
    
}
}
