namespace Raytracer.Core
{
    public abstract class Shape
    {
        public abstract Material Material { get; set; }
        public abstract bool Intersect(Ray Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV);
        public abstract Vector3 Sample();
        public abstract double Area();
    }
}
