using Godot;
using System;

namespace KitchenDesigner.Features.Kitchen.Data
{
    [GlobalClass]
    public partial class CabinetData : Resource
    {
        [Signal] public delegate void DimensionsChangedEventHandler();
        [Signal] public delegate void WorktopToggledEventHandler();
        [Signal] public delegate void DoorChangedEventHandler();

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

        [ExportGroup("Fronts")]
        [Export]
        public DoorType DoorType
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.DoorChanged);
            }
        } = DoorType.None;

        [Export]
        public DoorStyle DoorStyle
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.DoorChanged);
            }
        } = DoorStyle.Solid;

        [Export]
        public Material HandleMaterial
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.DoorChanged);
            }
        }

        [ExportGroup("Corner Settings")]
        [Export]
        public CabinetShape Shape
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.DimensionsChanged);
            }
        } = CabinetShape.Standard;
        [Export]
        public float CornerBlindWidth
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.DimensionsChanged);
            }
        } = 0.60f;
        [Export]
        public bool CornerIsLeft
        {
            get; set
            {
                field = value;
                EmitSignal(SignalName.DimensionsChanged);
            }
        } = true;

        public CabinetData Duplicate()
        {
            CabinetData copy = new CabinetData();
            copy.Width = this.Width;
            copy.Height = this.Height;
            copy.Depth = this.Depth;
            copy.ShelfCount = this.ShelfCount;
            copy.HasWorktop = this.HasWorktop;
            copy.DoorType = this.DoorType;
            copy.DoorStyle = this.DoorStyle;
            copy.HandleMaterial = this.HandleMaterial;
            copy.Shape = this.Shape;
            copy.CornerBlindWidth = this.CornerBlindWidth;
            copy.CornerIsLeft = this.CornerIsLeft;
            return copy;
        }

    }
}