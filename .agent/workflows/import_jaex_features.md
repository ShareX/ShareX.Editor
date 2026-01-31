---
description: Sync selected features from jaex fork into develop without losing UI items
---

# Task: Sync selected features from `jaex` fork into `develop` without losing UI items

## Repository scope
This task applies to the XerahS.Editor repository, which is now a maintained fork.

Local Repository URL  
https://github.com/ShareX/XerahS.Editor

Jaex Fork Repository (Source of features)  
https://github.com/ShareX/ShareX.ImageEditor

All commands and analysis must be executed inside the XerahS.Editor repository.

## Remote configuration
- `origin` points to XerahS.Editor (our primary repo): `https://github.com/ShareX/XerahS.Editor.git`
- `jaex` remote points to ShareX.ImageEditor fork: `https://github.com/ShareX/ShareX.ImageEditor.git`
- `develop` and `master` are long lived branches on both `origin` and `jaex` remotes.

Remote references  
- Jaex branch (source of features): `jaex/main` (or `jaex/master` - confirm which is active)  
- Our primary repo branch: `origin/develop`  

Do not assume `jaex` remote is configured. Check and add if needed.

## Mandatory setup
No work may proceed until both remotes are configured and branches are fetched.

1. Verify remotes.
   - `git remote -v`

2. Ensure `origin` points to:
   - `https://github.com/ShareX/XerahS.Editor.git`

3. Add jaex remote if not present:
   - `git remote add jaex https://github.com/ShareX/ShareX.ImageEditor.git`

4. Fetch all branches from both remotes.
   - `git fetch origin`
   - `git fetch jaex`

5. Verify branch availability.
   - `git branch -r`

Both `origin/develop` and `jaex/main` (or `jaex/master`) must be present.  
If not present, stop and report this as a blocking issue.

## Branch safety rules
The following actions are strictly forbidden.

- Do not force push to `jaex` branches (read-only jaex fork).
- Do not modify history on `jaex`.
- Do not open pull requests targeting `jaex` (it is external).
- Do not delete `origin/develop` or `origin/master`.

`jaex` is a read-only remote reference.  
It must remain unchanged at all times. We pull from it; we never push to it.

## Goal
Periodically sync high-value features from the jaex fork into our primary `origin/develop` branch with minimal risk.

UI regressions are not allowed.  
This explicitly includes all menu items such as:
- `Import Preset...`
- `Export Preset...`

Note: XerahS.Editor is the primary maintained repository. ShareX.ImageEditor (jaex) is an external fork maintained separately from which we selectively integrate features.

## Documentation placement
All documentation artifacts produced by this workflow must live under [docs/jaex-integration](XerahS.Editor/docs/jaex-integration), respecting its folder structure.

- Feature candidate lists: [docs/jaex-integration/jaex_feature_candidates.md](XerahS.Editor/docs/jaex-integration/jaex_feature_candidates.md)
- UI snapshots (develop baseline): keep [docs/ui_snapshot_develop.md](XerahS.Editor/docs/ui_snapshot_develop.md) per AGENTS.md, and also store a copy under [docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md](XerahS.Editor/docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md)
- UI snapshots (after feature): [docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md](XerahS.Editor/docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md)
- State markers and provenance: [docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt](XerahS.Editor/docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt) and [docs/jaex-integration/completed/features_log.md](XerahS.Editor/docs/jaex-integration/completed/features_log.md)

Note: The readiness signal file [integrate/ready.md](XerahS.Editor/integrate/ready.md) remains at the repository root as a non-documentation marker per AGENTS.md.

## State tracking and resume
To avoid re-processing older jaex commits across runs, maintain a simple state marker:

- Marker path: `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt`
- Content: single `jaex` commit SHA corresponding to the newest commit integrated in the last completed batch.

Usage:
- When starting comparisons in Phase 0, if the marker file exists, use its SHA as the BASE reference; otherwise use `origin/develop`.
- Update the marker after each successful merge to record the newest processed jaex commit SHA.
- Optionally maintain `docs/jaex-integration/completed/features_log.md` with rows: `Date | Feature ID | Name | Jaex SHAs | Notes` for provenance.

### Resume gate (required)
Before starting Phase 0, verify you are resuming from the last completed feature:
1. Read `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` and record it as `<LAST>`.
2. Compare `origin/develop` vs `jaex/main` (or `jaex/master`) using `<LAST>` as the base. Do **not** use `origin/develop` if `<LAST>` exists.
3. If `<LAST>` is missing, stop and report this as blocking (do not re-start from the beginning).

