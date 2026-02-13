using Godot;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Resources
{
    public partial class KitchenComponentDefinition : Resource
    {
        [Export] public string Name;
        [Export] public Texture2D Icon;
        [Export] public PackedScene Prefab;
    }
}
