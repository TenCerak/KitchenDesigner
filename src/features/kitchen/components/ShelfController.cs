using Godot;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class ShelfController : Node3D
    {
        [Export] public MeshInstance3D VisualMesh;
        [Export] public CollisionShape3D Collider;

        public void SetDimensions(Vector3 size)
        {
            // 1. Změna vizuálu
            if (VisualMesh.Mesh is BoxMesh box)
            {
                if (box.GetReferenceCount() > 1)
                    VisualMesh.Mesh = (Mesh)box.Duplicate();

                ((BoxMesh)VisualMesh.Mesh).Size = size;
            }

            // 2. Změna kolize
            if (Collider.Shape is BoxShape3D shape)
            {
                if (shape.GetReferenceCount() > 1)
                    Collider.Shape = (Shape3D)shape.Duplicate();

                ((BoxShape3D)Collider.Shape).Size = size;
            }
        }

        public void SetMaterial(Material mat)
        {
            VisualMesh.MaterialOverride = mat;
        }
    }
}