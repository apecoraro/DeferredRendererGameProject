using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.gui
{
    public class MenuItemSelection : TextureWidget
    {
        TextureWidget _upButton;
        TextureWidget _downButton;
        TextWidget _selectedText;

        class UpButtonMessageHandler : gui.WidgetMessageHandler
        {
            public UpButtonMessageHandler(TextureWidget upBtn) :
                base(upBtn)
            {
            }

            public override void OnMouseEvent(MsgMouseEvent msgMouseEvt)
            {
                //TODO change selection
            }
        }

        UpButtonMessageHandler _upButtonMessageHandler = null;

        class DownButtonMessageHandler : gui.WidgetMessageHandler
        {
            public DownButtonMessageHandler(TextureWidget downBtn) :
                base(downBtn)
            {
            }

            public override void OnMouseEvent(MsgMouseEvent msgMouseEvt)
            {
                //TODO change selection
            }
        }

        DownButtonMessageHandler _downButtonMessageHandler = null;
        //TODO implement message handlers and saving

        public MenuItemSelection(Texture2D backgroundTexture,
                                 Texture2D upButtonTexture,
                                 Texture2D downButtonTexture,
                                 SpriteFont font,
                                 float textScale,
                                 core.ComponentConfig config) :
            base(backgroundTexture, config)
        {
            Construct(upButtonTexture, downButtonTexture, font, textScale);
        }

        void Construct(Texture2D upButtonTexture,
                       Texture2D downButtonTexture,
                       SpriteFont font,
                       float textScale)
        {
            _upButton = new TextureWidget("UpButton", 
                                          this.Width * 0.1f, 
                                          this.Height * 0.48f, 
                                          -1.0f, 
                                          upButtonTexture, 
                                          this.TintColor);
            _upButton.SetOffsetTranslation(this.Width - _upButton.Width, 0.01f);
            this.AddChild(_upButton);

            _downButton = new TextureWidget("DownButton",
                                          this.Width * 0.1f,
                                          this.Height * 0.48f,
                                          -1.0f,
                                          downButtonTexture,
                                          this.TintColor);
            _downButton.SetOffsetTranslation(this.Width - _upButton.Width, 0.02f + _upButton.Height);
            this.AddChild(_downButton);

            _selectedText = new TextWidget("SelectionText",
                                            font,
                                            "-- None Selected --",
                                            this.TintColor,
                                            textScale);
        }

        public MenuItemSelection(string name, 
                                 float width, 
                                 float height, 
                                 float depth,
                                 Texture2D backgroundTexture,
                                 Color backgroundColor,
                                 Texture2D upButtonTexture,
                                 Texture2D downButtonTexture,
                                 SpriteFont font,
                                 float textScale) :
            base(name, width, height, depth, backgroundTexture, backgroundColor)
        {
            Construct(upButtonTexture, downButtonTexture, font, textScale);
        }

    }
}
