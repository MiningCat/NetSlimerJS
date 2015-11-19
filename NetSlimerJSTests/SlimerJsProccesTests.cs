using System;
using FluentAssertions;
using NetSlimerJS;
using NetSlimerJS.Runner;
using NetSlimerJSTests.Attributes;
using Ploeh.AutoFixture.Idioms;
using Xunit;

namespace NetSlimerJSTests
{
    public class SlimerJsProccesTests
    {
        [Theory, AutoMoqData]
        public void ClassGuardClause(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(SlimerJsProcces));
        }

        [Theory, AutoMoqData]
        public void RunAsync_ExceptionThrown_CorrectlyHandled(SlimerJsProcces sut, string args)
        {
            sut.Awaiting(s => s.RunAsync(args, a => { })).ShouldThrow<Exception>();
        }
    }
}
