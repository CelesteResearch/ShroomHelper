using System.Collections;
using System.Numerics;
using Celeste.Core.Audio;
using Celeste.Core.Geometry;
using Celeste.Core.Maps;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Game.Audio.Surfaces;
using Celeste.Game.Content.Maps;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Levels;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Tiles;
using Celeste.Physics.Collision;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation;
using Celeste.Simulation.Gameplay.Actors.Debris;
using Celeste.Simulation.Gameplay.Platforms.Foundation;
using Celeste.Simulation.Gameplay.Platforms.Collision;
using Celeste.Simulation.Gameplay.Platforms.StaticMovers;
using Celeste.Simulation.Gameplay.Tiles;
using Celeste.Simulation.Presentation.Lighting;

namespace Celeste.Mods.ShroomHelper.Entities.Blocks
{
    [CustomEntity("ShroomHelper/CrumbleBlockOnTouch = Create")]
    [SceneTracked]
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Entities.CrumbleBlockOnTouch")]
    public class CrumbleBlockOnTouch : SourceSolidEntityState
    {
        public bool Permanent;
        public float Delay;
        public bool Triggered;
        public bool BlendIn;
        public bool DestroyAttachedMovers;
        private readonly char tileType;
        private readonly MapEntityId id;
        private readonly ISourceEngineContext engine;

        public static CrumbleBlockOnTouch Create(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset, OriginalLevelEntityData data) => new(data.Position + offset, data.Char("tiletype", 'm'),
            data.Width, data.Height, data.Bool("blendin", true), data.Bool("persistent"), data.Float("delay"),
            data.Bool("destroyStaticMovers"), new MapEntityId(room.Name, data.ID),
            ((ISourcePlatformCollisionScene)level).SolidWorldFactory.Create(data.Position + offset, true));

        public CrumbleBlockOnTouch(Vector2 position, char tileType, float width, float height, bool blendIn,
            bool persistent, float delay, bool destroyStaticMovers, MapEntityId id, ISourceSolidWorld world)
            : base(position, width, height, true, world)
        {
            engine = world.EngineContext;
            Depth = -12999;
            this.id = id;
            this.tileType = tileType;
            BlendIn = blendIn;
            Delay = delay;
            Permanent = persistent;
            DestroyAttachedMovers = destroyStaticMovers;
            SurfaceSoundIndex = ((IOriginalTileSurfaceEnvironment)engine).SurfaceIndices.TileToIndex[tileType];
        }

        public override void Awake(object scene)
        {
            base.Awake(scene);
            var autotiler = ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics)
                .ForegroundAutotiler;
            TileGrid tileGrid;
            if (!BlendIn) tileGrid = autotiler.GenerateBox(tileType, (int)Width / 8, (int)Height / 8).TileGrid;
            else
            {
                var level = (OriginalLevelSceneRoot)Scene;
                IntRect bounds = ((OriginalChapterMapData)level.Session.MapData).TileBounds;
                tileGrid = autotiler.GenerateOverlay(tileType, (int)(X / 8f) - bounds.Left,
                    (int)(Y / 8f) - bounds.Top, (int)Width / 8, (int)Height / 8, level.SolidsData).TileGrid;
                Depth = -10501;
            }

            Add(tileGrid);
            Add(new SimulationCoroutine(engine, Sequence()));
            Add(new TileInterceptorState(tileGrid, true));
            Add(new LightOccludeState());
            if (SourceTrackedEntityCollision.Check(((ISourceScene)Scene).SourceScene.Entities,
                    typeof(RoomPlayerSession).FullName, this)) RemoveSelf();
        }

        public override void OnStaticMoverTrigger(SourceStaticMoverState mover) => Triggered = true;

        public void Break()
        {
            if (!Collidable || Scene == null) return;
            ((ISourceSpatialAudioEnvironment)engine).SpatialAudio.Play(
                "event:/new_content/game/10_farewell/quake_rockbreak", Position);
            Collidable = false;
            var sourceScene = ((ISourceScene)Scene).SourceScene;
            for (int i = 0; i < Width / 8f; i++)
            for (int j = 0; j < Height / 8f; j++)
                if (!SourceSceneCollision.CheckRect(sourceScene.Entities, "Celeste.Solid",
                        new IntRect((int)X + i * 8, (int)Y + j * 8, 8, 8)))
                    sourceScene.QueueSourceEntity(engine.EntityPools.Create<DebrisState>()
                        .Init(Position + new Vector2(4 + i * 8, 4 + j * 8), tileType, true).BlastFrom(TopCenter));
            if (Permanent) ((OriginalLevelSceneRoot)Scene).Session.DoNotLoad.Add(id);
            if (DestroyAttachedMovers) DestroyStaticMovers();
            RemoveSelf();
        }

        public bool HasPlayerLeaning()
        {
            var tracked =
                ((ISourceScene)Scene).SourceScene.Entities.GetTrackedEntities(typeof(RoomPlayerSession).FullName);
            if (tracked.Count == 0) return false;
            var player = (RoomPlayerSession)tracked[0];
            if (player.Facing == -1 && SourceEntityCollision.CheckAt(this, player, Position + Vector2.UnitX))
                return true;
            return player.Facing == 1 && SourceEntityCollision.CheckAt(this, player, Position - Vector2.UnitX);
        }

        private bool PlayerBreakCheck() => HasPlayerRider() || HasPlayerLeaning();

        private IEnumerator Sequence()
        {
            while (!Triggered && !PlayerBreakCheck()) yield return null;
            while (Delay > 0f)
            {
                Delay -= engine.DeltaTime;
                yield return null;
            }

            Break();
        }
    }
}
