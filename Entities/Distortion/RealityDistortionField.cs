using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking.Attributes;

namespace Celeste.Mods.ShroomHelper.Entities.Distortion
{
    [CustomEntity("ShroomHelper/RealityDistortionField")]
    [SceneTracked(false)]
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Entities.RealityDistortionField")]
    public sealed class RealityDistortionField : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        public float rippleAreaMultiplier;
        public uint Seed;
        public uint Seed2;
        public float floatingOpacity = .01f;
        private readonly bool active = true;
        private bool glitcherAdded;

        public RealityDistortionField(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset)
        {
            this.engine = engine;
            rippleAreaMultiplier = data.Float("rippleAreaMultiplier", 1f);
            Depth = 2000;
        }

        public override void Update()
        {
            if (!glitcherAdded) Add(new SimulationCoroutine(engine, GlitchEffect()));
            base.Update();
        }

        private IEnumerator GlitchEffect()
        {
            glitcherAdded = true;
            var level = Scene as OriginalLevelSceneRoot;
            float width = Width;
            _ = Height;
            while (active)
            {
                level.Displacement.AddBurst(Center, 10f * (rippleAreaMultiplier / 6f), 0f,
                    width + 10f * rippleAreaMultiplier, .8f);
                yield return 5;
            }

            glitcherAdded = false;
            yield return true;
        }
    }
}
