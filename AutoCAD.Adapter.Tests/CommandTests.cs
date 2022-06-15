using ICA.AutoCAD.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoCAD.Adapter.Tests
{
    public class CommandTests
    {
        #region GetDrawing

        [Fact]
        public void GetDrawingRejectsNonIntegerInput()
        {
            string message = "Invalid input. Must be a number.";

            var ex = Assert.Throws<ArgumentException>(() => Commands.GetDrawing("test", new Project()));
            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public void GetDrawingRejectsOutOfBoundsValue()
        {
            Project project = new Project
            {
                Drawings = new List<Drawing>
                {
                    new Drawing(),
                    new Drawing(),
                    new Drawing(),
                }
            };

            string message = "Invalid input. Page number out of range.\r\nParameter name: input";

            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Commands.GetDrawing("4", project));
            Assert.Equal(message, ex.Message);

            ex = Assert.Throws<ArgumentOutOfRangeException>(() => Commands.GetDrawing("0", project));
            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public void GetDrawingRejectsPageNotFound()
        {
            Project project = new Project
            {
                Drawings = new List<Drawing>
                {
                    new Drawing { PageNumber = "1" },
                    new Drawing { PageNumber = "2" },
                    new Drawing { PageNumber = "4" },
                }
            };

            string message = "Page number not found within project.";

            var ex = Assert.Throws<ArgumentException>(() => Commands.GetDrawing("3", project));
            Assert.Equal(message, ex.Message);
        }

        [Fact]
        public void GetDrawingFindsCorrectDrawing()
        {
            Project project = new Project
            {
                Drawings = new List<Drawing>
                {
                    new Drawing { PageNumber = "1" },
                    new Drawing { PageNumber = "2" },
                    new Drawing { PageNumber = "4" },
                    new Drawing { PageNumber = "5" },
                }
            };

            Assert.All(project.Drawings.Select(drawing => (Self: drawing, drawing.PageNumber)),
                       drawing => Assert.Equal(drawing.Self, Commands.GetDrawing(drawing.PageNumber, project)));

            Assert.All(project.Drawings.Select(drawing => (Self: drawing, PageNumber: $"   {drawing.PageNumber}   ")),
                       drawing => Assert.Equal(drawing.Self, Commands.GetDrawing(drawing.PageNumber, project)));

            Assert.All(project.Drawings.Select(drawing => (Self: drawing, PageNumber: $"000{drawing.PageNumber}")),
                       drawing => Assert.Equal(drawing.Self, Commands.GetDrawing(drawing.PageNumber, project)));
        }

        #endregion


    }
}
