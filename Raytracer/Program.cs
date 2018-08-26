namespace Raytracer
{
    public static class Program
    {
        public static void Main(string[] Args)
        {
            string Path = "";
            if (Args.Length == 0)
            {
                Path = "Render.png";
            }
            else
            {
                Path = Args[0];
                if (!Path.Contains("."))
                {
                    Args[0] = Args[0] + ".png";
                }
            }

            Raytracer Raytracer = new Raytracer(600, 400, 75);
            Raytracer.RenderToFile(Path);

            #if DEBUG
            System.Diagnostics.Process.Start(Path);
            #endif
        }
    }
}
