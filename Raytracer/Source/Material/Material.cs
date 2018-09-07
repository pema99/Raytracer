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
            Output.Inputs[5] = new MaterialConstantNode(Emission);
        }

        public Material(MaterialNode Albedo, MaterialNode Metalness, MaterialNode Roughness, MaterialNode Normal, MaterialNode AmbientOcclusion, MaterialNode Emission)
        {
            this.Output = new MaterialOutputNode();
            Output.Inputs[0] = Albedo;
            Output.Inputs[1] = Metalness;
            Output.Inputs[2] = Roughness;
            Output.Inputs[3] = Normal;
            Output.Inputs[4] = AmbientOcclusion;
            Output.Inputs[5] = Emission;
        }

        public Material(string Name)
        {
            this.Output = new MaterialOutputNode();
            Output.Inputs[0] = new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_basecolor.png", true));
            Output.Inputs[1] = new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_metallic.png"));
            Output.Inputs[2] = new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_roughness.png"));
            Output.Inputs[3] = new MaterialNormalNode(new Texture("Assets/Materials/" + Name + "_normal.png"));
            //Output.Inputs[2] = new MaterialTextureNode(new Texture("Assets/Materials/" + Name + "_roughness.png"));
            Output.Inputs[5] = new MaterialConstantNode(Vector3.Zero);
        }

        #region Get methods
        public Vector3 GetAlbedo(Vector2 UV)
        {
            return Output.GetFinalValue(0, UV);
        }

        public double GetMetalness(Vector2 UV)
        {
            return Output.GetFinalValue(1, UV);
        }

        public double GetRoughness(Vector2 UV)
        {
            return Output.GetFinalValue(2, UV);
        }

        public Vector3 GetNormal(Vector2 UV)
        {
            return Output.GetFinalValue(3, UV);
        }

        public double GetAmbientOcclusion(Vector2 UV)
        {
            return Output.GetFinalValue(4, UV);
        }

        public Vector3 GetEmission(Vector2 UV)
        {
            return Output.GetFinalValue(5, UV);
        }
        #endregion

        #region Existence check methods
        public bool HasAlbedo()
        {
            return Output.Inputs[0] != null;
        }

        public bool HasMetalness()
        {
            return Output.Inputs[1] != null;
        }

        public bool HasRoughness()
        {
            return Output.Inputs[2] != null;
        }

        public bool HasNormal()
        {
            return Output.Inputs[3] != null;
        }

        public bool HasAmbientOcclusion()
        {
            return Output.Inputs[4] != null;
        }

        public bool HasEmission()
        {
            return Output.Inputs[5] != null;
        }
        #endregion
    }
}
