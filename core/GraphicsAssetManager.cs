using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeskWars.gfx;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DeskWars.core
{
    interface GraphicsAssetManager
    {
        ShadowMapEffect GetOrCreateShadowMapEffect(Vector3 diffuseColor, float alpha);
        Model GetOrCreateModel(string path);
        SpriteFont GetOrCreateSpriteFont(string path);
        Texture2D GetOrCreateTexture(string path);
    }
}
