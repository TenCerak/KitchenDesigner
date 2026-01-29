using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Common.Utils;
using KitchenDesigner.Features.Kitchen.Components;
using KitchenDesigner.Features.Kitchen.UI;
using System;

namespace KitchenDesigner.Features.Kitchen.Tools
{
    public partial class CabinetPlacementTool : Node3D, IARTool
    {
        [Export] public string ToolName { get; private set; } = "Umístit skříňku";
        public bool IsActive { get; set; } = false;

        [Export] public PackedScene CabinetScene;

        [ExportGroup("UI")]
        [Export] public PackedScene SettingsUiPrefab;

        [Export] public float SnapConnectDistance = 0.20f;
        [Export] public float SnapBreakDistance = 0.60f;

        [ExportGroup("Manual Control")]
        [Export] public float RotationSpeed = 2.0f;
        [Export] public float MoveSpeed = 1.0f;
        [Export] public float MinDistance = 0.5f;
        [Export] public float MaxDistance = 3.0f;

        private float _currentRotationY = 0f;
        private float _currentDistance = 1.5f;

        private bool _isSnapped = false;
        private Vector3 _snappedPosition;
        private Quaternion _snappedRotation;

        private XrHandManager _handManager;
        private Node3D _ghostInstance;
        private CabinetController _ghostController;

        public void Initialize(XrHandManager handManager)
        {
            _handManager = handManager;
            handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, true);
        }

