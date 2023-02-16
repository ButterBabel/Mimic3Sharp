using System.Text.RegularExpressions;

namespace Mimic3Sharp;

internal static partial class Regexes
{
    public static readonly Regex VoicePathRegex = VoicedLineFileRegexGen();

    [GeneratedRegex("(?:(imperial_high|imperial_low|vlandian|sturgian|khuzait|aserai|battanian|forest_bandits|sea_raiders|mountain_bandits|desert_bandits|steppe_bandits|looters)_)?(?:(male|female)_)?(?:(curt|earnest|ironic|softspoken|generic)_)?(?:(.+?)\\.ogg)", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-US")]
    private static partial Regex VoicedLineFileRegexGen();

    public static readonly Regex LocalizedTextRegex = LocalizedTextRegexGen();

    [GeneratedRegex("^{=([\\w\\d]+|!)}(.+)$", RegexOptions.Compiled)]
    private static partial Regex LocalizedTextRegexGen();

    public static readonly Regex NormalizeTextRegex = NormalizeTextRegexGen();

    [GeneratedRegex("{[^}]+}[\\W]*", RegexOptions.Compiled)]
    private static partial Regex NormalizeTextRegexGen();
}
