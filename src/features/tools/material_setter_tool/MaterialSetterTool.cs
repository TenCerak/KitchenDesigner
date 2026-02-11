using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Common.Utils;
using KitchenDesigner.Features.Kitchen.Interfaces;
using KitchenDesigner.Features.Kitchen.UI;
using KitchenDesigner.src.features.kitchen.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Tools
{
    public partial class MaterialSetterTool : Node3D, IARTool
    {
        public string ToolName => "Nastavení materiálu";
        private MaterialZone _currentZoneMode;
        private Material _selectedMaterial = new();
        [Export] public PackedScene SettingsUiPrefab;
        private MaterialSettingsUi SettingsUiInstance;
        private IMaterialTarget _currentTarget = null;
        public bool IsActive { get; set; }
        XrHandManager _handManager;

        public void Activate()
        {
            IsActive = true;
            if (_handManager is not null)
            {
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.KITCHEN_COMPONENTS, true);
            }

        }

        public void ButtonPressed(string actionName)
        {
            if (actionName == "trigger_click" && _currentTarget != null && _selectedMaterial != null && _handManager.HandMenu.Visible == false)
            {
                _currentTarget.SetMaterial(_selectedMaterial, _currentZoneMode);
            }
        }

        public void ButtonReleased(string actionName)
        {
        }

        public void Deactivate()
        {
            IsActive = false;
            if (_handManager is not null)
            {
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.KITCHEN_COMPONENTS, false);
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

                _currentTarget = FindMaterialTargetRecursive(collider);
            }
            else
            {
                _currentTarget = null;
            }
        }

        private IMaterialTarget FindMaterialTargetRecursive(Node collider)
        {
            if (collider is IMaterialTarget target) return target;
            var parent = collider.GetParent();
            if (parent != null) return FindMaterialTargetRecursive(parent);
            return null;
        }

        public Control GetConfigurationControl()
        {
            if (SettingsUiPrefab == null) return null;

            SettingsUiInstance = SettingsUiPrefab.Instantiate<MaterialSettingsUi>();
            SettingsUiInstance.MaterialSelected += (material) => _selectedMaterial = material;
            SettingsUiInstance.MaterialZoneSelected += (zoneIndex) => _currentZoneMode = (MaterialZone)zoneIndex;
            return SettingsUiInstance;
        }
    }
}
