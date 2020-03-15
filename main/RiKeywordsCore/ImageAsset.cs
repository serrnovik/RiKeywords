
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using RiMetadataWriter.Formats.Jpeg.Writer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Abstractions;
using System.Linq;


namespace RiKeywordsCore
{
    public class ImageAsset
    {
        private readonly IFileSystem fileSystem;
        public string FilePath { get; }

        public string Title { get; private set; }
        public string Description { get; private set; }

        public List<string> Keywords { get; private set; }

        public ImageAsset(IFileSystem fileSystem, string filePath)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            if (!fileSystem.File.Exists(filePath)) throw new FileNotFoundException(filePath);

            FilePath = filePath;

            Title = "";
            Description = "";
            Keywords = new List<string>();
        }

        public void ReadMetaData()
        {
            if (!fileSystem.File.Exists(this.FilePath)) throw new FileNotFoundException(this.FilePath);

            using (var fs = fileSystem.File.OpenRead(FilePath))
            {
                using (var fsWrite = System.IO.File.OpenWrite(@"C:\tmp\wrongFileName.jpeg"))
                {
                    //var metadata = (JpegMetadataReader.ReadMetadata(fs));
                    var metadata = (JpegMetadataReWriter.ReadMetadata(fs, fsWrite));


                    //var iFD0Directory = metadata.OfType<ExifIfd0Directory>().FirstOrDefault();
                    //var exifIfd0Descriptor = new ExifIfd0Descriptor(iFD0Directory);
                    //var Ifd0Description = exifIfd0Descriptor.GetDescription(ExifIfd0Directory.TagImageDescription);
                    //var Ifd0Keywords = exifIfd0Descriptor.GetDescription(ExifIfd0Directory.TagWinKeywords);
                    //var Ifd0Title = exifIfd0Descriptor.GetDescription(ExifIfd0Directory.TagWinTitle);

                    //var subIfdDirectory = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                    //var subIfdDescriptor = new ExifSubIfdDescriptor(subIfdDirectory);
                    // returns something like 4261 pixels
                    //var dagExifImageWidth = subIfdDescriptor.GetDescription(ExifDirectoryBase.TagExifImageWidth);
                    //var dagExifImageHeight = subIfdDescriptor.GetDescription(ExifDirectoryBase.TagExifImageHeight);

                    //var thumbnailDirectory = metadata.OfType<ExifThumbnailDirectory>().FirstOrDefault();
                    //var thumbnailDescriptor = new ExifThumbnailDescriptor(thumbnailDirectory);
                    //var thumbnailOffset = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagThumbnailOffset);
                    //var thumbnailYResolution = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagYResolution);
                    //var thumbnailXResolution = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagXResolution);
                    //var thumbnailLength = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagThumbnailLength);
                    //var thumbnailCompression = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagCompression);



                    //var IPTCDirectory = metadata.OfType<IptcDirectory>().FirstOrDefault();                    
                    //var iptcDescriptor = new IptcDescriptor(IPTCDirectory);


                    //var IPTCDescription = iptcDescriptor.GetDescription(IptcDirectory.TagCaption);
                    //var IPTCKeywords = iptcDescriptor.GetDescription(IptcDirectory.TagKeywords); // like "TestFile1Kw1;TestFile1Kw2;TestFile1Kw3;TestFile1Kw4"
                    //var IPTCTitle = iptcDescriptor.GetDescription(IptcDirectory.TagObjectName);

                    ////var keywordsAsString = string.IsNullOrWhiteSpace(Ifd0Keywords) ? IPTCKeywords : Ifd0Keywords;
                    ////Keywords = keywordsAsString?.Split(';').ToList();

                    ////Title = string.IsNullOrWhiteSpace(Ifd0Title) ? IPTCTitle : Ifd0Title;
                    ////Description = string.IsNullOrWhiteSpace(Ifd0Description) ? IPTCDescription : Ifd0Description;    


                    //var keywordsAsString = IPTCKeywords;
                    //Keywords = keywordsAsString?.Split(';').ToList();

                    //Title = IPTCTitle;
                    //Description = IPTCDescription;
                }
                //return new Tuple<string, string, string[]>(title, description, arrayOrKeywords);

                //using (var stream = fileSystem.File.OpenRead(FilePath))
                //{

                //    var theImage = new System.Drawing.Bitmap(stream);
                //   var propItems = theImage.PropertyItems;


                //}



            }




        }
    }
}