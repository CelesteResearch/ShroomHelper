using System;
using System.Collections.Generic;
using System.Numerics;
using Celeste.Core.Components;
using Celeste.Core.Reflection.Runtime;
using Celeste.Core.Rendering;
using Celeste.Core.Scheduling;
using Celeste.Everest.Modules.Entities.Metadata;
using Celeste.Game.Content.Maps.Entities;
using Celeste.Game.Content.Maps.Levels;
using Celeste.Game.Execution.Levels;
using Celeste.Game.Execution.Startup;
using Celeste.Graphics.Animation;
using Celeste.Graphics.Content;
using Celeste.Physics.Collision;
using Celeste.Physics.Collision.Shapes;
using Celeste.Scene.Entities;
using Celeste.Scene.Tracking.Attributes;
using Celeste.Simulation.Gameplay.Collision.Blockers;
using Celeste.Simulation.Gameplay.Platforms.StaticMovers;

namespace Celeste.Mods.ShroomHelper.Entities.Walls
{
    [CustomEntity("ShroomHelper/AttachedIceWall = Create")]
    [SceneTracked]
    [RuntimeTypeAlias("Celeste.Mod.ShroomHelper.Entities.AttachedIceWall")]
    public sealed class AttachedIceWall : SceneEntityState
    {
        public int Facing;
        private readonly int spriteOffset;
        private readonly SourceStaticMoverState staticMover;
        private readonly ClimbBlockerState climbBlocker;
        private readonly List<Sprite> tiles;
        private readonly ISourceEngineContext engine;
        private Vector2 imageOffset;

        public static AttachedIceWall Create(OriginalLevelSceneRoot level, OriginalLevelRoomData room,
            Vector2 offset, OriginalLevelEntityData data)
            => new(level.EngineContext, data.Position + offset, data.Height, data.Bool("left", true),
                data.Int("spriteOffset", 0));

        public AttachedIceWall(ISourceEngineContext engine, Vector2 position, float height, bool left, int offset)
            : base(((ISourceEntitySourceDataEnvironment)engine).EntitySourceDataRegistration, position)
        {
            this.engine = engine;
            spriteOffset = offset;
            Tags = ((IOriginalGameTagsEnvironment)engine).Tags.TransitionUpdate.Value;
            Depth = 1999;
            staticMover = new SourceStaticMoverState
            {
                OnShake = OnShake,
                OnAttach = platform => Depth = platform.Depth + 1
            };
            if (left)
            {
                Facing = -1;
                Collider = new HitboxColliderGeometry(2f, height);
                staticMover.SolidChecker =
                    solid => SourceEntityCollision.CheckAt(this, solid, Position - Vector2.UnitX);
            }
            else
            {
                Facing = 1;
                Collider = new HitboxColliderGeometry(2f, height, 6f);
                staticMover.SolidChecker =
                    solid => SourceEntityCollision.CheckAt(this, solid, Position + Vector2.UnitX);
            }

            OwnedComponents.Add(staticMover);
            OwnedComponents.Add(climbBlocker = new ClimbBlockerState(false));
            tiles = BuildSprite(left);
            staticMover.OnEnable = OnEnable;
            staticMover.OnDisable = OnDisable;
        }

        public override void Added(object scene)
        {
            base.Added(scene);
            climbBlocker.Blocking = true;
            tiles.ForEach(tile => tile.Play("ice"));
        }

        public override void Update()
        {
            if (!((OriginalLevelSceneRoot)Scene).Transitioning) base.Update();
        }

        public override void Render(ISourceRenderContext context)
        {
            Vector2 position = Position;
            Position += imageOffset;
            base.Render(context);
            Position = position;
        }

        private void OnEnable() => Visible = Collidable = true;
        private void OnDisable() => Visible = Collidable = false;
        private void OnShake(Vector2 amount) => imageOffset += amount;

        private List<Sprite> BuildSprite(bool left)
        {
            List<Sprite> result = new();
            for (int i = 0; (float)i < Height; i += 8)
            {
                string name = i == 0 ? "WallBoosterTop" :
                    !((float)(i + 16) > Height) ? "WallBoosterMid" : "WallBoosterBottom";
                Sprite sprite = ((ISourceGameGraphicsEnvironment)engine).GameGraphics.SpriteFactory.Create(name);
                if (!left)
                {
                    sprite.FlipX = true;
                    sprite.Position = new Vector2(4f + spriteOffset, i);
                }
                else sprite.Position = new Vector2(0f - spriteOffset, i);

                result.Add(sprite);
                OwnedComponents.Add(sprite);
            }

            return result;
        }
    }
}
