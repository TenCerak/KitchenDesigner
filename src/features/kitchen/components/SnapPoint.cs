using Godot;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public enum SnapType
    {
        Left,
        Right,
        Back,
        Front,
        Top,
        Bottom,
        CornerFront
    }

    public partial class SnapPoint : Area3D
    {
        [Export] public SnapType Type;
        public CabinetBase ParentCabinet { get; set; }
        public bool IsGhost { get; set; } = false;
        private CollisionShape3D _colShape;
        public override void _Ready()
        {
            base._Ready();
        }
    }
}