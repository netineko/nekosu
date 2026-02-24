// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Copyright (c) netineko. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osuTK.Graphics;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class Bubbles : Circles
    {
        public Bubbles()
        {
            ColourLight = Color4.White;
            ColourDark = Color4.White;
            SpawnRatio = 0.3f;
            Alpha = 0.7f;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (CircleScale >= 2)
                CompressScale = false;
            else
                CompressScale = true;
        }
    }
}
