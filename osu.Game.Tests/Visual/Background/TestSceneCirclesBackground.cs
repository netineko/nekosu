// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Backgrounds;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Tests.Visual.Background
{
    public partial class TestSceneCirclesBackground : OsuTestScene
    {
        private readonly Circles circles;

        private int seed = 0;

        public TestSceneCirclesBackground()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                circles = new Circles
                {
                    RelativeSizeAxes = Axes.Both,
                    ColourLight = Color4.White,
                    ColourDark = Color4.Gray,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.9f),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Circle scale", 0f, 10f, 1f, s => circles.CircleScale = s);
            AddSliderStep("Seed", 0, 1000, 0, s => { circles.Reset(s); seed = s; });
            AddToggleStep("masking", (value) => circles.Masking = value);
            AddToggleStep("depth shading", (value) => { circles.ProgressiveShading = value; circles.Reset(seed); });
            AddToggleStep("scaling", (value) => { circles.HasScaling = value; circles.Reset(seed); } );
            AddToggleStep("outlines", (value) => { circles.Outline = value; circles.Reset(seed); });
            AddSliderStep("spawn ratio", 0f, 1f, 1f, s => { circles.SpawnRatio = s; circles.Reset(seed); });
            AddSliderStep("velocity", 1, 100, 1, s => circles.Velocity = s);

            AddStep("reset", () => circles.Reset(seed));
        }
    }
}
