// <copyright file="IElementalDamageViewPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.Views.World;

using MUnique.OpenMU.GameLogic.Elements;

/// <summary>
/// Interface of a view whose implementation shows the elemental part of a hit, beside the hit itself.
/// </summary>
public interface IElementalDamageViewPlugIn : IViewPlugIn
{
    /// <summary>
    /// Shows the elemental damage a target took.
    /// </summary>
    /// <param name="target">The target which took the damage.</param>
    /// <param name="element">The element of the damage.</param>
    /// <param name="damage">The elemental damage, a part of the hit's total damage.</param>
    ValueTask ShowElementalDamageAsync(IAttackable target, Element element, uint damage);
}
