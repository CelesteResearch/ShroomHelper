using System;
using Celeste.Core.Scheduling;
using Celeste.Everest.Runtime.Levels.Update;
using Celeste.Formats.Map.Binary.Source;
using Celeste.Game.Content.Maps;
using Celeste.Mods.ShroomHelper.Backdrops.Petals;
using Celeste.Simulation.Presentation.Backdrops.Runtime;

namespace Celeste.Mods.ShroomHelper.Runtime.Backdrops
{
    public sealed class ShroomBackdropProcess : IDisposable
    {
        private readonly ISourceEngineContext engine;
        private bool disposed;

        public ShroomBackdropProcess(ISourceEngineContext engine)
        {
            this.engine = engine;
            ((IEverestLevelEntityLoadEnvironment)engine).LevelUpdateEvents.OnLoadBackdrop += LoadBackdrop;
        }

        private BackdropPresentation LoadBackdrop(OriginalChapterMapData map, BinaryPackerElement child,
            BinaryPackerElement above)
        {
            if (!child.Name.Equals("ShroomHelper/ShroomPetals", StringComparison.OrdinalIgnoreCase)) return null;
            var petals = new ShroomPetals(engine);
            if (child.HasAttr("color")) petals.shroomColor = child.Attr("color", "");
            return petals;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            ((IEverestLevelEntityLoadEnvironment)engine).LevelUpdateEvents.OnLoadBackdrop -= LoadBackdrop;
        }
    }
}
