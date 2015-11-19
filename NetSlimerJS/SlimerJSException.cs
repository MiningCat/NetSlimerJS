using System;
using System.Collections.Generic;

namespace NetSlimerJS
{
    /// <summary>
    /// The exception that is thrown when SlimerJS process returns non-zero exit code
    /// </summary>
    public class SlimerJsException : Exception
    {
        public IEnumerable<string> SlimerErrors { get; }
        public int ErrorCode { get; }

        public SlimerJsException(int exitCode, IEnumerable<string> slimerErrors)
          : base($"SlimerJs exit code {exitCode}.  See the SlimerErrorsProperty for details")
        {
            SlimerErrors = slimerErrors;
            ErrorCode = exitCode;
        }
    }
}
