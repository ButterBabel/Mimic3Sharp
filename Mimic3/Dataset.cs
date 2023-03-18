namespace Mimic3Sharp.Mimic3;

public class Dataset
{
    public string name { get; set; }
    public string metadata_format { get; set; }
    public bool multispeaker { get; set; }
    public object text_language { get; set; }
    public string audio_dir { get; set; }
    public string cache_dir { get; set; }
}
