// ============================================================
// Jurassic Trainers -- Procedural Audio System
// Chiptune-inspired music with longer, pleasant loops
// ============================================================

type OscType = OscillatorType;

interface NoteEvent {
  freq: number;
  start: number;   // seconds from loop start
  dur: number;      // seconds
  type: OscType;
  vol: number;      // 0-1
}

export class AudioSystem {
  private static instance: AudioSystem | null = null;

  private ctx: AudioContext | null = null;
  private musicGain: GainNode | null = null;
  private sfxGain: GainNode | null = null;
  private currentMusicSources: AudioBufferSourceNode[] = [];
  private musicLoopTimer: number | null = null;
  private currentMusicType: string = '';

  public audioSupported = false;

  private musicVolume = 0.12;
  private sfxVolume = 0.25;
  private musicEnabled = true;
  private sfxEnabled = true;
  private initialized = false;

  private constructor() {}

  static getInstance(): AudioSystem {
    if (!AudioSystem.instance) {
      AudioSystem.instance = new AudioSystem();
    }
    return AudioSystem.instance;
  }

  init(): void {
    if (this.initialized) return;
    try {
      this.ctx = new AudioContext();
      this.musicGain = this.ctx.createGain();
      this.musicGain.gain.value = this.musicVolume;
      this.musicGain.connect(this.ctx.destination);

      this.sfxGain = this.ctx.createGain();
      this.sfxGain.gain.value = this.sfxVolume;
      this.sfxGain.connect(this.ctx.destination);

      this.initialized = true;
      this.audioSupported = true;
      if (this.ctx.state === 'suspended') {
        this.ctx.resume();
      }
    } catch (_e) { /* Web Audio not supported */ }
  }

  private ensureContext(): boolean {
    if (!this.ctx || !this.initialized) this.init();
    if (this.ctx && this.ctx.state === 'suspended') this.ctx.resume();
    return !!this.ctx && !!this.sfxGain && !!this.musicGain;
  }

  // ================================================================
  // Note frequencies (A4 = 440Hz)
  // ================================================================
  private static NOTE: Record<string, number> = {
    C3: 130.81, D3: 146.83, E3: 164.81, F3: 174.61, G3: 196.00, A3: 220.00, B3: 246.94,
    C4: 261.63, D4: 293.66, E4: 329.63, F4: 349.23, G4: 392.00, A4: 440.00, B4: 493.88,
    C5: 523.25, D5: 587.33, E5: 659.25, F5: 698.46, G5: 783.99, A5: 880.00,
    C2: 65.41, D2: 73.42, E2: 82.41, F2: 87.31, G2: 98.00, A2: 110.00, B2: 123.47,
    'Bb3': 233.08, 'Eb4': 311.13, 'Ab3': 207.65, 'Bb4': 466.16, 'Eb5': 622.25,
    'F#4': 369.99, 'G#4': 415.30,
  };
  private N(name: string): number { return AudioSystem.NOTE[name] || 440; }

  // ================================================================
  // SFX — Short sounds
  // ================================================================

  private playSfxTone(freq: number, duration: number, type: OscType = 'square', freqEnd?: number, volume = 1.0): void {
    if (!this.audioSupported && this.initialized) return;
    if (!this.sfxEnabled || !this.ensureContext()) return;
    const ctx = this.ctx!;
    const now = ctx.currentTime;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(freq, now);
    if (freqEnd !== undefined) osc.frequency.linearRampToValueAtTime(freqEnd, now + duration);
    gain.gain.setValueAtTime(volume * this.sfxVolume, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + duration);
    osc.connect(gain);
    gain.connect(this.sfxGain!);
    osc.start(now);
    osc.stop(now + duration);
  }

  private playSfxArpeggio(freqs: number[], noteLen: number, type: OscType = 'square', volume = 1.0): void {
    if (!this.audioSupported && this.initialized) return;
    if (!this.sfxEnabled || !this.ensureContext()) return;
    const ctx = this.ctx!;
    freqs.forEach((freq, i) => {
      const start = ctx.currentTime + i * noteLen;
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.type = type;
      osc.frequency.setValueAtTime(freq, start);
      gain.gain.setValueAtTime(volume * this.sfxVolume, start);
      gain.gain.exponentialRampToValueAtTime(0.001, start + noteLen * 0.9);
      osc.connect(gain);
      gain.connect(this.sfxGain!);
      osc.start(start);
      osc.stop(start + noteLen);
    });
  }

