// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Backgrounds;
using osu.Framework.Graphics;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osuTK;

namespace osu.Game.Tests.Visual.Background
{
    public partial class TestSceneBubblesBackground : OsuTestScene
    {
        private readonly Bubbles bubbles;

        private int seed = 0;

        public TestSceneBubblesBackground()
        {
            Children = new Drawable[]
            {
                bubbles = new Bubbles
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.9f),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Circle scale", 0f, 10f, 1f, s => bubbles.CircleScale = s);
            AddSliderStep("Seed", 0, 1000, 0, s => { bubbles.Reset(s); seed = s; });
            AddToggleStep("masking", (value) => bubbles.Masking = value);
            AddSliderStep("velocity", 1, 100, 1, s => bubbles.Velocity = s);

            AddStep("reset", () => bubbles.Reset(seed));
        }
    }
}
