using System;
using System.Threading.Tasks;

namespace NetSlimerJS.Runner
{
    public interface ISlimerJsProcces
    {
        Task RunAsync(string args, Action<string> subscriber);
        void Abort();
    }
}