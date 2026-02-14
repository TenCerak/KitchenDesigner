using Godot;
using KitchenDesigner.Features.Kitchen.Components;
using KitchenDesigner.Features.Kitchen.Data;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System.Collections.Generic;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public partial class CabinetOven : CabinetBase
    {
        [ExportGroup("Data")]
        [Export] public float BoardThickness = 0.018f;

        [ExportGroup("Standard Panels")]
        [Export] public MeshInstance3D LeftPanel;
        [Export] public MeshInstance3D RightPanel;
        [Export] public MeshInstance3D TopPanel;
        [Export] public MeshInstance3D BottomPanel;
        [Export] public MeshInstance3D BackPanel;

        [ExportGroup("Components")]
        [Export] public MeshInstance3D WorktopMesh;
        [Export] public ApplianceSlot OvenSlot;

        [ExportGroup("Panel Colliders")]
        [Export] public StaticBody3D StaticBody;
        [Export] public CollisionShape3D ColLeft;
        [Export] public CollisionShape3D ColRight;
        [Export] public CollisionShape3D ColBottom;
        [Export] public CollisionShape3D ColTop;
        [Export] public CollisionShape3D ColBack;





        private List<ShelfController> _activeShelves = new List<ShelfController>();

        protected override void RebuildGeometry()
        {
            if (Data == null) return;
            float w = float.Clamp(Data.Width, 0.6f, float.MaxValue);
            float h = float.Clamp(Data.Height, 0.6f, float.MaxValue);
            float d = float.Clamp(Data.Depth, 0.6f, float.MaxValue);
            float t = BoardThickness;

            // 1. Nastavení Pivotu (Align to Bottom-Back-Center)
            Vector3 centerOffset = new Vector3(0, h / 2.0f, d / 2.0f);

            if (VisualsRoot != null) VisualsRoot.Position = centerOffset;
            if (StaticBody != null) StaticBody.Position = centerOffset;

            // 2. Rozměry a pozice dílců (Relativní vůči středu VisualsRoot)

            // Bočnice (Stojí na zemi, celá výška)
            UpdatePart(LeftPanel, ColLeft, CabinetMaterial, new Vector3(t, h, d), new Vector3(-w / 2 + t / 2, 0, 0));
            UpdatePart(RightPanel, ColRight, CabinetMaterial, new Vector3(t, h, d), new Vector3(w / 2 - t / 2, 0, 0));

            // Dno a Strop (Vložené mezi bočnice)
            float innerWidth = w - (2 * t);
            UpdatePart(BottomPanel, ColBottom, CabinetMaterial, new Vector3(innerWidth, t, d), new Vector3(0, -h / 2 + t / 2, 0));
            UpdatePart(TopPanel, ColTop, CabinetMaterial, new Vector3(innerWidth, t, d), new Vector3(0, h / 2 - t / 2, 0));

            // Záda (Naložená zezadu)
            // Pozice Z je úplně vzadu (-d/2) plus polovina tloušťky zad
            UpdatePart(BackPanel, ColBack, CabinetMaterial, new Vector3(w, h, t), new Vector3(0, 0, -d / 2 + t / 2));

            UpdateWorktop();

            if (OrientationArrow != null)
            {
                float arrowZ = Data.Depth + 0.15f;
                OrientationArrow.Position = new Vector3(0, 0.01f, arrowZ);
            }

            if (OvenSlot is not null)
            {
                OvenSlot.Position = new Vector3(0, h / 2f - 0.30f, d / 2f);
            }


            SetGhostMode(_isGhost);
        }

        public override void _Ready()
        {
            base._Ready();
            UpdateDoors();
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
                Data.WorktopMaterial = currentMat;
            }

            WorktopMesh.Mesh = newWorktopMesh;

            float posY = Data.Height / 2 + (thickness / 2.0f);

            float posZ = overhang / 2;

            WorktopMesh.Position = new Vector3(0, posY, posZ);
        }

        protected override void UpdateDoors()
        {
            foreach (Node child in DoorsContainer.GetChildren()) child.QueueFree();

            if (DoorPrefab == null) return;

            float totalW = Data.Width;
            float h = Data.Height;
            float thickness = 0.02f;
            float gap = 0.002f;

            float doorHeight = h - 0.6f - gap;
            if (doorHeight < 0.12f) return;
            DoorsContainer.Position = new Vector3(totalW / 2f, 0, Data.Depth);
            DoorsContainer.RotationDegrees = new Vector3(0, 0, 90);

            var door = CreateDoor(doorHeight, totalW, thickness, false, 0);
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
            doorInstance.Setup(width, height, thickness, isGlass, isRight, 100, HandlePosition.Middle);
            return doorInstance;
        }

        public override void SetHighlight(bool active, bool isDeletePreview = false)
        {
            StandardMaterial3D highlightMat = null;
            if (active)
            {
                highlightMat = new StandardMaterial3D();
                highlightMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
                highlightMat.AlbedoColor = isDeletePreview ? new Color(1, 0, 0, 0.5f) : new Color(1, 1, 0, 0.5f);

                if (CabinetMaterial == null && LeftPanel != null)
                    CabinetMaterial = LeftPanel.GetSurfaceOverrideMaterial(0);
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

        protected override void UpdateSnapPoints()
        {
            float w = Data.Width;
            float h = Data.Height;
            float d = Data.Depth;

            foreach (Node child in SnapPointsContainer.GetChildren()) child.QueueFree();
            ActiveSnapPoints.Clear();

            if (OvenSlot is not null)
            {
                OvenSlot.SnapPoint?.ParentObject = this;
                ActiveSnapPoints.Add(OvenSlot.SnapPoint);
            }

            if (SnapPointScene == null) return;

            float centerY = 0;

            float centerZ = 0;


            CreateSnapPoint(SnapType.Left, new Vector3(-w / 2.0f, centerY, centerZ), Vector3.Zero);

            CreateSnapPoint(SnapType.Right, new Vector3(w / 2.0f, centerY, centerZ), Vector3.Zero);

            CreateSnapPoint(SnapType.Top, new Vector3(0, h, centerZ), Vector3.Zero);

            CreateSnapPoint(SnapType.Bottom, new Vector3(0, 0, centerZ), Vector3.Zero);
        }

        protected override void UpdateMaterials()
        {
            if (Data is null) return;

            LeftPanel?.MaterialOverride = Data.BodyMaterial;
            RightPanel?.MaterialOverride = Data.BodyMaterial;
            TopPanel?.MaterialOverride = Data.BodyMaterial;
            BottomPanel?.MaterialOverride = Data.BodyMaterial;
            BackPanel?.MaterialOverride = Data.BodyMaterial;

            foreach (var shelf in _activeShelves)
            {
                shelf.SetMaterial(Data.BodyMaterial);
            }

            if (Data.HasWorktop)
            {
                Material currentMat = WorktopMesh.MaterialOverride;
                if (currentMat == null && WorktopMesh.Mesh != null)
                {
                    currentMat = WorktopMesh.Mesh.SurfaceGetMaterial(0);
                }

                if (currentMat != null)
                {
                    WorktopMesh.MaterialOverride = Data.WorktopMaterial;
                }
            }

            if (Data.DoorType == DoorType.Drawer)
            {
                foreach (Node child in DoorsContainer.GetChildren())
                {
                    if (child is CabinetDrawer drawer)
                    {
                        drawer.SetBodyMaterial(Data.BodyMaterial);
                        drawer.SetFrontMaterial(Data.FrontMaterial);
                    }
                }
            }
            else
            {
                foreach (Node child in DoorsContainer.GetChildren())
                {
                    if (child is CabinetDoor door)
                    {
                        door.SetMaterial(Data.FrontMaterial);
                    }
                }
            }
        }
    }
}

