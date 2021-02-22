using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    class Text2DComponent : gui.TextWidget
    {
        public Text2DComponent(SpriteFont font,
                               Color color,
                               string text) : base("Text2DComponent", font, text, color, 1.0f)
        {
            this.ReceiveEvents = false;
        }

        public class TextUpdater : core.ComponentVisitor
        {
            string _text;
            public TextUpdater(string text)
            {
                _text = text;
            }

            #region ComponentVisitor Members

            public void Apply(DeskWars.core.Component comp)
            {
                Text2DComponent t2d = comp as Text2DComponent;
                if (t2d != null)
                {
                    t2d.Text = _text;
                }
            }

            #endregion
        }

        public override void OnLoadLevel(DeskWars.core.GameObject gameObject, int level)
        {
            //throw new NotImplementedException();
        }
    }
}
