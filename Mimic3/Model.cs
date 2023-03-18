namespace Mimic3Sharp.Mimic3;

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
