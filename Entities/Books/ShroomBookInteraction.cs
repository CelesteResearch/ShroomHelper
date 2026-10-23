using System;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Geometry;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Formats.Map.Entities.Structure;
using Celeste.Game.Components.Talking;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Execution.Levels;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;
using Celeste.Simulation;

namespace Celeste.Mods.ShroomHelper.Entities.Books
{
    [CustomEntity("ShroomHelper/ShroomBookInteraction = Load")]
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Entities.ShroomBookInteraction")]
    public sealed class ShroomBookInteraction : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        public OriginalTalkComponent Talker;
        public string assetKey;
        public string enableFlag;
        public string readFlag;
        public bool enableFlagInverted;
        public bool readFlagInverted;

        public ShroomBookInteraction(ISourceEngineContext engine, EntityData data, Vector2 offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, data.Position + offset)
        {
            this.engine = engine;
            Collider = new HitboxColliderGeometry(data.Width, data.Height);
            assetKey = data.Attr("assetKey", "shroompage");
            enableFlag = data.Attr("enableFlag", "");
            if (!string.IsNullOrWhiteSpace(enableFlag) && enableFlag[0] == '!')
            {
                enableFlagInverted = true;
                enableFlag = enableFlag.Substring(1);
            }

            readFlag = data.Attr("readFlag", "");
            if (!string.IsNullOrWhiteSpace(readFlag) && readFlag[0] == '!')
            {
                readFlagInverted = true;
                readFlag = readFlag.Substring(1);
            }

            Vector2 at = new(data.Width / 2, 0);
            if (data.Nodes.Length != 0) at = data.Nodes[0] - data.Position;
            Add(Talker = new OriginalTalkComponent((IOriginalTalkProcess)engine,
                new IntRect(0, 0, data.Width, data.Height), at, OnTalk));
            Talker.PlayerMustBeFacing = false;
        }

        public static ShroomBookInteraction Load(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset,
            OriginalLevelEntityData data) => new(level.EngineContext, data, offset);

        public override void Awake(object scene)
        {
            if (string.IsNullOrWhiteSpace(enableFlag) ||
                ((OriginalLevelSceneRoot)Scene).Session.GetFlag(enableFlag) != enableFlagInverted) base.Awake(scene);
            else RemoveSelf();
        }

        public void OnTalk(RoomPlayerSession player) => ((ISourceScene)Scene).SourceScene.QueueSourceEntity(
            new ShroomBook(engine, player, assetKey, readFlag, readFlagInverted));
    }
}
