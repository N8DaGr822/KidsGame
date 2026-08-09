// In development this service worker is intentionally a no-op so you
// always see your latest changes without cache-busting tricks.
// The publish-time build swaps this out for service-worker.published.js,
// which does the real offline asset caching.
self.addEventListener('fetch', () => { });
