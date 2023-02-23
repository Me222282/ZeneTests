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
            //window.LoadXml(File.ReadAllText("GUI2.xml"), typeof(Program));
            //window.LoadXml(File.ReadAllText("LayoutStress.xml"), typeof(Program));

            window.KeyDown += KeyDown;

            window.RunMultithread();
            //window.Run();

            // End glfw
            Core.Terminate();
        }

        private static void KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not GUIWindow w) { return; }

            if (e[Keys.D1] && e[Mods.Alt])
            {
                w.ClearChildren();
                w.RootElement.LayoutManager = null;
                w.LoadXml(File.ReadAllText("GUI.xml"), typeof(Program));
                Console.WriteLine("Loaded");
            }
            if (e[Keys.D2] && e[Mods.Alt])
            {
                w.ClearChildren();
                w.RootElement.LayoutManager = null;
                w.LoadXml(File.ReadAllText("GUI2.xml"), typeof(Program));
                Console.WriteLine("Loaded");
            }
            if (e[Keys.D3] && e[Mods.Alt])
            {
                w.ClearChildren();
                w.RootElement.LayoutManager = null;
                w.LoadXml(File.ReadAllText("LayoutStress.xml"), typeof(Program));
                Console.WriteLine("Loaded");
            }
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