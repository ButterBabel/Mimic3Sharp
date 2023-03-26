# Mimic3Sharp

This repository is a sandbox containing a lot of QnD local development history sandbox while I was experimenting with TTS/STT/ASR solutions for the Butter Babel project. It's in no way complete, correct, or even good. If you're working with any of the topics below, I hope it can be a useful reference and save you some time!

* Converting between [VCTK](https://datashare.ed.ac.uk/handle/10283/3443) and [LJSpeech](https://keithito.com/LJ-Speech-Dataset/) dataset formats
* Running inference with an [ONNX](https://onnxruntime.ai/) TTS model in C#
* Pipeline pre/postprocessing with [TensorFlow.NET](https://github.com/SciSharp/TensorFlow.NET)
* Phonemization and utterance extraction with native interop to [espeak-ng](https://github.com/espeak-ng/espeak-ng)
* Phonemization and similarity search with the almost completely unknown [Maluuba](https://github.com/microsoft/PhoneticMatching) library
* Approximating [gruut](https://github.com/rhasspy/gruut) phoneme representation from espeak-ng phonemes
* ASR from [Coqui's libSTT](https://github.com/coqui-ai/STT/) based on the trained TensorFlow Lite model
* Convert word-level timings from [Vosk](https://alphacephei.com/vosk/) ASR into very rough phoneme-level timings through a forced alignment approximation
* Mapping phoneme-level timings into [Rhubarb](https://github.com/DanielSWolf/rhubarb-lip-sync)-format viseme-level timings for lip sync
* An end-to-end TTS+G2P2V pipeline based on all of the above