        public void Reattach(XrHandManager handManager)
        {
            _handManager = handManager;
            handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, true);

        }

        public void Activate()
        {
            IsActive = true;
            CreateGhost();

            _currentDistance = 1.5f;

            var cam = GetViewport().GetCamera3D();
            if (cam != null)
            {
                Vector3 direction = cam.GlobalPosition - GlobalPosition;
                _currentRotationY = Mathf.Atan2(direction.X, direction.Z);
            }

            GD.Print($"{ToolName} aktivován.");
            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, true);

        }

        public void Deactivate()
        {
            IsActive = false;
            DestroyGhost();
            GD.Print($"{ToolName} deaktivován.");
            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, false);

        }

        public void SetHighlight(bool enabled)
        {

        }

        public void UpdateTool(double delta, Vector3 currentPos)
        {
            if (!IsActive || _ghostInstance == null || _handManager == null) return;

            HandleInput((float)delta);

            UpdateGhostPosition();
        }

        private void HandleInput(float delta)
        {
            Vector2 input = _handManager.GetDominantHandJoystick();

            if (input.LengthSquared() < 0.01f) return;

            // OSA X (Doleva/Doprava) -> ROTACE
            _currentRotationY += input.X * RotationSpeed * delta;

            // OSA Y (Nahoru/Dolů) -> VZDÁLENOST
            _currentDistance += input.Y * MoveSpeed * delta;

            // Omezíme vzdálenost, aby nám skříňka neproletěla hlavou nebo nezmizela v dálce
            _currentDistance = Mathf.Clamp(_currentDistance, MinDistance, MaxDistance);
        }

        public void ButtonPressed(string actionName)
        {
            if (IsActive && _ghostInstance != null && _ghostInstance.Visible && _handManager.HandMenu.Visible == false)
            {
                if (actionName == "trigger_click")
                {
                    PlaceCabinet();
                }
            }
        }

        public void ButtonReleased(string actionName) { }

        private void UpdateGhostPosition()
        {
            var ray = _handManager.GetActiveRayCast();
            if (ray == null || !ray.IsColliding())
            {
                _ghostInstance.Visible = false;
                _isSnapped = false;
                return;
            }

            Vector3 hitPoint = ray.GetCollisionPoint();
            _ghostInstance.Visible = true;

            if (_isSnapped)
            {
                if (CheckBreakSnap(hitPoint) == false)
                {
                    return;
                }
            }


            Vector3 rayOrigin = ray.GlobalPosition;
            Vector3 rayDirection = -ray.GlobalTransform.Basis.Z; // V Godotu je Forward -Z

            Vector3 targetPoint = rayOrigin + (rayDirection * _currentDistance);

            if (ray.IsColliding())
            {
                float hitDistance = rayOrigin.DistanceTo(hitPoint);

                if (hitDistance < _currentDistance)
                {
                    targetPoint = hitPoint;
                }
            }

            _ghostInstance.GlobalPosition = targetPoint;

            _ghostInstance.GlobalTransform = new Transform3D(
                new Basis(Vector3.Up, _currentRotationY),
                _ghostInstance.GlobalPosition
            );


            if (_ghostInstance is Node3D node3d) node3d.ForceUpdateTransform();

            SnapPoint bestGhostPoint = null;
            SnapPoint bestTargetPoint = null;
            float closestDistSq = CheckSnappingPoints(ref bestGhostPoint, ref bestTargetPoint);

            float connectThresholdSq = SnapConnectDistance * SnapConnectDistance;

            if (bestGhostPoint != null && closestDistSq < connectThresholdSq)
            {
                ApplySnap(bestGhostPoint, bestTargetPoint);
            }
        }

        private float CheckSnappingPoints(ref SnapPoint bestGhostPoint, ref SnapPoint bestTargetPoint)
        {
            float closestDistSq = float.MaxValue;

            foreach (var ghostPoint in _ghostController.ActiveSnapPoints)
            {
                var overlaps = ghostPoint.GetOverlappingAreas();
                foreach (var area in overlaps)
                {
                    if (area is not SnapPoint targetPoint) continue;
                    if (targetPoint.IsGhost) continue;
                    if (targetPoint.ParentCabinet == ghostPoint.ParentCabinet) continue;
                    if (IsSnapCompatible(ghostPoint.Type, targetPoint.Type) == false) continue;

                    float d = ghostPoint.GlobalPosition.DistanceSquaredTo(targetPoint.GlobalPosition);
                    if (d < closestDistSq)
                    {
                        closestDistSq = d;
                        bestGhostPoint = ghostPoint;
                        bestTargetPoint = targetPoint;
                    }


                }
            }

            return closestDistSq;
        }

        private bool CheckBreakSnap(Vector3 hitPoint)
        {
            float distFromSnap = hitPoint.DistanceTo(_snappedPosition);

            if (distFromSnap > SnapBreakDistance)
            {
                _isSnapped = false;

                _handManager.VibrateDominantHand(0.1f, 0.1f);
                return true;
            }
            else
            {
                _ghostInstance.GlobalPosition = _snappedPosition;
                _ghostInstance.GlobalRotation = _snappedRotation.GetEuler();
                return false;
            }
        }

        private void ApplySnap(SnapPoint ghostPoint, SnapPoint targetPoint)
        {
            Quaternion targetRotation = Quaternion.Identity;
            if (targetPoint.ParentCabinet != null)
            {
                targetRotation = targetPoint.ParentCabinet.GlobalTransform.Basis.GetRotationQuaternion();
            }

            Vector3 localOffset = _ghostInstance.ToLocal(ghostPoint.GlobalPosition);

            Vector3 rotatedOffset = targetRotation * localOffset;

            Vector3 newPos = targetPoint.GlobalPosition - rotatedOffset;

            _ghostInstance.GlobalPosition = newPos;
            _ghostInstance.GlobalRotation = targetRotation.GetEuler();

            _snappedPosition = newPos;
            _snappedRotation = targetRotation;
            _isSnapped = true;

            _handManager.VibrateDominantHand(0.5f, 0.05f);
        }

        private bool IsSnapCompatible(SnapType ghostType, SnapType targetType)
        {
            if (ghostType == SnapType.Left && targetType == SnapType.Right) return true;
            if (ghostType == SnapType.Right && targetType == SnapType.Left) return true;
            if (ghostType == SnapType.Bottom && targetType == SnapType.Top) return true;
            if (ghostType == SnapType.Top && targetType == SnapType.Bottom) return true;

            return false;
        }

        private void RotateGhostToPlayer()
        {

            var camera = GetViewport().GetCamera3D();
            if (camera != null)
            {
                Vector3 targetPos = camera.GlobalPosition;
                targetPos.Y = _ghostInstance.GlobalPosition.Y;

                _ghostInstance.LookAt(targetPos, Vector3.Up);

                _ghostInstance.RotateObjectLocal(Vector3.Up, Mathf.Pi);
            }
        }

        private void PlaceCabinet()
        {
            if (CabinetScene == null) return;

            Node3D newCabinet = CabinetScene.Instantiate<Node3D>();
            (newCabinet as CabinetController).Data = _ghostController.Data.Duplicate();

            GetTree().Root.AddChild(newCabinet);

            newCabinet.GlobalTransform = _ghostInstance.GlobalTransform;

            _handManager.VibrateDominantHand(0.5f, 0.1f);

            GD.Print("Skříňka umístěna.");
        }
        private void CreateGhost()
        {
            if (CabinetScene == null)
            {
                GD.PrintErr("CabinetPlacementTool: Není přiřazena CabinetScene!");
                return;
            }


            Node instance = CabinetScene.Instantiate();
            _ghostController = instance as CabinetController;
            _ghostInstance = instance as Node3D;

            AddChild(_ghostInstance);

            MakeNodeTransparent(_ghostInstance);

            DisableCollisions(_ghostInstance);

            _ghostController.SetGhostMode(true);
        }

        private void DestroyGhost()
        {
            if (_ghostInstance != null)
            {
                _ghostInstance.QueueFree();
                _ghostInstance = null;
            }
        }

        private void MakeNodeTransparent(Node node)
        {
            if (node is MeshInstance3D mesh)
            {
                StandardMaterial3D ghostMat = new StandardMaterial3D();
                ghostMat.AlbedoColor = new Color(0, 1, 0, 0.4f); // Zelená, 40% viditelná
                ghostMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;

                ghostMat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;

                mesh.MaterialOverride = ghostMat;
            }

            foreach (Node child in node.GetChildren())
            {
                MakeNodeTransparent(child);
            }
        }

        private void DisableCollisions(Node node)
        {
            if (node is SnapPoint snapPoint)
            {
                return;
            }

            if (node is CollisionObject3D colObj)
            {
                colObj.CollisionLayer = 0;
                colObj.CollisionMask = 0;

                colObj.ProcessMode = ProcessModeEnum.Disabled;
            }

            foreach (Node child in node.GetChildren())
            {
                DisableCollisions(child);
            }
        }

        public Control GetConfigurationControl()
        {
            if (SettingsUiPrefab == null) return null;

            var uiInstance = SettingsUiPrefab.Instantiate<CabinetSettingsUi>();

            if (_ghostController != null)
            {
                uiInstance.BindData(ref _ghostController.Data);
            }
            else
            {
                CreateGhost();
                uiInstance.BindData(ref _ghostController.Data);
            }

            return uiInstance;
        }
    }
}