using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mimic3Sharp.CoquiSTT;
using static NativeMethods;
internal class SpeechToText {
    private static void ThrowIfNotOk([DoesNotReturnIf(true)] ErrorCodes result) {
        if (result is not ErrorCodes.STT_ERR_OK) {
            throw new InvalidOperationException(STT_ErrorCodeToErrorMessage(result));
        }
    }

    public unsafe SpeechToText(string modelPath) {
        ArgumentNullException.ThrowIfNullOrEmpty(modelPath);

        var result = STT_CreateModel(modelPath, out var modelState);
        ThrowIfNotOk(result);

        ;
    }
}