  playMenuSelect(): void { this.playSfxTone(880, 0.06, 'sine', undefined, 0.5); }
  playMenuMove(): void { this.playSfxTone(440, 0.04, 'sine', undefined, 0.3); }

  playHit(): void {
    if (!this.audioSupported && this.initialized) return;
    if (!this.sfxEnabled || !this.ensureContext()) return;
    const ctx = this.ctx!;
    const now = ctx.currentTime;
    const bufSize = ctx.sampleRate * 0.08;
    const buffer = ctx.createBuffer(1, bufSize, ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufSize; i++) data[i] = (Math.random() * 2 - 1) * (1 - i / bufSize);
    const source = ctx.createBufferSource();
    source.buffer = buffer;
    const gain = ctx.createGain();
    gain.gain.setValueAtTime(this.sfxVolume * 0.4, now);
    gain.gain.exponentialRampToValueAtTime(0.001, now + 0.08);
    source.connect(gain);
    gain.connect(this.sfxGain!);
    source.start(now);
  }

  playCritical(): void { this.playHit(); this.playSfxTone(1000, 0.1, 'triangle', 500, 0.5); }
  playSuperEffective(): void { this.playSfxTone(400, 0.25, 'sine', 900, 0.5); }
  playNotEffective(): void { this.playSfxTone(500, 0.25, 'sine', 250, 0.4); }
  playCaptureBall(): void { this.playSfxTone(800, 0.3, 'sine', 200, 0.4); }
  playCaptureShake(): void { this.playSfxTone(250, 0.1, 'triangle', 320, 0.3); }
  playCaptureSuccess(): void { this.playSfxArpeggio([523.25, 659.25, 783.99, 1046.50], 0.15, 'sine', 0.6); }
  playCaptureFail(): void { this.playSfxTone(500, 0.35, 'sine', 180, 0.4); }
  playLevelUp(): void { this.playSfxArpeggio([523.25, 587.33, 659.25, 783.99, 880], 0.1, 'sine', 0.5); }
  playHeal(): void { this.playSfxArpeggio([440, 523.25, 659.25, 880], 0.18, 'sine', 0.3); }
  playFaint(): void { this.playSfxTone(500, 0.5, 'sine', 120, 0.35); }

  // ================================================================
  // Music — Rendered to buffer, looped smoothly
  // ================================================================

  stopMusic(): void {
    this.currentMusicType = '';
    if (this.musicLoopTimer !== null) {
      clearInterval(this.musicLoopTimer);
      this.musicLoopTimer = null;
    }
    for (const src of this.currentMusicSources) {
      try { src.stop(); } catch (_e) { /* ignore */ }
    }
    this.currentMusicSources = [];
  }

  /** Render note events into an AudioBuffer and loop it */
  private renderAndLoop(events: NoteEvent[], loopDuration: number, tag: string): void {
    if (!this.audioSupported && this.initialized) return;
    if (!this.musicEnabled || !this.ensureContext()) return;
    if (this.currentMusicType === tag) return; // already playing
    this.stopMusic();
    this.currentMusicType = tag;

    const ctx = this.ctx!;
    const sampleRate = ctx.sampleRate;
    const totalSamples = Math.ceil(loopDuration * sampleRate);
    const buffer = ctx.createBuffer(1, totalSamples, sampleRate);
    const output = buffer.getChannelData(0);

    // Render each note event into the buffer
    for (const ev of events) {
      const startSample = Math.floor(ev.start * sampleRate);
      const endSample = Math.min(Math.floor((ev.start + ev.dur) * sampleRate), totalSamples);
      const noteLen = endSample - startSample;
      if (noteLen <= 0) continue;

      const phase_inc = (2 * Math.PI * ev.freq) / sampleRate;
      let phase = 0;

      for (let i = 0; i < noteLen; i++) {
        // Envelope: attack 5%, sustain 70%, release 25%
        const t = i / noteLen;
        let env: number;
        if (t < 0.05) env = t / 0.05;
        else if (t < 0.75) env = 1.0;
        else env = (1.0 - t) / 0.25;

        // Waveform
        let sample: number;
        switch (ev.type) {
          case 'sine':
            sample = Math.sin(phase);
            break;
          case 'triangle':
            sample = 2 * Math.abs(2 * ((phase / (2 * Math.PI)) % 1) - 1) - 1;
            break;
          case 'square':
            sample = Math.sin(phase) > 0 ? 0.5 : -0.5; // softer square
            break;
          case 'sawtooth':
            sample = 2 * ((phase / (2 * Math.PI)) % 1) - 1;
            break;
          default:
            sample = Math.sin(phase);
        }

        const idx = startSample + i;
        if (idx < totalSamples) {
          output[idx] += sample * env * ev.vol * 0.15;
        }
        phase += phase_inc;
      }
    }

    // Soft clamp
    for (let i = 0; i < totalSamples; i++) {
      output[i] = Math.tanh(output[i]);
    }

    // Create looping source
    const source = ctx.createBufferSource();
    source.buffer = buffer;
    source.loop = true;
    source.connect(this.musicGain!);
    source.start();
    this.currentMusicSources.push(source);
  }

