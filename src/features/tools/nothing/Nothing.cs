using Godot;
using KitchenDesigner.Common.Interfaces;
using KitchenDesigner.Common.Utils;
using KitchenDesigner.Features.Kitchen.Components;
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
        if (_handManager != null) {
            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, false);
            _handManager.SetPointerLayerEnabled(CollisionLayerHelper.INTERACTIBLES, false);
        }
        QueueFree();
    }


    public void SetHighlight(bool enabled)
    {
    }

    public void Initialize(XrHandManager handManager)
    {
        _handManager = handManager;
        _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, true);
        _handManager.SetPointerLayerEnabled(CollisionLayerHelper.INTERACTIBLES, true);
    }

    public void Reattach(XrHandManager handManager)
    {
        _handManager = handManager;
        _handManager.SetPointerLayerEnabled(CollisionLayerHelper.TOOLS, true);
        _handManager.SetPointerLayerEnabled(CollisionLayerHelper.INTERACTIBLES, true);
    }

    public void ButtonPressed(string actionName)
    {
        if (actionName == "trigger_click")
        {
            var ray = _handManager.GetActiveRayCast();
            if (ray.IsColliding())
            {
                var collider = ray.GetCollider();

                if (collider is Area3D area && area.GetParent() is CabinetDoor door)
                {
                    door.ToggleOpen();
                    return; 
                }
            }
        }
    }

    public void ButtonReleased(string actionName)
    {
    }
}