### Skip merged features (required)
For each candidate JX feature:
1. Check whether its cherry-picked SHAs already exist in `origin/develop` (or are logged in `docs/jaex-integration/completed/features_log.md`).
2. If all SHAs are present, mark the feature as **Already merged** and **do not** create a new integration branch.
3. Only proceed with features that are missing from `origin/develop`.

## Phase 0. Feature discovery and approval gate
No code changes are allowed until a high level feature list is produced and explicitly approved.

### 0.1 Identify candidate changes
Determine the BASE for comparison:

- If `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` exists, set `<BASE>` to its SHA.
- Otherwise stop and report missing marker as blocking (resume gate).

Run the following comparisons using `<BASE>`:

- `git log --oneline <BASE>..jaex/main` (or `jaex/master`)
- `git range-diff <BASE>...jaex/main` (or `jaex/master`)
- `git diff --name-only <BASE>...jaex/main` (or `jaex/master`)

Group commits into coherent user visible features.

### 0.1b Local state sanity check (required)
Before any cherry-picks:
- Ensure you are on a clean working tree (`git status -sb`).
- Ensure `origin/develop` is up to date (`git fetch origin`).
- Ensure `jaex` is up to date (`git fetch jaex`).
- Verify no local integration branches for already-merged features are being reused.

### 0.2 Produce feature list for approval
Create `docs/jaex-integration/jaex_feature_candidates.md`.

For each feature include:
- Feature ID `JX-###`
- Feature name
- One sentence user visible outcome
- Primary areas touched
- Commit SHAs and messages
- Risk rating Low Medium High
- UI impact statement
  - Whether `.axaml` files are touched
  - Whether menus or commands are touched
  - Explicit statement about `Import Preset...` and `Export Preset...`
- Recommended import method
  - Clean cherry pick
  - Cherry pick with conflicts
  - Manual re implementation

Interactive approval (chat):

- The agent will read [docs/jaex-integration/jaex_feature_candidates.md](XerahS.Editor/docs/jaex-integration/jaex_feature_candidates.md), display the candidate features in chat, and prompt for approval decisions.
- Supported responses:
   - "Approve all" (approves every JX feature listed)
   - "Approve all (except JX-###, JX-###)"
   - "Approve: JX-###, JX-###; Reject: JX-###, JX-###"
   - Explicit per-feature decisions (approve/reject) by JX ID
- After you reply, the agent will update the approval table in [docs/jaex-integration/jaex_feature_candidates.md](XerahS.Editor/docs/jaex-integration/jaex_feature_candidates.md) to reflect the final decisions.
   - The table format remains:
    
      | Feature ID | Name | Risk | UI touched | Approved (Y/N) | Notes |
      | --- | --- | --- | --- | --- | --- |

### 0.3 Stop point
After creating `docs/jaex_feature_candidates.md`:
- Do not cherry pick
- Do not modify source
- Wait for explicit approval per feature

## Hard rules
- Never merge jaex into develop directly; always cherry-pick features per the workflow
- Use `git cherry-pick -x` only
- One approved feature per integration batch
- `origin/develop` is the authoritative source of truth for the primary repository
- No UI removals, renames, or hiding; no regressions
- No new warnings
- No broken builds
- Target framework must remain cross-platform (e.g. `net10.0`, NOT `net10.0-windows`)
- If any conflict occurs, prefer `develop` behaviour across code and assets

## Geometry/parity protection (new)
The repository now has shared geometry/parity logic (e.g., `AnnotationGeometryHelper`) and unified
annotation visual creation. When importing features from jaex, do **not** overwrite or regress this work.
If a jaex commit touches these areas, prefer **manual re-implementation** that preserves:
- Shared geometry helpers and parity logic
- `CreateVisual()` usage for annotation restoration
- Any alignment between UI and snapshot renderers

If a jaex feature overlaps parity work (e.g., arrow/text/number/speech balloon geometry or
`EditorView.axaml.cs` annotation restoration), treat the feature as **manual re-implementation** only.
Cherry-pick is allowed only when it does not undo parity changes.

### UI Component Protection
The following files have diverged significantly and must **never** be overwritten by jaex files:
- `src/XerahS.Editor/Views/Controls/AnnotationToolbar.axaml`
- `src/XerahS.Editor/Views/Controls/EditorToolsPanel.axaml`

If a feature involves these files, you must **manually implement** the feature in the local files. **Do not** replace them with files from jaex.

## Pre flight UI protection
Before importing any approved feature:

1. Create `docs/ui_snapshot_develop.md` containing:
- Full main menu structure
- All `MenuItem` headers and bindings
- Confirmation of `Import Preset...`
- Confirmation of `Export Preset...`

