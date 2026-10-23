using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Maps;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Core.Mathematics.Extrema;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Execution.Levels;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Triggers.Foundation;

namespace Celeste.Mods.ShroomHelper.Triggers.Time
{
    [CustomEntity("ShroomHelper/TimeModulationTrigger = Load")]
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Triggers.TimeModulationTrigger")]
    public sealed class TimeModulationTrigger : SourceTriggerEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly MapEntityId id;
        public float timeFrom;
        public float timeTo;
        public PositionModes PositionMode;
        public bool completeAreaOnFrozen;
        public bool persistent;
        public bool destroyOnLeave;

        public TimeModulationTrigger(ISourceEngineContext engine, EntityData data, Vector2 offset, MapEntityId id)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset,
                data.Width, data.Height, ((ISourceExtremaEnvironment)engine).ExtremaArithmetic)
        {
            this.engine = engine;
            this.id = id;
            timeFrom = data.Float("timeFrom", 1f);
            timeTo = data.Float("timeTo", 1f);
            PositionMode = data.Enum("positionMode", PositionModes.NoEffect);
            destroyOnLeave = data.Bool("destroyOnLeave", false);
            persistent = data.Bool("persistent", false);
        }

        public static TimeModulationTrigger Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset,
            OriginalLevelEntityData data) =>
            new(level.EngineContext, data, offset, new MapEntityId(room.Name, data.ID));

        public override void OnStay(RoomPlayerSession player)
        {
            base.OnStay(player);
            engine.Effects.TimeRate = timeFrom + (timeTo - timeFrom) * GetPositionLerp(player, PositionMode);
        }

        public override void OnLeave(RoomPlayerSession player)
        {
            base.OnLeave(player);
            if (destroyOnLeave) Remove();
        }

        public void Remove()
        {
            RemoveSelf();
            if (persistent) ((OriginalLevelSceneRoot)Scene).Session.DoNotLoad.Add(id);
        }
    }
}
