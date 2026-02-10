using Godot;
using KitchenDesigner.Features.Kitchen.Resources;
using System;

public partial class HandleSettingsUi : Control
{
	[Export] public Container HandlesContainer;
    [Export] public HandleDefinition[] HandleDefinitions;

    [Signal] public delegate void HandleSelectedEventHandler(HandleDefinition handleDef);

    public override void _Ready()
	{
		GenerateButtons();
    }
    void GenerateButtons()
    {
        foreach (Node child in HandlesContainer.GetChildren()) child.QueueFree();
        foreach (var handleDef in HandleDefinitions)
        {
            Button btn = new Button();
            btn.Text = handleDef.Name;
            if (handleDef.Icon != null)
            {
                btn.Icon = handleDef.Icon;
                btn.ExpandIcon = true; 
            }
            btn.CustomMinimumSize = new Vector2(100, 100);
            btn.Pressed += () => OnHandleSelected(handleDef);
            HandlesContainer.AddChild(btn);
        }

    }
    void OnHandleSelected(HandleDefinition handleDef)
    {
        EmitSignal(nameof(HandleSelected), handleDef);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
