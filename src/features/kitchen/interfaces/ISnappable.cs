using Godot;
using KitchenDesigner.Features.Kitchen.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Interfaces
{

    public interface ISnappable
    {
        public Node3D RootNode { get; }
        public List<SnapPoint> ActiveSnapPoints { get; }
    }

}
