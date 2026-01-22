using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenDesigner.Interfaces
{
    public interface IMenuPage
    {
        string TabName { get; } 
        void OnPageOpened();
    }
}
