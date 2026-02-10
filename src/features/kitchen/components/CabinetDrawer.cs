using Godot;
using KitchenDesigner.Features.Kitchen.Interfaces;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class CabinetDrawer : Node3D, IInteractable
    {
        [Export] public MeshInstance3D FrontPanel;
        [Export] public MeshInstance3D DrawerBottom;
        [Export] public MeshInstance3D DrawerBack;
        [Export] public MeshInstance3D DrawerFront;
        [Export] public MeshInstance3D DrawerLeftSide;
        [Export] public MeshInstance3D DrawerRightSide;

        [Export] public Node3D HandleObj;

        [Export] public Area3D ClickArea;
        [Export] public CollisionShape3D ClickShape;

        private bool _isOpen = false;
        private float _slideDistance = 0.4f;
        private Tween _tween;



        public void Setup(float width, float height, float depth)
        {
            float materialThickness = 0.018f;

            BoxMesh frontMesh = new BoxMesh();
            frontMesh.Size = new Vector3(width, height, materialThickness);
            FrontPanel.Mesh = frontMesh;


            FrontPanel.Position = new Vector3(0, 0, materialThickness / 2.0f);


            if (DrawerBottom is not null)
            {
                BoxMesh bottomMesh = new BoxMesh();
                bottomMesh.Size = new Vector3(width - materialThickness * 2f, materialThickness, depth - materialThickness);
                DrawerBottom.Mesh = bottomMesh;
                DrawerBottom.Position = new Vector3(0, -height / 2f + materialThickness * 1.5f, -depth / 2f + (materialThickness / 2.0f));
            }

            if (DrawerBack is not null)
            {
                BoxMesh backMesh = new BoxMesh();
                backMesh.Size = new Vector3(width - materialThickness * 2f, height - materialThickness * 3f, materialThickness);
                DrawerBack.Mesh = backMesh;
                DrawerBack.Position = new Vector3(0, 0, -depth + (materialThickness * 1.5f));

            }

            if (DrawerFront is not null)
            {
                BoxMesh frontMesh2 = new BoxMesh();
                frontMesh2.Size = new Vector3(width - materialThickness * 2f, height - materialThickness * 3f, materialThickness);
                DrawerFront.Mesh = frontMesh2;
                DrawerFront.Position = new Vector3(0, 0, -(materialThickness / 2f));

            }

            if (DrawerLeftSide is not null)
            {
                BoxMesh leftMesh = new BoxMesh();
                leftMesh.Size = new Vector3(materialThickness, height - materialThickness * 3f, depth - materialThickness * 2f);
                DrawerLeftSide.Mesh = leftMesh;
                DrawerLeftSide.Position = new Vector3(-width / 2f + materialThickness * 1.5f, 0, -depth / 2f + (materialThickness / 2.0f));
            }

            if (DrawerRightSide is not null)
            {
                BoxMesh rightMesh = new BoxMesh();
                rightMesh.Size = new Vector3(materialThickness, height - materialThickness * 3f, depth - materialThickness * 2f);
                DrawerRightSide.Mesh = rightMesh;
                DrawerRightSide.Position = new Vector3(width / 2f - materialThickness * 1.5f, 0, -depth / 2f + (materialThickness / 2.0f));
            }


            if (ClickShape.Shape is BoxShape3D shape)
            {
                shape.Size = new Vector3(width, height, materialThickness);
            }
            ClickShape.Position = FrontPanel.Position;

            if (HandleObj != null)
            {
                HandleObj.Position = new Vector3(0, 0, materialThickness + 0.02f);
                HandleObj.RotationDegrees = new Vector3(0, 0, 90);
            }

            _slideDistance = depth * 0.8f;
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