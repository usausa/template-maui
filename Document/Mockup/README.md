# Mockup

Source of the proposal screen images in `../Images/Proposal`.

Each page is rendered with Chromium at 1080x2424, the same size as the device screenshots in `../Images`.
`common.css` reproduces the application shell (status bar / header / function bar) and loads the fonts and
the Material palette from `Template.MobileApp/Resources`. It sets `1rem = 1dp`, so sizes can be written
with the same numbers as the XAML styles.

```
npm install playwright
node shot.mjs            # render every page
node shot.mjs Transit    # render one page
```

Set `MOCKUP_CHROMIUM` to reuse a Chromium binary that is already installed.
