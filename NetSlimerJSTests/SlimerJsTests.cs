using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NetSlimerJS;
using NetSlimerJS.Runner;
using NetSlimerJS.Settings;
using NetSlimerJSTests.Attributes;
using Ploeh.AutoFixture.Idioms;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace NetSlimerJSTests
{
    public class SlimerJsTests
    {
        [Theory, AutoMoqData]
        public void ClassGuardClause(GuardClauseAssertion assertion)
        {
            assertion.Verify(typeof(SlimerJs));
        }

        [Theory, AutoMoqData]
        public void RunAsync_EmptyFilePath_ArgumentException(
            [Frozen]Mock<ISlimerJsProcessProvider> slimerJsProcessProvider,
            Mock<ISlimerJsProcces> slimerJsProcces,
            [Greedy]SlimerJs sut)
        {
            //arrange
            slimerJsProcces.Setup(p => p.RunAsync(It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Task.FromResult(0));
            slimerJsProcessProvider.Setup(p => p.Create(It.IsAny<SlimerJsSettings>())).Returns(slimerJsProcces.Object);

            //act
             sut.Awaiting(s => s.RunAsync(" ")).ShouldThrow<ArgumentException>();
        }

        [Theory]
        [InlineAutoMoqData("fat slimer.js", new string[0], " \"fat slimer.js\"")]
        [InlineAutoMoqData("clutch.band", new string[0], " \"clutch.band\"")]
        [InlineAutoMoqData("clutch.band", new[] {"r", "o", "c", "k"}, " \"clutch.band\" \"r\" \"o\" \"c\" \"k\"")]
        public async void RunAsync_AnyFile_CorrectParameterString(
            string filename, 
            string[] parameters,
            string parameterString,
            [Frozen]Mock<ISlimerJsProcessProvider> slimerJsProcessProvider,
            Mock<ISlimerJsProcces> slimerJsProcces,
            [Greedy]SlimerJs sut)
        {
            //arrange
            slimerJsProcces.Setup(p => p.RunAsync(It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Task.FromResult(0));
            slimerJsProcessProvider.Setup(p => p.Create(It.IsAny<SlimerJsSettings>())).Returns(slimerJsProcces.Object);

            //act
            await sut.RunAsync(filename, parameters);

            //assert
            slimerJsProcces.Verify(p => p.RunAsync(It.Is<string>(a => a == parameterString), It.IsAny<Action<string>>()));
        }

        [Theory, AutoMoqData]
        public void RunScriptAsync_AnyScript_CorrectScriptFileContent(
            Mock<ISlimerJsProcessProvider> slimerJsProcessProvider,
            Mock<ISlimerJsProcces> slimerJsProcces,
            CancellationTokenSource cancellationTokenSource,
            string script)
        {
            //arrange
            string filePath = null;
            var sut = new SlimerJs(new SlimerJsSettings(), slimerJsProcessProvider.Object);
            slimerJsProcces.Setup(p => p.RunAsync(It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Task.Delay(TimeSpan.FromMinutes(5), cancellationTokenSource.Token))
                .Callback<string, Action<string>>((arg, sub) => { filePath = arg; });
            slimerJsProcessProvider.Setup(p => p.Create(It.IsAny<SlimerJsSettings>())).Returns(slimerJsProcces.Object);

            //act
            var task = sut.RunScriptAsync(script);
            Task.Factory.StartNew(() => task).Unwrap();

            //assert
            filePath.Should().NotBeNullOrEmpty();
            filePath = filePath.Replace("\"", "");
            File.Exists(filePath).Should().BeTrue();
            File.ReadAllText(filePath).Should().Be(script);
            cancellationTokenSource.Cancel();
        }

        [Theory, AutoMoqData]
        public async void RunScriptAsync_AnyScript_FileDeletedAfterCompletion(
            Mock<ISlimerJsProcessProvider> slimerJsProcessProvider,
            Mock<ISlimerJsProcces> slimerJsProcces,
            string script)
        {
            //arrange
            string filePath = null;
            var sut = new SlimerJs(new SlimerJsSettings(), slimerJsProcessProvider.Object);
            slimerJsProcces.Setup(p => p.RunAsync(It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Returns(Task.FromResult(0))
                .Callback<string, Action<string>>((arg, sub) => { filePath = arg; });
            slimerJsProcessProvider.Setup(p => p.Create(It.IsAny<SlimerJsSettings>())).Returns(slimerJsProcces.Object);

            //act
            await sut.RunScriptAsync(script);

            //assert
            filePath.Should().NotBeNullOrEmpty();
            File.Exists(filePath.Replace("\"", "")).Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public void RunScriptAsync_AnyScript_FileDeletedAfterException(
            Mock<ISlimerJsProcessProvider> slimerJsProcessProvider,
            Mock<ISlimerJsProcces> slimerJsProcces,
            string script)
        {
            //arrange
            string filePath = null;
            var sut = new SlimerJs(new SlimerJsSettings(), slimerJsProcessProvider.Object);
            slimerJsProcces.Setup(p => p.RunAsync(It.IsAny<string>(), It.IsAny<Action<string>>()))
                .Callback<string, Action<string>>((arg, sub) => { filePath = arg; })
                .Returns(() =>
                {
                    throw new Exception();
                });
            slimerJsProcessProvider.Setup(p => p.Create(It.IsAny<SlimerJsSettings>())).Returns(slimerJsProcces.Object);

            //act
            sut.Awaiting(s => s.RunScriptAsync(script)).ShouldThrow<Exception>();

            //assert
            filePath.Should().NotBeNullOrEmpty();
            File.Exists(filePath.Replace("\"", "")).Should().BeFalse();
        }

        [Theory, AutoMoqData]
        public void RunScriptAsync_ManyInstances_WorkCorrectly(SlimerJs sut)
        {
            var tasks = Enumerable.Range(0, 10)
                .Select(
                    i => new Task((async () => await sut.RunScriptAsync($"console.log('{i}'); slimer.exit();"))))
                .ToArray();

            foreach (var task in tasks)
            {
                task.Start();
            }
            Task.WaitAll(tasks);

            tasks.Select(a => a.Status).Should().OnlyContain(s => s == TaskStatus.RanToCompletion);
        }
    }
}
