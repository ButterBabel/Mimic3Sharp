
using Vosk;

namespace Mimic3Sharp.Vosk;

internal class VoskDemo
{
    public static void DemoBytes(Model model, string grammar)
    {
        // Demo byte buffer
        VoskRecognizer rec = new VoskRecognizer(model, 22050f/*16000.0f*/, grammar);
        rec.SetMaxAlternatives(0);
        rec.SetWords(true);
        using (Stream source = File.OpenRead("test.wav"))
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (rec.AcceptWaveform(buffer, bytesRead))
                {
                    Console.WriteLine(rec.Result());
                }
                else
                {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
    }

    public static void DemoFloats(Model model)
    {
        // Demo float array
        VoskRecognizer rec = new VoskRecognizer(model, 22050f);
        using (Stream source = File.OpenRead("test.wav"))
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                float[] fbuffer = new float[bytesRead / 2];
                for (int i = 0, n = 0; i < fbuffer.Length; i++, n += 2)
                {
                    fbuffer[i] = BitConverter.ToInt16(buffer, n);
                }
                if (rec.AcceptWaveform(fbuffer, fbuffer.Length))
                {
                    Console.WriteLine(rec.Result());
                }
                else
                {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
    }

    public static void DemoSpeaker(Model model)
    {
        // Output speakers
        SpkModel spkModel = new SpkModel("model-spk");
        VoskRecognizer rec = new VoskRecognizer(model, 22050f);
        rec.SetSpkModel(spkModel);

        using (Stream source = File.OpenRead("test.wav"))
        {
            byte[] buffer = new byte[4096];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (rec.AcceptWaveform(buffer, bytesRead))
                {
                    Console.WriteLine(rec.Result());
                }
                else
                {
                    Console.WriteLine(rec.PartialResult());
                }
            }
        }
        Console.WriteLine(rec.FinalResult());
    }

    public static void Main(string modelname, string grammar)
    {
        // You can set to -1 to disable logging messages
        global::Vosk.Vosk.SetLogLevel(0);

        var model = new Model(modelname);
        DemoBytes(model, grammar);
        DemoFloats(model);
        DemoSpeaker(model);
    }
}
