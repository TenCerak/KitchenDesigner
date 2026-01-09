using Godot;
using KitchenDesigner.Interfaces;
using System;

public partial class Tape : Node3D, IARTool
{
    [Export] public XRController3D LeftController;  // Ovladač pro levou ruku
    [Export] public XRController3D RightController; // Ovladač pro pravou ruku
    [Export] public Marker3D LeftTip;               // Bod na špičce levého ovladače
    [Export] public Marker3D RightTip;              // Bod na špičce pravého ovladače

    private ImmediateMesh _immMesh;
    private MeshInstance3D _tapeRender;
    private MeshInstance3D _startPoint;
    private MeshInstance3D _endPoint;
    private Label3D _distanceLabel;

    public string ToolName => "Držící metr";

    public override void _Ready()
    {
        _startPoint = GetNode<MeshInstance3D>("StartPoint");
        _endPoint = GetNode<MeshInstance3D>("EndPoint");
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
        ResetTool();
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
        if (LeftController == null || RightController == null || LeftTip == null || RightTip == null)
            return;

        // 1. LEVÁ RUKA: Pokud drží trigger, hýbe počátečním bodem
        if (LeftController.IsButtonPressed("trigger_click"))
        {
            _startPoint.GlobalPosition = LeftTip.GlobalPosition;
            if (!_startPoint.Visible) _startPoint.Show();
        }

        // 2. PRAVÁ RUKA: Pokud drží trigger, hýbe koncovým bodem
        if (RightController.IsButtonPressed("trigger_click"))
        {
            _endPoint.GlobalPosition = RightTip.GlobalPosition;
            if (!_endPoint.Visible) _endPoint.Show();
        }

        // 3. VIZUALIZACE: Kreslíme čáru a label pouze pokud jsou oba body viditelné
        if (_startPoint.Visible && _endPoint.Visible)
        {
            _distanceLabel.Show();
            DrawVisuals(_startPoint.GlobalPosition, _endPoint.GlobalPosition);
        }
    }

    private void DrawVisuals(Vector3 start, Vector3 end)
    {
        _immMesh.ClearSurfaces();

        // Prevence nulové čáry
        if (start.DistanceTo(end) < 0.001f) return;

        _immMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        // Protože jsme TopLevel, ToLocal() zajistí správné vykreslení čáry v souřadnicích metru
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
}