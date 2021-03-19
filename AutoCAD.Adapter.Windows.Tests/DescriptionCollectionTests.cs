using ICA.AutoCAD.Adapter.Windows.Models;
using System.Collections.Generic;
using Xunit;

namespace AutoCAD.Adapter.Windows.Models.Tests
{
    public class DescriptionCollectionTests
    {
        [Theory]
        [MemberData(nameof(Data))]
        public void HasEmpty(DescriptionCollection descriptionLines)
        {
            Assert.Contains(new DescriptionLine(), descriptionLines);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new DescriptionCollection() },
                new object[] { new DescriptionCollection{"test"} },
                new object[] { new DescriptionCollection{ "1", "2", "3"} }
            };

        [Theory]
        [MemberData(nameof(Data))]
        public void EmptyIsLast(DescriptionCollection descriptionLines)
        {
            Assert.Equal(descriptionLines.IndexOf(new DescriptionLine()), descriptionLines.Count - 1);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void RemovesWhenEmptied(DescriptionCollection descriptionLines)
        {
            int expected;
            if (descriptionLines.Count == 1)
            {
                expected = 1;
            }
            else
            {
                expected = descriptionLines.Count - 1;
            }
            descriptionLines[0].Value = "";

            int actual = descriptionLines.Count;

            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(Data))]
        public void AddsWhenFilled(DescriptionCollection descriptionLines)
        {
            int expected = descriptionLines.Count + 1;

            descriptionLines[descriptionLines.Count - 1].Value = "1";

            int actual = descriptionLines.Count;

            Assert.Equal(expected, actual);
        }
    }
}
