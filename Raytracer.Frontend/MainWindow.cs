using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Windows.Forms;

namespace Raytracer.Frontend
{
    public class MainWindow : Game
    {
        private GraphicsDeviceManager graphics;
        private ImGuiRenderer ImGuiRenderer;
        private SpriteBatch spriteBatch;

        private Core.Raytracer RT;
        private float FOV = 75;
        private System.Numerics.Vector3 CamPos = new System.Numerics.Vector3(0, 0, 0);
        private System.Numerics.Vector3 CamRot = new System.Numerics.Vector3(0, 0, 0);
        private int MinBounces = 3;
        private int MaxBounces = 6;
        private int Samples = 1000;
        private int Threads = 11;
        private bool NEE = true;

        private bool Progressive = true;
        private int Frames = 0;
        private Core.Vector3[,] FrameBuffer;
        private Texture2D RenderTarget;
        private Thread RenderThread = new Thread(() => { });

        public MainWindow()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1024;
            graphics.PreferredBackBufferHeight = 768;
            graphics.PreferMultiSampling = true;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += delegate 
            {
                if (graphics.PreferredBackBufferWidth != Window.ClientBounds.Width || graphics.PreferredBackBufferHeight != Window.ClientBounds.Height)
                {
                    graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
                    graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
                    graphics.ApplyChanges();
                }
                RT.Width = graphics.PreferredBackBufferWidth;
                RT.Height = graphics.PreferredBackBufferHeight;
            };
        }

        protected override void Initialize()
        {
            ImGuiRenderer = new ImGuiRenderer(this);
            ImGuiRenderer.RebuildFontAtlas();
            SetStyle();

            RT = new Core.Raytracer(
                graphics.PreferredBackBufferWidth,
                graphics.PreferredBackBufferHeight,
                FOV,
                new Core.Vector3(CamPos.X, CamPos.Y, CamPos.Z),
                new Core.Vector3(CamRot.X, CamRot.Y, CamRot.Z),
                new Core.Texture("Assets/EnvMaps/portland.png", true),
                MinBounces,
                MaxBounces,
                Samples,
                Threads,
                NEE
            );

            FrameBuffer = new Core.Vector3[RT.Width, RT.Height];
            RenderTarget = new Texture2D(GraphicsDevice, RT.Width, RT.Height);
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);

            base.Initialize();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(RenderTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            ImGuiRenderer.BeforeLayout(gameTime);
            ImGuiLayout();
            ImGuiRenderer.AfterLayout();

            base.Draw(gameTime);
        }

        private void ImGuiLayout()
        {
            ImGui.BeginWindow("Settings", WindowFlags.AlwaysAutoResize);

            if (RenderThread.IsAlive)
            {
                if (ImGui.Button("Stop render"))
                {
                    Window.AllowUserResizing = true;
                    RenderThread.Abort();
                }
                if (Progressive)
                {
                    ImGui.Text("Current samples: " + (Frames * RT.Samples).ToString());
                }
                ImGui.SameLine();
            }
            else
            {
                if (ImGui.Button("Start render"))
                {
                    Window.AllowUserResizing = false;
                    RenderTarget = new Texture2D(GraphicsDevice, RT.Width, RT.Height);
                    if (Progressive)
                    {
                        RenderThread = new Thread(RenderProgressive);
                        RenderThread.Start();
                    }
                    else
                    {
                        RenderThread = new Thread(RenderStandard);
                        RenderThread.Start();
                    }
                }
                ImGui.SameLine();
                if (ImGui.Button("Clear framebuffer"))
                {
                    RenderTarget.SetData(new Color[RT.Width * RT.Height]);
                }

                ImGui.Checkbox("Progressive", ref Progressive);
                ImGui.SameLine();
                if (ImGui.Checkbox("Direct Light Sampling", ref NEE))
                {
                    RT.NEE = NEE;
                }
                if (ImGui.SliderFloat("FOV", ref FOV, 0, 180, "%f", 1))
                {
                    RT.FOV = FOV;
                }
                if (ImGui.DragVector3("Camera Position", ref CamPos, -float.MinValue, float.MaxValue, 0.1f))
                {
                    RT.CameraPosition = new Core.Vector3(CamPos.X, CamPos.Y, CamPos.Z);
                }
                if (ImGui.DragVector3("Camera Rotation", ref CamRot, -float.MinValue, float.MaxValue, 0.1f))
                {
                    RT.CameraRotation = new Core.Vector3(CamRot.X, CamRot.Y, CamRot.Z);
                }
                if (ImGui.DragIntRange2("Min/Max Bounces", ref MinBounces, ref MaxBounces, 1, 1, int.MaxValue))
                {
                    RT.MinBounces = MinBounces;
                    RT.MaxBounces = MaxBounces;
                }
                if (ImGui.DragInt("Samples per pixel", ref Samples, 1, 1, int.MaxValue, "%g"))
                {
                    RT.Samples = Samples;
                }
                if (ImGui.SliderInt("Threads", ref Threads, 1, Environment.ProcessorCount, "%g"))
                {
                    RT.Threads = Threads;
                }
            }

            ImGui.EndWindow();
        }

