using Godot;
using KitchenDesigner.src.features.kitchen.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KitchenDesigner.Features.Kitchen.Interfaces
{
    public interface IMaterialTarget
    {
        /// <summary>
        /// Aplikuje materiál na konkrétní zónu (např. přebarví dvířka).
        /// </summary>
        void SetMaterial(Material material, MaterialZone zone);

        /// <summary>
        /// Vrací true, pokud tento objekt má danou zónu (např. "Mám pracovní desku?").
        /// Užitečné pro zvýraznění (Highlight) při najetí myší.
        /// </summary>
        bool HasZone(MaterialZone zone);

        /// <summary>
        /// Získá aktuální materiál
        /// </summary>
        Material GetMaterial(MaterialZone zone);
    }
}
