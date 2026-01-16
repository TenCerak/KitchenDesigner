using Godot;
using KitchenDesigner.Interfaces;
using System;

public partial class Tape : Node3D, IARTool
{
     public XRController3D _leftController;  // Ovladač pro levou ruku
     public XRController3D _rightController; // Ovladač pro pravou ruku
    public Marker3D _leftTip;               // Bod na špičce levého ovladače
    public Marker3D _rightTip;              // Bod na špičce pravého ovladače

    private ImmediateMesh _immMesh;
    private MeshInstance3D _tapeRender;
    private MeshInstance3D _startPoint;
    private MeshInstance3D _endPoint;
    private Label3D _distanceLabel;

    private Area3D _selectionArea;
    private CollisionShape3D _collisionShape;
    private CylinderShape3D _cylinderShape;



    private StandardMaterial3D _pointsMaterial;

    [Export] public Color NormalColor { get; set; } = Colors.White;
    [Export] public Color HighlightColor { get; set; } = Colors.Yellow;

    [Export] public bool IsSnappingEnabled { get; set; } = true;
    [Export(PropertyHint.Layers3DPhysics)] public uint EnvironmentLayer = 1 << 9; // Vrstva 10

    public string ToolName => "Metr";

    public void Initialize(XRController3D leftController, XRController3D rightController, Marker3D leftTip, Marker3D rightTip)
    {
        _leftController = leftController;
        _rightController = rightController;
        _leftTip = leftTip;
        _rightTip = rightTip;
    }

    public override void _Ready()
    {
        _startPoint = GetNode<MeshInstance3D>("StartPoint");
        _endPoint = GetNode<MeshInstance3D>("EndPoint");

        SetupHighlightMaterials();

        _tapeRender = GetNode<MeshInstance3D>("TapeRender");
        _distanceLabel = GetNode<Label3D>("DistanceLabel");

        _immMesh = new ImmediateMesh();
        _tapeRender.Mesh = _immMesh;

        // Nastavení ostrého textu
        _distanceLabel.FontSize = 60;
        _distanceLabel.PixelSize = 0.001f;
        _distanceLabel.Billboard = BaseMaterial3D.BillboardModeEnum.FixedY;

        // Metr se pohybuje nezávisle na hierarchii ovladačů
        TopLevel = true;

        _selectionArea = GetNode<Area3D>("SelectionArea");
        _collisionShape = _selectionArea.GetNode<CollisionShape3D>("CollisionShape3D");

        _cylinderShape = (CylinderShape3D)_collisionShape.Shape;


        ResetTool();
    }

    private void UpdateCollisionShape(Vector3 start, Vector3 end)
    {
        float distance = start.DistanceTo(end);
        if (distance < 0.001f) return;

        // 1. Nastavení rozměrů válce
        _cylinderShape.Height = distance;
        _cylinderShape.Radius = 0.02f;

        // 2. Umístění do středu
        Vector3 midpoint = (start + end) / 2.0f;
        _selectionArea.GlobalPosition = midpoint;

        // 3. Výpočet směru
        Vector3 direction = (end - start).Normalized();

        // 4. Ošetření svislých čar (tzv. Gimbal Lock prevence)
        Vector3 upVector = Vector3.Up;
        if (Mathf.Abs(direction.Dot(Vector3.Up)) > 0.99f)
        {
            upVector = Vector3.Right; // Pokud míříme nahoru, "nahoru" pro LookAt bude doprava
        }

        _selectionArea.LookAt(end, upVector);

        _collisionShape.RotationDegrees = new Vector3(90, 0, 0);
    }

    private void ResetTool()
    {
        _startPoint.Hide();
        _endPoint.Hide();
        _distanceLabel.Hide();
        _immMesh.ClearSurfaces();
    }

    public void Activate() => Show();
    public void Deactivate() { ResetTool(); Hide(); }

    public void HandleInput(bool isPressed, bool isJustPressed) { }

