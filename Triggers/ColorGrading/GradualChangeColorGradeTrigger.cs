using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.ShroomHelper.Triggers.ColorGrading
{
    [CustomEntity("ShroomHelper/GradualChangeColorGradeTrigger")]
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Triggers.GradualChangeColorGradeTrigger")]
    public sealed class GradualChangeColorGradeTrigger : SourceTriggerEntityState
    {
        private string colorGrade;
        public float speed;

        public GradualChangeColorGradeTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
                data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            colorGrade = data.Attr("colorGrade", "none");
            speed = data.Float("speed", 1f);
        }

        public override void OnEnter(RoomPlayerSession player)
        {
            base.OnEnter(player);
            if (colorGrade == "none") colorGrade = null;
            if (Scene is OriginalLevelSceneRoot level) level.NextColorGrade(colorGrade, speed);
        }
    }
}
