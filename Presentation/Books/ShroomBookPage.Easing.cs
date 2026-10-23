using System.Collections;
using System.Numerics;
using Celeste.Core.Mathematics.Easing;
using Celeste.Core.Mathematics.LinearAlgebra;
using Celeste.Game.Execution.Engine;
using Celeste.Game.Execution;
using Celeste.Game.Execution.Processes;

namespace Celeste.Mods.ShroomHelper.Presentation.Books
{
    public sealed partial class ShroomBookPage
    {
        public IEnumerator EaseIn()
        {
            ((IOriginalGameplayProcess)engine).AudioSession.Play("event:/game/03_resort/memo_in");
            Vector2 center = new Vector2(EngineRuntime.Width, EngineRuntime.Height) / 2f;
            Vector2 from = center + new Vector2(0, 200);
            Vector2 to = center;
            float rFrom = -.1f, rTo = .05f;
            for (float p = 0; p < 1; p += engine.DeltaTime)
            {
                Position = SourceVector2Math.Add(from,
                    SourceVector2Math.Multiply(SourceVector2Math.Subtract(to, from),
                        ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeOut(p)));
                alpha = ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeOut(p);
                rotation = rFrom + (rTo - rFrom) * ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeOut(p);
                yield return null;
            }
        }

        public IEnumerator EaseOut()
        {
            ((IOriginalGameplayProcess)engine).AudioSession.Play("event:/game/03_resort/memo_out");
            easingOut = true;
            Vector2 from = Position;
            Vector2 to = new Vector2(EngineRuntime.Width, EngineRuntime.Height) / 2f + new Vector2(0, -200);
            float rFrom = rotation, rTo = rotation + .1f;
            for (float p = 0; p < 1; p += engine.DeltaTime * 1.5f)
            {
                Position = SourceVector2Math.Add(from,
                    SourceVector2Math.Multiply(SourceVector2Math.Subtract(to, from),
                        ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeIn(p)));
                alpha = 1 - ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeIn(p);
                rotation = rFrom + (rTo - rFrom) * ((ISourceEasingEnvironment)engine).EasingArithmetic.CubeIn(p);
                yield return null;
            }

            RemoveSelf();
        }
    }
}
