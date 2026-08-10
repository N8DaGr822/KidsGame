export function scrollCarousel(element, amount) {
    if (!element) return;
    element.scrollBy({ left: amount, behavior: 'smooth' });
}

export function getBoundingRect(element) {
    const r = element.getBoundingClientRect();
    return { left: r.left, top: r.top, width: r.width, height: r.height };
}

// Keeps pointermove/pointerup targeting `element` for the rest of this
// gesture even if the pointer moves outside its bounds (fast finger drags
// on touch devices leave small elements very easily otherwise).
export function setPointerCapture(element, pointerId) {
    element.setPointerCapture(pointerId);
}

// ---- Freehand drawing on a <canvas> overlay --------------------------
//
// Drawing happens entirely here rather than round-tripping every
// pointermove through Blazor - there's nothing to re-render, and this
// keeps fast strokes smooth.

const drawContexts = new WeakMap();
const drawUndoStacks = new WeakMap();
const MAX_UNDO_STROKES = 20;

function getDrawContext(canvas) {
    const dpr = window.devicePixelRatio || 1;
    const cssWidth = canvas.clientWidth;
    const cssHeight = canvas.clientHeight;
    let entry = drawContexts.get(canvas);

    // (Re)size the backing store to match the canvas's current on-screen
    // size, scaled for device pixel ratio so strokes stay crisp. Only
    // happens on first use or if the stage actually reflows (e.g. a
    // window resize) - but note it does clear any existing drawing (and
    // its undo history, since old snapshots would be the wrong size) when
    // it happens.
    if (!entry || entry.cssWidth !== cssWidth || entry.cssHeight !== cssHeight) {
        canvas.width = cssWidth * dpr;
        canvas.height = cssHeight * dpr;
        const ctx = canvas.getContext('2d');
        ctx.scale(dpr, dpr);
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        entry = { ctx, cssWidth, cssHeight };
        drawContexts.set(canvas, entry);
        drawUndoStacks.set(canvas, []);
    }

    return entry.ctx;
}

export function drawStart(canvas, x, y, color, size) {
    const ctx = getDrawContext(canvas);

    // Snapshot before this stroke begins, so undo restores exactly what
    // was there before it - capped so a very long doodling session can't
    // grow this without bound.
    const stack = drawUndoStacks.get(canvas);
    stack.push(ctx.getImageData(0, 0, canvas.width, canvas.height));
    if (stack.length > MAX_UNDO_STROKES) stack.shift();

    ctx.strokeStyle = color;
    ctx.lineWidth = size;
    ctx.beginPath();
    ctx.moveTo(x, y);
}

export function drawMove(canvas, x, y) {
    const ctx = getDrawContext(canvas);
    ctx.lineTo(x, y);
    ctx.stroke();
}

export function drawEnd(canvas) {
    getDrawContext(canvas).closePath();
}

export function clearDrawCanvas(canvas) {
    const ctx = getDrawContext(canvas);
    ctx.clearRect(0, 0, canvas.clientWidth, canvas.clientHeight);
    const stack = drawUndoStacks.get(canvas);
    if (stack) stack.length = 0;
}

// Pops the most recent pre-stroke snapshot and restores it. Returns
// false if there's no stroke left to undo (caller decides what that
// means - e.g. falling through to undoing a sticker action instead).
export function undoLastStroke(canvas) {
    const stack = drawUndoStacks.get(canvas);
    if (!stack || stack.length === 0) return false;
    getDrawContext(canvas).putImageData(stack.pop(), 0, 0);
    return true;
}

// ---- Dress Up outfit export ---------------------------------------------
//
// Composites the character art, placed stickers, and the freehand drawing
// into one flat PNG and triggers a download - reads the *actual* rendered
// position/size of the character image straight off the DOM (via
// getBoundingClientRect) rather than reimplementing its CSS layout rules
// (centered, aspect-preserved, height-capped) in JS, so the export always
// matches what's on screen even if that CSS changes later.
export function exportOutfit(stageEl, baseImageEl, stickers, drawCanvas, filename) {
    const stageRect = stageEl.getBoundingClientRect();
    const width = Math.max(1, Math.round(stageRect.width));
    const height = Math.max(1, Math.round(stageRect.height));

    const outCanvas = document.createElement('canvas');
    outCanvas.width = width;
    outCanvas.height = height;
    const ctx = outCanvas.getContext('2d');

    // Matches the stage's own pastel backdrop so the export doesn't have
    // transparent edges around the character.
    ctx.fillStyle = '#eef1fb';
    ctx.fillRect(0, 0, width, height);

    const baseRect = baseImageEl.getBoundingClientRect();
    ctx.drawImage(
        baseImageEl,
        baseRect.left - stageRect.left,
        baseRect.top - stageRect.top,
        baseRect.width,
        baseRect.height
    );

    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    const stickerFontPx = Math.round(height * 0.12);
    ctx.font = `${stickerFontPx}px "Segoe UI Emoji", "Noto Color Emoji", sans-serif`;
    for (const sticker of stickers) {
        ctx.fillText(sticker.emoji, (sticker.xPercent / 100) * width, (sticker.yPercent / 100) * height);
    }

    ctx.drawImage(drawCanvas, 0, 0, width, height);

    const link = document.createElement('a');
    link.download = filename;
    link.href = outCanvas.toDataURL('image/png');
    document.body.appendChild(link);
    link.click();
    link.remove();
}

