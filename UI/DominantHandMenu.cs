using Godot;
using KitchenDesigner;
using KitchenDesigner.Interfaces;
using System;

public partial class DominantHandMenu : Control, IMenuPage
{
    public string TabName => "Dominant Hand Menu";

    [Export]public Button LeftHandButton;
    [Export] public Button RightHandButton;

    private DesignerEvents designerEvents;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        LeftHandButton.Pressed += () =>
        {
            GD.Print("Left hand selected as dominant hand.");
            DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.RequestDominantHandChange, false);
        };
        RightHandButton.Pressed += () =>
        {
            GD.Print("Right hand selected as dominant hand.");
            DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.RequestDominantHandChange, true);
        };

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void OnPageOpened()
    {
    }
}
