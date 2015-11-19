using Ploeh.AutoFixture.Xunit2;

namespace NetSlimerJSTests.Attributes
{
    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] values)
            : base(new AutoMoqDataAttribute(), values)
        {
        }
    }
}