  // ---- Helper to build note events ----
  private buildMelody(
    notes: [string, number][], // [noteName, durationInBeats]
    startBeat: number,
    bpm: number,
    type: OscType,
    vol: number
  ): NoteEvent[] {
    const beatDur = 60 / bpm;
    const events: NoteEvent[] = [];
    let beat = startBeat;
    for (const [name, beats] of notes) {
      if (name !== 'R') { // R = rest
        events.push({
          freq: this.N(name),
          start: beat * beatDur,
          dur: beats * beatDur * 0.9, // slight gap between notes
          type,
          vol,
        });
      }
      beat += beats;
    }
    return events;
  }

  // ================================================================
  // Town Music — Warm, pleasant, 10s loop
  // Key: C major, BPM: 100, Gentle sine melody + triangle pad + soft bass
  // ================================================================
  playTownMusic(): void {
    const bpm = 100;
    const beatDur = 60 / bpm;

    // Melody (sine) — gentle, flowing, 16 beats = ~10s
    const melody = this.buildMelody([
      ['E4', 1], ['G4', 1], ['A4', 1.5], ['G4', 0.5],
      ['E4', 1], ['D4', 1], ['C4', 2],
      ['D4', 1], ['E4', 1], ['G4', 1.5], ['E4', 0.5],
      ['D4', 1], ['C4', 1], ['D4', 1], ['E4', 1],
    ], 0, bpm, 'sine', 0.6);

    // Counter melody (triangle, softer, higher) — 16 beats
    const counter = this.buildMelody([
      ['C5', 2], ['B4', 2],
      ['A4', 2], ['G4', 2],
      ['A4', 2], ['B4', 2],
      ['C5', 2], ['G4', 2],
    ], 0, bpm, 'triangle', 0.25);

    // Pad chords (sine, very soft, long notes)
    const pad: NoteEvent[] = [];
    const chords: [string[], number][] = [
      [['C3', 'E3', 'G3'], 4],
      [['A2', 'C3', 'E3'], 4],
      [['D3', 'F3', 'A3'], 4],
      [['G2', 'B2', 'D3'], 4],
    ];
    let padBeat = 0;
    for (const [chord, beats] of chords) {
      for (const note of chord) {
        pad.push({
          freq: this.N(note),
          start: padBeat * beatDur,
          dur: beats * beatDur * 0.95,
          type: 'sine',
          vol: 0.2,
        });
      }
      padBeat += beats;
    }

    // Bass (triangle, rhythmic)
    const bass = this.buildMelody([
      ['C2', 1], ['R', 1], ['C2', 0.5], ['R', 0.5], ['C2', 1],
      ['A2', 1], ['R', 1], ['A2', 0.5], ['R', 0.5], ['A2', 1],
      ['D2', 1], ['R', 1], ['D2', 0.5], ['R', 0.5], ['D2', 1],
      ['G2', 1], ['R', 1], ['G2', 0.5], ['R', 0.5], ['G2', 1],
    ], 0, bpm, 'triangle', 0.35);

    const loopDur = 16 * beatDur; // ~9.6s
    this.renderAndLoop([...melody, ...counter, ...pad, ...bass], loopDur, 'town');
  }

