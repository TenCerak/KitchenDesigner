using Godot;
using KitchenDesigner;
using System;

public partial class ArSceneManager : Node
{
    [Export] public Node SceneManagerNode;
    [Export] public Node SpatialAnchorManagerNode;

    public override void _Ready()
    {
        if (SceneManagerNode == null)
        {
            GD.PrintErr("ARSceneManager: SceneManagerNode není přiřazen!");
            return;
        }

        var global = GetNode<Global>("/root/Global");
        var DesignerEvents = GetNode<DesignerEvents>("/root/DesignerEvents");

        DesignerEvents.Instance.RequestSceneCapture += RequestSceneCapture;

        DesignerEvents.Instance.ShowSceneAnchors += ShowSceneAnchors;
        DesignerEvents.Instance.ShowSpatialAnchors += ShowSpatialAnchors;

        SceneManagerNode.Connect("openxr_fb_scene_data_missing", Callable.From(OnSceneDataMissing));
        SceneManagerNode.Connect("openxr_fb_scene_capture_completed", Callable.From<bool>(OnSceneCaptureCompleted));
    }

    private void Instance_ShowSceneAndSpatialAnchors(bool show)
    {
        ShowSceneAnchors(show);
        ShowSpatialAnchors(show);
    }
    void ShowSceneAnchors(bool show)
    {
        SceneManagerNode.Set("visible", show);
    }

    void ShowSpatialAnchors(bool show)
    {
        SpatialAnchorManagerNode.Set("visible", show);
    }

    private void RequestSceneCapture()
    {
        OnSceneDataMissing();
    }

    private void OnSceneDataMissing()
    {
        GD.Print("ARSceneManager: Data chybí. Spouštím Scene Capture...");
        SceneManagerNode.Call("request_scene_capture");
    }

    private void OnSceneCaptureCompleted(bool success)
    {
        if (!success) return;

        GD.Print("ARSceneManager: Skenování dokončeno.");

        bool anchorsCreated = (bool)SceneManagerNode.Call("are_scene_anchors_created");

        if (anchorsCreated)
        {
            SceneManagerNode.Call("remove_scene_anchors");
        }

        SceneManagerNode.Call("create_scene_anchors");
    }

    public void ForceSceneSync()
    {
        GD.Print("ARSceneManager: Ruční synchronizace...");

        SceneManagerNode.Call("create_scene_anchors");
    }
}