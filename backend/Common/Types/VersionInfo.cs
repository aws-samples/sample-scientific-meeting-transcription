using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Serilog.Core;

namespace Common.Types
{
    public static class VersionInfo
    {
        public static string Version => "1.1.5";
        public static string BuildDate => "2025-03-17";
    }
}
