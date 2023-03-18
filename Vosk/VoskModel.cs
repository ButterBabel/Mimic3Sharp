﻿using System.Buffers;
using Vosk;

namespace Mimic3Sharp.Vosk;

public sealed class VoskModel
{
    private readonly Model _model;

    static VoskModel()
    {
#if DEBUG
        global::Vosk.Vosk.SetLogLevel(0);
#else
        global::Vosk.Vosk.SetLogLevel(-1);
#endif
    }

    public VoskModel(string modelPath)
    {
        _model = new Model(modelPath);
    }

    public IEnumerable<string> Recognize(Stream wavStream, int sampleRate, string? grammar = null)
    {
        using VoskRecognizer rec = grammar switch
        {
            string g => new VoskRecognizer(_model, sampleRate, g),
            null => new VoskRecognizer(_model, sampleRate)
        };
        rec.SetMaxAlternatives(0);
        rec.SetWords(true);

        using (wavStream)
        {
            byte[] buffer = null;

            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(0x1000);

                int bytesRead;
                while ((bytesRead = wavStream.Read(buffer)) > 0)
                {
                    if (rec.AcceptWaveform(buffer, bytesRead))
                    {
                        yield return rec.Result();
                    }
                    else
                    {
                        yield return rec.PartialResult();
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        yield return rec.FinalResult();
    }
}
