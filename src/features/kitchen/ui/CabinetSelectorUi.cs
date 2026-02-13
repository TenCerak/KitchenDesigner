using Godot;
using KitchenDesigner;
using KitchenDesigner.Features.Kitchen.Resources;
using System;

public partial class CabinetSelectorUi : VBoxContainer
{
    [Export] public Node Container;
    [Export] public KitchenComponentDefinition[] AvailableItems;

    [Signal] public delegate void KitchenComponentDefinitionSelectedEventHandler(KitchenComponentDefinition definition);
    public override void _Ready()
	{
        GenerateButtons();
    }

	public override void _Process(double delta)
	{
	}

    private void GenerateButtons()
    {
        foreach (Node child in Container.GetChildren()) child.QueueFree();

        foreach (var item in AvailableItems)
        {
            Button btn = new Button();

            btn.Text = item.Name;
            if (item.Icon != null)
            {
                btn.Icon = item.Icon;
                btn.ExpandIcon = true; 
            }
            btn.CustomMinimumSize = new Vector2(100, 100);

            btn.Pressed += () => OnItemClicked(item);

            Container.AddChild(btn);
        }
    }

    private void OnItemClicked(KitchenComponentDefinition item)
    {
        EmitSignal(nameof(CabinetSelectorUi.KitchenComponentDefinitionSelected), item);
    }
}
