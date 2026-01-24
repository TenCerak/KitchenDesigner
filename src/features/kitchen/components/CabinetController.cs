using Godot;
using System.Collections.Generic;
using KitchenDesigner.Features.Kitchen.Data;
using KitchenDesigner.Features.Kitchen.Interfaces;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class CabinetController : Node3D, IKitchenComponent
    {
        [ExportGroup("Data")]
        [Export] public CabinetData Data;
        [Export] public float BoardThickness = 0.018f;

        [ExportGroup("External Scenes")]
        [Export] public PackedScene ShelfScene;

        [ExportGroup("Panel Meshes")]
        [Export] public Material Material;
        [Export] public Node3D VisualsRoot;
        [Export] public MeshInstance3D LeftPanel;
        [Export] public MeshInstance3D RightPanel;
        [Export] public MeshInstance3D BottomPanel;
        [Export] public MeshInstance3D TopPanel;
        [Export] public MeshInstance3D BackPanel;
        [Export] public Node3D ShelvesContainer;

        [ExportGroup("Panel Colliders")]
        [Export] public StaticBody3D StaticBody;
        [Export] public CollisionShape3D ColLeft;
        [Export] public CollisionShape3D ColRight;
        [Export] public CollisionShape3D ColBottom;
        [Export] public CollisionShape3D ColTop;
        [Export] public CollisionShape3D ColBack;


        private List<ShelfController> _activeShelves = new List<ShelfController>();


        public override void _Ready()
        {
            if (Data == null) Data = new CabinetData();

            Data.DimensionsChanged += RebuildCabinet;

            RebuildCabinet();
        }

        public void RebuildCabinet()
        {
            if (Data == null) return;

            float w = Data.Width;
            float h = Data.Height;
            float d = Data.Depth;
            float t = BoardThickness;

            // 1. Nastavení Pivotu (Align to Bottom-Back-Center)
            // Střed skříňky je geometricky v (0, h/2, d/2)
            // Posuneme kontejnery tak, aby (0,0,0) celého objektu bylo "dole vzadu".
            Vector3 centerOffset = new Vector3(0, h / 2.0f, d / 2.0f);

            if (VisualsRoot != null) VisualsRoot.Position = centerOffset;
            if (StaticBody != null) StaticBody.Position = centerOffset;

            // 2. Rozměry a pozice dílců (Relativní vůči středu VisualsRoot)

            // Bočnice (Stojí na zemi, celá výška)
            UpdatePart(LeftPanel, ColLeft, Material, new Vector3(t, h, d), new Vector3(-w / 2 + t / 2, 0, 0));
            UpdatePart(RightPanel, ColRight, Material, new Vector3(t, h, d), new Vector3(w / 2 - t / 2, 0, 0));

            // Dno a Strop (Vložené mezi bočnice)
            float innerWidth = w - (2 * t);
            UpdatePart(BottomPanel, ColBottom, Material, new Vector3(innerWidth, t, d), new Vector3(0, -h / 2 + t / 2, 0));
            UpdatePart(TopPanel, ColTop, Material, new Vector3(innerWidth, t, d), new Vector3(0, h / 2 - t / 2, 0));

            // Záda (Naložená zezadu)
            // Pozice Z je úplně vzadu (-d/2) plus polovina tloušťky zad
            UpdatePart(BackPanel, ColBack, Material, new Vector3(w, h, t), new Vector3(0, 0, -d / 2 + t / 2));

            RebuildShelves(innerWidth, h, d, t);
        }

        private void UpdatePart(MeshInstance3D mesh, CollisionShape3D col, Material material, Vector3 size, Vector3 pos)
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

        private void RebuildShelves(float innerWidth, float h, float d, float t)
        {
            foreach (Node child in ShelvesContainer.GetChildren()) child.QueueFree();
            _activeShelves.Clear();

            if (Data.ShelfCount <= 0 || ShelfScene == null) return;

            float innerHeight = h - (2 * t);
            float startY = -h / 2.0f + t; // Začínáme nad dnem
            float spacing = innerHeight / (Data.ShelfCount + 1);

            float shelfDepth = d - 0.02f; // Odsazení zepředu 2cm
            Vector3 shelfSize = new Vector3(innerWidth, t, shelfDepth);

            for (int i = 1; i <= Data.ShelfCount; i++)
            {
                ShelfController newShelf = ShelfScene.Instantiate<ShelfController>();
                ShelvesContainer.AddChild(newShelf);

                _activeShelves.Add(newShelf);

                float posY = startY + (spacing * i) - (t / 2.0f);
                newShelf.Position = new Vector3(0, posY, 0);

                newShelf.SetDimensions(shelfSize);
            }
        }
        public Node AsNode() => this;

        public void Delete()
        {
            QueueFree();
        }

        public void SetHighlight(bool active, bool isDeletePreview = false)
        {
            StandardMaterial3D highlightMat = null;
            if (active)
            {
                highlightMat = new StandardMaterial3D();
                highlightMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                highlightMat.AlbedoColor = isDeletePreview ? new Color(1, 0, 0, 0.5f) : new Color(1, 1, 0, 0.5f);

                if (Material == null && LeftPanel != null)
                    Material = LeftPanel.GetSurfaceOverrideMaterial(0);
            }

            ApplyMaterialToMesh(LeftPanel, active, highlightMat);
            ApplyMaterialToMesh(RightPanel, active, highlightMat);
            ApplyMaterialToMesh(TopPanel, active, highlightMat);
            ApplyMaterialToMesh(BottomPanel, active, highlightMat);
            ApplyMaterialToMesh(BackPanel, active, highlightMat);

            foreach (var shelf in _activeShelves)
            {
                shelf.SetMaterial(active ? highlightMat : null);
            }
        }

        private void ApplyMaterialToMesh(MeshInstance3D mesh, bool active, Material mat)
        {
            if (mesh == null) return;

            if (active)
                mesh.SetSurfaceOverrideMaterial(0, mat);
            else
                mesh.SetSurfaceOverrideMaterial(0, Material);
        }
    }
}