  // ================================================================
  // Battle Music — Energetic, driving, 10s loop
  // Key: A minor, BPM: 140, Square lead + triangle bass + rhythmic pulse
  // ================================================================
  playBattleMusic(): void {
    const bpm = 140;
    const beatDur = 60 / bpm;

    // Lead melody (square, punchy) — 20 beats = ~8.5s
    const melody = this.buildMelody([
      ['A4', 0.5], ['C5', 0.5], ['E5', 1], ['D5', 0.5], ['C5', 0.5],
      ['A4', 1], ['G4', 0.5], ['A4', 0.5],
      ['R', 0.5], ['A4', 0.5], ['B4', 0.5], ['C5', 0.5],
      ['D5', 1], ['C5', 0.5], ['B4', 0.5],
      ['A4', 1], ['R', 1],
      ['E4', 0.5], ['A4', 0.5], ['C5', 1],
      ['B4', 0.5], ['A4', 0.5], ['G4', 0.5], ['A4', 0.5],
      ['E5', 1], ['D5', 0.5], ['C5', 0.5], ['A4', 1],
    ], 0, bpm, 'square', 0.4);

    // Driving bass (triangle)
    const bass = this.buildMelody([
      ['A2', 0.5], ['R', 0.5], ['A2', 0.5], ['A2', 0.5],
      ['A2', 0.5], ['R', 0.5], ['E2', 0.5], ['E2', 0.5],
      ['F2', 0.5], ['R', 0.5], ['F2', 0.5], ['F2', 0.5],
      ['G2', 0.5], ['R', 0.5], ['G2', 0.5], ['E2', 0.5],
      ['A2', 0.5], ['R', 0.5], ['A2', 0.5], ['A2', 0.5],
      ['C3', 0.5], ['R', 0.5], ['C3', 0.5], ['B2', 0.5],
      ['A2', 0.5], ['R', 0.5], ['E2', 0.5], ['E2', 0.5],
      ['A2', 1], ['R', 0.5], ['A2', 0.5],
    ], 0, bpm, 'triangle', 0.45);

    // Rhythmic pulse (very soft square, percussive feel)
    const pulse: NoteEvent[] = [];
    const totalBeats = 20;
    for (let b = 0; b < totalBeats; b++) {
      pulse.push({
        freq: b % 2 === 0 ? 80 : 60,
        start: b * beatDur,
        dur: beatDur * 0.3,
        type: 'square',
        vol: 0.15,
      });
    }

    const loopDur = totalBeats * beatDur; // ~8.6s
    this.renderAndLoop([...melody, ...bass, ...pulse], loopDur, 'battle');
  }

  // ================================================================
  // Title Music — Atmospheric, mysterious, 12s loop
  // Key: C major → Am, slow, ambient pads
  // ================================================================
  playTitleMusic(): void {
    const bpm = 60;
    const beatDur = 60 / bpm;

    // Slow ethereal melody (sine)
    const melody = this.buildMelody([
      ['E4', 2], ['R', 1], ['G4', 2], ['R', 1],
      ['A4', 3], ['G4', 1], ['E4', 2],
    ], 0, bpm, 'sine', 0.4);

    // Ambient pad chords
    const pad: NoteEvent[] = [];
    const chords: [string[], number][] = [
      [['C3', 'E3', 'G3'], 4],
      [['A2', 'C3', 'E3'], 4],
      [['F2', 'A2', 'C3'], 4],
    ];
    let padBeat = 0;
    for (const [chord, beats] of chords) {
      for (const note of chord) {
        pad.push({
          freq: this.N(note),
          start: padBeat * beatDur,
          dur: beats * beatDur,
          type: 'sine',
          vol: 0.18,
        });
      }
      padBeat += beats;
    }

    // Deep bass drone
    const bass: NoteEvent[] = [
      { freq: this.N('C2'), start: 0, dur: 4 * beatDur, type: 'sine', vol: 0.2 },
      { freq: this.N('A2'), start: 4 * beatDur, dur: 4 * beatDur, type: 'sine', vol: 0.2 },
      { freq: this.N('F2'), start: 8 * beatDur, dur: 4 * beatDur, type: 'sine', vol: 0.2 },
    ];

    const loopDur = 12 * beatDur; // 12s
    this.renderAndLoop([...melody, ...pad, ...bass], loopDur, 'title');
  }

