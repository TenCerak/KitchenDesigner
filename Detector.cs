using Godot;
using System;
using System.Threading.Tasks.Dataflow;

public partial class Detector : Area3D
{
    // Called when the node enters the scene tree for the first time.

    [Signal]
    public delegate void ToggledEventHandler(bool isOn);

    [Export]
    public bool IsOn = false;

    [Export]
    public Color OnColor = Colors.Green;
    [Export]
    public Color OffColor = Colors.Red;

    private bool canToggle = true;
    private Timer Timer;
    public override void _Ready()
    {
        Timer = GetNode<Timer>("Timer");
        Timer.Timeout += () =>
        {
            canToggle = true;
        };
        this.BodyEntered += OnBodyEntered;

    }

    public void OnBodyEntered(Node3D body)
    {
        GD.Print("body entered" + body.Name);
        if (canToggle)
        {
            IsOn = !IsOn;
            updateOn();
        }
    }

    private void updateOn()
    {
        var material = GetNode<MeshInstance3D>("MeshInstance3D").MaterialOverride;
        if (material is StandardMaterial3D standardMaterial)
        {
            standardMaterial.AlbedoColor = IsOn ? OnColor : OffColor;
            EmitSignal(SignalName.Toggled, IsOn);
            canToggle = false;
            Timer.Start();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
