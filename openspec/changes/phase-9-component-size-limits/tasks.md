# Tasks

Each group ends with the three builds (`dotnet build src/OpenWave.Api`, `dotnet build src/OpenWave.App -f net10.0-windows10.0.19041.0`, `dotnet build src/OpenWave.App -f net10.0-android`) and a short summary of files, commands and what to test manually. "Visual check" means: Windows at the minimum window and maximised, and the Android phone in portrait, each at `Normal` and `Enorme`.

## 1. Shared size keys and the aspect box

- [ ] 1.1 Add the size keys of the design table to `Resources/Styles/Styles.xaml` (drawn pieces as constants with their ratio), and verify the Windows build has no missing-resource error
- [ ] 1.2 Add `Services/ComponentSizes.cs` with the `Normal` values of the text-component keys and make `AppearanceService.ApplyScale` write them multiplied by the text-size factor, and verify with the debugger or a temporary log that at `Enorme` `StationRowMinHeight` is 1.6 × its `Normal` value
- [ ] 1.3 Add `Views/Controls/AspectBox.cs` (ratio, minimum and maximum width, centred), and verify on a test page that it keeps a 1.6 : 1 box between its limits while the window is resized, then remove the test page
- [ ] 1.4 Set `MinimumWidth` 360 and `MinimumHeight` 600 on the window in `App.CreateWindow`, and verify the Windows window cannot be dragged smaller
- [ ] 1.5 Run the three builds and fix errors

## 2. Shared pages and components

- [ ] 2.1 Apply the header, bottom bar, settings column, settings row, chip, filter list and form limits to `SettingsHeader`, `BottomBar`, `SettingsRow`, `AjustesPage`, `PersonalizacionPage`, `PreferenciasPage`, `FiltroPage`, `LoginPage` and `RegisterPage`, with the content in a vertical `ScrollView`, and visual check them in Moderno
- [ ] 2.2 Put the Personalización preview card in its limits (it stays fixed above the scroll area), and visual check that it never overlaps the options at `Enorme` on the minimum window
- [ ] 2.3 Run the three builds and fix errors

## 3. Moderno and Tocadiscos

- [ ] 3.1 Moderno: the card grid keeps its columns with the card width limits, and the Reproductor artwork and panel use their keys; visual check
- [ ] 3.2 Tocadiscos: put the turntable, the big sleeve and the record crate in `AspectBox` with their keys, make `VinylDisc`, `Tonearm`, `SleeveStack` and `RecordCrate` take their size from the box, put the information panel in its limits and move the content into the vertical scroll; visual check, including that the tonearm still lands on the record
- [ ] 3.3 Run the three builds and fix errors

## 4. Radio de válvulas and Casete portátil

- [ ] 4.1 Radio de válvulas: tuning dial, magic eye, knobs and preset keys in their limits (dial and eye in `AspectBox`), panel limits replace the 560 cap, content in the vertical scroll; visual check
- [ ] 4.2 Casete portátil: the Reproductor cassette (window, reels and label) in `AspectBox` at 1.6 : 1 replacing the fixed 280 height and the 720 cap, the J-card in `AspectBox` at 3.3 : 1, spines and keys in their limits, the label name cut with `…`; visual check that the cassette keeps its shape from the minimum window to maximised
- [ ] 4.3 Run the three builds and fix errors

## 5. Boombox and Mesa de DJ

- [ ] 5.1 Boombox: speakers, level meter, display and memory keys in their limits (speakers in `AspectBox`), keeping the 720 breakpoint between beside and above/below; visual check on both sides of the breakpoint
- [ ] 5.2 Mesa de DJ: jog wheel and pads grid in `AspectBox`, browser rows and level meter in their limits, keeping its 720 breakpoint; visual check
- [ ] 5.3 Run the three builds and fix errors

## 6. MP3 and Clásico de rueda

- [ ] 6.1 MP3: the LCD screen in `AspectBox` at 4 : 3 replacing the 640 cap, lines and rubber buttons in their limits, the scrolling name inside the LCD; visual check
- [ ] 6.2 Clásico de rueda: the device screen in `AspectBox` at 4 : 3 and the click wheel between 180 and 260 (no fixed 220), replacing the 520 cap; visual check that the wheel and the bar keep their size at `Enorme`
- [ ] 6.3 Run the three builds and fix errors

## 7. Clean-up and documentation

- [ ] 7.1 Search the layouts and controls for remaining literal `WidthRequest`, `HeightRequest`, `MaximumWidthRequest` and `MaximumHeightRequest` numbers on components (thin lines and dividers of 1–3 units excepted), and verify every one left is a key
- [ ] 7.2 Update the README (a "Component sizes" section with the table, the window minimum, how scrolling works at `Enorme`, and how a new theme uses the keys and `AspectBox`) and the manual check list, and verify the steps match the app

## 8. Integration check

- [ ] 8.1 Manual run in every theme on Windows (minimum window, a medium window, maximised) and on the Android phone in portrait, at `Pequeño`, `Normal` and `Enorme`: no component outside its limits, drawn pieces keep their shape, no sideways scroll, every control reachable, header and bar fixed
- [ ] 8.2 Run the three builds a last time and report the real output

## Workflow follow-up

- Archive the change with `/opsx:archive` after the manual checks pass.
