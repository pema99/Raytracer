namespace Raytracer
{
    public class Material
    {
        public Vector3 Color { get; set; }
        public double Shininess { get; set; }
        public double Reflection { get; set; }
        public double Transparency { get; set; }
        public double RefractiveIndex { get; set; }

        public Material(Vector3 Color, double Shininess, double Reflectivity, double Transparency, double RefractiveIndex)
        {
            this.Color = Color;
            this.Shininess = Shininess;
            this.Reflection = Reflectivity;
            this.Transparency = Transparency;
            this.RefractiveIndex = RefractiveIndex;
        }
    }
}
