using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace KitchenDesigner
{
    public partial class Global : Node
    {
        public static Global Instance { get; private set; }
        public Node currentScene;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                currentScene = GetTree().CurrentScene;
                GD.Print("Global instance created.");
            }
            else
            {
                GD.Print("Global instance already exists. Freeing duplicate.");
                this.QueueFree();
            }
        }

    }
}
