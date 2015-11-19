using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NetSlimerJS.Settings;

namespace NetSlimerJS.Runner
{
    public class SlimerJsProcces : ISlimerJsProcces
    {
        private readonly SlimerJsSettings _slimerJsSettings;
        private readonly Process _slimerJsProcess;
        private readonly List<string> _slimerErrors; 

        public SlimerJsProcces(SlimerJsSettings slimerJsSettings)
        {
            if (slimerJsSettings == null) throw new ArgumentNullException(nameof(slimerJsSettings));
            _slimerJsSettings = slimerJsSettings;
            _slimerJsProcess = new Process() { EnableRaisingEvents = true, StartInfo = CreateProcessStartInfo() };
            _slimerErrors = new List<string>();
        }

        public Task RunAsync(string args, Action<string> subscriber)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));

            var tcs = new TaskCompletionSource<bool>();

            if (_slimerJsSettings.CustomArgs != null)
                args = $"{_slimerJsSettings.CustomArgs} {args}";

            _slimerJsProcess.StartInfo.Arguments = args;

            try
            {
                _slimerJsProcess.Start();
                ChangePriority();
                HandleOutput(subscriber);
                HandleExit(tcs);
            }
            catch (Exception exception)
            {
                tcs.SetException(exception);
                throw;
            }

            return tcs.Task;
        }

        private void ChangePriority()
        {
            if(_slimerJsProcess.PriorityClass != _slimerJsSettings.ProcessPriority)
                _slimerJsProcess.PriorityClass = _slimerJsSettings.ProcessPriority;
        }

        private void HandleExit(TaskCompletionSource<bool> tcs)
        {
            _slimerJsProcess.Exited += (sender, eventArgs) =>
            {
                if (_slimerJsProcess.ExitCode != 0)
                    tcs.SetException(new SlimerJsException(_slimerJsProcess.ExitCode, _slimerErrors));

                tcs.SetResult(true);
                _slimerJsProcess.Close();
                _slimerJsProcess.Dispose(); //poop
            };
        }

        private void HandleOutput(Action<string> subscriber)
        {

            _slimerJsProcess.OutputDataReceived +=
                    ((sender, eventArgs) => { if (eventArgs?.Data != null) subscriber(eventArgs.Data); });
            _slimerJsProcess.BeginOutputReadLine();


            _slimerJsProcess.ErrorDataReceived +=
                ((sender, eventArgs) => { if (eventArgs?.Data != null) _slimerErrors.Add(eventArgs.Data); });
            _slimerJsProcess.BeginErrorReadLine();
        }

        public void Abort()
        {
            if (_slimerJsProcess.HasExited)
                return;

            _slimerJsProcess.Kill();
        }

        private ProcessStartInfo CreateProcessStartInfo()
        {
            var slimerPath = Path.Combine(_slimerJsSettings.ToolPath, _slimerJsSettings.SlimerJsExeName);
            var processStartInfo = new ProcessStartInfo(slimerPath)
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = _slimerJsSettings.ToolPath,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            return processStartInfo;
        }
    }
}