using Godot;
using KitchenDesigner;
using KitchenDesigner.Interfaces;
using System;
using System.Threading.Tasks.Dataflow;

public partial class LeftHandController : XRController3D
{
	[Export]
	Node3D handMenu;

    public override void _Ready()
	{
		this.ButtonPressed += buttonPress;
        ButtonReleased += buttonRelease;
    }

    private void buttonPress(string name)
    {
        GD.Print("Pressed: " + name);
        switch (name)
        {
            case "grip_click":
                handMenu.Visible = true;
                break;
            case "primary_click":
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
    }
}
