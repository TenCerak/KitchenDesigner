using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using KitchenDesigner;

namespace KitchenDesigner.Features.Tools
{

    public partial class ARToolResource : Resource
    {
        [Export] public string ToolName;
        [Export] public PackedScene ToolScene;
        [Export] public Texture2D Icon;
    }
}
