using System;
using System.Diagnostics;

namespace Raytracer
{
    public static class Program
    {
        public static void Main(string[] Args)
        {
            Stopwatch Time = new Stopwatch();

            Raytracer Raytracer = new Raytracer(600, 400, 75, 2, 32, 12);

            Time.Start();
            Raytracer.Render();
            Time.Stop();

            Raytracer.ExportToFile("Render.png");

            System.Diagnostics.Process.Start("Render.png");

            Console.WriteLine("Finished in {0} seconds.", Time.Elapsed.TotalSeconds);
            Console.ReadKey();
        }
    }
}
