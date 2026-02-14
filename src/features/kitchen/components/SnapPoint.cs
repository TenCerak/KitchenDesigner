using Godot;
using KitchenDesigner.Features.Kitchen.Interfaces;

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
        ApplianceMount
    }

    public partial class SnapPoint : Area3D
    {
        [Export] public SnapType Type;
        public ISnappable ParentObject { get; set; }
        public bool IsGhost { get; set; } = false;
        private CollisionShape3D _colShape;
    }
}