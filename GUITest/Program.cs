using System;
using Zene.Windowing;
using Zene.GUI;
using System.IO;

namespace GUITest
{
    class Program
    {
        static void Main()
        {
            // Start glfw
            Core.Init();

            GUIWindow window = new GUIWindow(800, 500, "Work", 4.3);
            window.LoadXml(File.ReadAllText("GUI.xml"), typeof(Program));

            window.RunMultithread();
            //window.Run();

            // End glfw
            Core.Terminate();
        }

        private static void LabelClick(object sender, MouseEventArgs e)
        {
            if (sender is not Label l) { return; }

            l.Text = l.Text switch
            {
                "What's in his Shoe?" => "Not beans.",
                "Not beans." => "feet (Tim)",
                _ => "What's in his Shoe?"
            };
        }
    }
}