  // ================================================================
  // Route Music — Adventurous, upbeat, 10s loop
  // Key: G major, BPM: 120
  // ================================================================
  playRouteMusic(): void {
    const bpm = 120;
    const beatDur = 60 / bpm;

    const melody = this.buildMelody([
      ['G4', 1], ['A4', 0.5], ['B4', 0.5], ['D5', 1], ['B4', 1],
      ['A4', 0.5], ['G4', 0.5], ['A4', 1], ['R', 1],
      ['B4', 1], ['D5', 0.5], ['E5', 0.5], ['D5', 1], ['B4', 1],
      ['A4', 0.5], ['G4', 0.5], ['A4', 1], ['G4', 1],
      ['E4', 1], ['G4', 1], ['A4', 1.5], ['G4', 0.5],
    ], 0, bpm, 'triangle', 0.45);

    const bass = this.buildMelody([
      ['G2', 1], ['R', 0.5], ['G2', 0.5], ['B2', 1], ['R', 1],
      ['D2', 1], ['R', 0.5], ['D2', 0.5], ['G2', 1], ['R', 1],
      ['E2', 1], ['R', 0.5], ['E2', 0.5], ['G2', 1], ['R', 1],
      ['C2', 1], ['R', 0.5], ['D2', 0.5], ['G2', 1], ['R', 1],
    ], 0, bpm, 'triangle', 0.3);

    // Soft pad
    const pad: NoteEvent[] = [];
    const chords: [string[], number][] = [
      [['G3', 'B3', 'D4'], 4],
      [['D3', 'F#4', 'A3'], 4],
      [['E3', 'G3', 'B3'], 4],
      [['C3', 'E3', 'G3'], 4],
    ];
    let padBeat = 0;
    for (const [chord, beats] of chords) {
      for (const note of chord) {
        pad.push({
          freq: this.N(note),
          start: padBeat * beatDur,
          dur: beats * beatDur * 0.95,
          type: 'sine',
          vol: 0.12,
        });
      }
      padBeat += beats;
    }

    const loopDur = 16 * beatDur; // 8s
    this.renderAndLoop([...melody, ...bass, ...pad], loopDur, 'route');
  }

  // ================================================================
  // Victory jingle — Short, no loop
  // ================================================================
  playVictoryMusic(): void {
    if (!this.audioSupported && this.initialized) return;
    if (!this.musicEnabled || !this.ensureContext()) return;
    this.stopMusic();
    this.currentMusicType = 'victory';
    this.playSfxArpeggio(
      [523.25, 659.25, 783.99, 659.25, 783.99, 880, 1046.50],
      0.15, 'sine', 0.5
    );
  }

  // ================================================================
  // Gym Music — Tense, determined, 10s loop
  // Key: D minor, BPM: 130
  // ================================================================
  playGymMusic(): void {
    const bpm = 130;
    const beatDur = 60 / bpm;

    const melody = this.buildMelody([
      ['D4', 0.5], ['F4', 0.5], ['A4', 1], ['G4', 0.5], ['F4', 0.5],
      ['E4', 1], ['D4', 0.5], ['E4', 0.5],
      ['F4', 1], ['A4', 0.5], ['G4', 0.5], ['F4', 1],
      ['E4', 0.5], ['D4', 0.5], ['C4', 1], ['D4', 1],
      ['R', 0.5], ['D4', 0.5], ['F4', 1], ['A4', 1],
      ['Bb4', 1], ['A4', 0.5], ['G4', 0.5], ['F4', 1],
    ], 0, bpm, 'square', 0.35);

    const bass = this.buildMelody([
      ['D2', 0.5], ['R', 0.5], ['D2', 0.5], ['D2', 0.5],
      ['D2', 0.5], ['R', 0.5], ['A2', 0.5], ['A2', 0.5],
      ['F2', 0.5], ['R', 0.5], ['F2', 0.5], ['C3', 0.5],
      ['G2', 0.5], ['R', 0.5], ['G2', 0.5], ['D2', 0.5],
      ['D2', 0.5], ['R', 0.5], ['D2', 0.5], ['D2', 0.5],
      ['Bb3', 0.5], ['R', 0.5], ['A2', 0.5], ['D2', 0.5],
    ], 0, bpm, 'triangle', 0.4);

    const loopDur = 16 * beatDur; // ~7.4s
    this.renderAndLoop([...melody, ...bass], loopDur, 'gym');
  }

  // ================================================================
  // Toggle controls
  // ================================================================

  toggleMusic(): boolean {
    this.musicEnabled = !this.musicEnabled;
    if (!this.musicEnabled) this.stopMusic();
    return this.musicEnabled;
  }

  toggleSfx(): boolean {
    this.sfxEnabled = !this.sfxEnabled;
    return this.sfxEnabled;
  }

  isMusicEnabled(): boolean { return this.musicEnabled; }
  isSfxEnabled(): boolean { return this.sfxEnabled; }
}
