using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    interface Renderer
    {
        void Draw(GameObjectDB gameObjectDB, Microsoft.Xna.Framework.GameTime gameTime);
    }
}
