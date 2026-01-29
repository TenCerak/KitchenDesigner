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
        Bottom
    }

    public partial class SnapPoint : Area3D
    {
        [Export] public SnapType Type;
        public Node3D ParentCabinet;
        public bool IsGhost { get; set; } = false;
        private CollisionShape3D _colShape;
        public override void _Ready()
        {
            base._Ready();
        }
    }
}