2. Capture evidence using:
- `git grep -n "MenuItem" src/`
- `git grep -n "Import Preset" src/`
- `git grep -n "Export Preset" src/`
- Save output in the snapshot file

3. Store a copy of the develop UI snapshot under `docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md` to keep workflow artifacts scoped to `docs/jaex-integration`.

## Feature integration process
For each approved feature:

1. Create integration branch.
   - `git checkout -b integrate/jaex develop`

2. Branch start point rule (required).
   - Always branch from **current** `origin/develop` (after fetch).
   - Never re-run earlier JX branches if the feature is already merged.
   - If resuming at JX-012, do **not** reopen JX-001..JX-011.

2. Apply commits.
   - `git cherry-pick -x <sha1> <sha2> ...`

3. Conflict resolution rules:
- Preserve existing UI from `develop`
- Re apply only logic changes from jaex
- Do not remove or rename menu items
 - If conflicts arise, prefer `develop` behaviour; do not hide UI
 - **Geometry/parity overlap rule:** If changes touch annotation geometry/layout or shared helpers,
   do not accept jaex diffs that remove or bypass parity logic. Manually fold in the desired
   behavior while keeping the shared infrastructure intact.
 - **Protected UI components:** `AnnotationToolbar.axaml` and `EditorToolsPanel.axaml` must **not** be replaced. Manually implement any new features for these controls.

4. **Branch isolation rule (critical)**:
   - Do **NOT** merge these changes into `develop` locally before creating the PR.
   - The PR must contain the actual diff for the feature (non-empty).
   - Do **NOT** push or merge `develop` ahead of the feature PR.
   - If `develop` has moved since branching, rebase/merge `origin/develop` into the *feature branch only* (resolve conflicts there), then open/refresh the PR.

## Mandatory verification
After applying changes:

1. Review impact.
   - `git diff --stat develop...HEAD`

2. Review UI diffs.
   - `git diff develop...HEAD -- src/**/*.axaml src/**/*.axaml.cs src/**/Views/**`

3. Verify protected UI.
   - `git grep -n "Import Preset" src/`
   - `git grep -n "Export Preset" src/`
   - Confirm protected UI items still exist and bindings remain intact

4. Build and test.
- Run standard build
- Run tests
- Ensure zero new warnings
- Verify `src/XerahS.Editor/XerahS.Editor.csproj` TargetFramework does NOT contain `-windows`

5. Verify Image Effects.
   - Ensure all `ImageEffect` classes have public parameterless constructors.
   - Verify `Border`, `Glow`, `Outline`, `Reflection`, `Shadow`, `Slice`, `TornEdge` are present in the `AvailableEffects` list.

## UI regression gate
Before PR:

1. Create `docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md`
2. Compare against `docs/ui_snapshot_develop.md`
3. Confirm no removals or regressions
4. Perform manual UI smoke test

## PR summary artifact (required before merge)
For each feature PR, create a summary file under:
- `docs/jaex-integration/completed/feature-<feature-id>.md`

The summary must include:
- PR link
- Feature name and one-paragraph change summary
- Source upstream SHAs (from candidates)
- Files touched (high level)
- Tests run (or "Not run")
- UI snapshot references (if applicable)

Do **not** merge or delete the feature branch until this summary file exists.

## Readiness signal
Once the integration branch is verified end-to-end:

1. Create readiness marker file in the repository root:
   - `integrate/ready.md`
   - The existence of this file signals that the branch is ready for PR.

2. Proceed to create the PR targeting `develop`.

## Pull request requirements
PR title  
`Sync jaex feature: <feature-id> <feature-name>`

PR description must include:
- Cherry picked commit SHAs
- Summary of behaviour changes
- Explicit UI confirmation
- Links to UI snapshots

## Merge and cleanup
- Merge the PR into `develop` (do not pre-merge locally).
- Delete `integrate/jaex` branch only after PR merge.
- Do not delete any other branches.
- Prioritise merging the PR once checks pass.
- Ensure the PR merge actually introduces the feature diff (no “empty” merges).

### Update state marker
After merging the feature batch into `develop`:

1. Identify the newest jaex commit SHA used in this batch's cherry-picks.
2. Write that SHA to `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` (overwrite with the single SHA).
3. Append an entry to `docs/jaex-integration/completed/features_log.md` capturing: date, feature ID, feature name, list of jaex SHAs, and any notes.

## Fallback rule
If a feature cannot be cleanly cherry picked:
- Do not force resolve
- Re implement manually
- Use jaex as reference only
- Keep `develop` UI as authoritative
