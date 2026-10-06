import { readFile } from 'node:fs/promises';
import { runInNewContext } from 'node:vm';
import test from 'node:test';
import assert from 'node:assert/strict';

const source = await readFile(new URL('../wwwroot/service-worker.published.js', import.meta.url), 'utf8');

function worker(scope = 'https://example.test/KidsGame/') {
    const listeners = new Map();
    const cached = new Map();
    const installed = [];
    let skipped = false;
    const cache = {
        async match(request) { return cached.get(typeof request === 'string' ? new URL(request, scope).href : request.url); },
        async addAll(requests) { installed.push(...requests); }
    };
    runInNewContext(source, {
        self: {
            registration: { scope },
            assetsManifest: { version: 'test', assets: [{ url: 'index.html', hash: 'test' }, { url: 'images/thumb.svg', hash: 'test' }] },
            importScripts() {}, addEventListener(name, callback) { listeners.set(name, callback); },
            skipWaiting() { skipped = true; }, clients: { async claim() {} }
        },
        URL,
        Request: class { constructor(url, options) { this.url = new URL(url, scope).href; this.integrity = options.integrity; } },
        caches: { async open() { return cache; }, async keys() { return []; } },
        fetch: async request => `network:${request.url}`,
        console: { info() {} }
    });
    return {
        cached, installed, skipped: () => skipped,
        async fetch(path, mode = 'navigate', method = 'GET') {
            let response;
            listeners.get('fetch')({ request: { url: new URL(path, scope).href, mode, method }, respondWith(value) { response = value; } });
            return response;
        },
        async install() {
            let completion;
            listeners.get('install')({ waitUntil(value) { completion = value; } });
            await completion;
        }
    };
}

for (const scope of ['https://example.test/', 'https://example.test/KidsGame/']) {
    test(`launcher routes use the app shell under ${scope}`, async () => {
        const sw = worker(scope);
        sw.cached.set(`${scope}index.html`, 'launcher');
        for (const route of ['', 'games', 'play/abc', 'admin', 'admin/access/kid'])
            assert.equal(await sw.fetch(route), 'launcher');
    });

    test(`embedded games retain their own HTML under ${scope}`, async () => {
        const sw = worker(scope);
        sw.cached.set(`${scope}index.html`, 'launcher');
        for (const game of ['crown-and-banner', 'highway-hauler', 'truck-repair-bay']) {
            const path = `games/${game}/index.html`;
            sw.cached.set(`${scope}${path}`, game);
            assert.equal(await sw.fetch(path), game);
        }
    });
}

test('external navigations and non-GET requests do not receive the launcher', async () => {
    const sw = worker();
    sw.cached.set('https://example.test/KidsGame/index.html', 'launcher');
    assert.equal(await sw.fetch('https://elsewhere.test/play/abc'), 'network:https://elsewhere.test/play/abc');
    assert.equal(await sw.fetch('play/abc', 'navigate', 'POST'), 'network:https://example.test/KidsGame/play/abc');
    assert.equal(await sw.fetch('/outside'), 'network:https://example.test/outside');
});

test('installation includes SVG thumbnails and lets existing games finish before updating', async () => {
    const sw = worker();
    await sw.install();
    assert.ok(sw.installed.some(request => request.url.endsWith('.svg')));
    assert.ok(sw.installed.every(request => request.integrity === 'test'));
    assert.equal(sw.skipped(), false);
});
