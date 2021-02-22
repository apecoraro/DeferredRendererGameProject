using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace DeskWars.gfx
{
    class FramesPerSecondText : gfx.Text2DComponent
    {
        TimeSpan _elapsedTime = TimeSpan.FromSeconds(0);
        TimeSpan _oneFourthOfSecond = TimeSpan.FromSeconds(0.25);
        int _frameCounter = 0;
        int _updateCount = 0;
        public FramesPerSecondText(SpriteFont font,
                                   Color color) :
            base(font, color, "FPS: ")
        {
        }

        public override void Update(core.GameObject gameObject, GameTime gameTime)
        {
            core.Game game = core.Game.instance();
            _elapsedTime += gameTime.ElapsedRealTime;
            if (_elapsedTime > _oneFourthOfSecond)
            {
                _elapsedTime -= _oneFourthOfSecond;
                _frameCounter = game.FrameCounter * 4;
                game.FrameCounter = 0;
            }

            ++_updateCount;
            if (_updateCount == 5)
            {
                _updateCount = 0;

                float updateEngineMilliSecs = (float)game.EngineUpdateTime * 1000.0f;
                float updateDBMilliSecs = (float)game.GameObjectDBUpdateTime * 1000.0f;
                float renderMilliSecs = (float)game.RenderTime * 1000.0f;

                string uems = updateEngineMilliSecs.ToString();
                uems = uems.Substring(0, uems.Length < 4 ? uems.Length : 4);

                string dbms = updateDBMilliSecs.ToString();
                dbms = dbms.Substring(0, dbms.Length < 4 ? dbms.Length : 4);

                string rms = renderMilliSecs.ToString();
                rms = rms.Substring(0, rms.Length < 4 ? rms.Length : 4);
                this.Text = "(EU, DB, GFX)=("
                          + uems
                          + ", "
                          + dbms
                          + ", "
                          + rms
                          + ") FPS: " + _frameCounter;
            }
        }
    }
}
