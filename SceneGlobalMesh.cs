using Godot;
using System;

public partial class SceneGlobalMesh : Node3D
{
    [Export] public Material WireframeMaterial;

    private StaticBody3D _staticBody;

    public override void _Ready()
    {
        _staticBody = GetNode<StaticBody3D>("StaticBody3D");
        _staticBody.CollisionLayer = 1 << 9;
        _staticBody.CollisionMask = 0; 
        GD.Print("SceneGlobalMesh připraven.");
    }

    // Tuto metodu volá OpenXRFbSceneManager automaticky
    public void SetupScene(GodotObject entity)
    {
        var collisionShape = entity.Call("create_collision_shape").As<CollisionShape3D>();
        GD.Print("Vytvářím kolizní tvar pro SceneGlobalMesh.");
        if (collisionShape != null)
        {
            foreach (var child in _staticBody.GetChildren()) child.QueueFree();
            _staticBody.AddChild(collisionShape);

        }

        // 2. GENEROVÁNÍ VIZUÁLNÍ SÍTĚ
        var meshArray = (Godot.Collections.Array)entity.Call("get_triangle_mesh");
        GD.Print("Vytvářím vizuální síť pro SceneGlobalMesh.");
        if (meshArray != null && meshArray.Count > 0)
        {
            var meshInstance = new MeshInstance3D();
            var arrayMesh = new ArrayMesh();

            var indices = meshArray[(int)Mesh.ArrayType.Index].AsInt32Array();
            var originalVertices = meshArray[(int)Mesh.ArrayType.Vertex].AsVector3Array();
            var newVertices = new Vector3[indices.Length];

            for (int i = 0; i < indices.Length; i++)
            {
                newVertices[i] = originalVertices[indices[i]];
            }

            meshArray[(int)Mesh.ArrayType.Vertex] = newVertices;
            meshArray[(int)Mesh.ArrayType.Index] = default;

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, meshArray);
            meshInstance.Mesh = arrayMesh;

            if (WireframeMaterial != null)
                meshInstance.SetSurfaceOverrideMaterial(0, WireframeMaterial);

            AddChild(meshInstance);
            GD.Print("Vizualní síť pro SceneGlobalMesh vytvořena.");
        }
    }
}