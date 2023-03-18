using System.Text.Json.Serialization;

namespace Mimic3Sharp.Vosk;

public class VoskWordMatch
{
    [JsonPropertyName("conf")]
    public float Confidence { get; set; }
    [JsonPropertyName("end")]
    public float End { get; set; }
    [JsonPropertyName("start")]
    public float Start { get; set; }
    [JsonPropertyName("word")]
    public string Word { get; set; }
}
