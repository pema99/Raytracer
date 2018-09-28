using System;
using System.Diagnostics;

namespace Raytracer.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            #if (!DEBUG)
            Core.Raytracer Raytracer = new Core.Raytracer(1280, 720, 75, Core.Vector3.Zero, Core.Vector3.Zero, new Core.Texture("Assets/EnvMaps/portland.png", true), 3, 7, 10, 12, true);
            #else
            Core.Raytracer Raytracer = new Core.Raytracer(1280, 720, 75, Core.Vector3.Zero, Core.Vector3.Zero, new Core.Texture("Assets/EnvMaps/portland.png", true), 3, 7, 20, 1, true);
            #endif

            Stopwatch Time = new Stopwatch();
            Time.Start();
            Raytracer.Render();
            Time.Stop();

            Raytracer.ExportToFile("Render.png");

            Process.Start("Render.png");

            Console.WriteLine("Finished rendering in {0} seconds", Time.Elapsed.TotalSeconds);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
