using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Features.Kitchen.Data;

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

        private CabinetData _data;

        public string TabName => "Nastavení skříňky";

        public void BindData(ref CabinetData data)
        {
            _data = data;

            if (WidthSlider != null)
            {
                WidthSlider.Value = _data.Width;
                WidthSlider.ValueChanged -= OnWidthChanged;
                WidthSlider.ValueChanged += OnWidthChanged;
                if(WidthValueLabel != null)
                {
                    WidthValueLabel.Text = $"{_data.Width * 100:N0} cm";
                }
            }

            if (HeightSlider != null)
            {
                HeightSlider.Value = _data.Height;
                HeightSlider.ValueChanged -= OnHeightChanged;
                HeightSlider.ValueChanged += OnHeightChanged;
                if(HeightValueLabel != null)
                {
                    HeightValueLabel.Text = $"{_data.Height * 100:N0} cm";
                }
            }
            if (DepthSlider != null)
            {
                DepthSlider.Value = _data.Depth;
                DepthSlider.ValueChanged -= OnDepthChanged;
                DepthSlider.ValueChanged += OnDepthChanged;
                if(DepthValueLabel != null)
                {
                    DepthValueLabel.Text = $"{_data.Depth * 100:N0} cm";
                }
            }

            if (ShelfCount != null)
            {
                ShelfCount.Value = _data.ShelfCount;               
                ShelfCount.ValueChanged -= OnShelfChanged;
                ShelfCount.ValueChanged += OnShelfChanged;
            }

            if (WorktopCheckBox != null)
            {
                WorktopCheckBox.Toggled -= OnWorktopToggled;

                WorktopCheckBox.ButtonPressed = _data.HasWorktop;

                WorktopCheckBox.Toggled += OnWorktopToggled;
            }

            if(DoorStyleOption != null)
            {
                DoorStyleOption.Selected = (int)_data.DoorStyle;
                DoorStyleOption.ItemSelected -= OnDoorStyleChanged;
                DoorStyleOption.ItemSelected += OnDoorStyleChanged;
            }

            if(DoorTypeOption != null)
            {
                DoorTypeOption.Selected = (int)_data.DoorType;
                DoorTypeOption.ItemSelected -= OnDoorTypeChanged;
                DoorTypeOption.ItemSelected += OnDoorTypeChanged;
            }
        }

        private void OnWorktopToggled(bool pressed)
        {
            if (_data == null) return;
            _data.HasWorktop = pressed;
        }

        private void OnWidthChanged(double value)
        {
            if (_data != null) _data.Width = (float)value;

            if (WidthValueLabel is null) return;
            WidthValueLabel.Text = $"{value * 100:N0} cm";
        }

        private void OnHeightChanged(double value)
        {
            if (_data != null) _data.Height = (float)value;

            if (HeightValueLabel is null) return;
            HeightValueLabel.Text = $"{value * 100:N0} cm";
        }
        void OnDepthChanged(double value)
        {
            if (_data != null) _data.Depth = (float)value;

            if (DepthValueLabel is null) return;
            DepthValueLabel.Text = $"{value * 100:N0} cm";
        }

        private void OnShelfChanged(double value)
        {
            if (_data != null) _data.ShelfCount = (int)value;
        }

        void OnDoorStyleChanged(long index)
        {
            if (_data != null) _data.DoorStyle = (DoorStyle)index;
        }
        void OnDoorTypeChanged(long index)
        {
            if (_data != null) _data.DoorType = (DoorType)index;
        }
        public void OnPageOpened()
        {
        }
    }
}