    public void UpdateTool(double delta, Vector3 currentPos)
    {
        if (_leftController == null || _rightController == null || _leftTip == null || _rightTip == null)
        {
            SetHighlight(false);
            return;
        }

        if(_leftController.IsButtonPressed("ax_button"))
        {
            ToggleSnapping();
        }


        if (_leftController.IsButtonPressed("trigger_click"))
        {
            _startPoint.GlobalPosition = GetPositionWithSnap(_leftTip);
            _startPoint.Show();
        }

        if (_rightController.IsButtonPressed("trigger_click"))
        {
            _endPoint.GlobalPosition = GetPositionWithSnap(_rightTip);
            _endPoint.Show();
        }

        if (_startPoint.Visible && _endPoint.Visible)
        {
            _distanceLabel.Show();
            DrawVisuals(_startPoint.GlobalPosition, _endPoint.GlobalPosition);
        }

        UpdateCollisionShape(_startPoint.GlobalPosition, _endPoint.GlobalPosition);
    }

    public void ToggleSnapping()
    {
        IsSnappingEnabled = !IsSnappingEnabled;
        GD.Print($"Snapping: {IsSnappingEnabled}");

        Input.VibrateHandheld(500);
    }

    private Vector3 GetPositionWithSnap(Marker3D tip)
    {
        Vector3 defaultPos = tip.GlobalPosition;

        if (!IsSnappingEnabled) return defaultPos;

        var spaceState = GetWorld3D().DirectSpaceState;

        // Směr paprsku: Střílíme dopředu ze špičky ovladače (20 cm)
        Vector3 forward = -tip.GlobalTransform.Basis.Z;
        Vector3 target = defaultPos + forward * 0.2f;

        var query = PhysicsRayQueryParameters3D.Create(defaultPos, target);
        query.CollisionMask = EnvironmentLayer; // Hledáme jen stěny/nábytek
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var result = spaceState.IntersectRay(query);

        if (result.Count > 0)
        {
            return (Vector3)result["position"];
        }

        return defaultPos;
    }

    private void DrawVisuals(Vector3 start, Vector3 end)
    {
        _immMesh.ClearSurfaces();

        // Prevence nulové čáry
        if (start.DistanceTo(end) < 0.001f) return;

        _immMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

        _immMesh.SurfaceAddVertex(ToLocal(start));
        _immMesh.SurfaceAddVertex(ToLocal(end));
        _immMesh.SurfaceEnd();

        // Výpočet a zobrazení vzdálenosti
        float distance = start.DistanceTo(end);
        _distanceLabel.GlobalPosition = (start + end) / 2.0f + new Vector3(0, 0.05f, 0);

        if (distance < 1.0f)
            _distanceLabel.Text = $"{(distance * 100):F1} cm";
        else
            _distanceLabel.Text = $"{distance:F2} m";
    }

    public void Release()
    {
        SetHighlight(false);
    }

    public void Reattach(XRController3D leftController, XRController3D rightController, Marker3D leftTip, Marker3D rightTip)
    {
        _leftController = leftController;
        _rightController = rightController;
        _leftTip = leftTip;
        _rightTip = rightTip;
    }

    private void SetupHighlightMaterials()
    {
        Material originalMat = _startPoint.GetActiveMaterial(0);

        if (originalMat is StandardMaterial3D stdMat)
        {
            _pointsMaterial = (StandardMaterial3D)stdMat.Duplicate();

            _pointsMaterial.AlbedoColor = NormalColor;

            _startPoint.MaterialOverride = _pointsMaterial;
            _endPoint.MaterialOverride = _pointsMaterial;
        }
        else
        {
            GD.PrintErr("Tape: Koncové body nemají přiřazený StandardMaterial3D!");
        }
    }

    public void SetHighlight(bool enabled)
    {
        if (_pointsMaterial == null) return;
        GD.Print($"Tape: Nastavuji zvýraznění na {enabled}");
        if (enabled)
        {
            _pointsMaterial.AlbedoColor = HighlightColor;

            _pointsMaterial.EmissionEnabled = true;
            _pointsMaterial.Emission = HighlightColor;
            _pointsMaterial.EmissionEnergyMultiplier = 2.0f; 
        }
        else
        {
            _pointsMaterial.AlbedoColor = NormalColor;

            _pointsMaterial.EmissionEnabled = false;
        }
    }
}