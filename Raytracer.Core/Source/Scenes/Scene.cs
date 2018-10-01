using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Raytracer.Core
{
    public class Scene
    {
        public int PreferredWidth { get; private set; }
        public int PreferredHeight { get; private set; }
        public double PreferredFOV { get; private set; }
        public Vector3 PreferredCameraPosition { get; private set; }
        public Vector3 PreferredCameraRotation { get; private set; }
        public Texture PreferredSkyBox { get; private set; }

        public List<Shape> Shapes { get; private set; }
        public List<Shape> Lights { get; private set; }
        public double[] LightProbabilityTable { get; private set; }

        public Scene(string Path)
        {
            JObject Data = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(Path));

            //Header
            JObject Header = (JObject)Data["Header"];
            this.PreferredWidth = (int)Data["Header"]["Resolution"][0];
            this.PreferredHeight = (int)Data["Header"]["Resolution"][1];
            this.PreferredFOV = (int)Data["Header"]["FOV"];
            this.PreferredCameraPosition = Header["CameraPosition"].ToVector3();
            this.PreferredCameraRotation = Header["CameraRotation"].ToVector3();
            this.PreferredSkyBox = new Texture((string)Header["SkyBox"], true);

            //Material table
            JObject Materials = (JObject)Data["Materials"];
            Dictionary<string, Material> MaterialTable = new Dictionary<string, Material>();
            MaterialNode Normal, Emission, Albedo, Metalness, Roughness, IOR, Sigma;
            foreach (JProperty Material in Materials.Children())
            {
                string Name = Material.Name;

                if (Material.First["Normal"] == null)
                {
                    Normal = null;
                }
                else
                {
                    Normal = new MaterialNormalNode(new Texture((string)Material.First["Normal"]));
                }
                Emission = Material.First["Emission"].ToMaterialNode();
                Albedo = Material.First["Albedo"].ToMaterialNode();
                Metalness = Material.First["Metalness"].ToMaterialNode();
                Roughness = Material.First["Roughness"].ToMaterialNode();
                IOR = Material.First["IOR"].ToMaterialNode();
                Sigma = Material.First["Sigma"].ToMaterialNode();

                switch ((string)Material.First["Type"])
                {
                    case "Light":
                        MaterialTable.Add(Name, new EmissionMaterial(Emission));
                        break;

                    case "Glass":
                        MaterialTable.Add(Name, new GlassMaterial(Albedo, IOR, Roughness, Normal));
                        break;

                    case "Lambertian":
                        MaterialTable.Add(Name, new LambertianMaterial(Albedo, Normal));
                        break;

                    case "PBR":
                        MaterialTable.Add(Name, new PBRMaterial(Albedo, Metalness, Roughness, Normal, null));
                        break;

                    case "Transparent":
                        MaterialTable.Add(Name, new TransparentMaterial(Albedo));
                        break;

                    case "Velvet":
                        MaterialTable.Add(Name, new VelvetMaterial(Albedo, Sigma, Normal));
                        break;

                    default:
                        throw new Exception("Invalid material type in scene file.");
                        break;
                }
            }

            //World description
            JArray World = (JArray)Data["World"];
            this.Shapes = new List<Shape>();
            Shape S;
            foreach (JObject Obj in World)
            {
                switch ((string)Obj["Type"])
                {
                    case "Disc":
                        S = new Disc(
                            MaterialTable[(string)Obj["Material"]],
                            Obj["Origin"].ToVector3(),
                            Obj["Normal"].ToVector3(),
                            (double)Obj["Radius"]);
                        Shapes.Add(S);
                        break;

                    case "Plane":
                        S = new Plane(
                            MaterialTable[(string)Obj["Material"]],
                            Obj["Origin"].ToVector3(),
                            Obj["Normal"].ToVector3());
                        Shapes.Add(S);
                        break;

                    case "Quad":
                        S = new Quad(
                            MaterialTable[(string)Obj["Material"]],
                            Obj["Origin"].ToVector3(),
                            Obj["Normal"].ToVector3(),
                            Obj["Size"].ToVector2());
                        Shapes.Add(S);
                        break;

                    case "Sphere":
                        S = new Sphere(
                            MaterialTable[(string)Obj["Material"]],
                            Obj["Origin"].ToVector3(),
                            (double)Obj["Radius"]);
                        Shapes.Add(S);
                        break;

                    case "Mesh":
                        JToken GridLambda = Obj["GridLambda"] ?? 3;
                        JToken SmoothShading = Obj["SmoothShading"] ?? true;
                        JToken BackFaceCulling = Obj["BackFaceCulling"] ?? true;
                        JToken Origin = Obj["Origin"] ?? new JArray(0, 0, 0);
                        JToken Rotation = Obj["Rotation"] ?? new JArray(0, 0, 0);
                        Vector3 RotVec = Rotation.ToVector3();
                        JToken Scale = Obj["Scale"] ?? 1;
                        S = new TriangleMesh(
                            MaterialTable[(string)Obj["Material"]],
                            Matrix.CreateScale((double)Scale) * Matrix.CreateRotationX(RotVec.X) * Matrix.CreateRotationY(RotVec.Y) * Matrix.CreateRotationZ(RotVec.Z) * Matrix.CreateTranslation(Origin.ToVector3()),
                            (string)Obj["Path"],
                            (double)GridLambda,
                            (bool)SmoothShading,
                            (bool)BackFaceCulling);
                        Shapes.Add(S);                        
                        break;

                    default:
                        throw new Exception("Invalid object type in scene file.");
                        break;
                }
            }

            CalculateLighting();
        }

        public Scene(List<Shape> Shapes)
        {
            this.Shapes = Shapes;
            this.PreferredWidth = 600;
            this.PreferredHeight = 400;
            this.PreferredFOV = 75;
            this.PreferredCameraPosition = Vector3.Zero;
            this.PreferredCameraRotation = Vector3.Zero;
            this.PreferredSkyBox = null;
            CalculateLighting();
        }

        public bool Raycast(Ray Ray, out Shape FirstShape, out Vector3 FirstShapeHit, out Vector3 FirstShapeNormal, out Vector2 FirstShapeUV)
        {
            double MinDistance = double.MaxValue;
            FirstShape = null;
            FirstShapeHit = Vector3.Zero;
            FirstShapeNormal = Vector3.Zero;
            FirstShapeUV = Vector2.Zero;
            foreach (Shape Shape in Shapes)
            {
                if (Shape.Intersect(Ray, out Vector3 Hit, out Vector3 Normal, out Vector2 UV))
                {
                    double Distance = (Hit - Ray.Origin).Length();
                    if (Distance < MinDistance)
                    {
                        MinDistance = Distance;
                        FirstShape = Shape;
                        FirstShapeHit = Hit;
                        FirstShapeNormal = Normal;
                        FirstShapeUV = UV;
                    }
                }
            }
            return FirstShape != null;
        }

        public void PickLight(out Shape Light, out double PDF)
        {
            //Sample light with CDF table
            double Prob = Util.Random.NextDouble();
            int i = 0;
            while (i < Lights.Count)
            {
                if (Prob <= LightProbabilityTable[i])
                {
                    break;
                }
                i++;
            }
            Light = Lights[i];
            PDF = LightProbabilityTable[i] - (i == 0 ? 0 : LightProbabilityTable[i - 1]); //PDF = CDFCurrent - CDFPrevious
        }

        private void CalculateLighting()
        {
            //Setup lights for NEE
            this.Lights = new List<Shape>();
            foreach (Shape S in Shapes)
            {
                if (S.Material.HasProperty("emission") && !(S is Plane)) //Infinite planes not included
                {
                    Lights.Add(S);
                }
            }

            //Setup light probabiltiies
            this.LightProbabilityTable = new double[Lights.Count];
            double TotalLightWeight = 0;
            for (int i = 0; i < Lights.Count; i++)
            {
                double LightWeight = Lights[i].Material.GetProperty("emission", Vector2.Zero).Color.SumComponents() * Lights[i].Area();
                TotalLightWeight += LightWeight;
                LightProbabilityTable[i] = TotalLightWeight;
            }
            for (int i = 0; i < Lights.Count; i++)
            {
                LightProbabilityTable[i] /= TotalLightWeight;
            }
        }
    }
}
