using Godot;
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
        public static DesignerEvents Instance { get; private set; }
        public override void _Ready()
        {
            Instance = this;
        }
    }
}
