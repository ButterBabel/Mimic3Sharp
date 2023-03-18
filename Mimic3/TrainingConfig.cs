namespace Mimic3Sharp.Mimic3;

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
