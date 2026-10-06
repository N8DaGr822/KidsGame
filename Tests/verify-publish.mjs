import { readFile } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import { resolve } from 'node:path';
import { runInNewContext } from 'node:vm';
import assert from 'node:assert/strict';

const root = resolve(process.argv[2] ?? 'publish/wwwroot');
const manifest = {};
runInNewContext(await readFile(resolve(root, 'service-worker-assets.js'), 'utf8'), { self: manifest });
for (const asset of manifest.assetsManifest.assets) {
    const content = await readFile(resolve(root, asset.url));
    const hash = `sha256-${createHash('sha256').update(content).digest('base64')}`;
    assert.equal(hash, asset.hash, `Published integrity mismatch: ${asset.url}`);
}
const index = await readFile(resolve(root, 'index.html'), 'utf8');
assert.match(index, /navigator\.serviceWorker\.register\('service-worker\.js'/);
assert.ok(!manifest.assetsManifest.assets.some(asset => asset.url.startsWith('Tests/')));
console.log(`Verified ${manifest.assetsManifest.assets.length} published asset hashes and service-worker registration.`);
