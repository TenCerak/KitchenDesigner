using Godot;
using KitchenDesigner.Features.Kitchen.Data;
using KitchenDesigner.Features.Kitchen.Interfaces;
using KitchenDesigner.src.features.kitchen.enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public abstract partial class CabinetBase : Node3D, IInteractable, IKitchenComponent, IMaterialTarget
    {
        [ExportGroup("Data")]
        [Export] public CabinetData Data;

        [ExportGroup("Common Components")]
        [Export] public Node3D SnapPointsContainer;
        [Export] public PackedScene SnapPointScene;
        [Export] public Node3D DoorsContainer;
        [Export] public PackedScene DoorPrefab;
        [Export] public PackedScene DrawerPrefab;
        [Export] public Node3D VisualsRoot;

        [ExportGroup("Visual Aids")]
        [Export] public Node3D OrientationArrow;

        [ExportGroup("Materials")]
        [Export] public Material CabinetMaterial;

        public List<SnapPoint> ActiveSnapPoints { get; protected set; } = new List<SnapPoint>();

        protected bool _isGhost = false;

        public override void _Ready()
        {
            if (Data == null) Data = new CabinetData();

            Data.DimensionsChanged += Rebuild;

            Rebuild();
        }

        public void Rebuild()
        {
            if (Data == null) return;

            UpdatePivot();

            RebuildGeometry();

            UpdateSnapPoints(); 
            UpdateDoors();      

            SetGhostMode(_isGhost);
        }

        public override void _ExitTree()
        {
            if (Data != null)
            {
                Data.DimensionsChanged -= Rebuild;
            }

            base._ExitTree();
        }

        protected virtual void UpdatePivot()
        {
            if (VisualsRoot != null)
            {
                VisualsRoot.Position = new Vector3(0, Data.Height / 2.0f, Data.Depth / 2.0f);
            }
        }

        protected abstract void RebuildGeometry();
        protected abstract void UpdateSnapPoints();
        protected abstract void UpdateDoors();

        protected void UpdatePart(MeshInstance3D mesh, CollisionShape3D col, Material material, Vector3 size, Vector3 pos)
        {
            // A. Mesh
            if (mesh != null)
            {
                mesh.Position = pos;
                if (mesh.Mesh is BoxMesh box)
                {
                    // Duplikace resource, aby se nezměnily ostatní skříňky
                    if (box.GetReferenceCount() > 1) mesh.Mesh = (Mesh)box.Duplicate();
                    ((BoxMesh)mesh.Mesh).Size = size;
                    if (material != null)
                        mesh.SetSurfaceOverrideMaterial(0, material);
                }
            }

            // B. Collider
            if (col != null)
            {
                col.Position = pos;
                if (col.Shape is BoxShape3D shape)
                {
                    if (shape.GetReferenceCount() > 1) col.Shape = (Shape3D)shape.Duplicate();
                    ((BoxShape3D)col.Shape).Size = size;
                }
            }
        }

        public virtual void SetGhostMode(bool isGhost)
        {
            _isGhost = isGhost;
            foreach (var sp in ActiveSnapPoints)
            {
                sp.IsGhost = isGhost;

                if (OrientationArrow != null)
                {
                    OrientationArrow.Visible = isGhost;
                }

                if (isGhost)
                {
                    sp.Monitoring = true;
                    sp.Monitorable = false;
                }
                else
                {
                    sp.Monitoring = false;
                    sp.Monitorable = true;
                }
            }
        }

        protected SnapPoint CreateSnapPoint(SnapType type, Vector3 localPos, Vector3 rotation)
        {
            if (SnapPointScene == null || SnapPointsContainer == null) return null;

            var instance = SnapPointScene.Instantiate<SnapPoint>();
            SnapPointsContainer.AddChild(instance);
            instance.Position = localPos;
            instance.RotationDegrees = rotation;
            instance.Type = type;
             instance.ParentCabinet = this;

            ActiveSnapPoints.Add(instance);
            return instance;
        }

        public virtual void Interact()
        {
            GD.Print($"Interakce se skříňkou: {Data.ResourceName}");
        }

        public abstract void SetHighlight(bool active, bool isDeletePreview = false);

        protected void ApplyMaterialToMesh(MeshInstance3D mesh, bool active, Material mat)
        {
            if (mesh == null) return;

            if (active)
                mesh.MaterialOverride = mat;
            else
                mesh.MaterialOverride = Data.BodyMaterial;
        }
        public void Delete()
        {
            Data.DimensionsChanged -= Rebuild;
            QueueFree();
        }

        public Node AsNode()
        {
            return this;
        }

        public void SetMaterial(Material material, MaterialZone zone)
        {
            if (Data == null) return;

            switch (zone)
            {
                case MaterialZone.Body:
                    Data.BodyMaterial = material;
                    break;
                case MaterialZone.Front:
                    Data.FrontMaterial = material;
                    break;
                case MaterialZone.Worktop:
                    Data.WorktopMaterial = material;
                    break;
            }

            UpdateMaterials();
        }

        protected virtual void UpdateMaterials()
        {
            if (Data == null) return;
        }

        public bool HasZone(MaterialZone zone)
        {
            return zone switch
            {
                MaterialZone.Body => true,
                MaterialZone.Front => true,
                MaterialZone.Worktop => Data.HasWorktop,
                _ => false
            };
        }

        public Material GetMaterial(MaterialZone zone)
        {
            return zone switch
            {
                MaterialZone.Body => Data.BodyMaterial,
                MaterialZone.Front => Data.FrontMaterial,
                MaterialZone.Worktop => Data.WorktopMaterial,
                _ => null
            };

        }
    }
}
