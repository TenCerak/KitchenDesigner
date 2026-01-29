using Godot;
using System;

namespace KitchenDesigner.Features.Kitchen.Data
{
    [GlobalClass]
    public partial class CabinetData : Resource
    {
        [Signal] public delegate void DimensionsChangedEventHandler();
        [Signal] public delegate void WorktopToggledEventHandler();

        [Export]
        public bool HasWorktop
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.WorktopToggled);
            }
        } = true;

        public float WorktopThickness = 0.038f; // Standard 38mm
        public float WorktopOverhang = 0.02f;   // Přesah 2cm přes dvířka

        [Export]
        public float Width
        {
            get;
            set
            {
                if (!Mathf.IsEqualApprox(field, value))
                {
                    field = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        } = 0.6f;

        [Export]
        public float Height
        {
            get;
            set
            {
                if (!Mathf.IsEqualApprox(field, value))
                {
                    field = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        } = 0.86f;

        [Export]
        public float Depth
        {
            get;
            set
            {
                if (!Mathf.IsEqualApprox(field, value))
                {
                    field = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        } = 0.6f;

        [Export]
        public int ShelfCount
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    EmitSignal(SignalName.DimensionsChanged);
                }
            }
        } = 1;

        public CabinetData Duplicate()
        {
            CabinetData copy = new CabinetData();
            copy.Width = this.Width;
            copy.Height = this.Height;
            copy.Depth = this.Depth;
            copy.ShelfCount = this.ShelfCount;
            copy.HasWorktop = this.HasWorktop;
            return copy;
        }

    }
}