using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using Tensorflow;

namespace Mimic3Sharp.eSpeak;
using static NativeMethods;

internal static class eSpeakVoice {
    public static void Initialize(string path) {
        var khz = espeak_Initialize(AudioOutput.Retrieval, 0, path, 0) switch {
            Error.EE_INTERNAL_ERROR => throw new Exception($"Could not initialize eSpeak. Maybe there is no espeak data at {path}?"),
            Error e => (int)e
        };

        espeak_SetSynthCallback(Handle);

        //Initialized = true;
    }

    public static void SetVoiceByName(string voice) {
        if (espeak_SetVoiceByName(voice) is not Error.EE_OK) {
            throw new InvalidOperationException("Failed to set voice");
        }
    }

    public static unsafe string TextToPhonemes(string text) {
        return espeak_TextToPhonemes(text, CharEncodingType.espeakCHARS_UTF8, 0);
    }

    static bool CheckResult(Error result) {
        if (result == Error.EE_OK) {
            return true;
        }
        else if (result == Error.EE_BUFFER_FULL) {
            return false;
        }
        else if (result == Error.EE_INTERNAL_ERROR) {
            throw new Exception("Internal error in ESpeak.");
        }
        else {
            return false;
        }
    }

    public static bool Speak(string text) {
        var result = espeak_Synth(text, text.Length);
        return CheckResult(result);
    }

    public static int Handle(IntPtr wavePtr, int bufferLength, IntPtr eventsPtr) {
        Console.WriteLine("Received event!");
        Console.WriteLine("Buffer length is " + bufferLength);

        //if (bufferLength == 0)
        //{
        //    /*
        //    var file = new FileStream("alarm01.wav", FileMode.Open);
        //    Stream.Seek(0, SeekOrigin.Begin);
        //    file.CopyTo(Stream);
        //    */
        //    PlayAudio();
        //    Console.Write(ConvertHeadersToString(Stream.GetBuffer()));
        //    Stream.Dispose();
        //    return 0;
        //}

        //WriteAudioToStream(wavePtr, bufferLength);

        //var events = MarshalEvents(eventsPtr);

        //foreach (Event anEvent in events)
        //{
        //    Console.WriteLine(anEvent.Type);
        //    Console.WriteLine(anEvent.Id);
        //}

        return 0; // continue synthesis
    }
}
