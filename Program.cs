// See https://aka.ms/new-console-template for more information
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Mimic3Sharp.eSpeak;
using Mimic3Sharp.Rhubarb;
using Mimic3Sharp.Vosk;
using NAudio.Wave;
using System.Buffers;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;
using Tensorflow;
using Tensorflow.NumPy;
using static Mimic3Sharp.Regexes;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Hello, World!");

//ForceAlignSTT(@"C:\Users\Zebedee\Downloads\model.tflite");

//LarynxTrainDatasetToSubfolders(@"S:\Work\larynx2_train\training_vctk_with_bannerlord");
//;

//VctkToLjspeechFormat(@"S:\Work\larynx2_train\vctk");
//;

//BannerlordToLjspeechFormat(@"S:\Games\Mount & Blade II Bannerlord _ beta 1.1.0\Modules\SandBox\ModuleData\Languages\VoicedLines\EN\PC");

LoadOnnx(@"S:\Work\mimic3\en_US vctk_low\", out var duration);
;

RunVoskDemo(@"C:\Users\Zebedee\Downloads\vosk-model-small-en-us-0.15",
    "The lords and the merchants, they sit in their lofty towers making grand decisions, but they don't see into the back alleys to know what plots are going on. They don't know where to find the debtor who won't pay his debt. They don't know where to find the wagging tongues starting rumors. So that's where I come in.",
    duration
);
;

void RunVoskDemo(string model_name, string transcript, TimeSpan duration)
{
    var stripper = new Regex(@"([\w'-]+)", RegexOptions.Compiled);
    var grammar = $"[{string.Join(", ", stripper.Matches(transcript).Select(x => $"\"{x}\""))}]".ToLowerInvariant();
    var vmodel = new VoskModel(model_name);

    if (vmodel.Recognize(File.OpenRead("test.wav"), 22050, grammar) is not VoskResult voskResult)
    {
        Console.WriteLine("FATAL: Vosk did not recognize any text");
        return;
    }
    Console.WriteLine("Vosk detected text: {0}", voskResult.Text);

    var arpabetToIpa = File.ReadLines(@"C:\Users\Zebedee\Downloads\arpabet-to-ipa.csv")
        .Select(l => l.Split(',', StringSplitOptions.RemoveEmptyEntries))
        .Select(a => new
        {
            arpabet = a[0].ToLowerInvariant(),
            ipa = a[1]
        })
        .ToDictionary(a => a.arpabet, a => a.ipa);

    var arpabetToViseme = File.ReadLines(@"C:\Users\Zebedee\Downloads\arpabet-to-viseme.csv")
        .Where(l => !l.StartsWith('#'))
        .Select(l => l.Split(',', StringSplitOptions.RemoveEmptyEntries))
        .Select(a => new
        {
            arpabet = a[0],
            viseme = a[1]
        })
        .ToDictionary(a => a.arpabet, a => a.viseme);

    var ipaToVisemeMpb = arpabetToViseme
        .ToDictionary(a => arpabetToIpa[a.Key], a => Enum.Parse<Mimic3Sharp.Rhubarb.Visemes.PrestonBlair>(a.Value));

    var ipaToVisemesAlphabetic = ipaToVisemeMpb
        .ToDictionary(a => a.Key, a => (Mimic3Sharp.Rhubarb.Visemes.Alphabetic)a.Value);

    //eSpeakVoice.Initialize(@"C:\Program Files\eSpeak NG\libespeak-ng.dll");
    //eSpeakVoice.SetVoiceByName("en-us");

    var cueList = new List<rhubarbResultMouthCue>();
    var rhubarb = new rhubarbResult()
    {
        metadata = new()
        {
            soundFile = Path.GetFullPath("test.wav"),
            duration = (decimal)duration.TotalSeconds
        },
        mouthCues = cueList
    };

    foreach (var wordMatch in voskResult.Result)
    {
        List<Mimic3Sharp.Rhubarb.Visemes.Alphabetic> vislist = new();

        var phones = eSpeakVoice.TextToPhonemes(wordMatch.Word).Single();
        while (phones.Length > 0)
        {
            var match = ipaToVisemesAlphabetic.Where(kvp => phones.StartsWith(kvp.Key)).FirstOrDefault();
            if (match is { Key: null })
            {
                if (phones[0] == 'ˈ' || phones[0] == 'ː')
                {
                    vislist.Add(Mimic3Sharp.Rhubarb.Visemes.Alphabetic.X);
                    phones = phones[1..];
                    continue;
                }

                Console.WriteLine("WARNING: unknown viseme for phone `{0}`", phones[0]);
                phones = phones[1..];
                continue;
            }

            vislist.Add(match.Value);
            phones = phones[match.Key.Length..];
        }

        Decimal start = (decimal)wordMatch.Start;
        Decimal end = (decimal)wordMatch.End;
        Decimal phDuration = (end - start) / vislist.Count;
        for (int i = 0; i < vislist.Count; i++)
        {
            var vis = vislist[i];
            cueList.Add(new()
            {
                start = start + (phDuration * i),
                end = i + 1 == vislist.Count ? end : start + (phDuration * (i + 1)),
                Value = $"{vis}"
            });

        }
    }

    const string rhubarbFilename = "test.xml";

    var seri = new XmlSerializer(typeof(rhubarbResult));
    using var xfs = File.Create(rhubarbFilename);
    seri.Serialize(xfs, rhubarb);

    Console.WriteLine("Wrote Rhubarb file {0} containing {1} cue records", rhubarbFilename, cueList.Count);
}

