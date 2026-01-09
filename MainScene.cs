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

            // Turn off v-sync!
            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);

            // Change our main viewport to output to the HMD
            GetViewport().UseXR = true;

            Detector detector = GetNode<Detector>("ARToggle");
            detector.IsOn = _xrInterface.EnvironmentBlendMode != XRInterface.EnvironmentBlendModeEnum.Opaque;
            detector.Toggled += Detector_Toggled;
        }
        else
        {
            GD.Print("OpenXR not initialized, please check if your headset is connected");
        }
    }

    private void Detector_Toggled(bool isOn)
    {
        if (isOn)
        {
            ARHelper.SwitchToAR(_viewport);
        }
        else
        {
            
        }

    }

    
}
