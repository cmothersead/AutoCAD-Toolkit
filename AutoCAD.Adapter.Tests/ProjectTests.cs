using ICA.AutoCAD.Adapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoCAD.Adapter.Tests
{
    public class ProjectTests
    {
        private readonly Project project = new Project
        {
            Drawings = new List<Drawing>
                {
                    new Drawing { PageNumber = "1" },
                    new Drawing { PageNumber = "2", Spare = true },
                    new Drawing { PageNumber = "3" },
                }
        };

        #region NextDrawing

        [Fact]
        public void NextDrawingSkipsSpares()
        {
            Drawing current = project.Drawings[0];

            Assert.Equal(project.Drawings[1], project.NextDrawing(current, false));
            Assert.Equal(project.Drawings[2], project.NextDrawing(current, true));
        }

        [Fact]
        public void NextDrawingReturnsNullForLastDrawing()
        {
            Drawing current = project.Drawings.Last();

            Assert.Null(project.NextDrawing(current, true));
            Assert.Null(project.NextDrawing(current, false));
        }

        #endregion

        #region PreviousDrawing

        [Fact]
        public void PreviousDrawingSkipsSpares()
        {
            Drawing current = project.Drawings[2];

            Assert.Equal(project.Drawings[1], project.PreviousDrawing(current, false));
            Assert.Equal(project.Drawings[0], project.PreviousDrawing(current, true));
        }

        [Fact]
        public void PreviousDrawingReturnsNullForFirstDrawing()
        {
            Drawing current = project.Drawings.First();

            Assert.Null(project.PreviousDrawing(current, true));
            Assert.Null(project.PreviousDrawing(current, false));
        }

        #endregion
    }
}
