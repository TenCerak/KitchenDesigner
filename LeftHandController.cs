using Godot;
using KitchenDesigner.Interfaces;
using System;
using System.Threading.Tasks.Dataflow;

public partial class LeftHandController : XRController3D
{
	[Export]
	Node3D handMenu;
    [Export] public Node3D[] ToolNodes;
    private int _currentToolIndex = 0;
    private IARTool _activeTool;

    public override void _Ready()
	{
		this.ButtonPressed += buttonPress;
        ButtonReleased += buttonRelease;

        foreach (var node in ToolNodes) (node as IARTool)?.Deactivate();
        SwitchToTool(0);
    }
    private void SwitchToTool(int index)
    {
        _activeTool?.Deactivate();
        _currentToolIndex = index;
        _activeTool = ToolNodes[_currentToolIndex] as IARTool;
        _activeTool?.Activate();
        GD.Print($"Aktivní nástroj: {_activeTool.ToolName}");
    }
    private void SwitchToNextTool() => SwitchToTool((_currentToolIndex + 1) % ToolNodes.Length);
    private void buttonPress(string name)
    {
        GD.Print("Pressed: " + name);
        switch (name)
        {
            case "grip_click":
                handMenu.Visible = true;
                break;
            case "primary_click":
                SwitchToNextTool();
                break;
            default:
                break;
        }
    }

    private void buttonRelease(string name)
    {
        GD.Print("Released: " + name);
        switch (name)
        {

            case "grip_click":
                handMenu.Visible = false;
                break;
            default:
                break;
        }

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        _activeTool?.UpdateTool(delta, GlobalPosition);
    }
}
