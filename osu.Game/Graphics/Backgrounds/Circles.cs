// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Copyright (c) netineko. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;
using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Graphics.Backgrounds
{
    public partial class Circles : Container
    {
        private const float circle_size = 100;
        private const float base_velocity = 50;

        private Color4 colourLight = Color4.White;

        public Color4 ColourLight
        {
            get => colourLight;
            set
            {
                if (colourLight == value) return;

                colourLight = value;
                updateColours();
            }
        }

        public bool ProgressiveShading = false;

        public bool HasScaling = true;

        public bool Outline = false;

        /// <summary>
        /// Gives smaller circles a higher speed
        /// Better to turn this off when working with large circles
        /// </summary>
        public bool CompressScale = true;

        /// <summary>
        /// Higher number = Less circles
        /// </summary>
        public int Frequency = 1;

        private Color4 colourDark = Color4.Black;

        public Color4 ColourDark
        {
            get => colourDark;
            set
            {
                if (colourDark == value) return;

                colourDark = value;
                updateColours();
            }
        }

        /// <summary>
        /// Whether we should create new circles as others expire.
        /// </summary>
        protected virtual bool CreateNewCircles => true;

        /// <summary>
        /// The amount of circles we want compared to the default distribution.
        /// </summary>
        protected virtual float SpawnRatio => 1;

        private readonly BindableFloat circleScale = new BindableFloat(1f);

        public float CircleScale
        {
            get => circleScale.Value;
            set => circleScale.Value = value;
        }

        /// <summary>
        /// Whether we should drop-off alpha values of circles more quickly to improve
        /// the visual appearance of fading. This defaults to on as it is generally more
        /// aesthetically pleasing, but should be turned off in buffered containers.
        /// </summary>
        public bool HideAlphaDiscrepancies = true;

        /// <summary>
        /// The relative velocity of the circles. Default is 1.
        /// </summary>
        public float Velocity = 1;

        //private readonly SortedList<CircleParticle> parts = new SortedList<CircleParticle>(Comparer<CircleParticle>.Default);

        private Random stableRandom;

        /// <summary>
        /// Construct a new circle visualisation.
        /// </summary>
        /// <param name="seed">An optional seed to stabilise random positions / attributes. Note that this does not guarantee stable playback when seeking in time.</param>
        public Circles(int? seed = null)
        {
            if (seed != null)
                stableRandom = new Random(seed.Value);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Reset();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            circleScale.BindValueChanged(_ => Reset(), true);
        }

        protected override void Update()
        {
            base.Update();

            float elapsedSeconds = (float)Time.Elapsed / 1000;

            if (elapsedSeconds == 0)
                return;

            float movedDistance = -elapsedSeconds * Velocity * base_velocity;

            foreach (CircleParticle circle in this)
            {
                float factor = -.007f;
                float compressedScale = (float)(1 / Math.Log((factor - 1) / factor) * Math.Log((factor - circle.Scale) / factor)); // compresses the scale range to make smaller circles move faster
                if (CompressScale)
                    circle.Y += compressedScale * movedDistance;
                else
                    circle.Y += circle.Scale * movedDistance;

                float bottomPos = circle.Y + circle_size * (HasScaling ? circle.Scale : CircleScale) / 2;
                if (bottomPos < Y - DrawHeight / 2)
                    resetCircle(circle);
            }
        }

        /// <summary>
        /// Clears and re-initialises circles according to a given seed.
        /// </summary>
        /// <param name="seed">An optional seed to stabilise random positions / attributes. Note that this does not guarantee stable playback when seeking in time.</param>
        public void Reset(int? seed = null)
        {
            if (seed != null)
                stableRandom = new Random(seed.Value);

            //parts.Clear();
            Clear();
            addCircles(true);
        }

        protected int AimCount { get; private set; }

        private void addCircles(bool randomY)
        {
            // Limited by the maximum size of QuadVertexBuffer for safety.
            const int max_circles = ushort.MaxValue / (IRenderer.VERTICES_PER_QUAD + 2);

            AimCount = (int)Math.Min(max_circles, DrawWidth * DrawHeight * 0.002f / (CircleScale * CircleScale) * SpawnRatio) / (Frequency == 0 ? 1 : Frequency);

            int currentCount = Count;

            if (AimCount - currentCount == 0)
                return;

            for (int i = 0; i < AimCount - currentCount; i++)
            {
                Add(createCircle(randomY));
            }
        }

        private CircleParticle createCircle(bool randomY)
        {
            CircleParticle particle = CreateCircle();

            particle.Position = HasScaling ? getRandomPosition(randomY, particle.Scale) : getRandomPosition(randomY, CircleScale);
            particle.ColourShade = ProgressiveShading ? particle.Scale * -4 + 2 : nextRandom();
            particle.Colour = CreateCircleShade(particle.ColourShade);
            particle.Depth = HasScaling ? particle.MentalScale : -particle.MentalScale;

            return particle;
        }

        private void resetCircle(CircleParticle circle)
        {
            const float std_dev = 0.16f;
            const float mean = 0.5f;
            float u1 = 1 - nextRandom(); //uniform(0,1] random floats
            float u2 = 1 - nextRandom();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
            float scale = Math.Max(CircleScale * (mean + std_dev * randStdNormal), 0.1f);

            circle.Scale = scale;
            circle.Position = HasScaling ? getRandomPosition(false, circle.Scale) : getRandomPosition(false, CircleScale);
            circle.ColourShade = ProgressiveShading ? scale * -4 + 2 : nextRandom();
            circle.Colour = CreateCircleShade(circle.ColourShade);
            ChangeChildDepth(circle, HasScaling ? circle.MentalScale : -circle.MentalScale);
        }

        private Vector2 getRandomPosition(bool randomY, float scale)
        {
            float y = DrawHeight / 2 + circle_size * scale / 2;

            if (randomY)
            {
                y = nextRandom((int)(DrawHeight + circle_size * scale)) - DrawHeight / 2 - circle_size * scale / 2;
            }

            return new Vector2(nextRandom((int)DrawWidth) - DrawWidth / 2, y);
        }

        /// <summary>
        /// Creates a circle particle with a random scale.
        /// </summary>
        /// <returns>The circle particle.</returns>
        protected virtual CircleParticle CreateCircle()
        {
            const float std_dev = 0.16f;
            const float mean = 0.5f;

            float u1 = 1 - nextRandom(); //uniform(0,1] random floats
            float u2 = 1 - nextRandom();
            float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
            float scale = Math.Max(CircleScale * (mean + std_dev * randStdNormal), 0.1f); // random normal(mean,stdDev^2)

            return new CircleParticle(this, colourDark, colourLight) { Scale = scale };
        }

        /// <summary>
        /// Creates a shade of colour for the circles.
        /// </summary>
        /// <returns>The colour.</returns>
        protected virtual Color4 CreateCircleShade(float shade) => Interpolation.ValueAt(shade, colourDark, colourLight, 0, 1);

        private void updateColours()
        {
            foreach (CircleParticle circle in this)
            {
                circle.Colour = CreateCircleShade(circle.ColourShade);
            }
        }

        private float nextRandom() => (float)(stableRandom?.NextDouble() ?? RNG.NextSingle());
        private int nextRandom(int count) => stableRandom?.Next(count) ?? RNG.Next(count);

        protected partial class CircleParticle : Sprite
        {
            private Circles circles;

            /// <summary>
            /// The colour shade of the circle.
            /// This is needed for colour recalculation of visible circles when <see cref="ColourDark"/> or <see cref="ColourLight"/> is changed.
            /// </summary>
            public float ColourShade;

            /// <summary>
            /// The scale of the circle.
            /// </summary>
            public new float Scale
            {
                get => MentalScale;
                set
                {
                    if (circles.HasScaling)
                    {
                        base.Scale = new Vector2(value);
                    }
                    else
                    {
                        base.Scale = new Vector2(circles.CircleScale);
                    }
                    MentalScale = value;
                }
            }

            public float MentalScale; // its all in your head

            /// <summary>
            /// The colour of the triangle.
            /// </summary>
            public new Color4 Colour { get => base.Colour; set => base.Colour = value; }

            // /// <summary>
            // /// Compares two <see cref="CircleParticle"/>s. This is a reverse comparer because when the
            // /// triangles are added to the particles list, they should be drawn from largest to smallest
            // /// such that the smaller circles appear on top.
            // /// </summary>
            // /// <param name="other"></param>
            // public int CompareTo(CircleParticle other) => other.Scale.CompareTo(Scale);

            public CircleParticle(Circles circles, Color4 darkColour, Color4 lightColour)
            {
                this.circles = circles;
                Size = new Vector2(circle_size);
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Depth = base.Scale.X;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                Texture = circles.Outline ? textures.Get(@"Menu/bubble-effect-hollow") : textures.Get(@"Menu/bubble-effect");
            }
        }
    }
}
