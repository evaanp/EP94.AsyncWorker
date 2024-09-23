using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EP94.AsyncWorker.Internal.Utils
{
    internal static class DebugUtils
    {
        public static string CreateDisplayName(string memberName, string sourceFilePath, int lineNumber)
        {
            return $"File: '{sourceFilePath}', method: '{memberName}', line: '{lineNumber}'";
        }
    }
}