//void ForceAlignSTT(string modelPath) {
//    var stt = new Mimic3Sharp.CoquiSTT.SpeechToText(modelPath);
//    ;
//}

void LoadOnnx(string model_dir, out TimeSpan duration)
{
    Dictionary<string, int> phonemes = File.ReadLines(Path.Join(model_dir, "phonemes.txt"))
        .Select(line => line.Split(' '))
        .Select(arr => new
        {
            id = int.Parse(arr[0]),
            //maluuba doesn't use 'COMBINING DOUBLE INVERTED BREVE' (U+0361)
            phoneme = arr[1].Replace("\u0361", null)
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

    //const string prompt = """But this cousin... I would not marry that man! He was a boor, a drunk - never there was a night that he did not reek of wine, never a morning that he did not reek of vomit! But a cataphract's daughter is not some chit you can marry against her will. I took a horse from my father's estate - my horse, legally - his old sword, and rode off.""";
    //const string prompt = "I think it's very nice out today.";
    //const string prompt = "Despite the fact I hate maths, I quite like learning about fractions.";
    //const string prompt = "Roger roger, raise the ragged, rhotic career of a better butter bandit.";
    //const string prompt = "My parents were merchants, and I inherited a share of their workshops and camels. But banditry and the fortunes of trade ruined me, which is also common, and now I must make my money some other way.";
    //const string prompt = "Blessed be the Gods, happened that my cousin Aed was in the guard. He sprung me that night from the prison, and together we went roaming round the country. But a passing magistrate decided he weren't parting with his purse, and pulled his blade rather than handing it over like a sensible lad. I took him down, but now before my poor Aed was butchered. See now the price of woman's ingratitude?";
    //const string prompt = "Because I loved my mother, and because I was faster and stronger than the boys, I did all that she said I would do. Of those born in my year, I was the first to kill an enemy. My mother boasted even more, so that the other women came to hate her. They turned us all out of our encampment. We were forced to sell our lands and our slaves, as we could not take them with us, and we were given but a fraction of the price. All we had was our sheep. Of course raiders found us soon enough, and killed my mother, and took our flock. I escaped.";
    const string prompt = "The lords and the merchants, they sit in their lofty towers making grand decisions, but they don't see into the back alleys to know what plots are going on. They don't know where to find the debtor who won't pay his debt. They don't know where to find the wagging tongues starting rumors. So that's where I come in.";
    //const string prompt = "He wound it around the wound, saying \"I read it was $10 to read.\"";
    Console.WriteLine("Prompt: {0}", prompt);

    eSpeakVoice.Initialize(@"C:\Program Files\eSpeak NG\libespeak-ng.dll");
    eSpeakVoice.SetVoiceByName("en-us");
    string prompt_fixup = prompt;
    {
        prompt_fixup = prompt_fixup.Replace(" - ", "... ");
    }
    var result = eSpeakVoice.TextToPhonemes(prompt_fixup);

    Console.WriteLine("Phonemized: ");
    foreach (var r in result)
    {
        Console.WriteLine("\t{0}", r);
    }

    //float max_wav_value;
    //{
    //    var caudio = config["audio"];
    //    max_wav_value = caudio["max_norm"].GetValue<float>();
    //}

    long speaker_id;
    {
        speaker_id = Random.Shared.Next(speakers.Keys.Max());
        Console.WriteLine("Using speaker id {0}: {1}", speaker_id, speakers[(int)speaker_id]);
    }

    //checking espeak -> maluuba
    //{
    //    Console.OutputEncoding = Encoding.UTF8;
    //    foreach (var kvp in phonemes)
    //    {
    //        try
    //        {
    //            var pronunciation = EnPronunciation.FromIpa(kvp.Key);
    //            Console.WriteLine($"Phone {kvp.Key}");
    //            Console.WriteLine(pronunciation.Ipa);
    //            var match = StringComparer.InvariantCultureIgnoreCase.Equals(kvp.Key, pronunciation.Ipa);
    //            Console.WriteLine(match ? "Match" : "Not Match");
    //            Console.WriteLine();
    //            if(!match)
    //            {
    //                Console.Write("Runes ");
    //                foreach (var rune in kvp.Key.EnumerateRunes())
    //                {
    //                    Console.Write(rune.Value);
    //                    Console.Write(" ");
    //                }
    //                Console.WriteLine();
    //            }
    //        }
    //        catch (ArgumentException)
    //        {
    //            Console.WriteLine($"Bad phone {kvp.Key}");
    //        }
    //    }
    //}

    //var pronounciation = Microsoft.PhoneticMatching.EnPronouncer.Instance.Pronounce(line);
    //foreach (var phone in pronounciation.Phones)
    //{
    //    var xphone = new XPhone(phone.Type, phone.Phonation, phone.Place, phone.Manner, phone.Height, phone.Backness, phone.Roundedness, phone.IsRhotic, phone.IsSyllabic);
    //    Console.WriteLine(xphone);
    //}

    phonemes.Add(" ", phonemes["#"]);
    //phonemes.Add("ː", phonemes["·"]); //too long
    phonemes.Add("ː", phonemes["_"]);
    //phonemes.Add("ː", phonemes["ˌ"]);
    phonemes.Add("ᵻ", phonemes["ɪ"]);
    phonemes.Add("ɾ", phonemes["t"]);
    //phonemes.Add("ʌ", phonemes["ə"]);
    phonemes.Add("ɐ", phonemes["ə"]);
    //phonemes.Add("ɪ", phonemes["ə"]);
    phonemes.Add("ɜː", phonemes["ɚ"]);
    phonemes.Add("oː", phonemes["ʊ"]); //also ɔ

    var phlist = new List<int>();
    //phlist.Add(phonemes["^"]);
    //phlist.Add(phonemes["#"]);
    string sh = string.Join(null, result.Select(r => $"^#{r}#$"));//string.Join('·', result);//.Normalize(NormalizationForm.FormD);
    while (sh.Length > 0)
    {
        //phoneme_map
        switch (sh[0])
        {
            case '|':
                sh = sh[1..];
                phlist.Add(phonemes["·"]);
                continue;
            case '‖':
                sh = sh[1..];
                phlist.Add(phonemes["·"]);
                phlist.Add(phonemes["·"]);
                continue;
        }

        var match = phonemes.OrderByDescending(ph => ph.Key.Length).Where(ph => sh.StartsWith(ph.Key, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        if (match is { Key: null })
        {
            break;
        }

        sh = sh[match.Key.Length..];
        phlist.Add(match.Value);
        phlist.Add(phonemes["_"]);
    }
    //phlist.Add(phonemes["#"]);
    //phlist.Add(phonemes["$"]);
    //var text_phoneme_ids = pronounciation.Ipa.Split('\u032F');// EnumerateRunes().Select(rune => phonemes[rune.ToString()]).Cast<long>().ToArray();
    var phlist_ids = phlist.Where(i => i >= 0).Select(i => (long)i).ToArray();
    //var x = phonemes.Values.ToArray();
    ;

    var text_array = new DenseTensor<long>(new[] { 1, phlist_ids.Length });
    phlist_ids.CopyTo(text_array.Buffer);

    var text_lengths_array = new DenseTensor<long>(1);
    text_lengths_array[0] = phlist_ids.Length;

    var scales_array = new DenseTensor<float>(3);
    scales_array[0] = (float)noise_scale;
    scales_array[1] = (float)length_scale;
    scales_array[2] = (float)noise_w;

    var sid = new DenseTensor<long>(1);
    sid[0] = speaker_id;

    // Create input data for session.
    var input = new List<NamedOnnxValue> {
        NamedOnnxValue.CreateFromTensor<long>("input", text_array),
        NamedOnnxValue.CreateFromTensor<long>("input_lengths", text_lengths_array),
        NamedOnnxValue.CreateFromTensor<float>("scales", scales_array),
        NamedOnnxValue.CreateFromTensor<long>("sid", sid)
    };

    {
        using var model = new InferenceSession(Path.Join(model_dir, "generator.onnx"));

        using var outputs = model.Run(input);

        if (outputs.Single() is not { Value: DenseTensor<float> onnxTensor } output
         || onnxTensor.Dimensions is not [1, 1, var sampleCount])
        {
            //TODO: throwhelper
            throw new InvalidOperationException("Unexpected ONNX output");
        }

        //audio_float_to_int16
        {
            var tf = new tensorflow();

            //squeeze is unnecessary
            using var audio_handle = DenseTensorToNDArray(onnxTensor, new Shape(sampleCount), TF_DataType.TF_FLOAT, out var audio);

            const float max_wav_value = 32767.0f;
            var absaudio = tf.abs(audio);
            var amaxaudio = tf.reduce_max(absaudio);
            var audio_norm = audio * (max_wav_value / tf.maximum(0.01f, amaxaudio));
            audio_norm = tf.clip_by_value(audio_norm, -max_wav_value, max_wav_value);
            audio_norm = tf.cast(audio_norm, TF_DataType.TF_INT16);

            const string wavFilename = "test.wav";

            using var afs = File.Create(wavFilename);
            duration = WriteWavStream(afs, audio_norm);
            Console.WriteLine("Wrote {0} with duration {1}", wavFilename, duration);
        }
    }

    static TimeSpan WriteWavStream(Stream stream, Tensorflow.Tensor tensor)
    {
        WaveFormat waveFormat = new WaveFormat(rate: 22050, bits: 16, channels: 1);
        using var wfw = new WaveFileWriter(stream, waveFormat);
        TensorCopyTo(tensor, wfw);
        wfw.Flush();
        return wfw.TotalTime;
    }

    unsafe static void TensorCopyTo(Tensorflow.Tensor tensor, Stream stream)
    {
        Debug.Assert(tensor.bytesize <= int.MaxValue);
        ReadOnlySpan<byte> tensorSpan = new(tensor.buffer.ToPointer(), (int)tensor.bytesize);
        stream.Write(tensorSpan);
    }

    unsafe static MemoryHandle DenseTensorToNDArray<T>(DenseTensor<T> tensor, Shape shape, TF_DataType type, out NDArray ndArray)
    {
        var pin = tensor.Buffer.Pin();
        ndArray = new((nint)pin.Pointer, shape, type);
        return pin;
    }
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