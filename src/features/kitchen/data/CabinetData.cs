using Godot;
using System;

namespace KitchenDesigner.Features.Kitchen.Data
{
    [GlobalClass]
    public partial class CabinetData : Resource
    {
        [Signal] public delegate void DimensionsChangedEventHandler();

        private float _width = 0.6f;
        private float _height = 0.86f;
        private float _depth = 0.6f;

        [Export]
        public float Width
        {
            get => _width;
            set
            {
                if (!Mathf.IsEqualApprox(_width, value))
                {
                    _width = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        }

        [Export]
        public float Height
        {
            get => _height;
            set
            {
                if (!Mathf.IsEqualApprox(_height, value))
                {
                    _height = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        }

        [Export]
        public float Depth
        {
            get => _depth;
            set
            {
                if (!Mathf.IsEqualApprox(_depth, value))
                {
                    _depth = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        }
    }
}