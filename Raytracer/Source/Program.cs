
using System;
using System.Diagnostics;

namespace Raytracer
{
    public static class Program
    {
        public static void Main(string[] Args)
        {
            #if (!DEBUG)
            Raytracer Raytracer = new Raytracer(600, 400, 75, Vector3.Zero, Vector3.Zero, new Texture("Assets/EnvMaps/portland.png", true), 3, 3, 1000, 12);
            #else
            Raytracer Raytracer = new Raytracer(600, 400, 75, Vector3.Zero, Vector3.Zero, new Texture("Assets/EnvMaps/portland.png", true), 5, 100, 1);
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
