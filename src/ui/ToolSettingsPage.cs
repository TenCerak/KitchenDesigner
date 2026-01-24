using Godot;
using KitchenDesigner.Common.Interfaces;

public partial class ToolSettingsPage : Control, IMenuPage
{
    [Export] public string TabName { get; set; } = "Nastavení nástroje";
    [Export] public Control SettingsContainer;

    public void OnPageOpened()
    {
    }
}