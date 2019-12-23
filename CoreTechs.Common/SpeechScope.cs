//todo System.Speech.Synthesis is currently not available in .net standard
/*using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Synthesis;

namespace CoreTechs.Common
{
    public class SpeechScope : RepeatScope
    {
        private readonly string[] _phrases;
        private readonly SpeechSynthesizer _synth;

        public SpeechScope() : this("Computing", "Please Wait") { }
        public SpeechScope(params string[] phrases) : this(null, phrases) { }
        public SpeechScope(VoiceInfo voiceInfo, params string[] phrases)
        {
            _phrases = phrases;
            _synth = new SpeechSynthesizer();

            _synth.SelectVoice(voiceInfo != null
                ? voiceInfo.Name
                : _synth.GetInstalledVoices()
                    .Where(v => v.VoiceInfo.Culture.Equals(CultureInfo.CurrentCulture))
                    .RandomElement().VoiceInfo.Name);
        }

        protected override void Execute()
        {
            _synth.Speak(_phrases.RandomElement());
        }

        protected override IEnumerable<IDisposable> GetDisposables()
        {
            yield return _synth;
        }
    }
}*/