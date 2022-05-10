using ICA.AutoCAD.Adapter;
using ICA.AutoCAD.Adapter.Windows.Views;
using ICA.Schematic;
using System;
using System.Collections.Generic;
using System.Windows;

namespace QuickTestFramework
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application app = new Application();
            ITitleBlock titleBlock = new TestTB();
            titleBlock.Attributes = new List<ITBAttribute>();
            titleBlock.Attributes.Add(new TestTBAtt { Tag = "DWGNO", Value = "1V" });
            titleBlock.Attributes.Add(new TestTBAtt { Tag = "SHT", Value = "2V" });
            titleBlock.Attributes.Add(new TestTBAtt { Tag = "SHTS", Value = "3V" });
            app.Run(new TitleBlockSettingsView(titleBlock));
        }
    }

    class TestTB : ITitleBlock
    {
        public List<ITBAttribute> Attributes { get; set; }
    }

    class TestTBAtt : ITBAttribute
    {
        public string Tag { get; set; }
        public string Value { get; set; }
    }
}
