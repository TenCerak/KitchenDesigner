using Godot;
using KitchenDesigner.Helpers;
using System.Linq;

public partial class MainScene : Node3D
{

    private XRInterface _xrInterface;
    private WorldEnvironment _worldEnvironment;
    private Viewport _viewport;
    private Environment _environment;
    public override void _Ready()
    {
        _xrInterface = XRServer.FindInterface("OpenXR");
        _worldEnvironment = GetNode<WorldEnvironment>("WorldEnvironment");
        _viewport = GetViewport();
        _environment = GetNode<WorldEnvironment>("WorldEnvironment").Environment;

        if (_xrInterface != null && _xrInterface.IsInitialized())
        {
            GD.Print("OpenXR initialized successfully");

            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);

            _xrInterface.XRPlayAreaMode = XRInterface.PlayAreaMode.Stage;

            GetViewport().UseXR = true;
            ARHelper.SwitchToAR(GetViewport());
        }
        else
        {
            GD.Print("OpenXR not initialized, please check if your headset is connected");
        }
    }    
}
