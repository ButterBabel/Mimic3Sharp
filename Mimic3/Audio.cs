namespace Mimic3Sharp.Mimic3;

public class Audio
{
    public int filter_length { get; set; }
    public int hop_length { get; set; }
    public int win_length { get; set; }
    public int mel_channels { get; set; }
    public int sample_rate { get; set; }
    public int sample_bytes { get; set; }
    public int channels { get; set; }
    public int mel_fmin { get; set; }
    public int mel_fmax { get; set; }
    public int ref_level_db { get; set; }
    public int spec_gain { get; set; }
    public bool signal_norm { get; set; }
    public int min_level_db { get; set; }
    public int max_norm { get; set; }
    public bool clip_norm { get; set; }
    public bool symmetric_norm { get; set; }
    public bool do_dynamic_range_compression { get; set; }
    public bool convert_db_to_amp { get; set; }
    public bool do_trim_silence { get; set; }
    public int trim_silence_db { get; set; }
    public float trim_margin_sec { get; set; }
    public float trim_keep_sec { get; set; }
    public bool scale_mels { get; set; }
}