// ---- Spoken narration (Web Speech API) --------------------------------
//
// No audio assets to ship - this reads narrator/dialogue lines aloud so
// pre-readers can follow along. Silently does nothing on browsers without
// speechSynthesis support rather than erroring.

export function speak(text) {
    if (!('speechSynthesis' in window)) return;

    // Cancel any line still playing so lines never overlap/queue up
    // behind fast-advancing gameplay.
    window.speechSynthesis.cancel();

    const utterance = new SpeechSynthesisUtterance(text);
    utterance.rate = 0.92;
    utterance.pitch = 1.15;
    window.speechSynthesis.speak(utterance);
}

export function stopSpeaking() {
    if ('speechSynthesis' in window) window.speechSynthesis.cancel();
}

// ---- Simple procedural sound effects (Web Audio API) --------------------
//
// No audio files to ship - every effect below is a couple of oscillator
// or noise-buffer nodes. These only ever fire in direct response to a
// user-initiated action (a tap, a shot the player caused), which is
// exactly what's needed to unlock AudioContext playback in every browser,
// so there's no separate "enable sound" step for the player.

let audioCtx = null;

function getAudioContext() {
    const Ctx = window.AudioContext || window.webkitAudioContext;
    if (!Ctx) return null;
    if (!audioCtx) audioCtx = new Ctx();
    if (audioCtx.state === 'suspended') audioCtx.resume();
    return audioCtx;
}

function tone(ctx, freq, startTime, duration, type, peakGain) {
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(freq, startTime);
    gain.gain.setValueAtTime(0, startTime);
    gain.gain.linearRampToValueAtTime(peakGain, startTime + 0.015);
    gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start(startTime);
    osc.stop(startTime + duration + 0.02);
}

function slide(ctx, fromFreq, toFreq, startTime, duration, type, peakGain) {
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();
    osc.type = type;
    osc.frequency.setValueAtTime(fromFreq, startTime);
    osc.frequency.exponentialRampToValueAtTime(Math.max(toFreq, 1), startTime + duration);
    gain.gain.setValueAtTime(0, startTime);
    gain.gain.linearRampToValueAtTime(peakGain, startTime + 0.015);
    gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);
    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start(startTime);
    osc.stop(startTime + duration + 0.02);
}

function noiseBurst(ctx, startTime, duration, peakGain, lowpassFreq) {
    const bufferSize = Math.max(1, Math.ceil(ctx.sampleRate * duration));
    const buffer = ctx.createBuffer(1, bufferSize, ctx.sampleRate);
    const data = buffer.getChannelData(0);
    for (let i = 0; i < bufferSize; i++) {
        data[i] = (Math.random() * 2 - 1) * (1 - i / bufferSize);
    }

    const source = ctx.createBufferSource();
    source.buffer = buffer;

    const filter = ctx.createBiquadFilter();
    filter.type = 'lowpass';
    filter.frequency.setValueAtTime(lowpassFreq, startTime);

    const gain = ctx.createGain();
    gain.gain.setValueAtTime(peakGain, startTime);
    gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);

    source.connect(filter);
    filter.connect(gain);
    gain.connect(ctx.destination);
    source.start(startTime);
    source.stop(startTime + duration + 0.02);
}

export function playMatchSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    const t = ctx.currentTime;
    tone(ctx, 523.25, t, 0.12, 'triangle', 0.18);
    tone(ctx, 659.25, t + 0.09, 0.16, 'triangle', 0.18);
    tone(ctx, 783.99, t + 0.18, 0.22, 'triangle', 0.2);
}

export function playMismatchSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    slide(ctx, 220, 140, ctx.currentTime, 0.22, 'sine', 0.15);
}

export function playCatchSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    const t = ctx.currentTime;
    tone(ctx, 700, t, 0.08, 'sine', 0.2);
    tone(ctx, 1050, t + 0.06, 0.12, 'sine', 0.2);
}

export function playSplashSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    noiseBurst(ctx, ctx.currentTime, 0.18, 0.12, 1400);
}

export function playFireSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    slide(ctx, 180, 60, ctx.currentTime, 0.18, 'sawtooth', 0.16);
}

export function playImpactSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    noiseBurst(ctx, ctx.currentTime, 0.15, 0.2, 900);
}

export function playExplosionSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    const t = ctx.currentTime;
    noiseBurst(ctx, t, 0.55, 0.35, 700);
    slide(ctx, 160, 40, t, 0.4, 'sawtooth', 0.22);
}
