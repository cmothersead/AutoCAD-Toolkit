using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ICA.AutoCAD.Adapter
{
    public class Handlers
    {
        public static void CommandEnded(object sender, CommandEventArgs commandEventArgs)
        {
            if (commandEventArgs.GlobalCommandName == "DWGPROPS")
                DrawingSettings.Update();
        }
    }
}
