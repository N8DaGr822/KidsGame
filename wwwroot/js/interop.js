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

// Finds which placed sticker (if any) sits at (x, y) - CSS-space
// coordinates relative to the stage, same as drawStart/floodFill take.
// Checked in reverse DOM order (last-placed/topmost first) against each
// sticker <img>'s own rendered bounding box - a rotated sticker's box is
// its axis-aligned enclosing rectangle, a deliberately generous
// approximation rather than per-pixel alpha testing. Returns the index
// into the stage's own .du-placed-sticker-img list (which the Blazor side
// maps back to a PlacedSticker), or -1 if the point hit no sticker.
export function findStickerAt(stageEl, x, y) {
    const stageRect = stageEl.getBoundingClientRect();
    const clientX = stageRect.left + x;
    const clientY = stageRect.top + y;
    const stickers = stageEl.querySelectorAll('.du-placed-sticker-img');
    for (let i = stickers.length - 1; i >= 0; i--) {
        const r = stickers[i].getBoundingClientRect();
        if (clientX >= r.left && clientX <= r.right && clientY >= r.top && clientY <= r.bottom) {
            return i;
        }
    }
    return -1;
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

function hexToRgba(hex) {
    const h = hex.replace('#', '');
    const full = h.length === 3 ? h.split('').map((c) => c + c).join('') : h;
    const n = parseInt(full, 16);
    return [(n >> 16) & 255, (n >> 8) & 255, n & 255, 255];
}

// Squared color distance rather than exact equality - canvas strokes are
// anti-aliased, so a strict match would leave a thin ring of untouched
// pixels right at every stroke edge instead of filling flush against it.
function colorsClose(data, idx, r, g, b, a, tolerance) {
    const dr = data[idx] - r;
    const dg = data[idx + 1] - g;
    const db = data[idx + 2] - b;
    const da = data[idx + 3] - a;
    return dr * dr + dg * dg + db * db + da * da <= tolerance * tolerance;
}

const FILL_TOLERANCE = 48;

// Paint-bucket fill of the contiguous region under (x, y), like Paint's
// fill tool: flood-fills every 4-connected pixel whose color is close to
// the clicked pixel's. x/y are CSS-space coordinates (same space
// drawStart/drawMove take) - converted to backing-store pixels via DPR
// since getImageData/putImageData operate on the raw canvas buffer.
// Returns whether anything actually changed, so the caller only records
// an undo step when there's something to undo.
export function floodFill(canvas, x, y, colorHex) {
    const ctx = getDrawContext(canvas);
    const dpr = window.devicePixelRatio || 1;
    const width = canvas.width;
    const height = canvas.height;
    const startX = Math.floor(x * dpr);
    const startY = Math.floor(y * dpr);
    if (startX < 0 || startY < 0 || startX >= width || startY >= height) return false;

    const imageData = ctx.getImageData(0, 0, width, height);
    const data = imageData.data;

    const startIdx = (startY * width + startX) * 4;
    const startR = data[startIdx];
    const startG = data[startIdx + 1];
    const startB = data[startIdx + 2];
    const startA = data[startIdx + 3];
    const [fillR, fillG, fillB, fillA] = hexToRgba(colorHex);

    if (colorsClose(data, startIdx, fillR, fillG, fillB, fillA, FILL_TOLERANCE)) return false;

    // Snapshot before the fill, same as drawStart does before a stroke -
    // keeps this indistinguishable from a stroke to the undo stack.
    const stack = drawUndoStacks.get(canvas);
    stack.push(ctx.getImageData(0, 0, width, height));
    if (stack.length > MAX_UNDO_STROKES) stack.shift();

    const visited = new Uint8Array(width * height);
    const pixelStack = [startY * width + startX];
    visited[startY * width + startX] = 1;

    while (pixelStack.length > 0) {
        const packed = pixelStack.pop();
        const py = (packed / width) | 0;
        const px = packed % width;
        const idx = packed * 4;
        data[idx] = fillR;
        data[idx + 1] = fillG;
        data[idx + 2] = fillB;
        data[idx + 3] = fillA;

        const neighbors = [
            [px + 1, py], [px - 1, py], [px, py + 1], [px, py - 1],
        ];
        for (const [nx, ny] of neighbors) {
            if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
            const nPacked = ny * width + nx;
            if (visited[nPacked]) continue;
            const nIdx = nPacked * 4;
            if (!colorsClose(data, nIdx, startR, startG, startB, startA, FILL_TOLERANCE)) continue;
            visited[nPacked] = 1;
            pixelStack.push(nPacked);
        }
    }

    ctx.putImageData(imageData, 0, 0);
    return true;
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
export async function exportOutfit(stageEl, baseImageEl, emojiStickers, drawCanvas, filename, sceneUrl, sceneAligned) {
    const stageRect = stageEl.getBoundingClientRect();
    const width = Math.max(1, Math.round(stageRect.width));
    const height = Math.max(1, Math.round(stageRect.height));

    const outCanvas = document.createElement('canvas');
    outCanvas.width = width;
    outCanvas.height = height;
    const ctx = outCanvas.getContext('2d');

    // Flat fallback in case the scene image below fails to load in time -
    // matches .du-stage's own background-color fallback, so there's never
    // a transparent gap around the character either way.
    ctx.fillStyle = '#eef1fb';
    ctx.fillRect(0, 0, width, height);

    const baseRect = baseImageEl.getBoundingClientRect();

    // Draw the selected scene the same way .du-stage's CSS does (see
    // DressUpGame.razor.css): the aligned "Sky" scene shares the
    // character's own sizing rule exactly, so it's drawn at baseRect's
    // own position/size rather than recomputing that math here; the rest
    // are plain landscape art at much lower native resolution than the
    // stage, sized like background-size:contain so they're never
    // upscaled past their native size into a blurry, over-zoomed crop.
    if (sceneUrl) {
        try {
            const sceneImg = await loadImage(sceneUrl);
            if (sceneAligned) {
                ctx.drawImage(
                    sceneImg,
                    baseRect.left - stageRect.left,
                    baseRect.top - stageRect.top,
                    baseRect.width,
                    baseRect.height
                );
            } else {
                const containScale = Math.min(width / sceneImg.naturalWidth, height / sceneImg.naturalHeight);
                const drawWidth = sceneImg.naturalWidth * containScale;
                const drawHeight = sceneImg.naturalHeight * containScale;
                ctx.drawImage(sceneImg, (width - drawWidth) / 2, (height - drawHeight) / 2, drawWidth, drawHeight);
            }
        } catch {
            // Flat fallback fill above already covers this.
        }
    }

    // Image-backed stickers are real <img> elements on the stage - draw
    // each at its actual on-screen position/size rather than
    // reimplementing the drag-position-to-CSS math here. Rotation is a
    // CSS transform on the parent .du-placed-sticker, which enlarges what
    // getBoundingClientRect reports (the rotated bounding box) rather
    // than the image's true size - offsetWidth/offsetHeight are layout
    // dimensions and unaffected by transforms, so those give the real
    // (unrotated) size, while the bounding box's center point is still
    // accurate (rotating about the center doesn't move the center).
    //
    // Wings use the "behind" class (see .du-placed-sticker.behind) to
    // render behind the character on screen - split them out and draw
    // them before the base image so the export matches, instead of every
    // sticker landing on top of the character regardless of that class.
    const stickerImgs = [...stageEl.querySelectorAll('.du-placed-sticker-img')];
    const behindImgs = stickerImgs.filter((img) => img.closest('.du-placed-sticker')?.classList.contains('behind'));
    const frontImgs = stickerImgs.filter((img) => !behindImgs.includes(img));

    for (const img of behindImgs) {
        drawStickerImg(ctx, img, stageRect);
    }

    ctx.drawImage(
        baseImageEl,
        baseRect.left - stageRect.left,
        baseRect.top - stageRect.top,
        baseRect.width,
        baseRect.height
    );

    for (const img of frontImgs) {
        drawStickerImg(ctx, img, stageRect);
    }

    // Stickers still on emoji fallback (no matching art yet) have no DOM
    // image to read, so those are passed in and drawn as text.
    if (emojiStickers && emojiStickers.length) {
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        const baseFontPx = height * 0.12;
        for (const sticker of emojiStickers) {
            const x = (sticker.xPercent / 100) * width;
            const y = (sticker.yPercent / 100) * height;
            const scale = sticker.scale || 1;
            const rotation = ((sticker.rotationDeg || 0) * Math.PI) / 180;

            ctx.save();
            ctx.translate(x, y);
            ctx.rotate(rotation);
            ctx.font = `${Math.round(baseFontPx * scale)}px "Segoe UI Emoji", "Noto Color Emoji", sans-serif`;
            ctx.fillText(sticker.emoji, 0, 0);
            ctx.restore();
        }
    }

    ctx.drawImage(drawCanvas, 0, 0, width, height);

    const link = document.createElement('a');
    link.download = filename;
    link.href = outCanvas.toDataURL('image/png');
    document.body.appendChild(link);
    link.click();
    link.remove();
}

// Shared by exportOutfit's behind/front sticker passes above.
function drawStickerImg(ctx, img, stageRect) {
    const stickerEl = img.closest('.du-placed-sticker');
    const r = img.getBoundingClientRect();
    const centerX = r.left + r.width / 2 - stageRect.left;
    const centerY = r.top + r.height / 2 - stageRect.top;
    const w = img.offsetWidth;
    const h = img.offsetHeight;
    const rotateDeg = stickerEl ? parseFloat(getComputedStyle(stickerEl).getPropertyValue('--du-rotate')) || 0 : 0;

    // A Fill-tool tint on this sticker is a CSS mask on a sibling
    // .du-placed-sticker-tint div (see the markup) rather than a pixel
    // change to the <img> itself, so it has to be reapplied here via
    // canvas compositing or the exported PNG would silently lose it.
    // Composited on its own small canvas first (source-in recolors only
    // where the sticker art is opaque) rather than directly on ctx's
    // canvas, which already has other content that source-in would
    // otherwise clobber.
    const tintEl = stickerEl ? stickerEl.querySelector('.du-placed-sticker-tint') : null;
    const tintColor = tintEl ? tintEl.style.backgroundColor : '';

    ctx.save();
    ctx.translate(centerX, centerY);
    ctx.rotate((rotateDeg * Math.PI) / 180);
    if (tintColor) {
        const tw = Math.max(1, Math.round(w));
        const th = Math.max(1, Math.round(h));
        const tintCanvas = document.createElement('canvas');
        tintCanvas.width = tw;
        tintCanvas.height = th;
        const tctx = tintCanvas.getContext('2d');
        tctx.drawImage(img, 0, 0, tw, th);
        tctx.globalCompositeOperation = 'source-in';
        tctx.fillStyle = tintColor;
        tctx.fillRect(0, 0, tw, th);
        ctx.drawImage(tintCanvas, -w / 2, -h / 2, w, h);
    } else {
        ctx.drawImage(img, -w / 2, -h / 2, w, h);
    }
    ctx.restore();
}

function loadImage(src) {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = reject;
        img.src = src;
    });
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

// ---- Fullscreen (tablet kiosk-style play) --------------------------------
//
// Fullscreens the whole page (not just one component's element) since the
// point is hiding the browser chrome/address bar on a tablet. The watcher
// notifies Blazor on ANY fullscreen change (including the kid hitting Esc
// or swiping to exit at the OS level) so a toggle button stays in sync
// instead of just assuming its own last action succeeded.

let fullscreenDotNetRef = null;

function notifyFullscreenChange() {
    const active = !!(document.fullscreenElement || document.webkitFullscreenElement);
    fullscreenDotNetRef?.invokeMethodAsync('OnFullscreenChanged', active);
}

export function watchFullscreen(dotNetRef) {
    fullscreenDotNetRef = dotNetRef;
    document.addEventListener('fullscreenchange', notifyFullscreenChange);
    document.addEventListener('webkitfullscreenchange', notifyFullscreenChange);
}

export function unwatchFullscreen() {
    document.removeEventListener('fullscreenchange', notifyFullscreenChange);
    document.removeEventListener('webkitfullscreenchange', notifyFullscreenChange);
    fullscreenDotNetRef = null;
}

export function requestFullscreen() {
    const el = document.documentElement;
    if (el.requestFullscreen) return el.requestFullscreen();
    if (el.webkitRequestFullscreen) return el.webkitRequestFullscreen();
}

export function exitFullscreen() {
    if (document.exitFullscreen) return document.exitFullscreen();
    if (document.webkitExitFullscreen) return document.webkitExitFullscreen();
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

export function playUnoPlaySound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    tone(ctx, 440, ctx.currentTime, 0.1, 'triangle', 0.15);
}

const SIMON_TONE_FREQS = [329.63, 261.63, 220.0, 164.81]; // green, red, yellow, blue

export function playSimonTone(index) {
    const ctx = getAudioContext();
    if (!ctx) return;
    const freq = SIMON_TONE_FREQS[index] ?? 261.63;
    tone(ctx, freq, ctx.currentTime, 0.35, 'sine', 0.22);
}

export function playSimonErrorSound() {
    const ctx = getAudioContext();
    if (!ctx) return;
    slide(ctx, 200, 80, ctx.currentTime, 0.4, 'sawtooth', 0.2);
}

// C4 through C5 - one octave, white keys only, for Baby Piano.
const PIANO_NOTE_FREQS = [261.63, 293.66, 329.63, 349.23, 392.0, 440.0, 493.88, 523.25];

export function playPianoNote(index) {
    const ctx = getAudioContext();
    if (!ctx) return;
    const freq = PIANO_NOTE_FREQS[index % PIANO_NOTE_FREQS.length];
    tone(ctx, freq, ctx.currentTime, 0.5, 'sine', 0.22);
}

const DRUM_PAD_FREQS = [90, 120, 150, 180, 210, 240, 270, 300];

export function playDrumHit(index) {
    const ctx = getAudioContext();
    if (!ctx) return;
    const freq = DRUM_PAD_FREQS[index % DRUM_PAD_FREQS.length];
    noiseBurst(ctx, ctx.currentTime, 0.16, 0.22, freq + 400);
    tone(ctx, freq, ctx.currentTime, 0.15, 'sine', 0.22);
}
