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

## JX-008 Downgrade Avalonia packages to 11.3.9
- Feature ID: JX-008
- Feature name: Downgrade Avalonia packages to 11.3.9
- User-visible outcome: Reverts Avalonia package versions to 11.3.9 for stability/compatibility.
- Primary areas touched: src/ShareX.Editor/ShareX.Editor.csproj, src/ShareX.Editor.Loader/ShareX.Editor.Loader.csproj
- Commits:
  - 430ce80 Downgrade Avalonia packages to version 11.3.9
- Risk rating: High
- UI impact statement: No .axaml files touched and no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Cherry pick with conflict risk (package version alignment)

## JX-009 Minimum shape size validation for annotation tools
- Feature ID: JX-009
- Feature name: Minimum shape size validation for annotation tools
- User-visible outcome: Prevents tiny accidental shapes by discarding shapes smaller than a minimum size.
- Primary areas touched: src/ShareX.Editor/Views/Controllers/EditorInputController.cs
- Commits:
  - 72bf393 Add minimum shape size validation to annotation tools
- Risk rating: Medium
- UI impact statement: No .axaml files touched and no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-010 Color picker palette customization
- Feature ID: JX-010
- Feature name: Color picker palette customization
- User-visible outcome: Tweaks ColorView palette colors and improves initial selection behavior in the color picker.
- Primary areas touched: src/ShareX.Editor/Controls/ColorPickerDropdown.axaml, src/ShareX.Editor/Controls/ColorPickerDropdown.axaml.cs
- Commits:
  - a6febe4 Customize ColorView palette colors in ColorPickerDropdown
- Risk rating: Low
- UI impact statement: Touches .axaml files (ColorPickerDropdown.axaml). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-011 Picker dropdown active styling and default font size
- Feature ID: JX-011
- Feature name: Picker dropdown active styling and default font size
- User-visible outcome: Highlights selected options in font size/width pickers and increases default font size to 30.
- Primary areas touched: src/ShareX.Editor/Controls/FontSizePickerDropdown.axaml, src/ShareX.Editor/Controls/FontSizePickerDropdown.axaml.cs, src/ShareX.Editor/Controls/WidthPickerDropdown.axaml, src/ShareX.Editor/Controls/WidthPickerDropdown.axaml.cs, src/ShareX.Editor/ViewModels/MainViewModel.cs
- Commits:
  - 6454b90 Add active state styling to picker dropdowns
  - 44040ff Increase default font size to 30
- Risk rating: Low
- UI impact statement: Touches .axaml files (FontSizePickerDropdown.axaml, WidthPickerDropdown.axaml). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-012 Annotation rendering parity fixes
- Feature ID: JX-012
- Feature name: Annotation rendering parity fixes
- User-visible outcome: Aligns annotation visuals (arrow geometry, text padding/properties, number radius) and standardizes visual creation.
- Primary areas touched: src/ShareX.Editor/Annotations (ArrowAnnotation.cs, TextAnnotation.cs, NumberAnnotation.cs), src/ShareX.Editor/Views/EditorView.axaml.cs
- Commits:
  - eebf982 Refactor annotation visual creation to use CreateVisual
  - a622384 Set arrow geometry in ArrowAnnotation path
  - 82f4b97 Update TextAnnotation padding and properties
  - 3c43e83 Fix number annotation radius usage in editor
- Risk rating: Medium
- UI impact statement: Touches .axaml.cs (EditorView.axaml.cs). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Cherry pick with conflict risk (overlaps with local refactors)

## JX-013 EditorOptions configuration + color alpha
- Feature ID: JX-013
- Feature name: EditorOptions configuration + color alpha
- User-visible outcome: Adds configurable editor options (including step border/fill), preserves alpha in color selection, and syncs view models with options.
- Primary areas touched: src/ShareX.Editor/EditorOptions.cs, src/ShareX.Editor/ViewModels (EditorViewModel.cs, MainViewModel.cs), src/ShareX.Editor/Views/Controllers/EditorInputController.cs
- Commits:
  - 5474a7a Add EditorOptions class for editor configuration
  - fc50445 Preserve alpha in color selection and add editor options
  - 134203d Add step border and fill color options to EditorOptions
  - 449556d Sync EditorViewModel and MainViewModel with EditorOptions
  - fb7c7c9 Fix swapped step fill and border color assignments
