using Godot;
using KitchenDesigner.Features.Kitchen.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Components;

public partial class ApplianceBase : Node3D, ISnappable, IKitchenComponent
{
    [Export] public Node3D SnapPointsContainer;

    // Seznam bodů
    public List<SnapPoint> ActiveSnapPoints { get; private set; } = new List<SnapPoint>();

    public Node3D RootNode => this;

    public override async void _Ready()
    {
        CollectSnapPoints();
    }

    private void CollectSnapPoints()
    {
        ActiveSnapPoints.Clear();

        if (SnapPointsContainer is null) return;

        foreach (Node child in SnapPointsContainer.GetChildren())
        {
            if (child is SnapPoint sp)
            {
                sp.ParentObject = this;

                ActiveSnapPoints.Add(sp);
            }
        }
    }


    public void SetHighlight(bool active, bool isDeletePreview = false)
    {
    }

    public void Delete()
    {
        QueueFree();
    }

    public Node3D AsNode()
    {
        return this;
    }
}

