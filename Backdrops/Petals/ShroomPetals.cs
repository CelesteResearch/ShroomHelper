using System.Numerics;
using Celeste.Core.Display;
using Celeste.Core.Mathematics;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Randomness;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Graphics.Content;
using Celeste.Graphics.Display;
using Celeste.Graphics.Display.Colors;
using Celeste.Simulation.Presentation.Backdrops.Runtime;
using Celeste.Simulation.Presentation.Backdrops.Weather.Petals;

namespace Celeste.Mods.ShroomHelper.Backdrops.Petals
{
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Effects.ShroomPetals")]
    public sealed class ShroomPetals : BackdropPresentation
    {
        public struct Particle
        {
            public Vector2 Position;
            public float Speed;
            public float Spin;
            public float MaxRotate;
            public float RotationCounter;
        }

        public string shroomColor = "8B0000";
        private readonly Particle[] particles = new Particle[40];
        private readonly ISourceEngineContext engine;
        private float fade;

        public ShroomPetals(ISourceEngineContext engine)
        {
            this.engine = engine;
            var random = ((ISourceRandomEnvironment)engine).Random;
            for (int i = 0; i < particles.Length; i++)
            {
                PetalParticleMotion.InitializeMotion(random, out particles[i].Position, out particles[i].Speed,
                    out particles[i].Spin);
                PetalParticleMotion.InitializeRotation(random, out particles[i].RotationCounter,
                    out particles[i].MaxRotate);
            }
        }

        public override void Update(ISourceBackdropScene scene)
        {
            base.Update(scene);
            for (int i = 0; i < particles.Length; i++)
            {
                PetalParticleMotion.AdvancePosition(ref particles[i].Position, particles[i].Speed, engine.DeltaTime);
                PetalParticleMotion.AdvanceRotation(ref particles[i].RotationCounter, particles[i].Spin,
                    engine.DeltaTime);
            }

            fade = ScalarMath.Approach(fade, Visible ? 1f : 0f, engine.DeltaTime * 10f,
                ((ISourceExtremaEnvironment)engine).ExtremaArithmetic);
        }

        public override void Render(ISourceRenderContext context, ISourceBackdropScene level)
        {
            if (fade > 0f)
            {
                var camera = level.Camera;
                var texture = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.GameAtlas["particles/petal"];
                for (int i = 0; i < particles.Length; i++)
                {
                    Vector2 position = PetalParticleMotion.DrawPosition(particles[i].Position, camera.Position,
                        particles[i].RotationCounter, particles[i].MaxRotate, out float rotation);
                    var draw = new AtlasTextureDraw(texture, position, texture.Center,
                        SourceHexColor.HexToColor(shroomColor) * fade, Vector2.One, rotation - .8f);
                    ((ISourceRenderCommandSink<AtlasTextureDraw>)context).Draw(in draw, this);
                }
            }
        }
    }
}
