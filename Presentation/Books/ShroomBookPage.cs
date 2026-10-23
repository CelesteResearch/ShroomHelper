using System.Collections;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Components.Rendering;
using Celeste.Core.Display;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Rendering.States;
using Celeste.Core.Scheduling;
using Celeste.Game.Execution.Engine;
using Celeste.Game.Execution.Startup;
using Celeste.Game.Execution.Graphics;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Processes;
using Celeste.Graphics.Content;
using Celeste.Graphics.Content.Atlas;
using Celeste.Graphics.Content.VirtualAssets;
using Celeste.Graphics.Rendering;
using Celeste.Graphics.Rendering.Devices;
using Celeste.Graphics.Rendering.Drawing;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking;

namespace Celeste.Mods.ShroomHelper.Presentation.Books
{
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Entities.ShroomBook+PoemPage")]
    public sealed partial class ShroomBookPage : SceneEntityState
    {
        private readonly ISourceEngineContext engine;
        private readonly float scale = 1f;
        private readonly AtlasEntry paper;
        private VirtualRenderTarget target;
        private float alpha = 1f;
        private float rotation;
        private float timer;
        private bool easingOut;

        public ShroomBookPage(ISourceEngineContext engine, string assetKey)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration)
        {
            this.engine = engine;
            Tags = ((IOriginalGameTagsEnvironment)engine).Tags.HUD.Value;
            paper =
                ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics)
                .GuiAtlas[assetKey];
            Add(new BeforeRenderCallbackComponentState(BeforeRender));
        }

        public override void Removed(object scene)
        {
            target?.Dispose();
            target = null;
            base.Removed(scene);
        }

        public override void SceneEnd(object scene)
        {
            target?.Dispose();
            target = null;
            base.SceneEnd(scene);
        }

        public override void Update()
        {
            timer += engine.DeltaTime;
            base.Update();
        }

        public override void Render(ISourceRenderContext context)
        {
            if (target == null) return;
            var level = Scene as OriginalLevelSceneRoot;
            if (level == null || (!level.FrozenOrPaused && level.RetryPlayerCorpse == null && !level.SkippingCutscene))
            {
                ((ISourceDrawScope)context).Draw.SpriteBatch.Draw(target.Target, Position, target.Bounds,
                    (PackedColor.White * alpha).PackedValue, rotation, new Vector2(target.Width, target.Height) / 2f,
                    scale, SourceSpriteEffects.None, 0f);
                if (!easingOut)
                    SourceAtlasDrawing.DrawCentered(context, this,
                        ((OriginalGameGraphicsResources)((ISourceGameGraphicsEnvironment)engine).GameGraphics).GuiAtlas[
                            "textboxbutton"],
                        Position + new Vector2(target.Width / 2 + 40, target.Height / 2 + (timer % 1f < .25f ? 6 : 0)),
                        PackedColor.White);
            }
        }

        public void BeforeRender()
        {
            if (target == null)
                target = ((ISourceVirtualContentEnvironment)engine).VirtualContent.CreateRenderTarget(
                    "journal-poem", paper.FrameWidth, paper.FrameHeight, false, true, 0);
            ((ISourceGraphicsDeviceEnvironment)engine).GraphicsDevice.SetRenderTarget(target.Target);
            ((ISourceGraphicsDeviceEnvironment)engine).GraphicsDevice.Clear(PackedColor.Transparent);
            var context = ((ISourceRenderContextScene)Scene).SourceRenderContext;
            ((ISourceDrawScope)context).Draw.SpriteBatch.Begin(new SourceRenderBatchState(SourceSpriteSortMode.Deferred,
                SourceBlendState.AlphaBlend, SourceSamplerState.LinearClamp, SourceDepthStencilState.None,
                SourceRasterizerState.CullCounterClockwise, null, Matrix4x4.Identity));
            SourceAtlasDrawing.Draw(context, this, paper, Vector2.Zero, Vector2.Zero, PackedColor.White);
            ((ISourceDrawScope)context).Draw.SpriteBatch.End();
        }
    }
}
