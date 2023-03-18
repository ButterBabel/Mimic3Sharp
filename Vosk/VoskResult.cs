using System.Text.Json.Serialization;

namespace Mimic3Sharp.Vosk;

public class VoskResult
{
    [JsonPropertyName("result")]
    public VoskWordMatch[] Result { get; set; }
    [JsonPropertyName("text")]
    public string Text { get; set; }
}
