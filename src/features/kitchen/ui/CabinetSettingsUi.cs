using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Features.Kitchen.Data;

namespace KitchenDesigner.Features.Kitchen.UI
{
    public partial class CabinetSettingsUi : Control, IMenuPage
    {
        [Export] public HSlider WidthSlider;
        [Export] public VSlider HeightSlider;
        [Export] public VSlider DepthSlider;

        [Export] public SpinBox ShelfCount;

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
            }

            if (HeightSlider != null)
            {
                HeightSlider.Value = _data.Height;
                HeightSlider.ValueChanged -= OnHeightChanged;
                HeightSlider.ValueChanged += OnHeightChanged;
            }
            if (DepthSlider != null)
            {
                DepthSlider.Value = _data.Depth;
                DepthSlider.ValueChanged -= OnDepthChanged;
                DepthSlider.ValueChanged += OnDepthChanged;
            }

            if (ShelfCount != null)
            {
                ShelfCount.Value = _data.ShelfCount;               
                ShelfCount.ValueChanged -= OnShelfChanged;
                ShelfCount.ValueChanged += OnShelfChanged;
            }
        }

        private void OnWidthChanged(double value)
        {
            if (_data != null) _data.Width = (float)value;
        }

        private void OnHeightChanged(double value)
        {
            if (_data != null) _data.Height = (float)value;
        }
        void OnDepthChanged(double value)
        {
            if (_data != null) _data.Depth = (float)value;
        }

        private void OnShelfChanged(double value)
        {
            if (_data != null) _data.ShelfCount = (int)value;
        }

        public void OnPageOpened()
        {
        }
    }
}