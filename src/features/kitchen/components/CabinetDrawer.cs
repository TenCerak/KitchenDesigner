using Godot;
using KitchenDesigner.Features.Kitchen.Interfaces;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class CabinetDrawer : Node3D, IInteractable
    {
        [Export] public MeshInstance3D FrontPanel;
        [Export] public MeshInstance3D DrawerBox; 
        [Export] public Node3D HandleObj;

        [Export] public Area3D ClickArea;
        [Export] public CollisionShape3D ClickShape;

        private bool _isOpen = false;
        private float _slideDistance = 0.4f; 
        private Tween _tween;



        public void Setup(float width, float height, float depth)
        {
            float frontThickness = 0.02f;

            BoxMesh frontMesh = new BoxMesh();
            frontMesh.Size = new Vector3(width, height, frontThickness);
            FrontPanel.Mesh = frontMesh;


            FrontPanel.Position = new Vector3(0, 0, frontThickness / 2.0f);


            float boxW = width - 0.04f; 
            float boxH = height - 0.05f; 
            float boxD = depth - 0.05f; 

            if (DrawerBox != null)
            {
                BoxMesh boxMesh = new BoxMesh();
                boxMesh.Size = new Vector3(boxW, boxH, boxD);
                DrawerBox.Mesh = boxMesh;

                DrawerBox.Position = new Vector3(0, 0, -boxD / 2.0f);
            }

            if (ClickShape.Shape is BoxShape3D shape)
            {
                shape.Size = new Vector3(width, height, frontThickness);
            }
            ClickShape.Position = FrontPanel.Position;

            if (HandleObj != null)
            {
                HandleObj.Position = new Vector3(0, 0, frontThickness + 0.02f);
                HandleObj.RotationDegrees = new Vector3(0, 0, 90);
            }

            _slideDistance = boxD * 0.8f;
        }
        public void Interact()
        {
            ToggleOpen();
        }


        public void ToggleOpen()
        {
            _isOpen = !_isOpen;

            if (_tween != null && _tween.IsValid()) _tween.Kill();
            _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

            float targetZ = _isOpen ? _slideDistance : 0f;

            _tween.TweenProperty(this, "position:z", targetZ, 0.5f);
        }
    }
}