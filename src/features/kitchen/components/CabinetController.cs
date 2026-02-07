using Godot;
using KitchenDesigner.Features.Kitchen.Components;
using KitchenDesigner.Features.Kitchen.Data;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System.Collections.Generic;

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
        [Export] public MeshInstance3D FillerPanel;
        [Export] public Node3D ShelvesContainer;

        [ExportGroup("Worktop")]
        [Export] public MeshInstance3D WorktopMesh;

        [ExportGroup("Panel Colliders")]
        [Export] public StaticBody3D StaticBody;
        [Export] public CollisionShape3D ColLeft;
        [Export] public CollisionShape3D ColRight;
        [Export] public CollisionShape3D ColBottom;
        [Export] public CollisionShape3D ColTop;
        [Export] public CollisionShape3D ColBack;

        [ExportGroup("Door System")]
        [Export] public PackedScene DoorPrefab;
        [Export] public Node3D DoorsContainer;
        [Export] public PackedScene DrawerPrefab;

        [ExportGroup("Visual Aids")]
        [Export] public Node3D OrientationArrow;

        [ExportGroup("Snapping System")]
        [Export] public PackedScene SnapPointScene;
        [Export] public Node3D SnapPointsContainer;


        private List<ShelfController> _activeShelves = new List<ShelfController>();
        public List<SnapPoint> ActiveSnapPoints { get; private set; } = new List<SnapPoint>();
        private bool _isGhost = false;

        public override void _Ready()
        {
            if (Data == null) Data = new CabinetData();

            Data.DimensionsChanged += RebuildCabinet;
            Data.WorktopToggled += RebuildCabinet;
            Data.DoorChanged += RebuildCabinet;

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

            UpdateFillerPanel();
            RebuildShelves(innerWidth, h, d, t);
            UpdateSnapPoints(w, h, d);
            UpdateWorktop();
            UpdateDoors();

            if (OrientationArrow != null)
            {
                float arrowZ = Data.Depth + 0.15f;
                OrientationArrow.Position = new Vector3(0, 0.01f, arrowZ);
            }
            SetGhostMode(_isGhost);
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

        private void UpdateWorktop()
        {
            if (WorktopMesh == null) return;

            WorktopMesh.Visible = Data.HasWorktop;
            if (!Data.HasWorktop) return;

            Material currentMat = WorktopMesh.MaterialOverride;
            if (currentMat == null && WorktopMesh.Mesh != null)
            {
                currentMat = WorktopMesh.Mesh.SurfaceGetMaterial(0);
            }

            float w = Data.Width;
            float d = Data.Depth;
            float thickness = Data.WorktopThickness;
            float overhang = Data.WorktopOverhang;

            float totalDepth = d + overhang;

            BoxMesh newWorktopMesh = new BoxMesh();
            newWorktopMesh.Size = new Vector3(w, thickness, totalDepth);

            if (currentMat is not null)
            {
                WorktopMesh.MaterialOverride = currentMat;
            }

            WorktopMesh.Mesh = newWorktopMesh;

            float posY = Data.Height / 2 + (thickness / 2.0f);

            float posZ = overhang / 2;

            WorktopMesh.Position = new Vector3(0, posY, posZ);
        }

        private void UpdateDoors()
        {
            foreach (Node child in DoorsContainer.GetChildren()) child.QueueFree();

            if (DoorPrefab == null || Data.DoorType == DoorType.None) return;

            DoorsContainer.Position = new Vector3(0, 0, Data.Depth);

            if (Data.DoorType == DoorType.Drawer)
            {
                GenerateDrawers();
                return;
            }

            float totalW = Data.Width;
            float h = Data.Height;
            float thickness = 0.02f;
            float gap = 0.002f;

            float usableWidth = totalW;
            float startX = -totalW / 2.0f;

            if (Data.Shape == CabinetShape.CornerBlind)
            {
                usableWidth = totalW - Data.CornerBlindWidth;

                if (Data.CornerIsLeft)
                {
                    startX = (-totalW / 2.0f) + Data.CornerBlindWidth;
                }
                else
                {
                    startX = -totalW / 2.0f;
                }
            }

            float leftEdge = startX;
            float rightEdge = startX + usableWidth;

            float w = usableWidth;

            switch (Data.DoorType)
            {
                case DoorType.SingleLeft:
                    CreateDoor(w - (gap * 2), h - (gap * 2), thickness, false, leftEdge + gap);
                    break;

                case DoorType.SingleRight:
                    CreateDoor(w - (gap * 2), h - (gap * 2), thickness, true, rightEdge - gap);
                    break;

                case DoorType.Double:
                    float halfW = (w / 2.0f) - gap;
                    CreateDoor(halfW, h - (gap * 2), thickness, false, leftEdge + gap);
                    CreateDoor(halfW, h - (gap * 2), thickness, true, rightEdge - gap);
                    break;
            }
        }

        private CabinetDoor CreateDoor(float width, float height, float thickness, bool isRight, float xOffset)
        {
            var doorInstance = DoorPrefab.Instantiate<CabinetDoor>();
            DoorsContainer.AddChild(doorInstance);

            doorInstance.Position = new Vector3(xOffset, 0.002f, 0);

            if (isRight)
            {
                doorInstance.RotationDegrees = new Vector3(0, 180, 0);
            }

            bool isGlass = Data.DoorStyle == DoorStyle.Glass;
            doorInstance.Setup(width, height, thickness, isGlass, isRight);
            return doorInstance;
        }

        private void GenerateDrawers()
        {
            if (DrawerPrefab == null) return;

            int drawerCount = Data.ShelfCount + 1;

            float totalHeight = Data.Height;
            float gap = 0.003f; 

            
            float usableHeight = totalHeight - (gap * (drawerCount + 1));
            float singleDrawerHeight = usableHeight / drawerCount;

            float w = Data.Width - (gap * 2);
            float d = Data.Depth;

            float currentY = gap + (singleDrawerHeight / 2.0f);

            for (int i = 0; i < drawerCount; i++)
            {
                var drawer = DrawerPrefab.Instantiate<CabinetDrawer>();
                DoorsContainer.AddChild(drawer);

                drawer.Position = new Vector3(0, currentY, 0);

                drawer.Setup(w, singleDrawerHeight, d);

                currentY += singleDrawerHeight + gap;
            }
        }

        private void UpdateFillerPanel()
        {
            if (FillerPanel == null) return;

            if (Data.Shape != CabinetShape.CornerBlind)
            {
                FillerPanel.Visible = false;
                return;
            }

            FillerPanel.Visible = true;

            float blindW = Data.CornerBlindWidth;
            float h = Data.Height;
            float d = Data.Depth;
            float thickness = 0.018f;

            BoxMesh fillerMesh = new BoxMesh();
            fillerMesh.Size = new Vector3(blindW, h, thickness);
            FillerPanel.Mesh = fillerMesh;

            float totalW = Data.Width;
            float fillerX = 0;
           
            float fillerZ = Data.Depth / 2 + (thickness / 2.0f);

            if (Data.CornerIsLeft)
            {
                fillerX = (-totalW / 2.0f) + (blindW / 2.0f);
            }
            else
            {
                fillerX = (totalW / 2.0f) - (blindW / 2.0f);
            }

            FillerPanel.Position = new Vector3(fillerX, 0, fillerZ);

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

        private void UpdateSnapPoints(float w, float h, float d)
        {
            foreach (Node child in SnapPointsContainer.GetChildren()) child.QueueFree();
            ActiveSnapPoints.Clear();

            if (SnapPointScene == null) return;

            float centerY = 0;

            float centerZ = 0;


            CreateSnapPoint(SnapType.Left, new Vector3(-w / 2.0f, centerY, centerZ));

            CreateSnapPoint(SnapType.Right, new Vector3(w / 2.0f, centerY, centerZ));

            CreateSnapPoint(SnapType.Top, new Vector3(0, h, centerZ));

            CreateSnapPoint(SnapType.Bottom, new Vector3(0, 0, centerZ));

            if (Data.Shape == CabinetShape.CornerBlind)
            {

                float blindW = Data.CornerBlindWidth;
                float snapX = 0;
                float snapZ = d; 

                Vector3 rotation = Vector3.Zero;

                if (Data.CornerIsLeft)
                {
                    snapX = (-w / 2.0f);

                    rotation = new Vector3(0, -90, 0);
                }
                else
                {
                    snapX = (w / 2.0f);
                    rotation = new Vector3(0, 90, 0);
                }

                var snap = CreateSnapPoint(SnapType.CornerFront, new Vector3(snapX, centerY, snapZ));
                snap.RotationDegrees = rotation;
            }

        }


        private SnapPoint CreateSnapPoint(SnapType type, Vector3 localPos)
        {
            var instance = SnapPointScene.Instantiate<SnapPoint>();
            SnapPointsContainer.AddChild(instance);

            instance.Position = localPos;
            instance.Type = type;
            instance.ParentCabinet = this;
            instance.AreaEntered += Instance_AreaEntered;

            ActiveSnapPoints.Add(instance);
            return instance;
        }

        private void Instance_AreaEntered(Area3D area)
        {
        }

        public void SetGhostMode(bool isGhost)
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
    }
}

