namespace Raytracer
{
    public class Light
    {
        public Vector3 Origin { get; set; }
        public double Intensity { get; set; }
        public Vector3 Color { get; set; }
        
        public Light(Vector3 Origin, double Intensity, Vector3 Color)
        {
            this.Origin = Origin;
            this.Intensity = Intensity;
            this.Color = Color;
        }
    }
}
