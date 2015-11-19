using System;
using System.Threading.Tasks;

namespace NetSlimerJS
{
    public interface ISlimerJs
    {
        Task RunAsync(string jsFile);
        Task RunAsync(string jsFile, string[] jsArgs);
        Task RunAsync(string jsFile, string[] jsArgs, Action<string> outputSubscriber);
        Task RunAsync(string jsFile, Action<string> outputSubscriber);

        Task RunScriptAsync(string jsSource);
        Task RunScriptAsync(string jsSource, string[] jsArgs);
        Task RunScriptAsync(string jsSource, string[] jsArgs, Action<string> outputSubscriber);
        Task RunScriptAsync(string jsSource, Action<string> outputSubscriber);
    }
}