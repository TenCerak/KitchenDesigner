using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using KitchenDesigner;

namespace KitchenDesigner.Common.Interfaces
{
    public interface IARTool
    {
        string ToolName { get; }
        bool IsActive { get; set; }
        void Initialize(XrHandManager handManager);
        void Reattach(XrHandManager handManager);
        void Activate();   // Zavolá se při přepnutí na tento nástroj
        void Deactivate(); // Zavolá se při schování nástroje
        void UpdateTool(double delta, Vector3 currentPos);  
        void ButtonPressed(string actionName); 
        void ButtonReleased(string actionName);
        void SetHighlight(bool enabled);
    }
}
