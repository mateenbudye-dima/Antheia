using System;
using System.Collections.Generic;
using System.Text;

namespace Antheia.Domain.Enums
{
    public enum IngredientType:byte
    {
        Unknown = 0,
        ActiveIngredient = 1,
        Solvent=2,
        EmulsifierBlend=3,
        Dispersant=4,
        WettingAgent=5,
        OtherAdditives = 100
    }
}
