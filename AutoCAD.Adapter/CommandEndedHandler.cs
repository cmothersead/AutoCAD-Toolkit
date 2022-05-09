using Autodesk.AutoCAD.ApplicationServices;

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
