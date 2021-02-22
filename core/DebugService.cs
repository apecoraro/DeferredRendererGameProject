using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeskWars.core
{
    interface DebugService
    {
        //lifetime specifies how long the text should continue to be
        //drawn on screen - set to -1.0f to make it infinite
        //returns the id of the debug text which can be used to delete 
        //it via DeleteDebugText
        int AddDebugText(string line, float lifetime);
        void DeleteDebugText(int id);
    }
}