        private void RenderProgressive()
        {
            Frames = 0;
            FrameBuffer = new Core.Vector3[RT.Width, RT.Height];

            RT.Samples = 1;

            for (int i = 0; i < Samples; i++)
            {
                RT.Render();

                Frames++;

                Color[] Data = new Color[RT.Width * RT.Height];

                for (int x = 0; x < RT.Width; x++)
                {
                    for (int y = 0; y < RT.Height; y++)
                    {
                        //Accumulate buffer
                        FrameBuffer[x, y] += RT.Framebuffer[x, y];

                        //Tone mapping
                        Core.Vector3 CurrentColor = FrameBuffer[x, y] / Frames;
                        Core.Vector3 Mapped = CurrentColor / (CurrentColor + Core.Vector3.One);
                        Mapped = new Core.Vector3(Math.Pow(Mapped.X, 1.0 / 2.2), Math.Pow(Mapped.Y, 1.0 / 2.2), Math.Pow(Mapped.Z, 1.0 / 2.2));

                        //Draw
                        Data[RT.Width * y + x] = new Color((int)(Mapped.X * 255), (int)(Mapped.Y * 255), (int)(Mapped.Z * 255));
                    }
                }

                RenderTarget.SetData(Data);
            }

            Window.AllowUserResizing = true;
        }

        private void RenderStandard()
        {
            RT.Samples = Samples;

            RT.Render();

            Color[] Data = new Color[RT.Width * RT.Height];
            for (int x = 0; x < RT.Width; x++)
            {
                for (int y = 0; y < RT.Height; y++)
                {
                    //Tone mapping
                    Core.Vector3 CurrentColor = RT.Framebuffer[x, y];
                    Core.Vector3 Mapped = CurrentColor / (CurrentColor + Core.Vector3.One);
                    Mapped = new Core.Vector3(Math.Pow(Mapped.X, 1.0 / 2.2), Math.Pow(Mapped.Y, 1.0 / 2.2), Math.Pow(Mapped.Z, 1.0 / 2.2));

                    //Draw
                    Data[RT.Width * y + x] = new Color((int)(Mapped.X * 255), (int)(Mapped.Y * 255), (int)(Mapped.Z * 255));
                }
            }
            RenderTarget.SetData(Data);

            Window.AllowUserResizing = true;
        }

