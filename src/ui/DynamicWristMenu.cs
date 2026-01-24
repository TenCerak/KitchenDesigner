using Godot;
using KitchenDesigner.Common.Interfaces;
using System.Collections.Generic;

public partial class DynamicWristMenu : Control
{
    [Export] public PackedScene[] PageScenes; 

    public HBoxContainer _navBar;
    private Control _contentArea;
    private Node _currentPage;
    private Control _currentToolSettingsUI;

    public override void _Ready()
    {
        _navBar = GetNode<HBoxContainer>("VBoxContainer/NavBar");
        _contentArea = GetNode<Control>("VBoxContainer/ContentArea");

        BuildMenu();
    }
    public void BuildMenu()
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

    public void SetToolSettingsUI(Control toolUI)
    {
        if (_currentToolSettingsUI != null)
        {
            _currentToolSettingsUI.GetParent()?.RemoveChild(_currentToolSettingsUI);
            _currentToolSettingsUI.QueueFree();
        }

        _currentToolSettingsUI = toolUI;

        TryEmbedUiIntoCurrentPage();
    }
    public void ClearToolSettingsUI()
    {
        if (_currentToolSettingsUI != null)
        {
            _currentToolSettingsUI.GetParent()?.RemoveChild(_currentToolSettingsUI);
            _currentToolSettingsUI.QueueFree();
            _currentToolSettingsUI = null;
        }
    }




    public void OpenPage(int index)
    {
        if (_currentToolSettingsUI != null && _currentToolSettingsUI.GetParent() != null)
        {
            _currentToolSettingsUI.GetParent().RemoveChild(_currentToolSettingsUI);
        }

        if (_currentPage != null) _currentPage.QueueFree();

        _currentPage = PageScenes[index].Instantiate();
        _contentArea.AddChild(_currentPage);

        TryEmbedUiIntoCurrentPage();

        if (_currentPage is IMenuPage page)
        {
            page.OnPageOpened();
        }
    }

    private void TryEmbedUiIntoCurrentPage()
    {
        if (_currentToolSettingsUI != null && _currentPage is ToolSettingsPage settingsPage)
        {
            if (settingsPage.SettingsContainer != null)
            {
                settingsPage.SettingsContainer.AddChild(_currentToolSettingsUI);
            }
        }
    }
}