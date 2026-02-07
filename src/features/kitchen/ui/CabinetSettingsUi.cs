using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Features.Kitchen.Data;
using System;

namespace KitchenDesigner.Features.Kitchen.UI
{
    public partial class CabinetSettingsUi : Control, IMenuPage
    {
        [Export] public Container WidthContainer;
        [Export] public HSlider WidthSlider;
        [Export] public Label WidthValueLabel;

        [Export] public Container HeightContainer;
        [Export] public VSlider HeightSlider;
        [Export] public Label HeightValueLabel;

        [Export] public Container DepthContainer;
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

        [Export] public Container CornerLeftDepthContainer;
        [Export] public VSlider CornerLeftDepthSlider;
        [Export] public Label CornerLeftDepthValueLabel;

        [Export] public Container CornerRightDepthContainer;
        [Export] public VSlider CornerRightDepthSlider;
        [Export] public Label CornerRightDepthValueLabel;

        [Export] public Container CornerLeftWidthContainer;
        [Export] public HSlider CornerLeftWidthSlider;
        [Export] public Label CornerLeftWidthValueLabel;

        [Export] public Container CornerRightWidthContainer;
        [Export] public HSlider CornerRightWidthSlider;
        [Export] public Label CornerRightWidthValueLabel;

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

            if (CornerLeftDepthSlider is not null)
            {
                CornerLeftDepthSlider.Value = _data.CornerLeftDepth;
                CornerLeftDepthSlider.ValueChanged -= OnCornerLeftDepthChanged;
                CornerLeftDepthSlider.ValueChanged += OnCornerLeftDepthChanged;
                if (CornerLeftDepthValueLabel is not null)
                {
                    CornerLeftDepthValueLabel.Text = $"{_data.CornerLeftDepth * 100:N0} cm";
                }
            }

            if (CornerRightDepthSlider is not null)
            {
                CornerRightDepthSlider.Value = _data.CornerRightDepth;
                CornerRightDepthSlider.ValueChanged -= OnCornerRightDepthChanged;
                CornerRightDepthSlider.ValueChanged += OnCornerRightDepthChanged;
                if (CornerRightDepthValueLabel is not null)
                {
                    CornerRightDepthValueLabel.Text = $"{_data.CornerRightDepth * 100:N0} cm";
                }
            }

            if (CornerLeftWidthSlider is not null)
            {
                CornerLeftWidthSlider.Value = _data.CornerLeftWidth;
                CornerLeftWidthSlider.ValueChanged -= OnCornerLeftWidthChanged;
                CornerLeftWidthSlider.ValueChanged += OnCornerLeftWidthChanged;
                if (CornerLeftWidthValueLabel is not null)
                {
                    CornerLeftWidthValueLabel.Text = $"{_data.CornerLeftWidth * 100:N0} cm";
                }
            }

            if (CornerRightWidthSlider is not null)
            {
                CornerRightWidthSlider.Value = _data.CornerRightWidth;
                CornerRightWidthSlider.ValueChanged -= OnCornerRightWidthChanged;
                CornerRightWidthSlider.ValueChanged += OnCornerRightWidthChanged;
                if (CornerRightWidthValueLabel is not null)
                {
                    CornerRightWidthValueLabel.Text = $"{_data.CornerRightWidth * 100:N0} cm";
                }
            }
        }
        private void OnCornerRightWidthChanged(double value)
        {
            if (_data is not null) _data.CornerRightWidth = (float)value;
            if (CornerRightWidthValueLabel is null) return;
            CornerRightWidthValueLabel.Text = $"{value * 100:N0} cm";

        }

        private void OnCornerLeftWidthChanged(double value)
        {
            if (_data is not null) _data.CornerLeftWidth = (float)value;
            if (CornerLeftWidthValueLabel is null) return;
            CornerLeftWidthValueLabel.Text = $"{value * 100:N0} cm";
        }

        private void OnCornerRightDepthChanged(double value)
        {
            if (_data is not null) _data.CornerRightDepth = (float)value;
            if (CornerRightDepthValueLabel is null) return;
            CornerRightDepthValueLabel.Text = $"{value * 100:N0} cm";
        }

        private void OnCornerLeftDepthChanged(double value)
        {
            if (_data is not null) _data.CornerLeftDepth = (float)value;
            if (CornerLeftDepthValueLabel is null) return;
            CornerLeftDepthValueLabel.Text = $"{value * 100:N0} cm";
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

            WidthBlindSlider?.Visible = false;
            WidthSlider?.MaxValue = MaxCabinetWidth;

            CornerLeftDepthContainer?.Visible = false;
            CornerRightDepthContainer?.Visible = false;

            CornerLeftWidthContainer?.Visible = false;
            CornerRightWidthContainer?.Visible = false;

            WidthContainer?.Visible = true;
            DepthContainer?.Visible = true;

            if ((index) == (int)CabinetShape.CornerBlind)
            {
                WidthBlindSlider?.Visible = true;
                WidthSlider?.MaxValue = MaxCabinetWidth * 2;
            }
            else
            {
                WidthSlider?.MaxValue = MaxCabinetWidth;
            }

            if ((index) == (int)CabinetShape.CornerL || (index) == (int)CabinetShape.CornerDiagonal)
            {
                CornerLeftDepthContainer?.Visible = true;
                CornerRightDepthContainer?.Visible = true;

                CornerLeftWidthContainer?.Visible = true;
                CornerRightWidthContainer?.Visible = true;

                WidthContainer?.Visible = false;
                DepthContainer?.Visible = false;
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