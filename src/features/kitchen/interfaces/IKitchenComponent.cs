using Godot;

namespace KitchenDesigner.Features.Kitchen.Interfaces
{
    public interface IKitchenComponent
    {
        void SetHighlight(bool active, bool isDeletePreview = false);

        void Delete();

        Node3D AsNode();
    }
}