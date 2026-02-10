using Godot;
using KitchenDesigner.Features.Kitchen.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Resources
{
    public partial class HandleDefinition : Resource
    {
        [Export] public string Name;
        [Export] public Texture2D Icon;
        [Export] public PackedScene Prefab;
    }
}
