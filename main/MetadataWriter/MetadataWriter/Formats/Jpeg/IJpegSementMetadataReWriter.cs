using MetadataExtractor;
using MetadataExtractor.Formats.Jpeg;
using System;
using System.Collections.Generic;
using System.Text;

namespace RiMetadataWriter.MetadataWriter.Formats.Jpeg
{
    public interface IJpegSementMetadataReWriter: IJpegSegmentMetadataReader
    {
        //
        // Summary:
        //     Gets the set of JPEG segment types that this reader is interested in.
        //ICollection<JpegSegmentType> SegmentTypes { get; }

        //
        // Summary:
        //     Extracts metadata from all instances of a particular JPEG segment type.
        //
        // Parameters:
        //   segments:
        //     A sequence of JPEG segments from which the metadata should be extracted. These
        //     are in the order encountered in the original file.
        IReadOnlyList<Directory> ReadJpegSegments(IEnumerable<JpegSegment> segments, System.IO.Stream writer);
    }
}
