using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace RiKeywordsCore
{
    public class ImageAsset
    {
        private readonly IFileSystem fileSystem;
        public string FilePath { get; }

        public string Title { get; set; }
        public string Description { get; set; }

        public List<string> Keywords { get; }

        public ImageAsset(IFileSystem fileSystem, string filePath)
        {
            this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            if (string.IsNullOrWhiteSpace(  filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }

            if (!fileSystem.File.Exists(filePath)) throw new FileNotFoundException(filePath);
            
            FilePath = filePath;
        }

    }
}
