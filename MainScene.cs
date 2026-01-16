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

        }
        else
        {
            GD.Print("OpenXR not initialized, please check if your headset is connected");
        }

        var xrInterface = XRServer.FindInterface("OpenXR") as OpenXRInterface;

        if (xrInterface != null && xrInterface.IsInitialized())
        {
            GD.Print("✅ OpenXR inicializováno úspěšně.");

            // 2. Kontrola, zda je Meta Quest aktivním runtime
            string runtimeName = xrInterface.GetName();
            GD.Print($"ℹ️ Aktivní Runtime: {runtimeName}");

            if (!runtimeName.ToLower().Contains("oculus") && !runtimeName.ToLower().Contains("meta"))
            {
                GD.PrintErr("⚠️ VAROVÁNÍ: Runtime není Meta/Oculus. Passthrough pravděpodobně nebude fungovat!");
            }

            // 3. Kontrola podpory Passthrough
            bool isPassthroughSupported = xrInterface.IsPassthroughSupported();
            GD.Print($"✨ Podpora Passthrough: {isPassthroughSupported}");

            if (isPassthroughSupported)
            {
                // Pokus o zapnutí Passthrough (Alpha Blend mode)
                bool success = xrInterface.SetEnvironmentBlendMode(XRInterface.EnvironmentBlendModeEnum.AlphaBlend);
                GD.Print(success ? "🚀 Passthrough úspěšně aktivován (Alpha Blend)."
                               : "❌ Selhalo nastavení Alpha Blend režimu.");
            }
            else
            {
                GD.PrintErr("❌ Passthrough NENÍ podporován. Zkontrolujte 'Meta Quest Link' nastavení (Beta tab).");
            }
        }
        else
        {
            GD.PrintErr("❌ OpenXR rozhraní nebylo nalezeno nebo inicializováno. Máte zapnutý XR v Project Settings?");
        }
    }    
}
