# Design

## Context

- `OpenWave.App.csproj` declares the icon with `<MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#512BD4" />`: the template's purple background square (`appicon.svg`) and the ".NET" foreground (`appiconfg.svg`).
- The MAUI resizetizer turns that item into every icon the platforms need at build time: the Android launcher icons (legacy and adaptive, with separate background and foreground layers) and the Windows `.ico` used by the unpackaged executable and its window.
- The logo `C:\Users\unair\Pictures\Screenshots\LogoFinalOW.png` is a 1000×1000 PNG. Its background is opaque white, and the drawing occupies x 100–896 and y 196–788, so about 80% of the width.
- Android adaptive icons are masked by the launcher (circle, rounded square, squircle) and only the central ~66% (72 of 108 dp) is guaranteed to be visible.

## Goals / Non-Goals

**Goals:**
- One source image for the icon on both platforms, kept in the repository.
- The whole logo visible under every Android mask.

**Non-Goals:**
- The splash screen (still the template's purple splash). It can be a later change.
- Redrawing or vectorising the logo; the PNG is used as provided.
- Any change to in-app icons or themes.

## Decisions

### 1. The logo as the icon foreground, on a white background

- Copy `LogoFinalOW.png` to `src/OpenWave.App/Resources/AppIcon/appiconfg.png`. Resizetizer file names must be lowercase letters, digits and underscores, so it keeps the template's foreground name. Delete `appiconfg.svg`.
- Keep `appicon.svg` as the background layer, but change its fill to white (`#FFFFFF`), the colour of the logo's own background, so no seam appears around the logo.
- The `MauiIcon` item becomes `Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.png" ForegroundScale="0.7" Color="#FFFFFF"`.
- These literal colours belong to an image asset and the build item, not to pages or styles, so the "no hard-coded colours in pages" rule does not apply.

**Alternative rejected:** the PNG as `Include` with no foreground. Android would then build the adaptive icon from the whole square, and the mask would cut the logo's edges.

### 2. Scale to the safe zone

- `ForegroundScale="0.7"` keeps every drawn point within about 32 dp of the centre, inside the 33 dp safe circle of adaptive icons, so no mask shape cuts it. A first value of 0.75 put the cable end at 34.5 dp on the generated layer, just outside the safe circle, so it was lowered.
- The value is checked on the emulator with a round icon shape. If the logo looks too small on Windows, where there is no mask, it is still kept the same on both platforms (one source and one setting), as the resizetizer shares it.

**Alternative rejected:** cropping the PNG's white margins. That would make the logo touch the mask edge and be clipped.

### 3. Source file stays outside the repo untouched

- The original in `Pictures\Screenshots` is only read and copied. The copy in `Resources/AppIcon/` is the project's source of truth from now on.

## Risks / Trade-offs

- **[Launchers and Windows cache icons]** → After the change, reinstall the app on the emulator (uninstall, then deploy) and, if needed, restart Explorer or clear the icon cache on Windows before judging the result.
- **[Raster source]** → A 1000×1000 PNG is enough for every generated size (largest Android icon is 432 px for xxxhdpi adaptive layers; the Windows `.ico` is 256 px). The edges may look slightly soft at very large sizes; accepted.
- **[Thin black outlines at small sizes]** → At 16–24 px (title bar) the drawing loses detail. It stays recognisable by its colours and shapes; accepted.
