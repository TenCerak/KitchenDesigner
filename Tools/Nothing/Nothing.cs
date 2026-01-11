using Godot;
using KitchenDesigner.Interfaces;
using System;

public partial class Nothing : Node3D, IARTool
{
    public string ToolName => "Nic";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void Activate()
    {
    }

    public void Deactivate()
    {
    }

    public void HandleInput(bool isPressed, bool isJustPressed)
    {
    }

    public void UpdateTool(double delta, Vector3 currentPos)
    {
    }

    public void Initialize(XRController3D leftController, XRController3D rightController, Marker3D leftTip, Marker3D rightTip)
    {
    }

    public void Release()
    {
    }

    public void Reattach(XRController3D leftController, XRController3D rightController, Marker3D leftTip, Marker3D rightTip)
    {
    }

    public void SetHighlight(bool enabled)
    {
    }
}
