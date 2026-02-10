using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Common.Utils;
using KitchenDesigner.Features.Kitchen.Components;
using KitchenDesigner.Features.Kitchen.Interfaces;
using KitchenDesigner.Features.Kitchen.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Tools
{
    public partial class HandlePlacementTool : Node3D, IARTool
    {
        XrHandManager _handManager;
        private IHandleContainer _selectedHandleContainer = null;
        [Export] public PackedScene SettingsUiPrefab;
        public HandleSettingsUi SettingsUiInstance { get; private set; }
        private PackedScene _handlePrefab;
        public string ToolName => "Umístění úchytů";

        public bool IsActive { get; set; }

        public void Activate()
        {
            IsActive = true;
            if (_handManager is not null)
            {
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, true);
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.INTERACTIBLES, true);
            }
        }

        public void ButtonPressed(string actionName)
        {
            if (actionName == "trigger_click" && _selectedHandleContainer != null && _handlePrefab != null && _handManager.HandMenu.Visible == false)
            {
                _selectedHandleContainer.SetHandle(_handlePrefab);
            }

        }

        public void ButtonReleased(string actionName)
        {
        }

        public void Deactivate()
        {
            if (_handManager is not null)
            {
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, false);
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.INTERACTIBLES, false);
            }
            QueueFree();
        }

        public void Initialize(XrHandManager handManager)
        {
            _handManager = handManager;
        }

        public void Reattach(XrHandManager handManager)
        {
            _handManager = handManager;
        }

        public void SetHighlight(bool enabled)
        {
        }

        public void UpdateTool(double delta, Vector3 currentPos)
        {
            if (!IsActive || _handManager is null) return;

            CheckForTarget();

        }

        private void CheckForTarget()
        {
            var ray = _handManager.GetActiveRayCast();
            if (ray == null) return;

            if (ray.IsColliding())
            {
                var collider = ray.GetCollider() as Node;

                _selectedHandleContainer = FindHandleTarget(collider);
            }
            else
            {
                _selectedHandleContainer = null;
            }
        }

        public Control GetConfigurationControl()
        {
            if (SettingsUiPrefab == null) return null;

            SettingsUiInstance = SettingsUiPrefab.Instantiate<HandleSettingsUi>();
            SettingsUiInstance.HandleSelected += (handleDef) =>
            {
                _handlePrefab = handleDef.Prefab;
            };

            return SettingsUiInstance;
        }

        private IHandleContainer FindHandleTarget(Node node)
        {
            while (node != null)
            {
                if (node is IHandleContainer target) return target;
                node = node.GetParent();
                if (node is CabinetBase) break;
            }
            return null;
        }
    }
}
