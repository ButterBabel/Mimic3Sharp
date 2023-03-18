namespace Mimic3Sharp.Mimic3;

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
