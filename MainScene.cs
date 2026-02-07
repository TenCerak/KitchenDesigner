using Godot;
using KitchenDesigner.Common.Utils;
using System.Linq;

public partial class MainScene : Node3D
{

    private XRInterface _xrInterface;
    private Viewport _viewport;
    public override void _Ready()
    {
        _xrInterface = XRServer.FindInterface("OpenXR");
        _viewport = GetViewport();

        if (_xrInterface != null && _xrInterface.IsInitialized())
        {
            GD.Print("OpenXR initialized successfully");

            DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            
            _xrInterface.XRPlayAreaMode = XRInterface.PlayAreaMode.Stage;

            GetViewport().UseXR = true;
            ARHelper.SwitchToAR(GetViewport());
           
            var ArSceneManager = GetNode<ArSceneManager>("XrOrigin3D/ARSceneManager");

            if(ArSceneManager != null)
            {
                ArSceneManager.ShowSceneAnchors(false);
            }
        }
        else
        {
            GD.Print("OpenXR not initialized, please check if your headset is connected");
        }
    }    
}
