using UnityEngine;

namespace OutpostBalance.UI
{
    /// <summary>
    /// Generates clean synthesized UI/Alert sound clips procedurally so zero external audio files are required.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SfxPlayer : MonoBehaviour
    {
        public bool SoundEnabled { get; private set; } = true;
        private AudioSource _source;
        private AudioClip _clickClip;
        private AudioClip _alertClip;
        private AudioClip _successClip;
        private AudioClip _dangerClip;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _clickClip = MakeToneClip("click", 540f, 0.05f, 0.18f);
            _alertClip = MakeToneClip("alert", 680f, 0.14f, 0.22f);
            _successClip = MakeToneClip("success", 784f, 0.16f, 0.2f);
            _dangerClip = MakeToneClip("danger", 210f, 0.24f, 0.25f);
        }

        public void ToggleSound()
        {
            SoundEnabled = !SoundEnabled;
            if (SoundEnabled) PlayClick();
        }

        public void PlayClick() => Play(_clickClip);
        public void PlayAlert() => Play(_alertClip);
        public void PlaySuccess() => Play(_successClip);
        public void PlayDanger() => Play(_dangerClip);

        private void Play(AudioClip clip)
        {
            if (SoundEnabled && _source != null && clip != null)
                _source.PlayOneShot(clip);
        }

        private static AudioClip MakeToneClip(string name, float freq, float duration, float volume)
        {
            int sampleRate = 22050;
            int samples = Mathf.Max(1, Mathf.FloorToInt(sampleRate * duration));
            float[] data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float env = 1f - ((float)i / samples);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * env;
            }
            var clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
