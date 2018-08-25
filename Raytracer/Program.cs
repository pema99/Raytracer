namespace Raytracer
{
    public static class Program
    {
        public static void Main()
        {
            Raytracer Raytracer = new Raytracer(600, 400, 75);
            Raytracer.RenderToFile("Render.png");
        }
    }
}
