using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Common.Utils; 
using KitchenDesigner.Features.Kitchen.Components;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System;

namespace KitchenDesigner.Features.Tools
{
    public partial class DeleteTool : Node3D, IARTool
    {
        [Export] public string ToolName { get; private set; } = "Odstranit";
        public bool IsActive { get; set; } = false;

        private XrHandManager _handManager;
        private IKitchenComponent _highlightedComponent = null;

        public void Initialize(XrHandManager handManager)
        {
            _handManager = handManager;
        }

        public void Reattach(XrHandManager handManager)
        {
            _handManager = handManager;
        }

        public void Activate()
        {
            IsActive = true;

            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, false);
            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.KITCHEN_COMPONENTS, true);

            GD.Print("Delete Tool aktivován - Raycast nastaven na kuchyň.");
        }

        public void Deactivate()
        {
            IsActive = false;
            RemoveHighlight();

            if (_handManager != null)
            {
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, true);
                _handManager.SetPointerLayerEnabled(CollisionLayerHelper.KITCHEN_COMPONENTS, false);
            }
        }

        public void SetHighlight(bool enabled) { } 

        public void UpdateTool(double delta, Vector3 currentPos)
        {
            if (!IsActive || _handManager is null) return;

            CheckForTarget();
        }

        public void ButtonPressed(string actionName)
        {
            if (!IsActive) return;
            GD.Print($"DeleteTool: Tlačítko stisknuto: {actionName}");
            if (actionName == "trigger_click" && _highlightedComponent is not null && _handManager.HandMenu.Visible == false)
            {
                GD.Print("DeleteTool: Mazání objektu...");
                DeleteTarget();
            }
        }

        public void ButtonReleased(string actionName) { }


        private void CheckForTarget()
        {
            var ray = _handManager.GetActiveRayCast();
            if (ray == null) return;

            if (ray.IsColliding())
            {
                var collider = ray.GetCollider() as Node;

                var component = FindKitchenComponent(collider);

                if (component != null)
                {
                    if (_highlightedComponent != component)
                    {
                        RemoveHighlight();
                        _highlightedComponent = component;

                        _highlightedComponent.SetHighlight(true, true);

                        _handManager.VibrateDominantHand(0.2f, 0.05f);
                    }
                    return;
                }
            }

            RemoveHighlight();
        }

        private IKitchenComponent FindKitchenComponent(Node node)
        {
            if (node == null) return null;

            if (node is IKitchenComponent component)
            {
                return component;
            }

            Node parent = node.GetParent();
            if (parent != null && parent != GetTree().Root)
            {
                return FindKitchenComponent(parent);
            }

            return null;
        }

        private void DeleteTarget()
        {
            if (_highlightedComponent != null)
            {
                _handManager.VibrateDominantHand(0.8f, 0.15f);

                _highlightedComponent.Delete();
                _highlightedComponent = null;

                GD.Print("Objekt smazán.");
            }
        }
        private void RemoveHighlight()
        {
            if (_highlightedComponent != null && GodotObject.IsInstanceValid(_highlightedComponent.AsNode()))
            {
                _highlightedComponent.SetHighlight(false);
            }
            _highlightedComponent = null;
        }
    }
}