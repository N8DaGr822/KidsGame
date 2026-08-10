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

function getDrawContext(canvas) {
    const dpr = window.devicePixelRatio || 1;
    const cssWidth = canvas.clientWidth;
    const cssHeight = canvas.clientHeight;
    let entry = drawContexts.get(canvas);

    // (Re)size the backing store to match the canvas's current on-screen
    // size, scaled for device pixel ratio so strokes stay crisp. Only
    // happens on first use or if the stage actually reflows (e.g. a
    // window resize) - not on every stroke - but note it does clear any
    // existing drawing when it happens.
    if (!entry || entry.cssWidth !== cssWidth || entry.cssHeight !== cssHeight) {
        canvas.width = cssWidth * dpr;
        canvas.height = cssHeight * dpr;
        const ctx = canvas.getContext('2d');
        ctx.scale(dpr, dpr);
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        entry = { ctx, cssWidth, cssHeight };
        drawContexts.set(canvas, entry);
    }

    return entry.ctx;
}

export function drawStart(canvas, x, y, color, size) {
    const ctx = getDrawContext(canvas);
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
