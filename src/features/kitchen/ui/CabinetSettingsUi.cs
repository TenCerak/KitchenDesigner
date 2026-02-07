using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Features.Kitchen.Data;
using System;

namespace KitchenDesigner.Features.Kitchen.UI
{
    public partial class CabinetSettingsUi : Control, IMenuPage
    {
        [Export] public HSlider WidthSlider;
        [Export] public Label WidthValueLabel;

        [Export] public VSlider HeightSlider;
        [Export] public Label HeightValueLabel;

        [Export] public VSlider DepthSlider;
        [Export] public Label DepthValueLabel;

        [Export] public SpinBox ShelfCount;
        [Export] public CheckBox WorktopCheckBox;

        [Export] public OptionButton DoorTypeOption;
        [Export] public OptionButton DoorStyleOption;

        [Export] public OptionButton CabinetShapeOption;
        [Export] public CheckBox CabinetIsLeftCorner;

        [Export] public HSlider WidthBlindSlider;
        [Export] public Label WidthBlindValueLabel;

        const double MaxCabinetWidth = 0.8f;
        const double MinCabinetWidth = 0.2f;

        private CabinetData _data;

        public string TabName => "Nastavení skříňky";

        public void BindData(ref CabinetData data)
        {
            _data = data;

            if (WidthSlider is not null)
            {
                WidthSlider.Value = _data.Width;
                WidthSlider.ValueChanged -= OnWidthChanged;
                WidthSlider.ValueChanged += OnWidthChanged;
                if (WidthValueLabel is not null)
                {
                    WidthValueLabel.Text = $"{_data.Width * 100:N0} cm";
                }
            }

            if (HeightSlider is not null)
            {
                HeightSlider.Value = _data.Height;
                HeightSlider.ValueChanged -= OnHeightChanged;
                HeightSlider.ValueChanged += OnHeightChanged;
                if (HeightValueLabel is not null)
                {
                    HeightValueLabel.Text = $"{_data.Height * 100:N0} cm";
                }
            }
            if (DepthSlider is not null)
            {
                DepthSlider.Value = _data.Depth;
                DepthSlider.ValueChanged -= OnDepthChanged;
                DepthSlider.ValueChanged += OnDepthChanged;
                if (DepthValueLabel is not null)
                {
                    DepthValueLabel.Text = $"{_data.Depth * 100:N0} cm";
                }
            }

            if (ShelfCount is not null)
            {
                ShelfCount.Value = _data.ShelfCount;
                ShelfCount.ValueChanged -= OnShelfChanged;
                ShelfCount.ValueChanged += OnShelfChanged;
            }

            if (WorktopCheckBox is not null)
            {
                WorktopCheckBox.Toggled -= OnWorktopToggled;

                WorktopCheckBox.ButtonPressed = _data.HasWorktop;

                WorktopCheckBox.Toggled += OnWorktopToggled;
            }

            if (DoorStyleOption is not null)
            {
                DoorStyleOption.Selected = (int)_data.DoorStyle;
                DoorStyleOption.ItemSelected -= OnDoorStyleChanged;
                DoorStyleOption.ItemSelected += OnDoorStyleChanged;
            }

            if (DoorTypeOption is not null)
            {
                DoorTypeOption.Selected = (int)_data.DoorType;
                DoorTypeOption.ItemSelected -= OnDoorTypeChanged;
                DoorTypeOption.ItemSelected += OnDoorTypeChanged;
            }

            if (CabinetShapeOption is not null)
            {
                CabinetShapeOption.Selected = (int)_data.Shape;
                CabinetShapeOption.ItemSelected -= OnCabinetShapeChanged;
                CabinetShapeOption.ItemSelected += OnCabinetShapeChanged;
            }
            if (CabinetIsLeftCorner is not null)
            {
                CabinetIsLeftCorner.ButtonPressed = _data.CornerIsLeft;
                CabinetIsLeftCorner.Toggled -= OnCabinetIsLeftCornerToggled;
                CabinetIsLeftCorner.Toggled += OnCabinetIsLeftCornerToggled;
            }
            if (WidthBlindSlider is not null)
            {
                WidthBlindSlider.Value = _data.CornerBlindWidth;
                WidthBlindSlider.ValueChanged -= OnWidthBlindChanged;
                WidthBlindSlider.ValueChanged += OnWidthBlindChanged;
                if (WidthBlindValueLabel is not null)
                {
                    WidthBlindValueLabel.Text = $"{_data.CornerBlindWidth * 100:N0} cm";
                }
            }
        }

        void OnWidthBlindChanged(double value)
        {
            if (_data is not null) _data.CornerBlindWidth = (float)value;
            if (WidthBlindValueLabel is null) return;
            WidthBlindValueLabel.Text = $"{value * 100:N0} cm";
        }
        private void OnCabinetIsLeftCornerToggled(bool toggledOn)
        {
            if (_data is not null) _data.CornerIsLeft = toggledOn;
        }

        private void OnCabinetShapeChanged(long index)
        {
            if (_data is not null) _data.Shape = (CabinetShape)index;
            if ((index) == (int)CabinetShape.CornerBlind)
            {
                if (WidthBlindSlider is not null) WidthBlindSlider.Visible = true;
                if (WidthSlider is not null)
                {
                    WidthSlider.MaxValue = (MaxCabinetWidth * 2);
                }
            }
            else
            {
                if (WidthBlindSlider is not null) WidthBlindSlider.Visible = false;
                if (WidthSlider is not null)
                {
                    WidthSlider.MaxValue = MaxCabinetWidth;
                }
            }
        }

        private void OnWorktopToggled(bool pressed)
        {
            if (_data == null) return;
            _data.HasWorktop = pressed;
        }

        private void OnWidthChanged(double value)
        {
            if (_data is not null) _data.Width = (float)value;

            if (WidthValueLabel is null) return;
            WidthValueLabel.Text = $"{value * 100:N0} cm";
        }

        private void OnHeightChanged(double value)
        {
            if (_data is not null) _data.Height = (float)value;

            if (HeightValueLabel is null) return;
            HeightValueLabel.Text = $"{value * 100:N0} cm";
        }
        void OnDepthChanged(double value)
        {
            if (_data is not null) _data.Depth = (float)value;

            if (DepthValueLabel is null) return;
            DepthValueLabel.Text = $"{value * 100:N0} cm";
        }

        private void OnShelfChanged(double value)
        {
            if (_data is not null) _data.ShelfCount = (int)value;
        }

        void OnDoorStyleChanged(long index)
        {
            if (_data is not null) _data.DoorStyle = (DoorStyle)index;
        }
        void OnDoorTypeChanged(long index)
        {
            if (_data is not null) _data.DoorType = (DoorType)index;
        }
        public void OnPageOpened()
        {
        }
    }
}