using System;
using System.Threading.Tasks;
using NetSlimerJS;


namespace TestApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            var runner = new SlimerJs();
            runner.RunAsync(@"test.js", new[] { @"http://happyworks.ru/netslimerjs/" }, Console.WriteLine).GetAwaiter().GetResult();

            System.Console.Read();

        }
    }
}