- Risk rating: Medium
- UI impact statement: No .axaml files touched; no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-014 Speech balloon stroke width + transparent fill handling
- Feature ID: JX-014
- Feature name: Speech balloon stroke width + transparent fill handling
- User-visible outcome: Ensures speech balloon respects stroke width and handles transparent fill color correctly.
- Primary areas touched: src/ShareX.Editor/Controls/SpeechBalloonControl.cs, src/ShareX.Editor/Views/Controllers/EditorInputController.cs
- Commits:
  - cccfede Handle transparent fill color for speech balloon tool
  - b0f4cf2 Add stroke width support for SpeechBalloonControl
- Risk rating: Low
- UI impact statement: No .axaml files touched; no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-015 Smart padding edit-state preservation
- Feature ID: JX-015
- Feature name: Smart padding edit-state preservation
- User-visible outcome: Preserves editing state when smart padding is applied to avoid losing active edits.
- Primary areas touched: src/ShareX.Editor/ViewModels/MainViewModel.cs
- Commits:
  - 8c71db0 Preserve editing state during smart padding
- Risk rating: Low
- UI impact statement: No .axaml files touched; no menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-016 Selection handling for tool change
- Feature ID: JX-016
- Feature name: Selection handling for tool change
- User-visible outcome: Adds deselect event handling to cleanly exit selection when switching tools.
- Primary areas touched: src/ShareX.Editor/ViewModels/MainViewModel.cs, src/ShareX.Editor/Views/EditorView.axaml.cs
- Commits:
  - 306c3fe Add DeselectRequested event to handle tool selection
- Risk rating: Low
- UI impact statement: Touches .axaml.cs (EditorView.axaml.cs). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

## JX-017 Effect visual refresh consistency
- Feature ID: JX-017
- Feature name: Effect visual refresh consistency
- User-visible outcome: Ensures effect visuals refresh reliably after interactions across annotation types.
- Primary areas touched: src/ShareX.Editor/Views/Controllers/EditorInputController.cs, src/ShareX.Editor/Views/EditorView.axaml.cs
- Commits:
  - e12fd73 Update effect visual trigger for all annotations
- Risk rating: Low
- UI impact statement: Touches .axaml.cs (EditorView.axaml.cs). No menu or command definitions changed; EffectsMenuDropdown.axaml is untouched, so Import Preset... and Export Preset... are not modified.
- Suggested import method: Clean cherry pick

| Feature ID | Name | Risk | UI touched | Approved (Y/N) | Notes |
| --- | --- | --- | --- | --- | --- |
| JX-001 | Annotation styling controls and color picker UI | Medium | Yes (.axaml) | Y |  |
| JX-002 | Editor toolbar layout updates and pin-to-screen | Medium | Yes (.axaml) | Y |  |
| JX-003 | Text and speech balloon editing UX | Medium | No (.axaml.cs only) | Y |  |
| JX-004 | Annotation interaction and state fixes | Low | No | Y |  |
| JX-005 | Effect annotation robustness | Low | No | Y |  |
| JX-006 | License header updates | Low | No (.axaml.cs only) | Y |  |
| JX-007 | Project cleanup: remove verification project and build props | High | No | N | Removal excluded per approval |
| JX-008 | Downgrade Avalonia packages to 11.3.9 | High | No | Y | Approved |
| JX-009 | Minimum shape size validation for annotation tools | Medium | No | Y | Approved |
| JX-010 | Color picker palette customization | Low | Yes (.axaml) | Y | Approved |
| JX-011 | Picker dropdown active styling and default font size | Low | Yes (.axaml) | Y | Approved |
| JX-012 | Annotation rendering parity fixes | Medium | No (.axaml.cs only) | Y | Manual re-implementation to preserve shared geometry/parity |
| JX-013 | EditorOptions configuration + color alpha | Medium | No | Y | Approved |
| JX-014 | Speech balloon stroke width + transparent fill handling | Low | No | Y | Approved |
| JX-015 | Smart padding edit-state preservation | Low | No | Y | Approved |
| JX-016 | Selection handling for tool change | Low | No (.axaml.cs only) | Y | Approved |
| JX-017 | Effect visual refresh consistency | Low | No (.axaml.cs only) | Y | Approved |
