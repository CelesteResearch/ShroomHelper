using Celeste.Core.Reflection.Runtime;
using Celeste.Mods.ShroomHelper.Runtime.Backdrops;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Runtime;

namespace Celeste.Mods.ShroomHelper.Modules
{
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.ShroomHelperModule")]
    public sealed class ShroomHelperModule(ISourceEngineContext engine) : SourceEverestModule(engine)
    {
        private ShroomBackdropProcess backdrops;

        public override void Load()
        {
            backdrops = new ShroomBackdropProcess(SourceEngine);
        }

        public override void Unload()
        {
            backdrops?.Dispose();
            backdrops = null;
        }
    }
}
