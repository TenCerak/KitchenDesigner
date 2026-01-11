using Godot;
using KitchenDesigner;
using KitchenDesigner.Interfaces;
using KitchenDesigner.Tools;
using System.Collections.Generic;

public partial class ToolMenu : Control, IMenuPage
{
    public string TabName => "Nástroje";

    private VBoxContainer _buttonContainer;
    [Export] public ARToolResource[] AvailableTools;

    public override void _Ready()
    {
        _buttonContainer = GetNode<VBoxContainer>("VBoxContainer/ScrollContainer/ButtonContainer");
    }

    public void OnPageOpened()
    {
        RefreshToolList();
    }

    private void RefreshToolList()
    {
        foreach (var toolDef in AvailableTools)
        {
            Button btn = new Button();
            btn.Text = toolDef.ToolName;

            btn.CustomMinimumSize = new Vector2(0, 80); 
            btn.ExpandIcon = true;                     
            btn.IconAlignment = HorizontalAlignment.Left; 
            btn.SizeFlagsVertical = SizeFlags.ShrinkBegin; 

            if (toolDef.Icon != null)
            {
                btn.Icon = toolDef.Icon;
            }

            btn.Pressed += () => {
                // Používáme tvůj ToolManager pro vytvoření instance
                var manager = GetTree().Root.FindChild("ToolManager", true, false) as ToolManager;
                manager?.SpawnTool(toolDef);
            };

            _buttonContainer.AddChild(btn);
        }
    }

    private void CreateToolButton(string name, int index)
    {
        Button btn = new Button();
        btn.Text = name;
        btn.CustomMinimumSize = new Vector2(80, 80);

        btn.Pressed += () => DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.RequestToolChange, index);

        _buttonContainer.AddChild(btn);
    }
}