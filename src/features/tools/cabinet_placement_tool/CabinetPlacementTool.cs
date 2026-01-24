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

            UpdateGhostPosition();
        }

        public void ButtonPressed(string actionName)
        {
            // Potvrzení umístění (Trigger)
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
            if (ray == null) return;

            if (ray.IsColliding())
            {
                Vector3 hitPoint = ray.GetCollisionPoint();
                Vector3 hitNormal = ray.GetCollisionNormal();

                _ghostInstance.Visible = true;

                _ghostInstance.GlobalPosition = hitPoint;

                var camera = GetViewport().GetCamera3D();
                if (camera != null)
                {
                    Vector3 targetPos = camera.GlobalPosition;
                    targetPos.Y = _ghostInstance.GlobalPosition.Y;

                    _ghostInstance.LookAt(targetPos, Vector3.Up);

                    _ghostInstance.RotateObjectLocal(Vector3.Up, Mathf.Pi);
                }
            }
            else
            {
                _ghostInstance.Visible = false;
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