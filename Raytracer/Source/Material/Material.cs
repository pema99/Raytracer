namespace Raytracer
{
    public class Material
    {
        private MaterialOutputNode Output { get; set; }

        public Material(Vector3 Albedo, double Metalness, double Roughness, Vector3 Emission)
        {
            this.Output = new MaterialOutputNode();
            Output.Inputs[0] = new MaterialConstantNode(Albedo);
            Output.Inputs[1] = new MaterialConstantNode(Metalness);
            Output.Inputs[2] = new MaterialConstantNode(Roughness);
            Output.Inputs[4] = new MaterialConstantNode(Emission);
        }

        public Material(MaterialNode Albedo, MaterialNode Metalness, MaterialNode Roughness, MaterialNode Emission)
        {
            this.Output = new MaterialOutputNode();
            Output.Inputs[0] = Albedo;
            Output.Inputs[1] = Metalness;
            Output.Inputs[2] = Roughness;
            Output.Inputs[4] = Emission;
        }

        public Material(MaterialOutputNode Output)
        {
            this.Output = Output;
        }

        public Vector3 Albedo(Vector2 UV)
        {
            return Output.GetFinalValue(0, UV).Color;
        }

        public double Metalness(Vector2 UV)
        {
            return Output.GetFinalValue(1, UV).Number;
        }

        public double Roughness(Vector2 UV)
        {
            return Output.GetFinalValue(2, UV).Number;
        }

        //TODO: Normal mapping
        public Vector3 Normal(Vector2 UV)
        {
            return Output.GetFinalValue(3, UV).Color;
        }

        public Vector3 Emission(Vector2 UV)
        {
            return Output.GetFinalValue(4, UV).Color;
        }
    }
}
