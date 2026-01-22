using Godot;
using KitchenDesigner;
using KitchenDesigner.Common.Interfaces;
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
		var DesignerEvents = GetNode<DesignerEvents>("/root/DesignerEvents");

        if (ButtonAR is not null)
		{
			ButtonAR.Pressed += () =>
			{
				KitchenDesigner.src.common.utils.ARHelper.SwitchToAR(global.currentScene.GetViewport());
				DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.SwitchToAR);
            };
        }

		if (ButtonVR is not null)
		{
			ButtonVR.Pressed += () =>
			{
				KitchenDesigner.src.common.utils.ARHelper.SwitchToVR(global.currentScene.GetViewport());
				DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.SwitchToVR);
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
