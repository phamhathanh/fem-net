using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FEM_NET.Examples
{
    internal class MirrorWriter : TextWriter
    {
        private readonly IEnumerable<TextWriter> writers;

        public override Encoding Encoding { get; }

        public MirrorWriter(params TextWriter[] writers)
        {
            if (writers.Length == 0)
                throw new ArgumentException("There must be at least one writer.");
            this.writers = writers;

            Encoding = writers[0].Encoding;
        }

        public override void Write(char value)
        {
            foreach (var writer in writers)
                writer.Write(value);
        }

        public override void Flush()
        {
            foreach (var writer in writers)
                writer.Flush();
        }
    }
}