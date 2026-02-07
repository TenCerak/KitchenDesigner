using Godot;
using KitchenDesigner.Features.Kitchen.Components;
using System;

public partial class CornerLCabinet : CabinetBase
{

    [ExportGroup("Walls (Vertical)")]
    [Export] public MeshInstance3D LeftBack;
    [Export] public CollisionShape3D ColLeftBack;

    [Export] public MeshInstance3D RightBack;
    [Export] public CollisionShape3D ColRightBack;

    [Export] public MeshInstance3D LeftSide;
    [Export] public CollisionShape3D ColLeftSide;

    [Export] public MeshInstance3D RightSide;
    [Export] public CollisionShape3D ColRightSide;

    [ExportGroup("Floor & Ceiling (Horizontal)")]
    [Export] public MeshInstance3D BottomPartA;
    [Export] public CollisionShape3D ColBottomA;
    [Export] public MeshInstance3D BottomPartB;
    [Export] public CollisionShape3D ColBottomB;

    [Export] public MeshInstance3D TopPartA;
    [Export] public CollisionShape3D ColTopA;
    [Export] public MeshInstance3D TopPartB;
    [Export] public CollisionShape3D ColTopB;

    [Export] public StaticBody3D StaticBody;


    [ExportGroup("Components")]
    [Export] public MeshInstance3D WorktopPartA;
    [Export] public MeshInstance3D WorktopPartB;

    [Export] public Node3D ShelvesContainer;
    [Export] public PackedScene ShelfScene;

    protected override void RebuildGeometry()
    {
        if (Data == null) return;

        float LeftWidth = Data.CornerLeftWidth;
        float RightWidth = Data.CornerRightWidth;

        float LeftDepth = Data.CornerLeftDepth;
        float RightDepth = Data.CornerRightDepth;

        float h = Data.Height;

        float t = 0.018f;

        Vector3 centerOffset = new Vector3(0, h / 2.0f, 0);

        if (VisualsRoot != null) VisualsRoot.Position = centerOffset;
        if (StaticBody != null) StaticBody.Position = centerOffset;

        Material mat = CabinetMaterial;


        UpdatePart(LeftBack, ColLeftBack, mat,
            new Vector3(LeftWidth + RightDepth - t, h, t),
            new Vector3((LeftWidth + RightDepth) / -2f + t / 2f, 0, t / 2));


        UpdatePart(RightBack, ColRightBack, mat,
            new Vector3(t, h, RightWidth + LeftDepth - 2 * t),
            new Vector3(-t / 2f, 0, (RightWidth + LeftDepth) / 2f));


        UpdatePart(RightSide, ColRightSide, mat,
            new Vector3(RightDepth, h, t),
            new Vector3(-RightDepth / 2.0f, 0, LeftDepth + RightWidth - t / 2f)
            );


        UpdatePart(LeftSide, ColLeftSide, mat,
            new Vector3(t, h, LeftDepth),
            new Vector3(-LeftWidth - RightDepth + t / 2f, 0, LeftDepth / 2.0f)
            );


        Vector3 sizeBottomA = new Vector3(LeftWidth - t, t, LeftDepth - t);

        Vector3 posBottomA = new Vector3(-RightDepth - LeftWidth / 2.0f + t / 2f,
            -h / 2.0f + t / 2.0f,
            LeftDepth / 2.0f + t / 2f);

        UpdatePart(BottomPartA, ColBottomA, mat, sizeBottomA, posBottomA);



        Vector3 sizeBottomB = new Vector3(RightDepth - t, t, RightWidth + LeftDepth - 2 * t);

        Vector3 posBottomB = new Vector3(
            -RightDepth / 2 - t / 2f,
            -h / 2.0f + t / 2.0f,
            (RightWidth + LeftDepth) / 2.0f
        );

        UpdatePart(BottomPartB, ColBottomB, mat, sizeBottomB, posBottomB);

        Vector3 posTopA = posBottomA;
        posTopA.Y = h / 2.0f - t / 2.0f;
        UpdatePart(TopPartA, ColTopA, mat, sizeBottomA, posTopA);

        Vector3 posTopB = posBottomB;
        posTopB.Y = h / 2.0f - t / 2.0f;
        UpdatePart(TopPartB, ColTopB, mat, sizeBottomB, posTopB);

        UpdateWorktop();
        RebuildShelves();
        SetGhostMode(_isGhost);
    }

