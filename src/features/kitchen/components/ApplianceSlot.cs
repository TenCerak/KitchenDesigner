using Godot;

namespace KitchenDesigner.Features.Kitchen.Components
{
    public enum ApplianceType
    {
        Oven,       // Trouba
        Microwave,  // Mikrovlnka
        Fridge,     // Vestavná lednice
        Dishwasher  // Myčka
    }

    public partial class ApplianceSlot : Marker3D
    {
        [Export] public ApplianceType AcceptedType;
        [Export] public SnapPoint SnapPoint;
        public ApplianceBase InstalledAppliance { get; private set; }


        public bool IsOccupied => InstalledAppliance is not null;

        public void Occupy(ApplianceBase appliance)
        {
            InstalledAppliance = appliance;
        }

        public void Vacate()
        {
            InstalledAppliance = null;
        }
    }
}