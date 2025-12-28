using System.ComponentModel;
using static PKHeX.Core.LanguageID;

namespace PKHeX.Core;

/// <summary>
/// Settings for Chinese language support features.
/// </summary>
[TypeConverter(typeof(ExpandableObjectConverter))]
public sealed class ChineseSupportSettings
{
    [LocalizedDescription("Enable Chinese language support features. When enabled, Chinese characters in nicknames and trainer names will be accepted even for non-Chinese version Pokémon, Chinese Pokémon names will be used from resources, and Gen 4 strings encoded with Korean values will be displayed as Chinese characters (for Chinese fan-translated ROMs that use Korean encoding with Chinese font data).")]
    public bool Enabled { get; set; } = true;
}

