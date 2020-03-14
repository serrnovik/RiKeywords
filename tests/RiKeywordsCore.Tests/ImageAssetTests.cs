using FluentAssertions;
using NUnit.Framework;
using RiKeywordsCore.Tests.TestFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;

namespace RiKeywordsCore.Tests
{
    public class Tests
    {
        const string wrongFilePath = @"C:\wrongFileName.jpeg";
        const string mockFolderParth = @"C:\someFolder\";
        const string TestJpegFile1FileName = @"TestJpegFile1.jpg";
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
    }
}