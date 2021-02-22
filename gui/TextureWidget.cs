using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gui
{
    public class TextureWidget : Widget
    {
        public Texture2D Texture;

        public Rectangle DestinationRectangle;
        public Rectangle SourceRectangle;
        public Vector2 Origin = Vector2.Zero;
        public SpriteEffects FlipEffect = SpriteEffects.None;
        public float Rotation = 0.0f;
        public Color TintColor;
        
        //ComponentConfig requires: Name, Width, Height, Color (array of 3 floats)
        //Optional: 
        //Depth (defaults to 0.01 less than parent widget), Parent (defaults to "Root")
        //OffsetXY (the offset frohm the parent widget's origin, defaults to zero)
        public TextureWidget(Texture2D texture, core.ComponentConfig config) :
            base(config)
        {
            this.Texture = texture;

            DestinationRectangle = new Rectangle();
            SourceRectangle = DestinationRectangle;

            float[] colorArray = config.GetFloatArrayParameterValue("Color");
            float alpha = 1.0f;
            if (colorArray.Length > 3)
                alpha = colorArray[3];

            this.TintColor = new Color(colorArray[0], colorArray[1], colorArray[2], alpha);
        }

        public TextureWidget(string name, float width, float height, float depth,
                      Texture2D texture,
                      Color color) :
            base(name, width, height, depth)
        {
            Texture = texture;
            DestinationRectangle = new Rectangle();
            SourceRectangle = DestinationRectangle;
            TintColor = color;
        }

        public override void  Update(core.GameObject gameObject, GameTime gameTime)
        {
            DestinationRectangle.X = BoundingRect.X;
            DestinationRectangle.Y = BoundingRect.Y;
            DestinationRectangle.Width = BoundingRect.Width;
            DestinationRectangle.Height = BoundingRect.Height;
            SourceRectangle = DestinationRectangle;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.Texture,
                              this.DestinationRectangle,
                              null,
                              this.TintColor,
                              this.Rotation,
                              this.Origin,
                              this.FlipEffect,
                              this.Depth);
        }
    }
}