    private void UpdateWorktop()
    {
        bool hasWorktop = Data.HasWorktop;
        if (WorktopPartA != null) WorktopPartA.Visible = hasWorktop;
        if (WorktopPartB != null) WorktopPartB.Visible = hasWorktop;

        if (!hasWorktop) return;

        float LeftWidth = Data.CornerLeftWidth;
        float RightWidth = Data.CornerRightWidth;
        float LeftDepth = Data.CornerLeftDepth;
        float RightDepth = Data.CornerRightDepth;

        float thickness = Data.WorktopThickness;
        float overhang = Data.WorktopOverhang;

        float posY = Data.Height / 2.0f + thickness / 2.0f;

        Material currentMat = null;
        if (WorktopPartA != null)
            currentMat = WorktopPartA.MaterialOverride ?? WorktopPartA.Mesh?.SurfaceGetMaterial(0);

        float sizeB_X = RightDepth + overhang;
        float sizeB_Z = RightWidth + LeftDepth;

        if (WorktopPartB != null)
        {
            BoxMesh meshB = new BoxMesh();
            meshB.Size = new Vector3(sizeB_X, thickness, sizeB_Z);
            WorktopPartB.Mesh = meshB;
            if (currentMat != null) WorktopPartB.MaterialOverride = currentMat;

            float posX = -(RightDepth + overhang) / 2.0f;


            float posZ = (RightWidth + LeftDepth) / 2.0f;

            WorktopPartB.Position = new Vector3(posX, posY, posZ);
        }

        float sizeA_X = LeftWidth - overhang;

        float sizeA_Z = LeftDepth + overhang;

        if (WorktopPartA != null)
        {
            BoxMesh meshA = new BoxMesh();
            meshA.Size = new Vector3(sizeA_X, thickness, sizeA_Z);
            WorktopPartA.Mesh = meshA;
            if (currentMat != null) WorktopPartA.MaterialOverride = currentMat;

            float posX = -(RightDepth + overhang) - (sizeA_X / 2.0f);

            float posZ = (LeftDepth + overhang) / 2.0f;

            WorktopPartA.Position = new Vector3(posX, posY, posZ);
        }
    }

