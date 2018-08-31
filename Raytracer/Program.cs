using System;

namespace Raytracer
{
    public static class Program
    {
        public static void Main(string[] Args)
        {
            Raytracer Raytracer = new Raytracer(600, 400, 75, 2, 16, 12);

            Raytracer.Render();

            Raytracer.ExportToFile("Render.png");

            System.Diagnostics.Process.Start("Render.png");
        }
    }
}
