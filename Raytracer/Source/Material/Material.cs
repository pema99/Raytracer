namespace Raytracer
{
    public class Material
    {
        private MaterialOutputNode Output { get; set; }

        public Material(Vector3 Albedo, double Metalness, double Roughness, Vector3 Emission, double Transparency = 0, double RefractiveIndex = 1)
        {
            this.Output = new MaterialOutputNode();
            Output.Inputs[0] = new MaterialConstantNode(Albedo);
            Output.Inputs[1] = new MaterialConstantNode(Metalness);
            Output.Inputs[2] = new MaterialConstantNode(Roughness);
            Output.Inputs[5] = new MaterialConstantNode(Emission);
            Output.Inputs[6] = new MaterialConstantNode(Transparency);
            Output.Inputs[7] = new MaterialConstantNode(RefractiveIndex);
        }

        public Material(MaterialNode Albedo, MaterialNode Metalness, MaterialNode Roughness, MaterialNode Normal, MaterialNode AmbientOcclusion, MaterialNode Emission, MaterialNode Transparency, MaterialNode RefractiveIndex)
        {
            this.Output = new MaterialOutputNode();
            Output.Inputs[0] = Albedo;
            Output.Inputs[1] = Metalness;
            Output.Inputs[2] = Roughness;
            Output.Inputs[3] = Normal;
            Output.Inputs[4] = AmbientOcclusion;
            Output.Inputs[5] = Emission;
            Output.Inputs[6] = Transparency;
            Output.Inputs[7] = RefractiveIndex;
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
            Output.Inputs[6] = new MaterialConstantNode(0);
            Output.Inputs[7] = new MaterialConstantNode(1);
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

        public double GetTransparency(Vector2 UV)
        {
            return Output.GetFinalValue(6, UV);
        }

        public double GetRefractiveIndex(Vector2 UV)
        {
            return Output.GetFinalValue(7, UV);
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

        public bool HasTransparency()
        {
            return Output.Inputs[6] != null;
        }

        public bool HasRefractiveIndex()
        {
            return Output.Inputs[7] != null;
        }
        #endregion
    }
}
