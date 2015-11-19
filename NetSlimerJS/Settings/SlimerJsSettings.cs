using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NetSlimerJS.Settings
{
    public class SlimerJsSettings
    {
        public string SlimerJsExeName { get; set; } = @"SlimerJS\slimerjs.bat";
        public string ToolPath { get; set; } = GetAppLocation();
        public ProcessPriorityClass ProcessPriority { get; set; } = ProcessPriorityClass.Normal;
        public string CustomArgs { get; set; } = string.Empty;

        private static string GetAppLocation()
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}