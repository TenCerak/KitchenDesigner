using Godot;
using KitchenDesigner.Features.Kitchen.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Interfaces
{
    public interface ISnappable
    {
        public interface ISnappable
        {
            Node3D RootNode { get; }
            List<SnapPoint> ActiveSnapPoints { get; }
            void UpdateSnapPoints();
            Quaternion GetRotation();
        }
    }
}
