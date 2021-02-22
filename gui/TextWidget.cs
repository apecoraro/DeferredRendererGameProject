using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gui
{
    public class TextWidget : Widget
    {
        SpriteFont _font = null;
        string _text = "";
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        Color _color = Color.Black;
        SpriteEffects _spriteEffect;
        Vector2 _position = Vector2.Zero;
        float _scale = 1.0f;

        public float Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        //ComponentConfig requires: Name, Width, Height, Color (array of 3 floats)
        //Optional: Depth (defaults to 0.01 less than parent widget), Parent (defaults to "Root")
        public TextWidget(SpriteFont font, DeskWars.core.ComponentConfig config) :
            base(config, font.MeasureString(config.GetParameterValue("Text")))
        {
            float[] colorArray = config.GetFloatArrayParameterValue("Color");
            Color color = new Color(colorArray[0], colorArray[1], colorArray[2]);

            string text = config.GetParameterValue("Text");
            float scale = float.Parse(config.GetParameterValue("Scale"));
            Construct(font, text, color, scale);
        }

        public TextWidget(string name,
                          SpriteFont font, 
                          string text, 
                          Color color, 
                          float scale) :
            base(name, font.MeasureString(text), -1.0f)
        {
            Construct(font, text, color, scale);
        }

        private void Construct(SpriteFont font, string text, Color color, float scale)
        {
            _font = font;
            _text = (text == null ? "" : text);
            _color = color;
            _spriteEffect = new SpriteEffects();
            _scale = scale;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            _position.X = this.BoundingRect.X;
            _position.Y = this.BoundingRect.Y;

            spriteBatch.DrawString(_font, 
                                   _text, 
                                   _position, 
                                   _color, 
                                   0.0f, 
                                   Vector2.Zero, 
                                   _scale, 
                                   _spriteEffect, 
                                   this.Depth);
        }
    }
}
