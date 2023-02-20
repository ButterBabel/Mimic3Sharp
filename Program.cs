// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.PhoneticMatching;
using Mimic3Sharp;
using Mimic3Sharp.eSpeak;
using NAudio.Wave;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Tensorflow;
using Tensorflow.NumPy;
using static Mimic3Sharp.Regexes;

Console.WriteLine("Hello, World!");

RunVoskDemo(@"C:\Users\Zebedee\Downloads\vosk-model-small-en-us-0.15");
;

//LarynxTrainDatasetToSubfolders(@"S:\Work\larynx2_train\training_vctk_with_bannerlord");
//;

//VctkToLjspeechFormat(@"S:\Work\larynx2_train\vctk");
//;

//BannerlordToLjspeechFormat(@"S:\Games\Mount & Blade II Bannerlord _ beta 1.1.0\Modules\SandBox\ModuleData\Languages\VoicedLines\EN\PC");

LoadOnnx(@"S:\Work\mimic3\en_US vctk_low\");

void RunVoskDemo(string model_name)
{
    VoskDemo.Main(model_name);
}

void LoadOnnx(string model_dir)
{
    Dictionary<string, int> phonemes = File.ReadLines(Path.Join(model_dir, "phonemes.txt"))
        .Select(line => line.Split(' '))
        .Select(arr => new
        {
            id = int.Parse(arr[0]),
            phoneme = arr[1]
        })
        .ToDictionary(a => a.phoneme, a => a.id);

    using var speakerReader = GetLjspeechReader(Path.Join(model_dir, "speaker_map.csv"));
    Dictionary<int, string> speakers = speakerReader
        .GetRecords(new
        {
            id = 0,
            model = "",
            speaker = ""
        })
        .ToDictionary(a => a.id, a => a.speaker);

    JsonNode config;
    {
        using var ifs = File.OpenRead(Path.Join(model_dir, "config.json"));
        config = JsonNode.Parse(ifs);
    }

    double noise_scale, length_scale, noise_w;
    {
        var inference = config["inference"];
        noise_scale = inference["noise_scale"].GetValue<double>();
        length_scale = inference["length_scale"].GetValue<double>();
        noise_w = inference["noise_w"].GetValue<double>();
    }

    //eSpeakVoice.Initialize(@"C:\Program Files\eSpeak NG\libespeak-ng.dll");
    //eSpeakVoice.Speak("I think it's very nice out today.");

    //float max_wav_value;
    //{
    //    var caudio = config["audio"];
    //    max_wav_value = caudio["max_norm"].GetValue<float>();
    //}

    long speaker_id;
    {
        speaker_id = 6;
    }

    //const string line = """But this cousin... I would not marry that man! He was a boor, a drunk - never there was a night that he did not reek of wine, never a morning that he did not reek of vomit! But a cataphract's daughter is not some chit you can marry against her will. I took a horse from my father's estate - my horse, legally - his old sword, and rode off.""";
    const string line = "I think it's very nice out today.";

    var pronounciation = Microsoft.PhoneticMatching.EnPronouncer.Instance.Pronounce(line);
    foreach(var phone in pronounciation.Phones)
    {
        var xphone = new XPhone(phone.Type, phone.Phonation, phone.Place, phone.Manner, phone.Height, phone.Backness, phone.Roundedness, phone.IsRhotic, phone.IsSyllabic);
        Console.WriteLine(xphone);
    }
    phonemes.Add("r", phonemes["ɹ"]);

    

    var phlist = new List<int>();
    phlist.Add(phonemes["^"]);
    phlist.Add(phonemes["#"]);
    string sh = pronounciation.Ipa;
    while(sh.Length > 0)
    {
        if (sh[0] == '\u032F')
        {
            phlist.Add(phonemes["#"]);
            //phlist.Add(phonemes["·"]);
            //phlist.Add(phonemes["·"]);
            //phlist.Add(phonemes["·"]);
            //phlist.Add(phonemes["·"]);
            sh = sh[1..];
            continue;
        }

        var match = phonemes.OrderByDescending(ph => ph.Key.Length).Where(ph => sh.StartsWith(ph.Key, StringComparison.Ordinal)).FirstOrDefault();
        if(match is { Key: null })
        {
            break;
        }

        sh = sh[match.Key.Length..];
        phlist.Add(match.Value);
        phlist.Add(phonemes["_"]);
    }
    phlist.Add(phonemes["#"]);
    phlist.Add(phonemes["$"]);
    var text_phoneme_ids = pronounciation.Ipa.Split('\u032F');// EnumerateRunes().Select(rune => phonemes[rune.ToString()]).Cast<long>().ToArray();
    var x = phlist.Select(i => (long)i).ToArray();
    //var x = phonemes.Values.ToArray();
    ;

    var text_array = np.expand_dims(np.array(x, dtype: np.int64), 0);//
    var text_lengths_array = np.array(new[] { text_array.shape[1] }, dtype: np.int64);//
    var scales_array = np.array(new[] { noise_scale, length_scale, noise_w, }, dtype: np.float32);//
    var sid = np.array(new[] { speaker_id }, dtype: np.int64);

    // create input tensor (nlp example)
    //var inputTensor = new DenseTensor<string>(new string[] { review }, new int[] { 1, 1 });

    // Create input data for session.
    var input = new List<NamedOnnxValue> {
        NamedOnnxValue.CreateFromTensor<long>("input", text_array.ToMultiDimArray<long>().ToTensor<long>()),
        NamedOnnxValue.CreateFromTensor<long>("input_lengths", text_lengths_array.ToArray<long>().ToTensor()),
        NamedOnnxValue.CreateFromTensor<float>("scales", scales_array.ToArray<float>().ToTensor()),
        NamedOnnxValue.CreateFromTensor<long>("sid", sid.ToArray<long>().ToTensor())
    };

    // Create an InferenceSession from the Model Path.
    float[] audio_buffer;
    {
        using var model = new InferenceSession(Path.Join(model_dir, "generator.onnx"));

        // Run session and send input data in to get inference output. Call ToList then get the Last item. Then use the AsEnumerable extension method to return the Value result as an Enumerable of NamedOnnxValue.
        using var outputs = model.Run(input);

        var output = outputs.Last();
        var onnxTensor = output.AsTensor<float>().ToDenseTensor();
        var span = onnxTensor.Buffer.Span;
        audio_buffer = span.ToArray();
    }

    var tf = new tensorflow();
    var audio = np.squeeze(new NDArray(audio_buffer));

    //audio_float_to_int16
    {
        const float max_wav_value = 32767.0f;
        var absaudio = tf.abs(audio);
        var amaxaudio = tf.reduce_max(absaudio);
        var audio_norm = audio * (max_wav_value / tf.maximum(0.01f, amaxaudio));
        audio_norm = clip_ops.clip_by_value(audio_norm, -max_wav_value, max_wav_value);
        audio_norm = tf.cast(audio_norm, TF_DataType.TF_INT16);

        audio = audio_norm.numpy().astype(np.int16);
    }

    {
        var audioBytes = audio.ToByteArray();
        using var afs = File.Create("test.wav");
        WaveFormat waveFormat = new WaveFormat(22050, 16, 1);
        using var wfw = new WaveFileWriter(afs, waveFormat);
        wfw.Write(audioBytes);
        wfw.Flush();
    }

    // From the Enumerable output create the inferenceResult by getting the First value and using the AsDictionary extension method of the NamedOnnxValue.
    //var inferenceResult = output.First().AsDictionary<string, float>();

    // Return the inference result as json.
    //return inferenceResult;
}

void BannerlordToLjspeechFormat(string dir)
{
    const string voice_strings = @"C:\Games\Steam\steamapps\common\Mount & Blade II Bannerlord\Modules\SandBox\ModuleData\voice_strings.xml";
    using var fs = File.OpenRead(voice_strings);
    var doc = XDocument.Load(fs);

    var lookup = doc
        .Descendants("string")
        .Attributes("text")
        .Select(attr => attr.Value)
        .Select(text => LocalizedTextRegex.Match(text))
        .Where(m => m.Success)
        .Select(m => new
        {
            id = m.Groups[1].Value,
            text = m.Groups[2].Value
        })
        .ToLookup(a => a.id, a => a.text);

    var voiceLines = Directory.EnumerateFiles(dir, "*.ogg");
    var linesWithText = voiceLines
        .Select(path => new
        {
            path,
            id = Path.GetFileNameWithoutExtension(path).Split('_').Last()
        })
        .Where(a => lookup.Contains(a.id))
        .Select(a => new
        {
            a.path,
            text = lookup[a.id].First()
        });

    /*
        Metadata is provided in transcripts.csv. This file consists of one record per line, delimited by the pipe character (0x7c). The fields are:

        ID: this is the name of the corresponding .wav file
        Transcription: words spoken by the reader (UTF-8)
        Normalized Transcription: transcription with numbers, ordinals, and monetary units expanded into full words (UTF-8).

        Each audio file is a single-channel 16-bit PCM WAV with a sample rate of 22050 Hz. 
    */

    using var csv = GetLjspeechWriter(Path.Join(dir, "metadata.csv"));
    //csv.WriteRecords(linesWithText.Select(a => new {
    //    id = Path.GetFileNameWithoutExtension(a.path),
    //    transcription = a.text,
    //    normalized = a.text
    //}));
    csv.WriteRecords(linesWithText.Select(a => new
    {
        id = Path.GetFileNameWithoutExtension(a.path),
        speaker = VoicePathRegex.Match(Path.GetFileName(a.path)) switch
        {
            Match m when m is { Success: false } => throw new InvalidOperationException("Speaker identification: not identified from filename"),
            Match m when m.Groups is [_, var accentGroup, var genderGroup, var personaGroup, _] g => string.Join('_', accentGroup.Value, genderGroup.Value, personaGroup.Value),
            _ => throw new InvalidOperationException("Speaker identification: Expected groups not matched")
        },
        text = NormalizeTextRegex.Replace(a.text, "")
    }));
}

void VctkToLjspeechFormat(string dir)
{
    var txtFiles = Directory.EnumerateFiles(dir, "*.txt")
        .Select(path => new
        {
            path,
            filename = Path.GetFileNameWithoutExtension(path),
        })
        .Select(a => new
        {
            a.path,
            a.filename,
            speaker = a.filename[..a.filename.IndexOf('_')]
        });

    var wavFiles = Directory.EnumerateFiles(dir, "*.wav")
        .Select(path => new
        {
            path,
            filename = Path.GetFileNameWithoutExtension(path),
        })
        .Select(a => new
        {
            a.path,
            a.filename,
            speaker = a.filename[..a.filename.IndexOf('_')],
            entry = a.filename[..a.filename.LastIndexOf('_')]
        });

    var all = txtFiles
        .GroupJoin(wavFiles, txt => txt.filename, wav => wav.entry,
            (txt, wavs) => new
            {
                txt,
                wavs
            });

    using var csv = GetLjspeechWriter(Path.Join(dir, "metadata.csv"));
    csv.WriteRecords(all.SelectMany(a => a.wavs, (a, wav) => new
    {
        id = wav.filename,
        wav.speaker,
        text = File.ReadAllText(a.txt.path).Trim()
    }));
}

void LarynxTrainDatasetToSubfolders(string dir)
{
    var datasetPath = Path.Join(dir, "dataset.jsonl");
    var datasetBakPath = Path.Join(dir, "dataset.jsonl.bak");
    File.Move(datasetPath, datasetBakPath);
    var datasetLines = File.ReadLines(datasetBakPath)
        .Select(line => System.Text.Json.Nodes.JsonNode.Parse(line));

    using var writer = File.CreateText(datasetPath);

    var cacheFolder = Path.Join(dir, "cache", "22050");
    foreach (var line in datasetLines)
    {
        var speaker = line["speaker"].GetValue<string>();
        var normPath = line["audio_norm_path"].GetValue<string>();
        var specPath = line["audio_spec_path"].GetValue<string>();
        Directory.CreateDirectory(Path.Join(cacheFolder, speaker));
        line["audio_norm_path"] = Path.Join(Path.GetDirectoryName(normPath), speaker, Path.GetFileName(normPath));
        line["audio_spec_path"] = Path.Join(Path.GetDirectoryName(specPath), speaker, Path.GetFileName(specPath));

        var realNormPath = FixupPath(normPath);
        var realSpecPath = FixupPath(specPath);
        var realNormPathSpeaker = FixupPath(line["audio_norm_path"].GetValue<string>());
        var realSpecPathSpeaker = FixupPath(line["audio_spec_path"].GetValue<string>());
        File.Move(realNormPath, realNormPathSpeaker);
        File.Move(realSpecPath, realSpecPathSpeaker);
        writer.WriteLine(line.ToJsonString());
    }

    string FixupPath(string path) => Path.Join(dir, Path.GetRelativePath("../../training_vctk_with_bannerlord", path));
}

CsvConfiguration GetLjspeechCsvConfiguration()
{
    var config = new CsvConfiguration(CultureInfo.InvariantCulture);
    config.Delimiter = "|";
    config.HasHeaderRecord = false;
    return config;
}

CsvWriter GetLjspeechWriter(string path)
{
    var config = GetLjspeechCsvConfiguration();
    var ofs = new StreamWriter(path);
    var csv = new CsvWriter(ofs, config);
    return csv;
}

CsvReader GetLjspeechReader(string path)
{
    var config = GetLjspeechCsvConfiguration();
    var ofs = new StreamReader(path);
    var csv = new CsvReader(ofs, config);
    return csv;
}

public record XPhone(PhoneType Type, Phonation Phonation, PlaceOfArticulation? Place, MannerOfArticulation? Manner, VowelHeight? Height, VowelBackness? Backness, VowelRoundedness? Roundedness, bool? IsRhotic, bool IsSyllabic);

