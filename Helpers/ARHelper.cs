using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenDesigner.Helpers
{
    public static class ARHelper
    {
        public static bool SwitchToAR(Viewport viewport)
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
                    //GD.Print("Supported Mode: " + mode.ToString());

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

                    viewport.TransparentBg = true;
                }
                else if (additiveSupported)
                {
                    xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.Additive;
                    viewport.TransparentBg = false;
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

        public static bool SwitchToVR(Viewport viewport)
        {
            var xrInterface = XRServer.PrimaryInterface;
            if (xrInterface != null)
            {
                xrInterface.EnvironmentBlendMode = XRInterface.EnvironmentBlendModeEnum.Opaque;
                viewport.TransparentBg = false;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
