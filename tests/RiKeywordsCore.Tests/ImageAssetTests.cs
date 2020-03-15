using FluentAssertions;
using NUnit.Framework;
using RiKeywordsCore.Tests.TestFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.Iptc;
using MetadataExtractor.Formats.Jpeg;
using System.Linq;
using System.IO.Abstractions;

namespace RiKeywordsCore.Tests
{
    public class Tests
    {

        const string testJpegFile1Description = "Test Description of Test file 1";
        const string testJpegFile1Title = "Test Title file 1";
        private string[] testJpegFile1Keywords = new string[] { "TestFile1Kw1", "TestFile1Kw2", "TestFile1Kw3", "TestFile1Kw4" };
        const string wrongFilePath = @"C:\wrongFileName.jpeg";
        const string mockFolderParth = @"C:\someFolder\";
        const string TestJpegFile1FileName = @"TestJpegFile1.jpg";
        const string TestFileWithRealMetadataFileName = @"TestFileWithRealMetadata.jpg";
        const string TestJpegFile2EmptyMetadataFileName = @"TestJpegFile2EmptyMetadata.jpg";
        const string correctTestFileName = TestJpegFile1FileName;
        private string correctTestFilePath;
        private ResourceFilesHelper resourceFilesHelper;
        private byte[] jpegTestFile1;


        private MockFileSystem testFileSystem;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            resourceFilesHelper = new ResourceFilesHelper();
            correctTestFilePath = $"{mockFolderParth}{correctTestFileName}";
            jpegTestFile1 = resourceFilesHelper.GetTextFileConenetFromResource(typeof(TestFilesAnchor), TestJpegFile1FileName);
        }

        [SetUp]
        public void Setup()
        {

            testFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                  { Path.Combine(mockFolderParth, correctTestFileName), new MockFileData(jpegTestFile1)}
            });
        }

        [Test]
        public void ImageAsset_InitializeWithWrongFileName_ExpectException()
        {
            Assert.Throws<ArgumentException>(() => new ImageAsset(testFileSystem, " "));

        }
        [Test]
        public void ImageAsset_InitializeWithNull_ExpectException()
        {
            Assert.Throws<ArgumentException>(() => new ImageAsset(testFileSystem, null));
        }

        [Test]
        public void ImageAsset_InitializeNullFileSystem_ExpectException()
        {
            Assert.Throws<ArgumentNullException>(() => new ImageAsset(null, correctTestFilePath));

        }
        [Test]
        public void ImageAsset_InitializeNonExistingFile_ExpectException()
        {
            Assert.Throws<FileNotFoundException>(() => new ImageAsset(testFileSystem, wrongFilePath));
        }
        [Test]
        public void ImageAsset_InitializeExistingFile_ShouldNotFail()
        {
            Action a = () => new ImageAsset(testFileSystem, correctTestFilePath);
            a.Should().NotThrow();
        }
        [Test]
        public void ImageAsset_InitializeWithFile_ButDoNotLoad_VerifyEmptyFields()
        {
            var sut = new ImageAsset(testFileSystem, testFileSystem.Path.Combine(mockFolderParth, TestJpegFile1FileName));
            sut.Should().NotBeNull();
            sut.Title.Should().Be(string.Empty);
            sut.Description.Should().Be(string.Empty);
            sut.Keywords.Should().BeEquivalentTo(new List<string>());
        }


        [Test]
        public void ImageAsset_InitializeWithExistingFile_RemoveFileAndTryRead_ExpectException()
        {
            var testFile = testFileSystem.Path.Combine(mockFolderParth, TestJpegFile1FileName);
            var sut = new ImageAsset(testFileSystem, testFile);
            sut.Should().NotBeNull();
            sut.Title.Should().Be(string.Empty);
            this.testFileSystem.RemoveFile(testFile);
            Assert.Throws<FileNotFoundException>(() => sut.ReadMetaData());
        }

        [Test]
        public void ImageAsset_LoadTestFileWithKnownMetadata_VerifyReadCorrectly()
        {

            var sut = new ImageAsset(testFileSystem, testFileSystem.Path.Combine(mockFolderParth, TestJpegFile1FileName));
            sut.Should().NotBeNull();

            // just double check setup
            sut.Title.Should().Be(string.Empty);
            sut.Description.Should().Be(string.Empty);
            sut.Keywords.Should().BeEquivalentTo(new List<string>());

            sut.ReadMetaData();

            sut.Title.Should().Be(testJpegFile1Title);
            sut.Description.Should().Be(testJpegFile1Description);
            sut.Keywords.Should().BeEquivalentTo(testJpegFile1Keywords);
        }


        private static Tuple<string, string, string[]> AlternativeReadingMetadataMethod(IFileSystem fileSystem, string filePath)
        {
            using var fs = fileSystem.File.OpenRead(filePath);
            var metadata = (JpegMetadataReader.ReadMetadata(fs));

            var iFD0Directory = metadata.OfType<ExifIfd0Directory>().FirstOrDefault();
            var exifIfd0Descriptor = new ExifIfd0Descriptor(iFD0Directory);
            var Ifd0Description = exifIfd0Descriptor.GetDescription(ExifIfd0Directory.TagImageDescription);
            var Ifd0Keywords = exifIfd0Descriptor.GetDescription(ExifIfd0Directory.TagWinKeywords);
            var Ifd0Title = exifIfd0Descriptor.GetDescription(ExifIfd0Directory.TagWinTitle);

            //var subIfdDirectory = metadata.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            //var subIfdDescriptor = new ExifSubIfdDescriptor(subIfdDirectory);
            // returns something like 4261 pixels
            //var dagExifImageWidth = subIfdDescriptor.GetDescription(ExifDirectoryBase.TagExifImageWidth);
            //var dagExifImageHeight = subIfdDescriptor.GetDescription(ExifDirectoryBase.TagExifImageHeight);

            var thumbnailDirectory = metadata.OfType<ExifThumbnailDirectory>().FirstOrDefault();
            var thumbnailDescriptor = new ExifThumbnailDescriptor(thumbnailDirectory);
            var thumbnailOffset = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagThumbnailOffset);
            var thumbnailYResolution = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagYResolution);
            var thumbnailXResolution = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagXResolution);
            var thumbnailLength = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagThumbnailLength);
            var thumbnailCompression = thumbnailDescriptor.GetDescription(ExifThumbnailDirectory.TagCompression);



            var IPTCDirectory = metadata.OfType<IptcDirectory>().FirstOrDefault();
            var iptcDescriptor = new IptcDescriptor(IPTCDirectory);


            var IPTCDescription = iptcDescriptor.GetDescription(IptcDirectory.TagCaption);
            var IPTCKeywords = iptcDescriptor.GetDescription(IptcDirectory.TagKeywords); // like "TestFile1Kw1;TestFile1Kw2;TestFile1Kw3;TestFile1Kw4"
            var IPTCTitle = iptcDescriptor.GetDescription(IptcDirectory.TagObjectName);

            var keywordsAsString = string.IsNullOrWhiteSpace(Ifd0Keywords) ? IPTCKeywords : Ifd0Keywords;
            var arrayOrKeywords = keywordsAsString?.Split(';');

            var title = string.IsNullOrWhiteSpace(Ifd0Title) ? IPTCTitle : Ifd0Title;
            var description = string.IsNullOrWhiteSpace(Ifd0Description) ? IPTCDescription : Ifd0Description;
            return new Tuple<string, string, string[]>(title, description, arrayOrKeywords);
        }
    }
}