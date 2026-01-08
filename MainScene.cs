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
                switch ((int)mode)
                {
                    case 2:
                        alphaBlendSupported = true;
                        break;
                    case 1:
                        additiveSupported = true;
                        break;
                }
                ;
                GD.Print("Supported Mode: " + mode.ToString());

                /*
                  EnvironmentBlendMode XR_ENV_BLEND_MODE_OPAQUE = 0

                    Opaque blend mode. This is typically used for VR devices.

                    EnvironmentBlendMode XR_ENV_BLEND_MODE_ADDITIVE = 1

                    Additive blend mode. This is typically used for AR devices or VR devices with passthrough.

                    EnvironmentBlendMode XR_ENV_BLEND_MODE_ALPHA_BLEND = 2

                    Alpha blend mode. This is typically used for AR or VR devices with passthrough capabilities.
                    The alpha channel controls how much of the passthrough is visible. Alpha of 0.0 means the passthrough is visible and this pixel works in ADDITIVE mode.
                    Alpha of 1.0 means that the passthrough is not visible and this pixel works in OPAQUE mode.
                 */
            }

            if (alphaBlendSupported)
            {
                xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.AlphaBlend;
                _viewport.TransparentBg = true;
            }
            else if (additiveSupported)
            {
                xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.Additive;
                _viewport.TransparentBg = false;
            }
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
