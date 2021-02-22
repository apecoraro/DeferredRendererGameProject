using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using DPSF;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx.ps
{
    public class Billboard : DPSF.DefaultQuadParticleSystem
    {
        public struct BillboardData
        {
            public BillboardData(Color color, Vector2 wh)
            {
                Color = color;
                WidthHeight = wh;
                OffsetXY = Vector2.Zero;
            }
            public Color Color;
            public Vector2 WidthHeight;
            public Vector2 OffsetXY;
        }

        public List<BillboardData> Billboards = null;
        public List<DefaultQuadParticle> BillboardParticles = new List<DefaultQuadParticle>();

        public Billboard(GraphicsDevice graphicsDevice,
                         ContentManager contentManager) : 
            base(null)
        {
            InitializeQuadParticleSystem(graphicsDevice, contentManager, 5, 5, UpdateVertexProperties);

            Name = "Billboard";

            Emitter.EmitParticlesAutomatically = false;
            Emitter.ParticlesPerSecond = 5000;
            Emitter.Enabled = true;

            ParticleEvents.RemoveAllEvents();
            ParticleSystemEvents.RemoveAllEvents();

            ParticleEvents.AddEveryTimeEvent(UpdateBillboard);
            ParticleEvents.AddEveryTimeEvent(UpdateParticleTransparencyToFadeInUsingLerp);

            ParticleInitializationFunction = InitializeParticleProperties;
        }

        public int FindParticleIndex(DefaultQuadParticle particle)
        {
            for (int i = 0; i < BillboardParticles.Count; ++i)
            {
                if (BillboardParticles[i] == particle)
                    return i;
            }

            return BillboardParticles.Count;
        }

        public void UpdateBillboard(DefaultQuadParticle cParticle, float fElapsedTimeInSeconds)
        {
            core.Camera camera = (core.Camera)core.Game.instance().Services.GetService(typeof(core.Camera));
            cParticle.Normal = camera.GetPosition() - cParticle.Position;
            cParticle.Right = Vector3.Cross(camera.GetUp(), camera.GetPosition() - camera.GetLookAt());

            int particleIndex = FindParticleIndex(cParticle);
            if (particleIndex < Billboards.Count)
            {
                cParticle.Width = Billboards[particleIndex].WidthHeight.X;
                cParticle.Height = Billboards[particleIndex].WidthHeight.Y;

                // Give the Particle a random Color
                cParticle.Color = Billboards[particleIndex].Color;
                cParticle.Position = Emitter.PositionData.Position;
                cParticle.Position += cParticle.Right * Billboards[particleIndex].OffsetXY.X;
                cParticle.Position += cParticle.Up * Billboards[particleIndex].OffsetXY.Y;
            }
            else
            {
                cParticle.Width = 0.0f;
                cParticle.Height = 0.0f;
            }
        }

        public void InitializeParticleProperties(DefaultQuadParticle cParticle)
        {
            // Set the Particle's Lifetime (how long it should exist for)
            cParticle.Lifetime = 0.0f;

            // Set the Particle's initial Position to be wherever the Emitter is
            cParticle.Position = Emitter.PositionData.Position;
            cParticle.Normal = Vector3.UnitZ;

            // Give the Particle a random Size
            cParticle.Width = Billboards[BillboardParticles.Count].WidthHeight.X;
            cParticle.Height = Billboards[BillboardParticles.Count].WidthHeight.Y;

            // Give the Particle a random Color
            cParticle.Color = Billboards[BillboardParticles.Count].Color;
            BillboardParticles.Add(cParticle);
        }
    }
}
