using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;


namespace KitchenDesigner.Interfaces
{
    public interface IARTool
    {
        string ToolName { get; }
        void Initialize(XRController3D leftController, XRController3D rightController, Marker3D leftTip, Marker3D rightTip);
        void Release();
        void Reattach(XRController3D leftController, XRController3D rightController, Marker3D leftTip, Marker3D rightTip);
        void Activate();   // Zavolá se při přepnutí na tento nástroj
        void Deactivate(); // Zavolá se při schování nástroje
        void HandleInput(bool isPressed, bool isJustPressed); // Reakce na Trigger
        void UpdateTool(double delta, Vector3 currentPos);    // Logika v každém snímku
        void SetHighlight(bool enabled);
    }
}
