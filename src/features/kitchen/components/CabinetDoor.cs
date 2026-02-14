using Godot;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System.Threading.Tasks;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public enum HandlePosition
    {
        Top,
        Middle,
        Bottom
    }
    public partial class CabinetDoor : Node3D, IInteractable, IHandleContainer
    {
        [Export] public MeshInstance3D PanelMesh;
        [Export] public Node3D HandleContainer;
        [Export] public Area3D ClickArea;
        [Export] public CollisionShape3D ClickShape;

        // Nastavení animace
        private bool _isOpen = false;
        private float _closedAngle = 0f;
        private float _openAngle = 90f;
        private bool _isRightDoor = false;
        private float _maxOpenAngle = 90f;
        private Tween _tween;




        public void Setup(float width, float height, float thickness, bool isGlass, bool isRightDoor)
        {
            Setup(width, height, thickness, isGlass, isRightDoor, 90, HandlePosition.Top);
        }

        public void Setup(float width, float height, float thickness, bool isGlass, bool isRightDoor, float maxOpenAgle)
        {
            Setup(width, height, thickness, isGlass, isRightDoor, maxOpenAgle, HandlePosition.Top);
        }

        public void Setup(float width, float height, float thickness, bool isGlass, bool isRightDoor, float maxOpenAgle, HandlePosition handlePosition)
        {
            BoxMesh newMesh = new BoxMesh();
            newMesh.Size = new Vector3(width, height, thickness);
            _isRightDoor = isRightDoor;
            _maxOpenAngle = maxOpenAgle;
            PanelMesh.Mesh = newMesh;

            float panelZ = isRightDoor ? -thickness / 2.0f : thickness / 2.0f;
            Vector3 centerPosition = new Vector3(width / 2.0f, height / 2.0f, panelZ);

            PanelMesh.Position = centerPosition;

            BoxShape3D newShape = new BoxShape3D();
            newShape.Size = new Vector3(width, height, thickness);

            ClickShape.Shape = newShape;

            ClickShape.Position = centerPosition;

            if (HandleContainer != null)
            {
                float handleX = width - 0.05f;
                float handleY = height - 0.10f;
                float handleZ = thickness;

                switch (handlePosition)
                {
                    case HandlePosition.Top:
                        break;
                    case HandlePosition.Middle:
                            handleY = height / 2.0f;
                        break;
                    case HandlePosition.Bottom:
                            handleY = 0.10f;
                        break;
                    default:
                        break;
                }

                if (isRightDoor)
                {

                    handleZ = -handleZ;

                    HandleContainer.RotationDegrees = new Vector3(0, 180, 90);
                }
                else
                {
                    HandleContainer.RotationDegrees = new Vector3(0, 0, 90);
                }

                HandleContainer.Position = new Vector3(handleX, handleY, handleZ);
            }

            _openAngle = isRightDoor ? 90f : 90f;

            if (isRightDoor)
            {
                _openAngle = 180f + maxOpenAgle;
                _closedAngle = 180f;
            }
            else
            {
                _openAngle = -maxOpenAgle;
                _closedAngle = 0f;
            }

            if (isGlass)
            {
                // PLACEHOLDER
                var mat = new StandardMaterial3D();
                mat.AlbedoColor = new Color(0, 0.5f, 1, 0.5f); // Poloprůhledná
                mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                PanelMesh.MaterialOverride = mat;
            }
        }

        public void Interact()
        {
            ToggleOpen();
        }
        public void ToggleOpen()
        {
            if (_isOpen == false && _closedAngle != RotationDegrees.Y)
            {
                _closedAngle = RotationDegrees.Y;

                if (_isRightDoor)
                {
                    _openAngle = _closedAngle + _maxOpenAngle;
                }
                else
                {
                    _openAngle = _closedAngle - _maxOpenAngle;
                }
            }

            _isOpen = !_isOpen;

            if (_tween != null && _tween.IsValid()) _tween.Kill();
            _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

            float targetRotation = _isOpen ? _openAngle : _closedAngle;

            _tween.TweenProperty(this, "rotation_degrees:y", targetRotation, 0.5f);
        }

        public void SetHandle(PackedScene handle)
        {
            if (HandleContainer == null) return;
            foreach (Node child in HandleContainer.GetChildren()) child.QueueFree();
            if (handle != null)
            {
                Node3D handleInstance = handle.Instantiate() as Node3D;
                HandleContainer.AddChild(handleInstance);
                handleInstance.Position = Vector3.Zero;
            }
        }
        public void SetMaterial(Material material)
        {
            PanelMesh?.MaterialOverride = material;
        }
    }
}