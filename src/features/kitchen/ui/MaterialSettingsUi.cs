using Godot;
using KitchenDesigner.src.features.kitchen.enums;
using System;

namespace KitchenDesigner.Features.Kitchen.UI
{
    public partial class MaterialSettingsUi : Control
    {

        [Signal] public delegate void MaterialSelectedEventHandler(Material material);
        [Signal] public delegate void MaterialZoneSelectedEventHandler(int materialZoneIndex);


        [Export] private Container _materialContainer;
        [Export] private Material[] Materials;
        [Export] private Button FrontZoneButton;
        [Export] private Button BodyZoneButton;
        [Export] private Button WorktopZoneButton;


        public override void _Ready()
        {
            FrontZoneButton?.Pressed += () => OnMaterialZoneSelected(MaterialZone.Front);
            BodyZoneButton?.Pressed += () => OnMaterialZoneSelected(MaterialZone.Body);
            WorktopZoneButton?.Pressed += () => OnMaterialZoneSelected(MaterialZone.Worktop);

            GenerateButtons();

        }

        private void GenerateButtons()
        {
            if (_materialContainer is null) return;

            foreach (Node child in _materialContainer.GetChildren()) child.QueueFree();

            foreach (var material in Materials)
            {
                Button btn = new Button();
                btn.Text = material.ResourceName;

                btn.CustomMinimumSize = new Vector2(100, 100);
                btn.Pressed += () => OnMaterialSelected(material);
                _materialContainer.AddChild(btn);
            }
        }

        private void OnMaterialSelected(Material material)
        {
            EmitSignal(SignalName.MaterialSelected, material);
        }

        void OnMaterialZoneSelected(MaterialZone zone)
        {
            EmitSignal(SignalName.MaterialZoneSelected, (int)zone);
        }
    }
}
