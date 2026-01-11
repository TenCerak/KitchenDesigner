using Godot;
using KitchenDesigner;
using KitchenDesigner.Helpers;
using KitchenDesigner.Interfaces;
using System;

public partial class TestUi : Control, IMenuPage
{
	// Called when the node enters the scene tree for the first time.
	[Export]
	Button ButtonAR { get; set; }
    [Export]
    Button ButtonVR { get; set; }

    public string TabName => "Test UI";

    public override void _Ready()
	{
        var global = GetNode<Global>("/root/Global");

        if (ButtonAR is not null)
		{
			ButtonAR.Pressed += () =>
			{
				ARHelper.SwitchToAR(global.currentScene.GetViewport());
            };
        }

		if (ButtonVR is not null)
		{
			ButtonVR.Pressed += () =>
			{
				ARHelper.SwitchToVR(global.currentScene.GetViewport());
			};
		}            
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public void OnPageOpened()
    {
        
    }
}
