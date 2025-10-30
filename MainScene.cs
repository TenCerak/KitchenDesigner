using Godot;
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
            SwitchToAR();
        }
        else
        {
            // Switch back to VR
            var xrInterface = XRServer.PrimaryInterface;
            if (xrInterface != null)
            {
                xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.Opaque;
            }
            _environment.BackgroundMode = Environment.BGMode.Sky;
            _viewport.TransparentBg = false;
        }

    }

    public bool SwitchToAR()
    {
        var xrInterface = XRServer.PrimaryInterface;
        if (xrInterface != null)
        {
            var modes = xrInterface.GetSupportedEnvironmentBlendModes();
            bool alphaBlendSupported = false;
            bool additiveSupported = false;
            foreach (var mode in modes)
            {
                switch ((XRInterface.EnvironmentBlendModeEnum)mode)
                {
                    case XRInterface.EnvironmentBlendModeEnum.AlphaBlend:
                        alphaBlendSupported = true;
                        break;
                    case XRInterface.EnvironmentBlendModeEnum.Additive:
                        additiveSupported = true;
                        break;
                }
                ;
                GD.Print("Supported Mode: " + mode.ToString());
            }

            //if (alphaBlendSupported)
            //{
            //    xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.AlphaBlend; 
            //    _viewport.TransparentBg = true;
            //}
            //else if (additiveSupported)
            //{
            //    xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.Additive;
            //    _viewport.TransparentBg = false;
            //}
        }
        else
        {
            return false;
        }

        //_environment.BackgroundMode = Environment.BGMode.Color;
        //_environment.BackgroundColor = new Color(0, 0, 0, 0);
        //_environment.AmbientLightSource = Environment.AmbientSource.Color;

        return true;
    }
}
