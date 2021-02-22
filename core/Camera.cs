using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace DeskWars.core
{
    interface Camera
    {
        Vector3 GetPosition();
        Vector3 GetLookAt();
        Vector3 GetUp();
        Matrix GetViewMatrix();
        Matrix GetProjectionMatrix();
    }
}
