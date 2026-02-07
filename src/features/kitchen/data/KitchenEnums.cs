using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Data
{
    public enum CabinetShape
    {
        Standard,
        CornerBlind,
        CornerL,       
        CornerDiagonal
    }
    public enum DoorType
    {
        None,           
        SingleLeft,     
        SingleRight,    
        Double,        
        FlipUp,         
        FlipUpDouble,
        Drawer
    }

    public enum DoorStyle
    {
        Solid,
        Glass
    }
}
