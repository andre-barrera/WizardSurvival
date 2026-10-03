using UnityEngine;

namespace ArcaneSurvival
{
    // Sound effect bank
    {
        private readonly AudioSource source;
        private readonly AudioClip cast;
        private readonly AudioClip defeat;
        private readonly AudioClip hurt;
        private readonly AudioClip heal;
        public bool Muted { get; private set; }

        // Generates gameplay sound effects.
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

        // Modifies sounds depending of the state of the game.
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

        // Attacks
        public void Cast() => source.PlayOneShot(cast, 0.5f);
        // Enemy defeated
        public void Defeat() => source.PlayOneShot(defeat, 0.7f);
        // Damage to the player
        public void Hurt() => source.PlayOneShot(hurt);
        // health restoration
        public void Heal() => source.PlayOneShot(heal);
        // Mute toggle
        public void ToggleMute() { Muted = !Muted; source.mute = Muted; }
        // Stops sounds 
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
