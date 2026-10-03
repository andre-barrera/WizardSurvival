using UnityEngine;

namespace ArcaneSurvival
{
    // Short synthesized sound effects avoid third-party audio dependencies.
    public sealed class SoundBank
    {
        private readonly AudioSource source;
        private readonly AudioClip cast;
        private readonly AudioClip defeat;
        private readonly AudioClip hurt;
        private readonly AudioClip heal;
        public bool Muted { get; private set; }

        // Configures an audio source and generates the four gameplay sound effects.
        public SoundBank(GameObject host)
        {
            source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = 0.28f;
            cast = Tone("Spell", 680, 1240, 0.11f);
            defeat = Tone("Wisp defeated", 440, 130, 0.18f);
            hurt = Tone("Wizard hit", 160, 48, 0.28f);
            heal = Tone("Healing", 440, 880, 0.35f);
        }

        // Synthesizes a fading tone that changes frequency and returns it as an audio clip.
        private static AudioClip Tone(string name, float startHz, float endHz, float duration)
        {
            const int sampleRate = 22050;
            int count = Mathf.CeilToInt(sampleRate * duration);
            var samples = new float[count];
            float phase = 0;
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / (count - 1);
                phase += 2 * Mathf.PI * Mathf.Lerp(startHz, endHz, t) / sampleRate;
                float envelope = Mathf.Min(1, t * 30) * (1 - t) * (1 - t);
                samples[i] = (Mathf.Sin(phase) + 0.2f * Mathf.Sin(phase * 2)) * envelope * 0.6f;
            }
            AudioClip clip = AudioClip.Create(name, count, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Plays the spell-casting sound at a reduced volume.
        public void Cast() => source.PlayOneShot(cast, 0.5f);
        // Plays the enemy-defeat sound.
        public void Defeat() => source.PlayOneShot(defeat, 0.7f);
        // Plays the sound for damage to the wizard.
        public void Hurt() => source.PlayOneShot(hurt);
        // Plays the health-restoration sound.
        public void Heal() => source.PlayOneShot(heal);
        // Toggles sound playback between muted and unmuted.
        public void ToggleMute() { Muted = !Muted; source.mute = Muted; }
        // Stops sounds currently playing through this audio source.
        public void Stop() => source.Stop();
        // Destroys the generated audio clips when they are no longer needed.
        public void Dispose()
        {
            Object.Destroy(cast);
            Object.Destroy(defeat);
            Object.Destroy(hurt);
            Object.Destroy(heal);
        }
    }
}
