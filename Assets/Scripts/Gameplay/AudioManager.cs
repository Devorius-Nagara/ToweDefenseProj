using UnityEngine;

/// <summary>
/// Generates all sound effects procedurally at runtime — no audio files needed.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private AudioClip clipPlace, clipSell, clipHit, clipDeath;
    private AudioClip clipWaveStart, clipVictory, clipDefeat, clipGold, clipButton;
    private AudioClip clipEnemyHurt;
    private AudioClip clipShootArcher, clipShootMage, clipShootFreezer, clipShootCannon;

    [Range(0f, 1f)] public float SFXVolume   = 0.7f;
    [Range(0f, 1f)] public float MusicVolume = 0.45f;

    private AudioClip clipMenuMusic, clipGameplayMusic;
    private MusicTrack currentTrack = MusicTrack.None;

    private enum MusicTrack { None, Menu, Gameplay }

#if UNITY_WEBGL && !UNITY_EDITOR
    // Browsers suspend AudioContext until the first user gesture.
    // We retry the pending music track every ~0.25 s until it actually starts.
    private float _webglMusicRetryTimer;
#endif

    void Awake()
    {
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop        = true;
        musicSource.volume      = MusicVolume;

        // Try to load music from Resources (mp3 files if imported)
        clipMenuMusic     = Resources.Load<AudioClip>("Music/menu_music");
        clipGameplayMusic = Resources.Load<AudioClip>("Music/gameplay_music");
#if UNITY_EDITOR
        if (clipMenuMusic == null)
            clipMenuMusic = LoadAudioFromDisk("Music/menu_music.mp3");
        if (clipGameplayMusic == null)
            clipGameplayMusic = LoadAudioFromDisk("Music/gameplay_music.mp3");
#endif
        // Procedural fallback — always plays even if mp3 files are not imported yet
        if (clipMenuMusic     == null) clipMenuMusic     = ProcMenuMusic();
        if (clipGameplayMusic == null) clipGameplayMusic = ProcGameplayMusic();

        clipButton    = Tone(550f, 0.07f, 0.35f);
        clipPlace     = Tone(440f, 0.09f, 0.45f);
        clipSell      = Sweep(440f, 260f, 0.14f, 0.40f);
        clipHit       = Noise(0.04f, 0.25f);
        clipDeath     = Sweep(320f, 100f, 0.22f, 0.50f);
        clipWaveStart = Sweep(300f, 700f, 0.35f, 0.55f);
        clipVictory   = Chord(new[] { 523f, 659f, 784f, 1047f }, 0.90f, 0.55f);
        clipDefeat    = Sweep(440f,  80f, 0.65f, 0.50f);
        clipGold      = Tone(880f, 0.07f, 0.28f);

        // Per-tower shoot sounds
        clipShootArcher  = TwangClip(0.10f, 0.38f);
        clipShootMage    = MagicZapClip(0.18f, 0.42f);
        clipShootFreezer = IceCrystalClip(0.12f, 0.32f);
        clipShootCannon  = CannonBoomClip(0.30f, 0.55f);

        // Enemy hurt grunt
        clipEnemyHurt = EnemyHurtClip(0.14f, 0.38f);
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// WebGL: AudioContext starts suspended and only unlocks after the first user
    /// gesture (click / tap). We poll every 0.25 s and retry Play() until the
    /// source actually starts. Harmless on other platforms — the block is compiled out.
    /// </summary>
    void Update()
    {
        if (currentTrack == MusicTrack.None || musicSource.isPlaying) return;

        _webglMusicRetryTimer += Time.unscaledDeltaTime;
        if (_webglMusicRetryTimer < 0.25f) return;
        _webglMusicRetryTimer = 0f;

        // Force a fresh Play() — PlayMusic() guards prevent it if isPlaying,
        // so we bypass the guard here.
        musicSource.volume = MusicVolume;
        AudioClip clip = currentTrack == MusicTrack.Menu ? clipMenuMusic : clipGameplayMusic;
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.Play();
    }
#endif

    public void PlayButton()    => Play(clipButton);
    public void PlayPlace()     => Play(clipPlace);
    public void PlaySell()      => Play(clipSell);
    public void PlayHit()       => Play(clipHit);
    public void PlayDeath()     => Play(clipDeath);
    public void PlayWaveStart() => Play(clipWaveStart);
    public void PlayVictory()   => Play(clipVictory);
    public void PlayDefeat()    => Play(clipDefeat);
    public void PlayGold()      => Play(clipGold);
    public void PlayEnemyHurt() => Play(clipEnemyHurt);

    public void PlayTowerShoot(string towerName)
    {
        AudioClip clip = towerName switch
        {
            "Archer"  => clipShootArcher,
            "Mage"    => clipShootMage,
            "Freezer" => clipShootFreezer,
            "Cannon"  => clipShootCannon,
            _         => clipPlace
        };
        Play(clip);
    }

    // ── Music control ─────────────────────────────────────────────────
    public void PlayMenuMusic()
    {
        if (currentTrack == MusicTrack.Menu) return;
        currentTrack = MusicTrack.Menu;
        PlayMusic(clipMenuMusic);
    }

    public void PlayGameplayMusic()
    {
        if (currentTrack == MusicTrack.Gameplay) return;
        currentTrack = MusicTrack.Gameplay;
        PlayMusic(clipGameplayMusic);
    }

    public void StopMusic()
    {
        currentTrack = MusicTrack.None;
        musicSource.Stop();
    }

    private void PlayMusic(AudioClip clip)
    {
        musicSource.volume = MusicVolume;
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    private void Play(AudioClip clip)
    {
        if (clip == null || SFXVolume <= 0f) return;
        sfxSource.volume = SFXVolume;
        sfxSource.PlayOneShot(clip);
    }

#if UNITY_EDITOR
    private static AudioClip LoadAudioFromDisk(string relativePath)
    {
        // In Editor, use UnityEditor.AssetDatabase to load the clip
        string assetPath = "Assets/Resources/" + relativePath;
        var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        return clip;
    }
#endif

    // ── Procedural clip generators ────────────────────────────────────

    private static AudioClip Tone(float freq, float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Pow(1f - t / dur, 0.6f);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * vol * env;
        }
        var c = AudioClip.Create("tone", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    private static AudioClip Sweep(float f1, float f2, float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float frac = t / dur;
            float freq = Mathf.Lerp(f1, f2, frac);
            float env  = Mathf.Sin(Mathf.PI * frac);
            d[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * vol * env;
        }
        var c = AudioClip.Create("sweep", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    private static AudioClip Noise(float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        var rng = new System.Random(42);
        for (int i = 0; i < n; i++)
        {
            float env = 1f - i / (float)n;
            d[i] = (float)(rng.NextDouble() * 2.0 - 1.0) * vol * env;
        }
        var c = AudioClip.Create("noise", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    private static AudioClip Chord(float[] freqs, float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        float perF = vol / freqs.Length;
        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;
            float env = Mathf.Pow(1f - t / dur, 0.35f);
            float s = 0f;
            foreach (var f in freqs) s += Mathf.Sin(2f * Mathf.PI * f * t) * perF;
            d[i] = s * env;
        }
        var c = AudioClip.Create("chord", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    // ── Tower shoot sounds ────────────────────────────────────────────

    /// <summary>Archer: bowstring twang — sharp attack, pitch glide 720→320 Hz.</summary>
    private static AudioClip TwangClip(float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        var rng = new System.Random(7);
        for (int i = 0; i < n; i++)
        {
            float t    = i / (float)rate;
            float frac = t / dur;
            float freq = 720f - 400f * frac;                         // glide down
            float env  = Mathf.Exp(-t * 30f);                        // sharp decay
            float s    = Mathf.Sin(2f * Mathf.PI * freq * t)
                       + 0.40f * Mathf.Sin(2f * Mathf.PI * freq * 2f * t)
                       + 0.15f * Mathf.Sin(2f * Mathf.PI * freq * 3f * t)
                       + 0.07f * (float)(rng.NextDouble() * 2.0 - 1.0); // string texture
            d[i] = s * vol * env;
        }
        var c = AudioClip.Create("shoot_archer", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    /// <summary>Mage: mystical rising zap with shimmer (180→1600 Hz sweep + odd harmonics).</summary>
    private static AudioClip MagicZapClip(float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        for (int i = 0; i < n; i++)
        {
            float t    = i / (float)rate;
            float frac = t / dur;
            float freq = 180f + 1420f * frac * frac;                 // accelerating sweep
            float env  = Mathf.Sin(Mathf.PI * frac) * Mathf.Pow(1f - frac, 0.4f);
            float s    = Mathf.Sin(2f * Mathf.PI * freq * t)
                       + 0.30f * Mathf.Sin(2f * Mathf.PI * freq * 3f * t + 0.5f)
                       + 0.15f * Mathf.Sin(2f * Mathf.PI * freq * 5f * t + 1.0f);
            d[i] = s * vol * env;
        }
        var c = AudioClip.Create("shoot_mage", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    /// <summary>Freezer: crisp ice-crystal chime — two high beating tones with metallic ring.</summary>
    private static AudioClip IceCrystalClip(float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        var rng = new System.Random(13);
        for (int i = 0; i < n; i++)
        {
            float t    = i / (float)rate;
            float frac = t / dur;
            float env  = Mathf.Exp(-t * 22f);
            float s    = Mathf.Sin(2f * Mathf.PI * 1380f * t)
                       + 0.70f * Mathf.Sin(2f * Mathf.PI * 1820f * t)
                       + 0.20f * Mathf.Sin(2f * Mathf.PI * 2400f * t)
                       + 0.06f * (float)(rng.NextDouble() * 2.0 - 1.0) * (1f - frac);
            d[i] = s * vol * env;
        }
        var c = AudioClip.Create("shoot_freezer", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    /// <summary>Cannon: deep sub-bass boom — pitch drops 90→40 Hz, heavy noise thud.</summary>
    private static AudioClip CannonBoomClip(float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        var rng = new System.Random(99);
        for (int i = 0; i < n; i++)
        {
            float t    = i / (float)rate;
            float frac = t / dur;
            float freq = 90f - 50f * frac;                           // sub-bass glide
            float env  = Mathf.Min(t * 80f, 1f) * Mathf.Exp(-t * 7f);
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            float s    = Mathf.Sin(2f * Mathf.PI * freq * t)
                       + 0.50f * Mathf.Sin(2f * Mathf.PI * freq * 2f * t)
                       + 0.60f * noise * Mathf.Exp(-t * 12f);
            d[i] = Mathf.Clamp(s * 1.3f, -1f, 1f) * vol * env;     // soft clip for warmth
        }
        var c = AudioClip.Create("shoot_cannon", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    /// <summary>Enemy hurt: short goblin grunt — pitch glide 280→140 Hz, odd harmonics + rasp.</summary>
    private static AudioClip EnemyHurtClip(float dur, float vol)
    {
        int rate = 44100, n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];
        var rng = new System.Random(55);
        for (int i = 0; i < n; i++)
        {
            float t     = i / (float)rate;
            float frac  = t / dur;
            float freq  = 280f - 140f * frac;
            float env   = Mathf.Sin(Mathf.PI * frac) * Mathf.Pow(1f - frac * 0.6f, 1.5f);
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            float s     = Mathf.Sin(2f * Mathf.PI * freq * t)
                        + 0.55f * Mathf.Sin(2f * Mathf.PI * freq * 3f * t)
                        + 0.30f * Mathf.Sin(2f * Mathf.PI * freq * 5f * t)
                        + 0.35f * noise;
            d[i] = Mathf.Clamp(s * 1.4f, -0.9f, 0.9f) * vol * env;
        }
        var c = AudioClip.Create("enemy_hurt", n, 1, rate, false);
        c.SetData(d, 0); return c;
    }

    // ── Procedural background music (fallback when mp3 files are not imported) ──

    /// <summary>
    /// 8-second looping medieval ambient: Am→F→C→G chord pads with shimmer.
    /// Used as menu music fallback.
    /// </summary>
    private static AudioClip ProcMenuMusic()
    {
        const int   rate = 44100;
        const float dur  = 8f;
        int n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];

        // Am · F · C · G  (2 s each)
        float[][] chords =
        {
            new[] { 110.00f, 130.81f, 164.81f, 220.00f },  // Am
            new[] {  87.31f, 130.81f, 174.61f, 261.63f },  // F
            new[] { 130.81f, 164.81f, 196.00f, 261.63f },  // C
            new[] {  98.00f, 130.81f, 196.00f, 246.94f },  // G
        };

        for (int i = 0; i < n; i++)
        {
            float t   = i / (float)rate;
            int   ci  = Mathf.FloorToInt(t / 2f) % 4;
            float ct  = (t % 2f) / 2f;                     // 0..1 within chord slot

            // Smooth fade in/out per chord to avoid clicks at chord boundaries
            float chordEnv = Mathf.SmoothStep(0f, 1f, ct / 0.12f) *
                             Mathf.SmoothStep(0f, 1f, (1f - ct) / 0.12f);

            // Fade whole clip in/out for seamless looping
            float loopFade = Mathf.SmoothStep(0f, 1f, t / 0.25f) *
                             Mathf.SmoothStep(0f, 1f, (dur - t) / 0.25f);

            float s = 0f;
            foreach (float f in chords[ci])
            {
                // Fundamental + soft 2nd harmonic (flute/organ pad)
                s += Mathf.Sin(2f * Mathf.PI * f * t);
                s += 0.22f * Mathf.Sin(2f * Mathf.PI * f * 2f * t + 0.35f);
            }
            s /= chords[ci].Length * 1.3f;

            // Shimmer: plucked high note on every chord change
            float shimmerT   = t % 2f;
            float shimmerEnv = Mathf.Exp(-shimmerT * 6f);
            float shimmerF   = chords[ci][chords[ci].Length - 1] * 2f;
            s += 0.18f * Mathf.Sin(2f * Mathf.PI * shimmerF * t) * shimmerEnv;

            d[i] = s * 0.40f * chordEnv * loopFade;
        }

        var c = AudioClip.Create("menu_proc", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }

    /// <summary>
    /// 8-second looping action music at 120 BPM: kick+snare+hihat + chord stabs.
    /// Used as gameplay music fallback.
    /// </summary>
    private static AudioClip ProcGameplayMusic()
    {
        const int   rate   = 44100;
        const float dur    = 8f;
        const float bpm    = 120f;
        const float beat   = 60f / bpm;   // 0.5 s
        const float bar    = beat * 4f;   // 2.0 s  (4/4)

        int n = Mathf.CeilToInt(rate * dur);
        float[] d = new float[n];

        // 8 chords × 1 beat = 8 beats = 2 bars loop
        // Am Em F G Am Em F G
        float[][] chords =
        {
            new[] { 110.00f, 164.81f, 220.00f },  // Am
            new[] {  82.41f, 123.47f, 164.81f },  // Em
            new[] {  87.31f, 130.81f, 174.61f },  // F
            new[] {  98.00f, 146.83f, 196.00f },  // G
            new[] { 110.00f, 164.81f, 220.00f },
            new[] {  82.41f, 123.47f, 164.81f },
            new[] {  87.31f, 130.81f, 174.61f },
            new[] {  98.00f, 146.83f, 196.00f },
        };

        // Pre-bake percussion impulse buffers (short arrays to look up by sample offset)
        int impulseLen = (int)(rate * 0.25f);
        float[] kick  = new float[impulseLen];
        float[] snare = new float[impulseLen];
        float[] hihat = new float[impulseLen];

        var rngK = new System.Random(11);
        var rngS = new System.Random(22);
        var rngH = new System.Random(33);

        for (int i = 0; i < impulseLen; i++)
        {
            float t = i / (float)rate;
            // Kick: sine glide 90→30 Hz + thump noise
            kick[i]  = Mathf.Sin(2f * Mathf.PI * Mathf.Max(90f - t * 500f, 28f) * t) * 0.75f
                     * Mathf.Exp(-t * 20f)
                     + (float)(rngK.NextDouble() * 2 - 1) * 0.35f * Mathf.Exp(-t * 80f);
            // Snare: band-passed noise burst
            snare[i] = (float)(rngS.NextDouble() * 2 - 1) * 0.55f * Mathf.Exp(-t * 28f)
                     + Mathf.Sin(2f * Mathf.PI * 210f * t) * 0.20f * Mathf.Exp(-t * 35f);
            // Hihat: high noise click
            hihat[i] = (float)(rngH.NextDouble() * 2 - 1) * 0.28f * Mathf.Exp(-t * 200f);
        }

        for (int i = 0; i < n; i++)
        {
            float t = i / (float)rate;

            // ── Chord stab ───────────────────────────────────────────
            int   ci     = Mathf.FloorToInt(t / beat) % 8;
            float ct     = (t % beat) / beat;
            float stab   = Mathf.SmoothStep(0f, 1f, ct / 0.08f) * Mathf.Pow(1f - ct, 0.4f);
            float chord  = 0f;
            foreach (float f in chords[ci])
                chord += Mathf.Sin(2f * Mathf.PI * f * t);
            chord /= chords[ci].Length;

            // ── Drums ────────────────────────────────────────────────
            float barPos = t % bar;
            float drums  = 0f;

            // Kick: beats 1 and 3  (position 0 and 1.0 s)
            for (int b = 0; b <= 2; b += 2)
            {
                float off = barPos - b * beat;
                if (off >= 0f)
                {
                    int ki = Mathf.Min((int)(off * rate), impulseLen - 1);
                    drums += kick[ki];
                }
            }
            // Snare: beats 2 and 4  (position 0.5 and 1.5 s)
            for (int b = 1; b <= 3; b += 2)
            {
                float off = barPos - b * beat;
                if (off >= 0f)
                {
                    int si = Mathf.Min((int)(off * rate), impulseLen - 1);
                    drums += snare[si];
                }
            }
            // Hihat: every 8th note (every half-beat = 0.25 s)
            float hiOff = barPos % (beat * 0.5f);
            {
                int hi = Mathf.Min((int)(hiOff * rate), impulseLen - 1);
                drums += hihat[hi];
            }

            float loopFade = Mathf.SmoothStep(0f, 1f, t / 0.15f) *
                             Mathf.SmoothStep(0f, 1f, (dur - t) / 0.15f);

            d[i] = Mathf.Clamp((chord * 0.18f * stab + drums) * loopFade, -0.92f, 0.92f);
        }

        var c = AudioClip.Create("gameplay_proc", n, 1, rate, false);
        c.SetData(d, 0);
        return c;
    }
}
