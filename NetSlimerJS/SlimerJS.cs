using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NetSlimerJS.Runner;
using NetSlimerJS.Settings;

namespace NetSlimerJS
{
    public class SlimerJs : ISlimerJs
    {
        private readonly SlimerJsSettings _slimerJsSettings;
        private readonly ISlimerJsProcessProvider _slimerJsProcessProvider;

        public SlimerJs() : this(new SlimerJsSettings())
        {
        }

        public SlimerJs(SlimerJsSettings slimerJsSettings):this(slimerJsSettings, new SlimerJsProcessProvider())
        {
        }

        public SlimerJs(SlimerJsSettings slimerJsSettings, ISlimerJsProcessProvider slimerJsProcessProvider)
        {
            if (slimerJsSettings == null) throw new ArgumentNullException(nameof(slimerJsSettings));
            if (slimerJsProcessProvider == null) throw new ArgumentNullException(nameof(slimerJsProcessProvider));

            _slimerJsSettings = slimerJsSettings;
            _slimerJsProcessProvider = slimerJsProcessProvider;
        }

        public Task RunAsync(string jsFile, string[] jsArgs, Action<string> outputSubscriber)
        {
            if (jsFile == null) throw new ArgumentNullException(nameof(jsFile));
            if (jsArgs == null) throw new ArgumentNullException(nameof(jsArgs));
            if (outputSubscriber == null) throw new ArgumentNullException(nameof(outputSubscriber));
            if(string.IsNullOrWhiteSpace(jsFile)) throw new ArgumentException("jsFile can't empty");

            var argString = CreateArgString(jsFile, jsArgs);

            var process = _slimerJsProcessProvider.Create(_slimerJsSettings);

            return process.RunAsync(argString, outputSubscriber);
        }

        public Task RunScriptAsync(string jsSource, string[] jsArgs, Action<string> outputSubscriber)
        {
            if (jsSource == null) throw new ArgumentNullException(nameof(jsSource));
            if (jsArgs == null) throw new ArgumentNullException(nameof(jsArgs));
            if (outputSubscriber == null) throw new ArgumentNullException(nameof(outputSubscriber));

            var tmpJsFilePath = Path.Combine(_slimerJsSettings.ToolPath, "TempScript_" + Path.GetRandomFileName() + ".js");
            File.WriteAllBytes(tmpJsFilePath, Encoding.UTF8.GetBytes(jsSource));

            try
            {
                return RunAsync(tmpJsFilePath, jsArgs, outputSubscriber).ContinueWith(t => DeleteFileIfExists(tmpJsFilePath));
            }
            catch
            {
                DeleteFileIfExists(tmpJsFilePath);
                throw;
            }
        }

        public Task RunAsync(string jsFile, string[] jsArgs) => RunAsync(jsFile, jsArgs, a => { });

        public Task RunAsync(string jsFile, Action<string> outputSubscriber) => RunAsync(jsFile, new string[0], outputSubscriber);

        public Task RunAsync(string jsFile) => RunAsync(jsFile, new string[0], a => { });

        public Task RunScriptAsync(string jsSource, string[] jsArgs) => RunScriptAsync(jsSource, jsArgs, a => { });

        public Task RunScriptAsync(string jsSource, Action<string> outputSubscriber) => RunScriptAsync(jsSource, new string[0], outputSubscriber);

        public Task RunScriptAsync(string jsSource) => RunScriptAsync(jsSource, new string[0], a => { });

        private static string PrepareCmdArg(string arg) => $"\"{arg.Replace("\"", "\\\"")}\"";

        private static string CreateArgString(string jsFile, IEnumerable<string> jsArgs)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat(" {0}", PrepareCmdArg(jsFile));
            foreach (var arg in jsArgs)
                stringBuilder.AppendFormat(" {0}", PrepareCmdArg(arg));
            return stringBuilder.ToString();
        }

        private static void DeleteFileIfExists(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            File.Delete(filePath);
        }
    }
}