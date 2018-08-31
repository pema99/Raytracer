namespace Raytracer
{
    public class Material
    {
        public Vector3 Color { get; set; }
        public double Metalness { get; set; }
        public double Roughness { get; set; }
        //public double Shininess { get; set; }
        //public double Reflection { get; set; }
        //public double Transparency { get; set; }
        //public double RefractiveIndex { get; set; }
        public Vector3 Emission { get; set; }

        public Material(Vector3 Color, double Metalness, double Roughness, Vector3 Emission)
        {
            this.Color = Color;
            this.Metalness = Metalness;
            this.Roughness = Roughness;
            this.Emission = Emission;
        }
    }
}
