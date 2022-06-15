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
        private readonly Project projectWithNoPageNumbers = new Project
        {
            Drawings = new List<Drawing>
            {
                new Drawing(),
                new Drawing(),
                new Drawing(),
            }
        };

        private readonly Project projectWithSpare = new Project
        {
            Drawings = new List<Drawing>
                {
                    new Drawing { PageNumber = "1" },
                    new Drawing { PageNumber = "2", Spare = true },
                    new Drawing { PageNumber = "3" },
                }
        };

        private readonly Project projectWithOutOfOrderPageNumbers = new Project
        {
            Drawings = new List<Drawing>
            {
                new Drawing { PageNumber = "4" },
                new Drawing { PageNumber = "162" },
                new Drawing { PageNumber = "5" },
            }
        };

        #region SheetCount

        [Fact]
        public void SheetCountReturnsCountIfNoPageNumbers()
        {
            int expected = projectWithNoPageNumbers.Drawings.Count;
            int actual = projectWithNoPageNumbers.SheetCount;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SheetCountReturnsHighestPageNumber()
        {
            var expected = projectWithOutOfOrderPageNumbers.Drawings.Max(drawing => int.Parse(drawing.PageNumber));
            var actual = projectWithOutOfOrderPageNumbers.SheetCount;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SheetCountIgnoresNullPageNumbers()
        {
            int expected = 16;

            Project projectWithSomePageNumbers = new Project
            {
                Drawings = new List<Drawing>
                {
                    new Drawing { PageNumber = $"{expected}" },
                    new Drawing(),
                    new Drawing(),
                }
            };

            int actual = projectWithSomePageNumbers.SheetCount;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region NextDrawing

        [Fact]
        public void NextDrawingSkipsSpares()
        {
            Drawing current = projectWithSpare.Drawings[0];

            Assert.Equal(projectWithSpare.Drawings[1], projectWithSpare.NextDrawing(current, false));
            Assert.Equal(projectWithSpare.Drawings[2], projectWithSpare.NextDrawing(current, true));
        }

        [Fact]
        public void NextDrawingReturnsNullForLastDrawing()
        {
            Drawing current = projectWithSpare.Drawings.Last();

            Assert.Null(projectWithSpare.NextDrawing(current, true));
            Assert.Null(projectWithSpare.NextDrawing(current, false));
        }

        #endregion

        #region PreviousDrawing

        [Fact]
        public void PreviousDrawingSkipsSpares()
        {
            Drawing current = projectWithSpare.Drawings[2];

            Assert.Equal(projectWithSpare.Drawings[1], projectWithSpare.PreviousDrawing(current, false));
            Assert.Equal(projectWithSpare.Drawings[0], projectWithSpare.PreviousDrawing(current, true));
        }

        [Fact]
        public void PreviousDrawingReturnsNullForFirstDrawing()
        {
            Drawing current = projectWithSpare.Drawings.First();

            Assert.Null(projectWithSpare.PreviousDrawing(current, true));
            Assert.Null(projectWithSpare.PreviousDrawing(current, false));
        }

        #endregion
    }
}
