using Godot;
using KitchenDesigner.Interfaces;
using KitchenDesigner.Tools;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;

public partial class ToolManager : Node
{
    [Export] public XRController3D LeftController;   // Levý ovladač
    [Export] public XRController3D RightController; // Pravý ovladač
    [Export] public Marker3D LeftTip;               // Špička levého
    [Export] public Marker3D RightTip;             // Špička pravého
    [Export] public Node3D LeftPointer;
    private RayCast3D _leftRayCast;
    [Export] public Node3D RightPointer;
    private RayCast3D _rightRayCast;

    [Export] Node3D handMenu;

    private IARTool _activeTool;
    private IARTool _hoveredTool = null;

    public override void _Ready()
    {
        base._Ready();

        _leftRayCast = LeftPointer.GetNode<RayCast3D>("RayCast");
        _rightRayCast = RightPointer.GetNode<RayCast3D>("RayCast");
        GD.Print($"ToolManager připraven. leftRay: {_leftRayCast is not null} rightRay: {_rightRayCast is not null}");
    }

    public void SpawnTool(ARToolResource definition)
    {
        if (_activeTool != null) { 
            _activeTool.SetHighlight(false);
        }

        Node instance = definition.ToolScene.Instantiate();
        AddChild(instance);

        if (instance is IARTool tool)
        {
            _activeTool = tool;
            _activeTool.Initialize(LeftController, RightController, LeftTip, RightTip);
            _activeTool.Activate();
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (_activeTool != null && (handMenu?.Visible ?? false) == false)
        {
            _activeTool.UpdateTool(delta, LeftTip.GlobalPosition);
            CheckPointerHover();
        }
    }
    private void CheckPointerHover()
    {
        if (_leftRayCast == null) return;

        // 1. Zjistíme, na co Pointer míří
        if (_leftRayCast.IsColliding())
        {
            GD.Print("ToolManager - Pointer koliduje s objektem.");
            var collider = _leftRayCast.GetCollider();

            // Hledáme IARTool v kolizním objektu nebo jeho rodiči
            IARTool targetTool = FindToolFromNode(collider as Node);

            if (targetTool != null)
            {
                GD.Print($"ToolManager - Pointer míří na nástroj: {targetTool.ToolName}");
                if (_hoveredTool != targetTool)
                {
                    _hoveredTool?.SetHighlight(false);
                    _hoveredTool = targetTool;
                    _hoveredTool.SetHighlight(true);
                }
                return;
            }
        }

        // Pokud na nic nemíříme, zrušíme highlight
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
    public void TryPickupTool(Node3D hitObject)
    {
        // 1. Zjistíme, zda objekt, na který míříme, je (nebo obsahuje) IARTool
        IARTool toolToPickup = null;

        if (hitObject is IARTool t) toolToPickup = t;
        else if (hitObject.GetParent() is IARTool tp) toolToPickup = tp;

        if (toolToPickup != null)
        {
            // 2. Pokud už nějaký nástroj držíme, "pustíme" ho
            if (_activeTool != null) _activeTool.Deactivate();

            // 3. Přepojíme nalezený nástroj pod náš ovladač (pokud není TopLevel, musíme ho přeparkovat)
            Node toolNode = (Node)toolToPickup;

            // Pokud chceme, aby se fyzicky vrátil do hierarchie ruky:
            if (toolNode.GetParent() != this)
            {
                toolNode.GetParent().RemoveChild(toolNode);
                AddChild(toolNode);
            }

            // 4. Inicializujeme ho našimi ovladači
            _activeTool = toolToPickup;
            _activeTool.Reattach(LeftController, RightController, LeftTip, RightTip);
            _activeTool.Activate();

            GD.Print($"Znovu aktivován nástroj: {toolToPickup.ToolName}");
        }
    }
}