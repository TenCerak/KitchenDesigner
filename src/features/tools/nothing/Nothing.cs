using Godot;
using KitchenDesigner.Common.Interfaces;
using System;

public partial class Nothing : Node3D, IARTool
{
    public string ToolName => "Nic";
    public bool IsActive { get; set; } = false;

    private XrHandManager _handManager;
    private const int TOOLS_LAYER = 11; 
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
        QueueFree();
    }

    public void HandleInput(bool isPressed, bool isJustPressed)
    {
    }

    public void UpdateTool(double delta, Vector3 currentPos)
    {
    }


    public void Release()
    {
        QueueFree();
    }


    public void SetHighlight(bool enabled)
    {
    }

    public void Initialize(XrHandManager handManager)
    {
        _handManager = handManager;
        _handManager.SetPointerLayerEnabled(TOOLS_LAYER, true);
    }

    public void Reattach(XrHandManager handManager)
    {
        _handManager = handManager;
        _handManager.SetPointerLayerEnabled(TOOLS_LAYER, true);
    }

    public void ButtonPressed(string actionName)
    {
    }

    public void ButtonReleased(string actionName)
    {
    }
}
