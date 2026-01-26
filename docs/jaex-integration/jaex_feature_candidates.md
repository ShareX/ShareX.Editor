# jaex feature candidates

## JX-001 Annotation styling controls and color picker UI
- Feature ID: JX-001
- Feature name: Annotation styling controls and color picker UI
- User-visible outcome: Adds fill color, strength, font size, and shadow options with updated color picker visuals.
- Primary areas touched: src/ShareX.Editor/Annotations (Annotation.cs, *Annotation.cs), src/ShareX.Editor/Controls (ColorPickerDropdown.axaml, FontSizePickerDropdown.axaml, StrengthSlider.axaml), src/ShareX.Editor/Views (EditorView.axaml, EditorView.axaml.cs), src/ShareX.Editor/ViewModels/MainViewModel.cs
- Commits:
  - 802ea53 Add fill color, font size, strength, and shadow options
  - 4127a8a Add shadow effect support to annotations
  - ae58b2e Refine annotation visuals and stroke/fill logic
  - a9f58f0 Refactor ColorPickerDropdown to use Avalonia ColorView
  - cde6136 Add checkered background to color preview
  - ca345c5 Replace shadow toggle checkbox with button
  - 5fbcca5 Replace shadow icon with moon icon in toolbar
  - 7908db0 Enable shadow by default for annotations
- Risk rating: Medium
- UI impact statement: Touches .axaml files (EditorView.axaml, ColorPickerDropdown.axaml, FontSizePickerDropdown.axaml, StrengthSlider.axaml). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Cherry pick with conflict risk (EditorView.axaml and control UI changes)

## JX-002 Editor toolbar layout updates and pin-to-screen
- Feature ID: JX-002
- Feature name: Editor toolbar layout updates and pin-to-screen
- User-visible outcome: Adds pin-to-screen action and refreshes bottom bar/toolbar layout and sizing.
- Primary areas touched: src/ShareX.Editor/Views/EditorView.axaml, src/ShareX.Editor/ViewModels/MainViewModel.cs, src/ShareX.Editor/Annotations/FreehandAnnotation.cs
- Commits:
  - 05d141a Add pin-to-screen feature and update bottom bar UI
  - 331b51c Remove delete button from editor toolbar
  - 73873e4 Standardize editor button sizes and tooltips
  - 703bb96 Reorder upload and pin buttons in editor toolbar
  - 8f4e1e0 Set button width to auto in ratio controls
  - b50568b Show tool options separator conditionally in editor
- Risk rating: Medium
- UI impact statement: Touches .axaml files (EditorView.axaml). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Cherry pick with conflict risk (EditorView.axaml layout conflicts likely)

## JX-003 Text and speech balloon editing UX
- Feature ID: JX-003
- Feature name: Text and speech balloon editing UX
- User-visible outcome: Improves text and speech balloon editing with in-place edit, double-click edit, font size support, padding/wrapping, and contrast updates.
- Primary areas touched: src/ShareX.Editor/Annotations (TextAnnotation.cs, SpeechBalloonAnnotation.cs), src/ShareX.Editor/Controls/SpeechBalloonControl.cs, src/ShareX.Editor/Views/Controllers (EditorInputController.cs, EditorSelectionController.cs), src/ShareX.Editor/Views/EditorView.axaml.cs, src/ShareX.Editor/ViewModels/MainViewModel.cs
- Commits:
  - 91b95ea Enable in-place editing for text annotations
  - 164ec5a Remove min width from text annotations and improve selection
  - e775c37 Enable double-click editing for speech balloons
  - 200ea0b Improve speech balloon text editor style updates
  - 66a2ce9 Improve speech balloon defaults and text editor contrast
  - 205b4c4 Set CaretBrush to match Foreground in text editors
  - 001391e Add font size support for speech balloon annotations
  - dbbc352 Adjust speech balloon text padding and enable wrapping
- Risk rating: Medium
- UI impact statement: No .axaml files touched (only .axaml.cs code-behind). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-004 Annotation interaction and state fixes
- Feature ID: JX-004
- Feature name: Annotation interaction and state fixes
- User-visible outcome: Improves highlight/shape behavior, line/arrow hit testing, and annotation state updates.
- Primary areas touched: src/ShareX.Editor/Annotations/HighlightAnnotation.cs, src/ShareX.Editor/Views/Controllers/EditorInputController.cs
- Commits:
  - 92eb4a1 Improve highlight and shape drawing behavior
  - b643a54 Disable hit testing for new line and arrow shapes
  - 90a0ef2 Update HasAnnotations after removing annotation
- Risk rating: Low
- UI impact statement: No .axaml files touched and no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-005 Effect annotation robustness
- Feature ID: JX-005
- Feature name: Effect annotation robustness
- User-visible outcome: Fixes blur/magnify/pixelate/spotlight behavior near edges and out-of-bounds selections.
- Primary areas touched: src/ShareX.Editor/Annotations (BlurAnnotation.cs, MagnifyAnnotation.cs, PixelateAnnotation.cs), src/ShareX.Editor/Views/Controllers/EditorInputController.cs, src/ShareX.Editor/Views/EditorView.axaml.cs
- Commits:
  - 9512e64 Map EffectStrength to DarkenOpacity for Spotlight tool
  - 28927d6 Improve blur annotation edge handling
  - b8594c6 Fix effect annotations to handle out-of-bounds regions
  - cbc5988 Improve magnify annotation region handling
- Risk rating: Low
- UI impact statement: No .axaml files touched and no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-006 License header updates
- Feature ID: JX-006
- Feature name: License header updates
- User-visible outcome: Updates copyright year/license headers in source files.
- Primary areas touched: src/ShareX.Editor (App.axaml.cs, EditorView.axaml.cs, EditorWindow.axaml.cs), src/ShareX.Editor.Loader (App.axaml.cs, MainWindow.axaml.cs, Program.cs), src/ShareX.Editor/Annotations, src/ShareX.Editor/Controls, src/ShareX.Editor/Converters
- Commits:
  - 7aeed79 Update year
  - a59e3c4 Update license information
  - be01589 Update license information
- Risk rating: Low
- UI impact statement: No .axaml files touched (only .axaml.cs code-behind). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-007 Project cleanup: remove verification project and build props
- Feature ID: JX-007
- Feature name: Project cleanup: remove verification project and build props
- User-visible outcome: Removes verification project and shared build props file.
- Primary areas touched: Directory.Build.props, src/ShareX.Editor.Verification/ShareX.Editor.Verification.csproj, src/ShareX.Editor.Verification/Program.cs
- Commits:
  - 98abbe2 Removed ShareX.Editor.Verification project
  - ec349f2 Delete Directory.Build.props
- Risk rating: High
- UI impact statement: No .axaml files touched and no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Manual re-implement recommended (build pipeline impact)

| Feature ID | Name | Risk | UI touched | Approved (Y/N) | Notes |
| --- | --- | --- | --- | --- | --- |
| JX-001 | Annotation styling controls and color picker UI | Medium | Yes (.axaml) | Y |  |
| JX-002 | Editor toolbar layout updates and pin-to-screen | Medium | Yes (.axaml) | Y |  |
| JX-003 | Text and speech balloon editing UX | Medium | No (.axaml.cs only) | Y |  |
| JX-004 | Annotation interaction and state fixes | Low | No | Y |  |
| JX-005 | Effect annotation robustness | Low | No | Y |  |
| JX-006 | License header updates | Low | No (.axaml.cs only) | Y |  |
| JX-007 | Project cleanup: remove verification project and build props | High | No | N | Removal excluded per approval |
