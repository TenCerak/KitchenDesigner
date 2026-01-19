using Godot;
using KitchenDesigner.Interfaces;
using KitchenDesigner.Tools;
using System;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

public partial class ToolManager : Node
{
    [Export] public XrHandManager HandManager; 
    [Export] public Node3D handMenu;


    private IARTool _activeTool;
    private IARTool _hoveredTool = null;

    private const int TOOLS_LAYER = 11;
    public override void _Ready()
    {
        base._Ready();
        HandManager.DominantHandButtonPressed += DominantControllerButtonPressed;
        HandManager.DominantHandButtonReleased += DominantControllerButtonReleased;
        HandManager.DominantRaycastStartedColliding += OnDominantRaycastStartedColliding;
        HandManager.DominantRaycastStoppedColliding += OnDominantRaycastStoppedColliding;
    }



    void DominantControllerButtonPressed(XRController3D controller,string actionName)
    {
        if (actionName == "trigger_click" && _hoveredTool is not null && _activeTool is Nothing)
        {
            TryPickupTool(_hoveredTool);
            HandManager.VibrateDominantHand();
        }
        if (actionName == "by_button" && _hoveredTool is not null)
        {
            DeleteHoveredTool();
            HandManager.VibrateDominantHand();
        }

        if(_activeTool is not null)
        {
            _activeTool.ButtonPressed(actionName);
        }
    }

    private void DominantControllerButtonReleased(XRController3D controller, string actionName)
    {
        if (_activeTool is not null)
        {
            _activeTool.ButtonReleased(actionName);
        }
    }

    public void SpawnTool(ARToolResource definition)
    {
        if (_activeTool != null) { 
            _activeTool.SetHighlight(false);
            DropActiveTool();
        }

        Node instance = definition.ToolScene.Instantiate();
        AddChild(instance);

        if (instance is IARTool tool)
        {
            _activeTool = tool;
            _activeTool.Initialize(HandManager);
            _activeTool.Activate();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_activeTool != null && (handMenu?.Visible ?? false) == false)
        {
            _activeTool.UpdateTool(delta, HandManager.GetActiveTip().GlobalPosition);
        }
    }
    private void OnDominantRaycastStartedColliding(Node collider)
    {
        IARTool targetTool = FindToolFromNode(collider);
        if (targetTool != null)
        {
            _hoveredTool = targetTool;
            _hoveredTool.SetHighlight(true);
        }
    }

    private void OnDominantRaycastStoppedColliding()
    {
        if (_hoveredTool != null)
        {
            _hoveredTool.SetHighlight(false);
            _hoveredTool = null;
        }
    }
    private IARTool FindToolFromNode(Node node)
    {
        if (node == null) return null;
        if (node is IARTool tool) return tool;
        return FindToolFromNode(node.GetParent());
    }
    public void TryPickupTool(IARTool toolToPickup)
    {
        if (toolToPickup is not null)
        {
            if (_activeTool is not null && _activeTool != toolToPickup)
            {
                _activeTool.Deactivate();
            }

            Node toolNode = (Node)toolToPickup;

            if (toolNode.GetParent() != this)
            {
                toolNode.GetParent()?.RemoveChild(toolNode);
                AddChild(toolNode);
            }

            HandManager.SetPointerLayerEnabled(TOOLS_LAYER, false);

            _activeTool = toolToPickup;
            _activeTool.Reattach(HandManager);
            _activeTool.Activate();

            OnDominantRaycastStoppedColliding();

            GD.Print($"Nástroj {toolToPickup.ToolName} sebrán.");
        }
    }

    public void DeleteHoveredTool()
    {
        if (_hoveredTool is not null && _hoveredTool is not Nothing)
        {
            Node toolNode = (Node)_hoveredTool;
            if(_hoveredTool == _activeTool)
            {
                DropActiveTool();
            }
            toolNode.QueueFree();
            _hoveredTool = null;
        }
    }
    public void DropActiveTool()
    {
        if (_activeTool != null)
        {
            _activeTool.Deactivate();
            _activeTool = null;

            HandManager.SetPointerLayerEnabled(TOOLS_LAYER, true);
        }
    }

}