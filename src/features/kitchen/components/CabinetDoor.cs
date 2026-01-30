using Godot;
using System.Threading.Tasks;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class CabinetDoor : Node3D
    {
        [Export] public MeshInstance3D PanelMesh;
        [Export] public Node3D HandleObj;
        [Export] public Area3D ClickArea;
        [Export] public CollisionShape3D ClickShape;

        // Nastavení animace
        private bool _isOpen = false;
        private float _closedAngle = 0f;
        private float _openAngle = 90f; 
        private Tween _tween;

        public void Setup(float width, float height, float thickness, bool isGlass, bool isRightDoor)
        {
            BoxMesh newMesh = new BoxMesh();
            newMesh.Size = new Vector3(width, height, thickness);

            PanelMesh.Mesh = newMesh;

            float panelZ = isRightDoor ? -thickness / 2.0f : thickness / 2.0f;
            Vector3 centerPosition = new Vector3(width / 2.0f, height / 2.0f, panelZ);

            PanelMesh.Position = centerPosition;

            BoxShape3D newShape = new BoxShape3D();
            newShape.Size = new Vector3(width, height, thickness);

            ClickShape.Shape = newShape; 

            ClickShape.Position = centerPosition;

            if (HandleObj != null)
            {
                float handleX = width - 0.05f;
                float handleY = height - 0.10f;
                float handleZ = thickness + 0.02f;

                if (isRightDoor)
                {

                    handleZ = -handleZ;

                    HandleObj.RotationDegrees = new Vector3(0, 180, 0);
                }
                else
                {
                    HandleObj.RotationDegrees = Vector3.Zero;
                }

                HandleObj.Position = new Vector3(handleX, handleY, handleZ);
            }

            _openAngle = isRightDoor ? 90f : 90f;

            if (isRightDoor)
            {
                _openAngle = 270f;
                _closedAngle = 180f;
            }
            else
            {
                _openAngle = -90f;
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


        public void ToggleOpen()
        {
            _isOpen = !_isOpen;

            if (_tween != null && _tween.IsValid()) _tween.Kill();
            _tween = CreateTween().SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);

            float targetRotation = _isOpen ? _openAngle : _closedAngle;

            _tween.TweenProperty(this, "rotation_degrees:y", targetRotation, 0.5f);
        }
    }
}