using System;
using System.Collections;
using Celeste.Core.Reflection.Runtime;
using Celeste.Everest.Diagnostics.Logging;
using Celeste.Everest;
using Celeste.Core.Scheduling;
using Celeste.Game.Controls.Source;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Levels.Cutscenes;
using Celeste.Game.Execution.Processes;
using Celeste.Mods.ShroomHelper.Presentation.Books;
using Celeste.Scene.Tracking;
using Celeste.Simulation;

namespace Celeste.Mods.ShroomHelper.Entities.Books
{
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Entities.ShroomBook")]
    public sealed class ShroomBook : OriginalCutsceneEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly RoomPlayerSession player;
        private readonly string bookTextKey;
        private readonly string readFlag;
        private readonly bool readFlagInverted;
        private ShroomBookPage poem;

        public ShroomBook(ISourceEngineContext engine, RoomPlayerSession player, string bookTextKey,
            string readFlag = "", bool readFlagInverted = false) : base(engine, true, false)
        {
            this.engine = engine;
            this.player = player;
            this.bookTextKey = bookTextKey;
            this.readFlag = readFlag;
            this.readFlagInverted = readFlagInverted;
        }

        public static void Open(ISourceEngineContext engine, string assetKey)
        {
            var scene = (ISourceScene)engine.Frame.Scenes.CurrentScene;
            var player =
                scene.SourceScene.Entities.GetTrackedEntity<RoomPlayerSession>(typeof(RoomPlayerSession).FullName);
            if (player != null) scene.SourceScene.QueueSourceEntity(new ShroomBook(engine, player, assetKey));
            else
                ((IEverestLoggerProcessEnvironment)engine).Logger.Log((EverestLogLevel)3, "ShroomHelper/ShroomBook",
                    "Could not open ShroomBook!");
        }

        public override void OnBegin(OriginalLevelSceneRoot level) => Add(new SimulationCoroutine(engine, Routine()));

        public override void OnEnd(OriginalLevelSceneRoot level)
        {
            player.Machine.Locked = false;
            player.Machine.State = 0;
            poem?.RemoveSelf();
            if (!string.IsNullOrWhiteSpace(readFlag)) level.Session.SetFlag(readFlag, !readFlagInverted);
        }

        private IEnumerator Routine()
        {
            player.Machine.State = 11;
            player.Machine.Locked = true;
            poem = new ShroomBookPage(engine, bookTextKey);
            ((ISourceScene)Scene).SourceScene.QueueSourceEntity(poem);
            yield return poem.EaseIn();
            while (!((IOriginalGameInputProcessEnvironment)engine).InputSource.MenuConfirm.Pressed) yield return null;
            ((IOriginalGameplayProcess)engine).AudioSession.Play("event:/ui/main/button_lowkey");
            yield return poem.EaseOut();
            poem = null;
            EndCutscene(Level, true);
        }
    }
}
