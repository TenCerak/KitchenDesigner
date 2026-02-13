using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Common.Utils;
using KitchenDesigner.Features.Kitchen.Components;
using KitchenDesigner.Features.Kitchen.Data;
using KitchenDesigner.Features.Kitchen.Interfaces;
using KitchenDesigner.Features.Kitchen.Resources;
using KitchenDesigner.Features.Kitchen.UI;
using System;

namespace KitchenDesigner.Features.Kitchen.Tools
{
    public partial class CabinetPlacementTool : Node3D, IARTool
    {
        [Export] public string ToolName { get; private set; } = "Umístit skříňku";
        public bool IsActive { get; set; } = false;

        [Export] CabinetDefinition defaultCabinetDefinition;
        private KitchenComponentDefinition _selectedItem;

        [ExportGroup("UI")]
        [Export] public PackedScene SettingsUiPrefab;
        private CabinetSettingsUi SettingsUiInstance;

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
        private IKitchenComponent _ghostInstance;

        public void Initialize(XrHandManager handManager)
        {
            _handManager = handManager;
            handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, true);
        }
        void HandleItemSelected(KitchenComponentDefinition definition)
        {
            DestroyGhost();
            _selectedItem = definition;
            CreateGhost();
        }

        public void Reattach(XrHandManager handManager)
        {
            _handManager = handManager;
            handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, true);

        }

        public void Activate()
        {
            IsActive = true;
            DestroyGhost();
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
            DestroyGhost();

            IsActive = false;
            SettingsUiInstance = null;
            GD.Print($"{ToolName} deaktivován.");
            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.ENVIRONMENT, false);
            DesignerEvents.Instance.CabinetDefinitionSelected -= HandleItemSelected;

            QueueFree();
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
            if (IsActive && _ghostInstance != null && _ghostInstance.AsNode().Visible && _handManager.HandMenu.Visible == false)
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
                _ghostInstance.AsNode().Visible = false;
                _isSnapped = false;
                return;
            }

            Vector3 hitPoint = ray.GetCollisionPoint();
            _ghostInstance.AsNode().Visible = true;

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

            _ghostInstance.AsNode().GlobalPosition = targetPoint;

            _ghostInstance.AsNode().GlobalTransform = new Transform3D(
                new Basis(Vector3.Up, _currentRotationY),
                _ghostInstance.AsNode().GlobalPosition
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

            if (_ghostInstance is not ISnappable snappable) return float.MaxValue;

            foreach (var ghostPoint in snappable.ActiveSnapPoints)
            {
                var overlaps = ghostPoint.GetOverlappingAreas();
                foreach (var area in overlaps)
                {
                    if (area is not SnapPoint targetPoint) continue;
                    if (targetPoint.IsGhost) continue;
                    if (targetPoint.ParentObject == ghostPoint.ParentObject) continue;
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
                _ghostInstance.AsNode().GlobalPosition = _snappedPosition;
                _ghostInstance.AsNode().GlobalRotation = _snappedRotation.GetEuler();
                return false;
            }
        }

        private void ApplySnap(SnapPoint ghostPoint, SnapPoint targetPoint)
        {
            Quaternion targetPointRotation = targetPoint.GlobalTransform.Basis.GetRotationQuaternion();

            Quaternion currentGhostRotation = _ghostInstance.AsNode().GlobalTransform.Basis.GetRotationQuaternion();

            Vector3 targetEuler = targetPointRotation.GetEuler();
            Vector3 ghostEuler = currentGhostRotation.GetEuler();

            float diffAngleY = ghostEuler.Y - targetEuler.Y;


            float snapStep = Mathf.Pi / 2.0f;
            float snappedDiffY = Mathf.Round(diffAngleY / snapStep) * snapStep;

            Quaternion snappedOffsetRotation = Quaternion.FromEuler(new Vector3(0, snappedDiffY, 0));
            Quaternion finalRotation = targetPointRotation * snappedOffsetRotation;


            Vector3 localOffset = _ghostInstance.AsNode().ToLocal(ghostPoint.GlobalPosition);

            Vector3 rotatedOffset = finalRotation * localOffset;

            Vector3 newPos = targetPoint.GlobalPosition - rotatedOffset;

            _ghostInstance.AsNode().GlobalPosition = newPos;
            _ghostInstance.AsNode().GlobalRotation = finalRotation.GetEuler();

            _snappedPosition = newPos;
            _snappedRotation = finalRotation;
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
                targetPos.Y = _ghostInstance.AsNode().GlobalPosition.Y;

                _ghostInstance.AsNode().LookAt(targetPos, Vector3.Up);

                _ghostInstance.AsNode().RotateObjectLocal(Vector3.Up, Mathf.Pi);
            }
        }

        private void PlaceCabinet()
        {
            if (_selectedItem == null) return;

            XROrigin3D xrOrigin = GetXROrigin();
            if (xrOrigin == null)
            {
                GD.PrintErr("CHYBA: Nelze vytvořit Spatial Anchor - XROrigin3D nenalezen!");

                IKitchenComponent fallbackKitchenComponent = _selectedItem.Prefab.Instantiate<IKitchenComponent>();


                if (_ghostInstance is CabinetBase cabinet)
                {
                    cabinet.Data = cabinet.Data.Duplicate();
                    cabinet.Rebuild();
                }


                GetTree().Root.AddChild(fallbackKitchenComponent.AsNode());

                fallbackKitchenComponent.AsNode().GlobalTransform = _ghostInstance.AsNode().GlobalTransform;

                _handManager.VibrateDominantHand(0.5f, 0.1f);

                GD.Print("Skříňka umístěna.");

                return;
            }

            XRAnchor3D anchor = new XRAnchor3D();

            xrOrigin.AddChild(anchor);

            anchor.GlobalTransform = _ghostInstance.AsNode().GlobalTransform;

            IKitchenComponent newKitchenComponent = _selectedItem.Prefab.Instantiate<IKitchenComponent>();
            

            anchor.AddChild(newKitchenComponent.AsNode());

            newKitchenComponent.AsNode().Position = Vector3.Zero;
            newKitchenComponent.AsNode().Rotation = Vector3.Zero;

            if (_ghostInstance is CabinetBase ghostCabinet && newKitchenComponent is CabinetBase newCabinet)
            {
                newCabinet.Data = ghostCabinet.Data.Duplicate();
                newCabinet.Rebuild();
            }

            _handManager.VibrateDominantHand(0.5f, 0.1f);
            GD.Print("Skříňka ukotvena (Spatial Anchor created).");

        }
        private void CreateGhost()
        {
            if (_selectedItem == null)
            {
                _selectedItem = defaultCabinetDefinition;

                if (defaultCabinetDefinition is null)
                {
                    GD.PrintErr("Není vybrána vchozí nastavení nástroje CabinetPlacementTool!");
                    return;
                }
            }


            Node instance = _selectedItem.Prefab.Instantiate();
            CabinetBase cabinet = instance as CabinetBase;

            _ghostInstance = instance as IKitchenComponent;
            if (cabinet is not null && _selectedItem is CabinetDefinition cData)
            {
                cabinet.Data = cData.DefaultData.Duplicate();
            }

            AddChild(_ghostInstance.AsNode());

            if (GodotObject.IsInstanceValid(SettingsUiInstance))
            {
                if (cabinet is not null)
                {
                    SettingsUiInstance.BindData(ref cabinet.Data);
                }
            }
            else
            {
                SettingsUiInstance = null;
            }

            MakeNodeTransparent(_ghostInstance.AsNode());

            DisableCollisions(_ghostInstance.AsNode());

            cabinet?.SetGhostMode(true);
        }


        private void DestroyGhost()
        {
            if (_ghostInstance != null)
            {
                _ghostInstance.Delete();
                _ghostInstance = null;
            }
            else
            {
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

            SettingsUiInstance = SettingsUiPrefab.Instantiate<CabinetSettingsUi>();
            SettingsUiInstance.CabinetSelectorUi?.KitchenComponentDefinitionSelected += HandleItemSelected;

            if (GodotObject.IsInstanceValid(_ghostInstance.AsNode()))
            {
                if (_ghostInstance is CabinetBase cabinet)
                    SettingsUiInstance.BindData(ref cabinet.Data);
            }

            return SettingsUiInstance;
        }

        private XROrigin3D GetXROrigin()
        {

            Node current = this;
            while (current != null)
            {
                if (current is XROrigin3D origin)
                {
                    return origin;
                }
                current = current.GetParent();
            }

            return null;
        }
    }
}