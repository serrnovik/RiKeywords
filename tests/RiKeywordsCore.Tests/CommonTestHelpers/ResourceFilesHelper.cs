using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RiKeywordsCore.Tests
{
    public class ResourceFilesHelper
    {
        public ResourceFilesHelper()
        {

        }
        public byte[] GetTextFileConenetFromResource(Type type, string path)
        {
            byte[] fxRate;
            var assembly = GetType().GetTypeInfo().Assembly;

            using (var stream = assembly.GetManifestResourceStream(type, path))
            {
                stream.Should().NotBeNull($@"Could not load steram from embeded resource file {path}");

                fxRate = new byte[stream.Length];
                stream.Read(fxRate, 0, fxRate.Length);
            }

            return fxRate;
        }
    }
}
