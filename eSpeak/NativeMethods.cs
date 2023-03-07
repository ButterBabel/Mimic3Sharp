using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static System.Net.Mime.MediaTypeNames;

namespace Mimic3Sharp.eSpeak;

internal static partial class NativeMethods {
    private const string LibraryName = "libespeak-ng";

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial Error espeak_SetVoiceByName(string name);

    [LibraryImport(LibraryName)]
    public static partial Error espeak_SetParameter(Parameter parameter, int value, ParameterType type);

    [LibraryImport(LibraryName)]
    public static partial IntPtr espeak_GetCurrentVoice();

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    public static partial Error espeak_Synth(string text, int size, uint startPosition = 0, PositionType positionType = PositionType.Character, uint endPosition = 0, SpeechFlags flags = SpeechFlags.CharsUtf8, UIntPtr uniqueIdentifier = default(UIntPtr), IntPtr userData = default(IntPtr));

    [LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf16)]
    public static partial Error espeak_Initialize(AudioOutput output, int bufferLength, string path, int options);

    [LibraryImport(LibraryName)]
    public static partial Error espeak_Cancel();

    [LibraryImport(LibraryName)]
    public static partial void espeak_SetSynthCallback(SynthCallback callback);

    /// <summary>
    /// Translates text into phonemes.  Call espeak_SetVoiceByName() first, to select a language.
    /// <para>
    /// It returns a pointer to a character string which contains the phonemes for the text up to
    /// end of a sentence, or comma, semicolon, colon, or similar punctuation.
    /// </para>
    /// </summary>
    /// <param name="text">The address of a pointer to the input text which is terminated by a zero character.
    /// On return, the pointer has been advanced past the text which has been translated, or else set
    /// to NULL to indicate that the end of the text has been reached.
    /// </param>
    /// <param name="textmode">Type of character codes</param>
    /// <param name="phonememode">
    /// <para>bit 1:   0=eSpeak's ascii phoneme names, 1= International Phonetic Alphabet (as UTF-8 characters).</para>
    /// <para>bit 7:   use (bits 8-23) as a tie within multi-letter phonemes names</para>
    /// <para>bits 8-23:  separator character, between phoneme names</para>
    /// </param>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Interop.LibraryImportGenerator", "7.0.8.6910")]
    [System.Runtime.CompilerServices.SkipLocalsInitAttribute]
    public static unsafe List<string> espeak_TextToPhonemes(string t, global::Mimic3Sharp.eSpeak.CharEncodingType textmode, int phonememode) {
        byte* __text_native_orig = default;
        byte* __text_native = default;
        string __retVal;
        byte* __retVal_native = default;

        string text = t;

        var groups = new List<string>();

        try {
            // Marshal - Convert managed data to native data.
            __text_native = __text_native_orig = global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ConvertToUnmanaged(text);
            do {
                __retVal_native = __PInvoke(&__text_native, textmode, phonememode);

                // Unmarshal - Convert native data to managed data.
                __retVal = global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ConvertToManaged(__retVal_native);
                //text = global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ConvertToManaged(__text_native);
                groups.Add(__retVal);
            } while (__text_native != null);
        }
        finally {
            // Cleanup - Perform required cleanup.
            global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.Free(__retVal_native);
            global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.Free(__text_native_orig);
        }

        return groups;
        // Local P/Invoke
        [System.Runtime.InteropServices.DllImportAttribute("libespeak-ng", EntryPoint = "espeak_TextToPhonemes", ExactSpelling = true)]
        static extern unsafe byte* __PInvoke(byte** text, global::Mimic3Sharp.eSpeak.CharEncodingType textmode, int phonememode);
    }
    //[LibraryImport(LibraryName, StringMarshalling = StringMarshalling.Utf8)]
    //public static unsafe partial string espeak_TextToPhonemes(ref string text, CharEncodingType textmode, int phonememode);

    //public static unsafe string espeak_TextToPhonemes(in string text, global::Mimic3Sharp.eSpeak.CharEncodingType textmode, int phonememode) {
    //    byte* __text_native = default;
    //    string __retVal;
    //    byte* __retVal_native = default;
    //    // Setup - Perform required setup.
    //    global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ManagedToUnmanagedIn __text_native__marshaller = new();
    //    try {
    //        // Marshal - Convert managed data to native data.
    //        byte* __text_native__stackptr = stackalloc byte[global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ManagedToUnmanagedIn.BufferSize];
    //        __text_native__marshaller.FromManaged(text, new System.Span<byte>(__text_native__stackptr, global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ManagedToUnmanagedIn.BufferSize));
    //        {
    //            // PinnedMarshal - Convert managed data to native data that requires the managed data to be pinned.
    //            __text_native = __text_native__marshaller.ToUnmanaged();
    //            __retVal_native = __PInvoke(&__text_native, textmode, phonememode);
    //        }

    //        // Unmarshal - Convert native data to managed data.
    //        __retVal = global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.ConvertToManaged(__retVal_native);
    //    }
    //    finally {
    //        // Cleanup - Perform required cleanup.
    //        global::System.Runtime.InteropServices.Marshalling.Utf8StringMarshaller.Free(__retVal_native);
    //        __text_native__marshaller.Free();
    //    }

    //    return __retVal;
    //    // Local P/Invoke
    //    [System.Runtime.InteropServices.DllImportAttribute(LibraryName, EntryPoint = "espeak_TextToPhonemes", ExactSpelling = true)]
    //    static extern unsafe byte* __PInvoke(byte** text, global::Mimic3Sharp.eSpeak.CharEncodingType textmode, int phonememode);
    //}
}

public delegate int SynthCallback(IntPtr wavePtr, int bufferLength, IntPtr eventsPtr);

enum Parameter {
    Rate = 1,
    Volume = 2,
    Pitch = 3,
    Range = 4,
    Punctuation = 5,
    Capitals = 6,
    WordGap = 7,
    Intonation = 9,
}

enum Error {
    EE_OK = 0,
    EE_INTERNAL_ERROR = -1,
    EE_BUFFER_FULL = 1,
    EE_NOT_FOUND = 2
}

enum ParameterType {
    Absolute = 0,
    Relative = 1
}

enum AudioOutput {
    Playback,
    Retrieval,
    Synchronous,
    SynchronousPlayback
};
enum PositionType {
    Character = 1,
    Word = 2,
    Sentence = 3
}

[Flags]
enum SpeechFlags {
    CharsUtf8 = 1,
    SSML = 0x10,
}

enum CharEncodingType {
    /// <summary>
    /// 8 bit or UTF8  (this is the default)
    /// </summary>
    espeakCHARS_AUTO = 0,
    /// <summary>
    /// UTF8 encoding
    /// </summary>
    espeakCHARS_UTF8 = 1,
    /// <summary>
    /// The 8 bit ISO-8859 character set for the particular language.
    /// </summary>
    espeakCHARS_8BIT = 2,
    /// <summary>
    /// Wide characters (wchar_t)
    /// </summary>
    espeakCHARS_WCHAR = 3,
    /// <summary>
    /// 16 bit characters.
    /// </summary>
    espeakCHARS_16BIT = 4
}