using Godot;
using KitchenDesigner.Features.Kitchen.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenDesigner
{
    public partial class DesignerEvents : Node
    {
        [Signal] public delegate void RequestToolChangeEventHandler(int index);
        [Signal] public delegate void SwitchToAREventHandler();
        [Signal] public delegate void SwitchToVREventHandler();


        [Signal] public delegate void RequestSceneCaptureEventHandler();
        [Signal] public delegate void ShowSceneAnchorsEventHandler(bool show);
        [Signal] public delegate void ShowSpatialAnchorsEventHandler(bool show);

        [Signal] public delegate void RequestDominantHandChangeEventHandler(bool rightHand);

        [Signal] public delegate void CabinetDefinitionSelectedEventHandler(CabinetDefinition definition);

        public static DesignerEvents Instance { get; private set; }
        public override void _Ready()
        {
            Instance = this;
        }
    }
}
