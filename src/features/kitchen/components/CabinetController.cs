using Godot;
using KitchenDesigner.Common.Utils;
using KitchenDesigner.Features.Kitchen.Data;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class CabinetController : Node3D, IKitchenComponent
    {
        [Export] public CabinetData Data;

        private MeshInstance3D _meshInstance;
        private StaticBody3D _staticBody;
        private Material _originalMaterial;

        public Node AsNode() => this;

        public void Delete()
        {
            GD.Print($"Mazání skříňky: {Name}");
            QueueFree();
        }
        public override void _Ready()
        {
            if (Data == null)
            {
                Data = new CabinetData();
            }

            Data.DimensionsChanged += RebuildCabinet;

            _meshInstance = new MeshInstance3D();
            AddChild(_meshInstance);

            _staticBody = new StaticBody3D();

            _staticBody.CollisionLayer = 1;
            _staticBody.CollisionLayer = CollisionLayerHelper.SetLayer(_staticBody.CollisionLayer, CollisionLayerHelper.KITCHEN_COMPONENTS, true);


            AddChild(_staticBody);

            RebuildCabinet();
        }

        public void RebuildCabinet()
        {
            if (Data == null) return;

            var boxMesh = new BoxMesh();
            boxMesh.Size = new Vector3(Data.Width, Data.Height, Data.Depth);
            _meshInstance.Mesh = boxMesh;

            // 2. KOREKCE POZICE (Pivot Logic)
            // Chceme, aby (0,0,0) tohoto Node3D bylo "dole uprostřed vzadu".
            // BoxMesh má střed v (0,0,0).
            // Posuneme mesh tak, aby jeho spodek seděl na Y=0 a záda na Z=0.

            float yOffset = Data.Height / 2.0f;
            float zOffset = Data.Depth / 2.0f;

            Vector3 correctedPosition = new Vector3(0, yOffset, zOffset);

            _meshInstance.Position = correctedPosition;

            foreach (var child in _staticBody.GetChildren()) child.QueueFree();

            var colShape = new CollisionShape3D();
            var boxShape = new BoxShape3D();
            boxShape.Size = new Vector3(Data.Width, Data.Height, Data.Depth);
            colShape.Shape = boxShape;
            colShape.Position = correctedPosition;

            _staticBody.AddChild(colShape);
        }

        public void SetHighlight(bool active, bool isDeletePreview = false)
        {
            if (_meshInstance == null) return;

            if (active)
            {
                if (_originalMaterial == null)
                    _originalMaterial = _meshInstance.GetSurfaceOverrideMaterial(0);

                StandardMaterial3D highlightMat = new StandardMaterial3D();

                if (isDeletePreview)
                {
                    // Červená pro mazání
                    highlightMat.AlbedoColor = new Color(1, 0, 0, 0.5f);
                    highlightMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                }
                else
                {
                    // Bílá/Žlutá pro výběr
                    highlightMat.AlbedoColor = new Color(1, 1, 0, 0.5f);
                }

                _meshInstance.SetSurfaceOverrideMaterial(0, highlightMat);
            }
            else
            {
                _meshInstance.SetSurfaceOverrideMaterial(0, _originalMaterial);
            }
        }
    }
}