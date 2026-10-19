// Renders every mockup page in this folder to Document/Images/Proposal/<Name>.png
//
//   npm install playwright
//   node shot.mjs            # all pages
//   node shot.mjs Transit    # one page
//
// Set MOCKUP_CHROMIUM to use a Chromium binary that is already on the machine.

import { readdirSync, mkdirSync } from 'node:fs';
import { basename, dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const out = resolve(here, '..', 'Images', 'Proposal');

const candidates = ['playwright', '/opt/node-tools/node_modules/playwright/index.mjs'];
let chromium;
for (const candidate of candidates) {
  try {
    ({ chromium } = await import(candidate));
    break;
  } catch {
    // Try the next location.
  }
}
if (!chromium) {
  throw new Error('playwright was not found. Run "npm install playwright" first.');
}

mkdirSync(out, { recursive: true });

const args = process.argv.slice(2);
const pages = (args.length > 0 ? args : readdirSync(here).filter((x) => x.endsWith('.html')))
  .map((x) => basename(x, '.html'))
  .sort();

const browser = await chromium.launch({ executablePath: process.env.MOCKUP_CHROMIUM || undefined });
const page = await browser.newPage({ viewport: { width: 1080, height: 2424 }, deviceScaleFactor: 1 });

page.on('pageerror', (e) => console.error('error', e.message));

for (const name of pages) {
  await page.goto(`file://${join(here, `${name}.html`)}`);
  await page.waitForTimeout(350);
  await page.screenshot({ path: join(out, `${name}.png`) });
  console.log('rendered', name);
}

await browser.close();