        private void SetStyle()
        {
            Style Style = ImGui.GetStyle();
            Style.Alpha = 1.0f;
            Style.FrameRounding = 3.0f;
            Style.SetColor(ColorTarget.Text, new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 1.00f));
            Style.SetColor(ColorTarget.TextDisabled, new System.Numerics.Vector4(0.60f, 0.60f, 0.60f, 1.00f));
            Style.SetColor(ColorTarget.WindowBg, new System.Numerics.Vector4(0.94f, 0.94f, 0.94f, 0.94f));
            Style.SetColor(ColorTarget.ChildBg, new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.00f));
            Style.SetColor(ColorTarget.PopupBg, new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 0.94f));
            Style.SetColor(ColorTarget.Border, new System.Numerics.Vector4(0.00f, 0.00f, 0.00f, 0.39f));
            Style.SetColor(ColorTarget.BorderShadow, new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 0.10f));
            Style.SetColor(ColorTarget.FrameBg, new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 0.94f));
            Style.SetColor(ColorTarget.FrameBgHovered, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.40f));
            Style.SetColor(ColorTarget.FrameBgActive, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.67f));
            Style.SetColor(ColorTarget.TitleBg, new System.Numerics.Vector4(0.96f, 0.96f, 0.96f, 1.0f));
            Style.SetColor(ColorTarget.TitleBgCollapsed, new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 1.0f));
            Style.SetColor(ColorTarget.TitleBgActive, new System.Numerics.Vector4(0.82f, 0.82f, 0.82f, 1.00f));
            Style.SetColor(ColorTarget.MenuBarBg, new System.Numerics.Vector4(0.86f, 0.86f, 0.86f, 1.00f));
            Style.SetColor(ColorTarget.ScrollbarBg, new System.Numerics.Vector4(0.98f, 0.98f, 0.98f, 0.53f));
            Style.SetColor(ColorTarget.ScrollbarGrab, new System.Numerics.Vector4(0.69f, 0.69f, 0.69f, 1.00f));
            Style.SetColor(ColorTarget.ScrollbarGrabHovered, new System.Numerics.Vector4(0.59f, 0.59f, 0.59f, 1.00f));
            Style.SetColor(ColorTarget.ScrollbarGrabActive, new System.Numerics.Vector4(0.49f, 0.49f, 0.49f, 1.00f));
            Style.SetColor(ColorTarget.CheckMark, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 1.00f));
            Style.SetColor(ColorTarget.SliderGrab, new System.Numerics.Vector4(0.24f, 0.52f, 0.88f, 1.00f));
            Style.SetColor(ColorTarget.SliderGrabActive, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 1.00f));
            Style.SetColor(ColorTarget.Button, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.40f));
            Style.SetColor(ColorTarget.ButtonHovered, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 1.00f));
            Style.SetColor(ColorTarget.ButtonActive, new System.Numerics.Vector4(0.06f, 0.53f, 0.98f, 1.00f));
            Style.SetColor(ColorTarget.Header, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.31f));
            Style.SetColor(ColorTarget.HeaderHovered, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.80f));
            Style.SetColor(ColorTarget.HeaderActive, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 1.00f));
            Style.SetColor(ColorTarget.ResizeGrip, new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 0.50f));
            Style.SetColor(ColorTarget.ResizeGripHovered, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.67f));
            Style.SetColor(ColorTarget.ResizeGripActive, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.95f));
            Style.SetColor(ColorTarget.CloseButton, new System.Numerics.Vector4(0.59f, 0.59f, 0.59f, 0.50f));
            Style.SetColor(ColorTarget.CloseButtonHovered, new System.Numerics.Vector4(0.98f, 0.39f, 0.36f, 1.00f));
            Style.SetColor(ColorTarget.CloseButtonActive, new System.Numerics.Vector4(0.98f, 0.39f, 0.36f, 1.00f));
            Style.SetColor(ColorTarget.PlotLines, new System.Numerics.Vector4(0.39f, 0.39f, 0.39f, 1.00f));
            Style.SetColor(ColorTarget.PlotLinesHovered, new System.Numerics.Vector4(1.00f, 0.43f, 0.35f, 1.00f));
            Style.SetColor(ColorTarget.PlotHistogram, new System.Numerics.Vector4(0.90f, 0.70f, 0.00f, 1.00f));
            Style.SetColor(ColorTarget.PlotHistogramHovered, new System.Numerics.Vector4(1.00f, 0.60f, 0.00f, 1.00f));
            Style.SetColor(ColorTarget.TextSelectedBg, new System.Numerics.Vector4(0.26f, 0.59f, 0.98f, 0.35f));
            Style.SetColor(ColorTarget.ModalWindowDarkening, new System.Numerics.Vector4(0.20f, 0.20f, 0.20f, 0.35f));
            for (int i = 0; i <= 43; i++)
            {
                var Color = Style.GetColor((ColorTarget)i);
                float H, S, V;
                ImGui.ColorConvertRGBToHSV(Color.X, Color.Y, Color.Z, out H, out S, out V);

                if (S < 0.1f)
                {
                    V = 1.0f - V;
                }
                ImGui.ColorConvertHSVToRGB(H, S, V, out Color.X, out Color.Y, out Color.Z);
                Style.SetColor((ColorTarget)i, Color);
            }
        }
    }
}