using Godot;
using System;
using System.Linq;

public partial class SceneAnchor : StaticBody3D
{
    private Label3D _label;

    [Export]
    public StandardMaterial3D AnchorMaterial { get; set; }

    public override void _Ready()
    {
        _label = GetNode<Label3D>("Label3D");
    }

    public void SetupScene(GodotObject entity)
    {
        string[] semanticLabels = entity.Call("get_semantic_labels").AsStringArray();

        if (semanticLabels.Length > 0)
        {
            _label.Text = semanticLabels[0].Capitalize();
        }

        var collisionShape = entity.Call("create_collision_shape").As<CollisionShape3D>();
        if (collisionShape != null)
        {
            AddChild(collisionShape);
        }

        var meshInstance = entity.Call("create_mesh_instance").As<MeshInstance3D>();

        if (meshInstance == null)
        {
            meshInstance = new MeshInstance3D();
            meshInstance.Mesh = new BoxMesh { Size = new Vector3(0.1f, 0.1f, 0.1f) };
        }

        if (AnchorMaterial != null)
        {
            StandardMaterial3D material = (StandardMaterial3D)AnchorMaterial.Duplicate();

            if (semanticLabels.Length > 0)
            {
                material.AlbedoColor = GetColorForLabel(semanticLabels[0]);
            }

            if (meshInstance.Mesh is BoxMesh)
            {
                material.Uv1Scale = new Vector3(3, 2, 1);
            }

            meshInstance.SetSurfaceOverrideMaterial(0, material);
        }

        AddChild(meshInstance);
    }

    private Color GetColorForLabel(string semanticLabel)
    {
        return semanticLabel.ToLower() switch
        {
            "ceiling" or "floor" => new Color(0, 0, 0, 0.6f),              // Černá
            "wall_face" or "invisible_wall_face" => new Color(0, 0, 1, 0.6f), // Modrá
            "window_frame" or "door_frame" => new Color(1, 0, 0, 0.6f),      // Červená
            "couch" or "table" or "bed" or "lamp" or "plant" or "screen" or "storage"
                => new Color(0, 1, 0, 0.6f),                                  // Zelená
            _ => new Color(1, 1, 1, 0.6f)                                     // Bílá (výchozí)
        };
    }
}