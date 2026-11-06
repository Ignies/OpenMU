// <copyright file="AddSeason21InitialSkillsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.PlugIns.CharacterCreated;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Classes;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Season21;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the first skills of the nine classes of Season 21 to a created character (S21Emu <c>CharacterSkillInit.xml</c>).
/// The Season 6 classes keep their own plugins. Does nothing on a world without the skills of Season 21.
/// </summary>
[Guid("A6C1E7D2-58B4-4F0A-8E39-6D2C4B7F1A93")]
[PlugIn]
[Display(Name = nameof(PlugInResources.AddSeason21InitialSkillsPlugIn_Name), Description = nameof(PlugInResources.AddSeason21InitialSkillsPlugIn_Description), ResourceType = typeof(PlugInResources))]
public class AddSeason21InitialSkillsPlugIn : ICharacterCreatedPlugIn
{
    /// <inheritdoc />
    public void CharacterCreated(Account account, Character createdCharacter, IContext persistenceContext, GameConfiguration gameConfiguration, ILogger logger)
    {
        if (createdCharacter.CharacterClass is not { } characterClass
            || ClassCodes.GetBaseClass(characterClass.Number) is not { } baseClass
            || baseClass <= BaseClassCode.RageFighter)
        {
            return;
        }

        foreach (var row in Season21SkillCatalog.Data.StartSkills.Where(r => r.Cls == (byte)baseClass))
        {
            if (gameConfiguration.Skills.FirstOrDefault(s => s.Number == row.Skill) is not { } skill
                || !skill.QualifiedCharacters.Contains(characterClass)
                || createdCharacter.LearnedSkills.Any(entry => entry.Skill == skill))
            {
                continue;
            }

            var skillEntry = persistenceContext.CreateNew<SkillEntry>();
            skillEntry.Skill = skill;
            createdCharacter.LearnedSkills.Add(skillEntry);
        }
    }
}
