namespace Raytracer
{
    public class Material
    {
        public Vector3 Color { get; set; }
        //public double Shininess { get; set; }
        //public double Reflection { get; set; }
        //public double Transparency { get; set; }
        //public double RefractiveIndex { get; set; }
        public Vector3 Emission { get; set; }

        public Material(Vector3 Color, Vector3 Emission)
        {
            this.Color = Color;
            this.Emission = Emission;
        }
    }
}
