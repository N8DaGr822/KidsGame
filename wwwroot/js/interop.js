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
