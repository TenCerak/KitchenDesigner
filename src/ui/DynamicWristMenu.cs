using Godot;
using System.Collections.Generic;
using KitchenDesigner.Interfaces;

public partial class DynamicWristMenu : Control
{
    [Export] public PackedScene[] PageScenes; 

    private HBoxContainer _navBar;
    private Control _contentArea;
    private Node _currentPage;

    public override void _Ready()
    {
        _navBar = GetNode<HBoxContainer>("VBoxContainer/NavBar");
        _contentArea = GetNode<Control>("VBoxContainer/ContentArea");

        BuildMenu();
    }

    private void BuildMenu()
    {
        foreach (Node child in _navBar.GetChildren()) child.QueueFree();

        for (int i = 0; i < PageScenes.Length; i++)
        {
            var instance = PageScenes[i].Instantiate();
            if (instance is IMenuPage page)
            {
                Button navButton = new Button();
                navButton.Text = page.TabName;
                navButton.CustomMinimumSize = new Vector2(100, 60);

                int index = i;
                navButton.Pressed += () => OpenPage(index);
                _navBar.AddChild(navButton);
            }
            instance.QueueFree(); 
        }

        if (PageScenes.Length > 0) OpenPage(0);
    }

    public void OpenPage(int index)
    {
        // Smažeme aktuální stránku v obsahu
        if (_currentPage != null) _currentPage.QueueFree();

        // Instancujeme novou stránku
        _currentPage = PageScenes[index].Instantiate();
        _contentArea.AddChild(_currentPage);

        if (_currentPage is IMenuPage page)
        {
            page.OnPageOpened();
        }
    }
}