    private void RebuildShelves()
    {
        if (ShelvesContainer != null)
        {
            foreach (Node child in ShelvesContainer.GetChildren()) child.QueueFree();
        }

        if (Data == null || Data.ShelfCount <= 0 || ShelfScene == null) return;

        float LeftWidth = Data.CornerLeftWidth;
        float RightWidth = Data.CornerRightWidth;
        float LeftDepth = Data.CornerLeftDepth;
        float RightDepth = Data.CornerRightDepth;
        float h = Data.Height;
        float t = 0.018f;
        float setback = 0.02f;

        float startY = -h / 2.0f + t;
        float innerHeight = h - (2 * t);
        float spacing = innerHeight / (Data.ShelfCount + 1);

        Vector3 sizeA = new Vector3(
            LeftWidth - t + setback,
            t,
            LeftDepth - t - setback
        );

        Vector3 posA_Base = new Vector3(
            -RightDepth - (LeftWidth / 2.0f) + (t / 2.0f) + (setback / 2.0f),
            0,
            (LeftDepth / 2.0f) + (t / 2.0f) - (setback / 2.0f)
        );

        Vector3 sizeB = new Vector3(
            RightDepth - t - setback,
            t,
            RightWidth + LeftDepth - (2 * t)
        );

        Vector3 posB_Base = new Vector3(
            (-RightDepth / 2.0f) - (t / 2.0f) + (setback / 2.0f),
            0,
            (RightWidth + LeftDepth) / 2.0f
        );

        for (int i = 1; i <= Data.ShelfCount; i++)
        {
            float currentY = startY + (spacing * i);

            Node shelfA = ShelfScene.Instantiate();
            ShelvesContainer.AddChild(shelfA);

            if (shelfA is Node3D s3dA)
            {
                s3dA.Position = new Vector3(posA_Base.X, currentY, posA_Base.Z);
            }

            if (shelfA.HasMethod("SetDimensions"))
            {
                shelfA.Call("SetDimensions", sizeA);
            }
            else if (shelfA is MeshInstance3D meshInstA)
            {
                BoxMesh newBox = new BoxMesh();
                newBox.Size = sizeA;
                meshInstA.Mesh = newBox;
            }

            Node shelfB = ShelfScene.Instantiate();
            ShelvesContainer.AddChild(shelfB);

            if (shelfB is Node3D s3dB)
            {
                s3dB.Position = new Vector3(posB_Base.X, currentY, posB_Base.Z);
            }

            if (shelfB.HasMethod("SetDimensions"))
            {
                shelfB.Call("SetDimensions", sizeB);
            }
            else if (shelfB is MeshInstance3D meshInstB)
            {
                BoxMesh newBox = new BoxMesh();
                newBox.Size = sizeB;
                meshInstB.Mesh = newBox;
            }
        }
    }
    private void CreateShelfInstance(Vector3 size, Vector3 pos)
    {
        var shelf = ShelfScene.Instantiate();
        ShelvesContainer.AddChild(shelf);

        // Nastavíme pozici
        if (shelf is Node3D shelf3D)
        {
            shelf3D.Position = pos;
        }

        if (shelf.HasMethod("SetDimensions"))
        {
            shelf.Call("SetDimensions", size);
        }
        else if (shelf is MeshInstance3D meshInst) 
        {
            if (meshInst.Mesh is BoxMesh box)
            {
                meshInst.Mesh = (Mesh)box.Duplicate();
                ((BoxMesh)meshInst.Mesh).Size = size;
            }
            else
            {
                BoxMesh newBox = new BoxMesh();
                newBox.Size = size;
                meshInst.Mesh = newBox;
            }
        }
        else if (shelf is ShelfController sc)
        {
            sc.SetDimensions(size);
            sc.SetMaterial(CabinetMaterial);
        }
    }

    protected override void UpdateSnapPoints()
    {
        foreach (Node child in SnapPointsContainer.GetChildren()) child.QueueFree();
        ActiveSnapPoints.Clear();

        float LeftWidth = Data.CornerLeftWidth;
        float RightWidth = Data.CornerRightWidth;
        float LeftDepth = Data.CornerLeftDepth;
        float RightDepth = Data.CornerRightDepth;

        float h = Data.Height;

        float centerY = 0;

        Vector3 leftSnapPos = new Vector3(
            -(RightDepth + LeftWidth),
            centerY,
            0
        );

        CreateSnapPoint(SnapType.Left, leftSnapPos, new Vector3(0,90,0));


        Vector3 rightSnapPos = new Vector3(
            0,
            centerY,
            LeftDepth + RightWidth
        );

        CreateSnapPoint(SnapType.Right, rightSnapPos, new Vector3(0, -90, 0));
    }
    protected override void UpdateDoors()
    {
        foreach (Node child in DoorsContainer.GetChildren()) child.QueueFree();

        //TODO
    }

    public override void SetHighlight(bool active, bool isDeletePreview = false)
    {
        StandardMaterial3D highlightMat = null;
        if (active)
        {
            highlightMat = new StandardMaterial3D();
            highlightMat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            highlightMat.AlbedoColor = isDeletePreview ? new Color(1, 0, 0, 0.5f) : new Color(1, 1, 0, 0.5f);
        }

        ApplyMaterialToMesh(LeftBack, active, highlightMat);
        ApplyMaterialToMesh(LeftSide, active, highlightMat);
        ApplyMaterialToMesh(BottomPartA, active, highlightMat);
        ApplyMaterialToMesh(RightBack, active, highlightMat);
        ApplyMaterialToMesh(RightSide, active, highlightMat);
        ApplyMaterialToMesh(BottomPartB, active, highlightMat);
        ApplyMaterialToMesh(TopPartA, active, highlightMat);
        ApplyMaterialToMesh(TopPartB, active, highlightMat);

        if (Data.HasWorktop)
        {
            ApplyMaterialToMesh(WorktopPartA, active, highlightMat);
            ApplyMaterialToMesh(WorktopPartB, active, highlightMat);
        }
    }
}
