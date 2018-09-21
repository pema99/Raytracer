namespace Raytracer.Frontend
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            using (var MW = new MainWindow())
            {
                MW.Run();
            }
        }
    }
}