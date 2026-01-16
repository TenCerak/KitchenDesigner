using Godot;
using System;

public partial class SpatialAnchor : Area3D
{
    private MeshInstance3D _meshInstance;
    private Color _originalColor;
    private bool _selected = false;
    [Export] StandardMaterial3D AnchorMaterial;

    public override void _Ready()
    {
        _meshInstance = GetNode<MeshInstance3D>("MeshInstance3D");
    }

    public void SetupScene(GodotObject spatialEntity)
    {
        var data = (Godot.Collections.Dictionary)spatialEntity.Get("custom_data");

        string colorHex = data.ContainsKey("color") ? data["color"].AsString() : "#FFFFFF";
        _originalColor = new Color(colorHex);

        UpdateMaterialColor(_originalColor);

        spatialEntity.Call("save_to_storage", 1);
    }

    public void SetSelected(bool pSelected)
    {
        _selected = pSelected;

        if (_selected)
        {
            UpdateMaterialColor(new Color(0.5f, 0.5f, 0.5f));
        }
        else
        {
            UpdateMaterialColor(_originalColor);
        }
    }

    private void UpdateMaterialColor(Color newColor)
    {
        if (_meshInstance == null) return;

        var material = AnchorMaterial;

        if (material != null)
        {
            material = (StandardMaterial3D)material.Duplicate();
            material.AlbedoColor = newColor;
            _meshInstance.SetSurfaceOverrideMaterial(0, material);
        }
    }
}