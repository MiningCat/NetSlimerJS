using System;
using NetSlimerJS.Settings;

namespace NetSlimerJS.Runner
{
    public class SlimerJsProcessProvider : ISlimerJsProcessProvider
    {
        public ISlimerJsProcces Create(SlimerJsSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            return new SlimerJsProcces(settings);
        }
    }
}