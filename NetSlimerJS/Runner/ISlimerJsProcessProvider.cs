using NetSlimerJS.Settings;

namespace NetSlimerJS.Runner
{
    public interface ISlimerJsProcessProvider
    {
        ISlimerJsProcces Create(SlimerJsSettings settings);
    }
}