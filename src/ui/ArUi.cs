using Godot;
using KitchenDesigner;
using KitchenDesigner.Common.Interfaces;
using System;

public partial class ArUi : Control, IMenuPage
{
    // Called when the node enters the scene tree for the first time.
    [Export] public Button SceneCaptureRequestButton { get; set; }
    [Export] public Button DisplaySceneAnchorsButton { get; set; }
    [Export] public Button DisplaySpatialAnchorsButton { get; set; }

    public string TabName => "AR Menu";

    private Global global;
    private DesignerEvents DesignerEvents;
    public override void _Ready()
    {
        global = GetNode<Global>("/root/Global");
        DesignerEvents = GetNode<DesignerEvents>("/root/DesignerEvents");

        if (SceneCaptureRequestButton is not null)
        {
            SceneCaptureRequestButton.Pressed += () =>
            {
                DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.RequestSceneCapture);
            };
        }

        if (DisplaySceneAnchorsButton is not null)
        {
            DisplaySceneAnchorsButton.Toggled += (bool t) =>
            {
                DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.ShowSceneAnchors, t);
            };
        }
        if (DisplaySpatialAnchorsButton is not null)
        {
            DisplaySpatialAnchorsButton.Toggled += (bool t) =>
            {
                DesignerEvents.Instance.EmitSignal(DesignerEvents.SignalName.ShowSpatialAnchors, t);
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
