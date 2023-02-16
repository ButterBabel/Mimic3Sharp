using System;

public class Mimic3Voice
{
    public Mimic3Voice()
    {
    }
}


public class TrainingConfig
{
    public int seed { get; set; }
    public int epochs { get; set; }
    public float learning_rate { get; set; }
    public float[] betas { get; set; }
    public float eps { get; set; }
    public int batch_size { get; set; }
    public bool fp16_run { get; set; }
    public float lr_decay { get; set; }
    public int segment_size { get; set; }
    public float init_lr_ratio { get; set; }
    public int warmup_epochs { get; set; }
    public int c_mel { get; set; }
    public float c_kl { get; set; }
    public object grad_clip { get; set; }
    public object min_seq_length { get; set; }
    public int max_seq_length { get; set; }
    public object min_spec_length { get; set; }
    public object max_spec_length { get; set; }
    public object min_speaker_utterances { get; set; }
    public int last_epoch { get; set; }
    public int global_step { get; set; }
    public object best_loss { get; set; }
    public Audio audio { get; set; }
    public Model model { get; set; }
    public Phonemes phonemes { get; set; }
    public Text_Aligner text_aligner { get; set; }
    public string text_language { get; set; }
    public string phonemizer { get; set; }
    public Dataset[] datasets { get; set; }
    public Inference inference { get; set; }
    public int version { get; set; }
    public string git_commit { get; set; }
}

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

public class Model
{
    public int num_symbols { get; set; }
    public int n_speakers { get; set; }
    public int inter_channels { get; set; }
    public int hidden_channels { get; set; }
    public int filter_channels { get; set; }
    public int n_heads { get; set; }
    public int n_layers { get; set; }
    public int kernel_size { get; set; }
    public float p_dropout { get; set; }
    public string resblock { get; set; }
    public int[] resblock_kernel_sizes { get; set; }
    public int[][] resblock_dilation_sizes { get; set; }
    public int[] upsample_rates { get; set; }
    public int upsample_initial_channel { get; set; }
    public int[] upsample_kernel_sizes { get; set; }
    public int n_layers_q { get; set; }
    public bool use_spectral_norm { get; set; }
    public int gin_channels { get; set; }
    public bool use_sdp { get; set; }
}

public class Phonemes
{
    public string phoneme_separator { get; set; }
    public string word_separator { get; set; }
    public object phoneme_to_id { get; set; }
    public string pad { get; set; }
    public string bos { get; set; }
    public string eos { get; set; }
    public string blank { get; set; }
    public string blank_word { get; set; }
    public string blank_between { get; set; }
    public bool blank_at_start { get; set; }
    public bool blank_at_end { get; set; }
    public bool simple_punctuation { get; set; }
    public object punctuation_map { get; set; }
    public string[] separate { get; set; }
    public bool separate_graphemes { get; set; }
    public bool separate_tones { get; set; }
    public bool tone_before { get; set; }
    public object phoneme_map { get; set; }
    public bool auto_bos_eos { get; set; }
    public string minor_break { get; set; }
    public object major_break { get; set; }
    public bool break_phonemes_into_graphemes { get; set; }
}

public class Text_Aligner
{
    public string aligner { get; set; }
    public string casing { get; set; }
}

public class Inference
{
    public float length_scale { get; set; }
    public float noise_scale { get; set; }
    public float noise_w { get; set; }
    public string auto_append_text { get; set; }
}

public class Dataset
{
    public string name { get; set; }
    public string metadata_format { get; set; }
    public bool multispeaker { get; set; }
    public object text_language { get; set; }
    public string audio_dir { get; set; }
    public string cache_dir { get; set; }
}
