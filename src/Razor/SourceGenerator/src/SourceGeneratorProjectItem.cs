using System.Buffers;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.CodeAnalysis.Razor
{
    internal class SourceGeneratorProjectItem : RazorProjectItem
    {
        private readonly string _fileKind;
        private byte[] _utf8Bytes;

        public SourceGeneratorProjectItem(string basePath, string filePath, string relativePhysicalPath, string fileKind, AdditionalText additionalText, string cssScope)
        {
            BasePath = basePath;
            FilePath = filePath;
            RelativePhysicalPath = relativePhysicalPath;
            _fileKind = fileKind;
            AdditionalText = additionalText;
            CssScope = cssScope;
            var text = AdditionalText.GetText();
            RazorSourceDocument = new SourceTextRazorSourceDocument(filePath, relativePhysicalPath, text);
        }

        public AdditionalText AdditionalText { get; }

        public override string BasePath { get; }

        public override string FilePath { get; }

        public override bool Exists => true;

        public override string PhysicalPath => AdditionalText.Path;

        public override string RelativePhysicalPath { get; }

        public override string FileKind => _fileKind ?? base.FileKind;

        public override string CssScope { get; }

        public override Stream Read()
        {
            if (_utf8Bytes is not null)
            {
                return new MemoryStream(_utf8Bytes);
            }

            var pool = ArrayPool<char>.Shared;
            var text = AdditionalText.GetText();
            var chars = pool.Rent(text.Length);
            text.CopyTo(0, chars, 0, text.Length);

            _utf8Bytes = Encoding.UTF8.GetBytes(chars, 0, text.Length);
            pool.Return(chars);

            return new MemoryStream(_utf8Bytes);
        }
    }
}
