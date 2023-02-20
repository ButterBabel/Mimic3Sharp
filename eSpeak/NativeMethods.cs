using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Mimic3Sharp.eSpeak;

internal static partial class NativeMethods
{
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
}

public delegate int SynthCallback(IntPtr wavePtr, int bufferLength, IntPtr eventsPtr);

enum Parameter
{
    Rate = 1,
    Volume = 2,
    Pitch = 3,
    Range = 4,
    Punctuation = 5,
    Capitals = 6,
    WordGap = 7,
    Intonation = 9,
}

enum Error
{
    EE_OK = 0,
    EE_INTERNAL_ERROR = -1,
    EE_BUFFER_FULL = 1,
    EE_NOT_FOUND = 2
}

enum ParameterType
{
    Absolute = 0,
    Relative = 1
}

enum AudioOutput
{
    Playback,
    Retrieval,
    Synchronous,
    SynchronousPlayback
};
enum PositionType
{
    Character = 1,
    Word = 2,
    Sentence = 3
}
[Flags]
enum SpeechFlags
{
    CharsUtf8 = 1,
    SSML = 0x10,
}
