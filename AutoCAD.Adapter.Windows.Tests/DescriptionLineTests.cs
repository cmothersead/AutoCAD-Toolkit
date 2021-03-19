using ICA.AutoCAD.Adapter.Windows.Models;
using Xunit;

namespace AutoCAD.Adapter.Windows.Models.Tests
{
    
    public class DescriptionLineTests
    {
        [Fact]
        public void ValueIsUppercase()
        {
            string expected = "TEST";

            string actual = new DescriptionLine("test").Value;

            Assert.Equal(expected, actual);
        }
    }
}
