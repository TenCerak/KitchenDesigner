using Godot;
using KitchenDesigner.Features.Kitchen.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Resources
{
    public partial class CabinetDefinition : KitchenComponentDefinition
    {
        [Export] public CabinetData DefaultData;
    }
}
