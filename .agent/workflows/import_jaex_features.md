---
description: Selectively integrate features from ShareX.ImageEditor into XerahS.Editor develop branch
---

# Task: Integrate features from ShareX.ImageEditor into XerahS.Editor

## Repository scope
This task applies to integrating features into the XerahS.Editor repository.

**Our Repository (Destination)**  
XerahS.Editor: https://github.com/ShareX/XerahS.Editor.git

**Reference Repository (Source of Features)**  
ShareX.ImageEditor: https://github.com/ShareX/ShareX.ImageEditor.git
- This is a **separate, independently maintained repository**.
- We selectively pull features from it into XerahS.Editor.
- We never push to ShareX.ImageEditor; it is read-only for our purposes.
- Locally, it is configured as a remote named `jaex` for convenience.

All commands must be executed **inside the XerahS.Editor repository** (the destination).  
The `jaex` remote is a local git label pointing to https://github.com/ShareX/ShareX.ImageEditor.git.

## Remote configuration
Your local git must reference both repositories:

- `origin` → XerahS.Editor (destination): https://github.com/ShareX/XerahS.Editor.git
- `jaex` → ShareX.ImageEditor (reference): https://github.com/ShareX/ShareX.ImageEditor.git

**Key point:** `jaex` is only a **local git remote label**, not a branch or component of XerahS.Editor. It is your local handle for the ShareX.ImageEditor repository.

Branch references:
- **Reference branch (to integrate from):** Determined during setup. Use `git branch -r | grep jaex/` to discover available branches. Substitute `<JAEX_BRANCH>` in commands with the identified branch name (e.g., `jaex/develop`).
- **Destination branch (to integrate into):** `origin/develop`

Do not assume the `jaex` remote exists. It must be explicitly configured.

## Mandatory setup
No work may proceed until both remotes are properly configured and branches are fetched.

1. **Verify remotes are configured correctly.**
   ```
   git remote -v
   ```
   Expected output:
   - `origin` → `https://github.com/ShareX/XerahS.Editor.git` (fetch and push)
   - `jaex` → `https://github.com/ShareX/ShareX.ImageEditor.git` (fetch)

2. **If `jaex` remote does not exist, add it:**
   ```
   git remote add jaex https://github.com/ShareX/ShareX.ImageEditor.git
   ```

3. **Fetch all branches from both remotes.**
   ```
   git fetch origin
   git fetch jaex
   ```

4. **Identify the reference branch in ShareX.ImageEditor.**
   ```
   git branch -r | grep jaex/
   ```
   Identify the primary branch (e.g., `jaex/develop`, `jaex/main`, `jaex/master`).  
   Store this name as `<JAEX_BRANCH>` and use it in all subsequent commands.

5. **Verify both destination and reference branches exist.**
   ```
   git branch -r | grep origin/develop
   git branch -r | grep jaex/
   ```
   Both `origin/develop` and at least one `jaex/` branch must be present.  
   If not, stop and report as blocking.

## Safety rules
These rules protect repository integrity:

- **Do not push to `jaex` branches.** ShareX.ImageEditor is read-only. Any push will fail.
- **Do not modify `jaex` history.** It is a reference remote; we only read from it.
- **Do not open pull requests to ShareX.ImageEditor.** That is a separate repository with separate governance.
- **Do not delete `origin/develop` or `origin/master`.** These are primary branches in our destination repo.

**Golden rule:** `jaex` is **read-only**. We pull from ShareX.ImageEditor; we never write to it.

## Goal
Selectively integrate high-value features from ShareX.ImageEditor into the `origin/develop` branch of XerahS.Editor with minimal risk of regression.

**Non-negotiable:**
- No UI regressions allowed.
- All menu items must remain intact, including:
  - `Import Preset...`
  - `Export Preset...`
- No removal or breaking of functionality.

XerahS.Editor is our primary destination repository. ShareX.ImageEditor is an independently maintained reference repository. We integrate features selectively via cherry-pick.

## Documentation placement
All integration documentation lives under `docs/jaex-integration/`:

- **Feature candidates:** `docs/jaex-integration/jaex_feature_candidates.md` — List of features proposed for integration, with approval tracking.
- **UI snapshots (baseline):** `docs/ui_snapshot_develop.md` — Current state of develop branch UI. Keep as single source of truth per AGENTS.md. Also store a copy under `docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md` for workflow scoping.
- **UI snapshots (post-integration):** `docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md` — UI state after integrating each feature.
- **State tracking:** `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` — SHA of the newest integrated commit from ShareX.ImageEditor, for resume. Also maintain `docs/jaex-integration/completed/features_log.md` for provenance.
- **Readiness signal:** `integrate/ready.md` (repository root) — Created when an integration branch is ready for PR.

## State tracking and resume
Maintain a state marker to avoid re-processing commits from ShareX.ImageEditor:

- **Marker file:** `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt`
- **Content:** Single commit SHA — the newest commit from ShareX.ImageEditor that was successfully integrated in the last batch.

**Usage:**
- When starting Phase 0, check if the marker exists.
  - If it exists: Use its SHA as the `<BASE>` for comparisons (this is a resume).
  - If missing: Stop and report as blocking (do not start from origin/develop).
- After each successful feature integration and merge, update the marker with the newest integrated SHA from ShareX.ImageEditor.
- Optionally maintain `docs/jaex-integration/completed/features_log.md` for provenance: `Date | Feature ID | Name | SHAs from ShareX.ImageEditor | Notes`

### Resume gate (required)
Before starting Phase 0, verify you are resuming from the last completed feature:
1. Read `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` and record it as `<LAST>`.
2. Compare `origin/develop` vs `<JAEX_BRANCH>` using `<LAST>` as the base. Do **not** use `origin/develop` if `<LAST>` exists.
   - Replace `<JAEX_BRANCH>` with the actual branch name identified in setup step 5.
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

Run the following comparisons using `<BASE>`. Replace `<JAEX_BRANCH>` with the actual branch name identified during setup:

- `git log --oneline <BASE>..<JAEX_BRANCH>`
- `git range-diff <BASE>...<JAEX_BRANCH>`
- `git diff --name-only <BASE>...<JAEX_BRANCH>`

Group commits into coherent user visible features.

### 0.1b Local state sanity check (required)
Before any cherry-picks:
- Ensure you are on a clean working tree: `git status -sb`
- Ensure `origin/develop` is current: `git fetch origin`
- Ensure `jaex` is current: `git fetch jaex`
- Verify no stale local integration branches exist for already-merged features.

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
- **Never** merge ShareX.ImageEditor branches directly into develop. Always cherry-pick individual features.
- Use `git cherry-pick -x` to preserve commit attribution.
- One approved feature per integration batch.
- `origin/develop` is the authoritative source of truth for XerahS.Editor.
- No UI removals, renames, or hiding. No regressions.
- Zero new warnings or broken builds.
- Target framework must remain cross-platform (e.g., `net10.0`, NOT `net10.0-windows`).
- On any conflict: prefer XerahS.Editor's behavior. Never accept ShareX.ImageEditor code that removes or breaks functionality.

## Geometry/parity protection
XerahS.Editor has shared geometry and parity logic (e.g., `AnnotationGeometryHelper`) and unified annotation visual creation. When integrating features from ShareX.ImageEditor, do not regress this work.

If a ShareX.ImageEditor commit touches geometry, parity, or annotation restoration:
- **Do not** cherry-pick blindly.
- **Do** manually re-implement the feature to preserve:
  - Shared geometry helpers and parity logic
  - `CreateVisual()` usage for annotation restoration
  - Alignment between UI and snapshot renderers

Areas requiring manual re-implementation:
- Arrow, text, number, speech balloon geometry
- `EditorView.axaml.cs` annotation restoration logic
- Any code that would bypass parity checks

Cherry-pick is acceptable only when it does not undo or bypass parity work.

### UI Component Protection
These files have diverged significantly and must **never** be overwritten:
- `src/XerahS.Editor/Views/Controls/AnnotationToolbar.axaml`
- `src/XerahS.Editor/Views/Controls/EditorToolsPanel.axaml`

If a ShareX.ImageEditor feature involves these files:
- **Do not** replace them.
- **Do** manually implement the feature in the local XerahS.Editor files.
- **Preserve** all existing UI structure and bindings.

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

1. **Create integration branch from current develop.**
   ```
   git fetch origin
   git checkout -b integrate/feature-<id> origin/develop
   ```
   Always branch from the latest `origin/develop`.

2. **Never re-run earlier features.** If resuming at JX-012, do not reopen JX-001 through JX-011.

3. **Cherry-pick approved commits from ShareX.ImageEditor.**
   ```
   git cherry-pick -x <sha1> <sha2> ...
   ```
   Use `-x` to preserve commit attribution.

4. **Resolve conflicts using these rules:**
   - Preserve all existing UI from `origin/develop`.
   - Re-apply only logic changes from ShareX.ImageEditor.
   - Do not remove or rename any menu items.
   - If conflicts occur: prefer XerahS.Editor behavior; do not hide or remove functionality.
   - **Geometry/parity rule:** If changes touch geometry, layout, or parity helpers: do not accept diffs that bypass parity. Manually fold in desired behavior while preserving shared infrastructure.
   - **Protected components:** `AnnotationToolbar.axaml` and `EditorToolsPanel.axaml` must **never** be replaced. Manually implement new features in local files.

5. **Branch isolation rule (critical):**
   - Do **NOT** merge the feature branch into `develop` locally before creating the PR.
   - The PR must contain the actual feature diff (non-empty merge).
   - If `origin/develop` has moved since branching: rebase/merge it into the *feature branch only*, resolve conflicts there, then open/refresh the PR.
   - Do **NOT** push or merge `develop` ahead of the feature PR.

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
For each feature PR, create a summary file:
- **Path:** `docs/jaex-integration/completed/feature-<feature-id>.md`

**Contents:**
- PR link
- Feature name and one-paragraph description of the change
- SHAs cherry-picked from ShareX.ImageEditor (from candidates list)
- Files touched (high-level summary)
- Tests run (or "Not run")
- UI snapshot references (if applicable)

Do **not** merge or delete the feature branch until this summary exists.

## Readiness signal
Once the integration branch is verified end-to-end:

1. Create readiness marker file in the repository root:
   - `integrate/ready.md`
   - The existence of this file signals that the branch is ready for PR.

2. Proceed to create the PR targeting `develop`.

## Pull request requirements
**PR title:**
```
Sync feature: <feature-id> <feature-name>
```
(Example: `Sync feature: JX-042 Advanced blur effect`)

**PR description must include:**
- Commit SHAs cherry-picked from ShareX.ImageEditor
- Summary of behavior changes
- Explicit UI confirmation ("No UI regressions", "All menus intact")
- Links to UI snapshots (before and after)

## Merge and cleanup
- Merge the PR into `develop` (do not pre-merge locally).
- Verify the merge introduces the actual feature diff (no empty merges).
- Delete the feature branch after PR merge.
- Do not delete any other branches.
- Prioritize merging once all checks pass.

### Update state marker (after merge)
After the feature batch is merged into `develop`:

1. Identify the newest commit SHA from ShareX.ImageEditor that was integrated.
2. Overwrite `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` with that single SHA.
3. Append an entry to `docs/jaex-integration/completed/features_log.md`:
   ```
   | Date | Feature ID | Feature Name | SHAs from ShareX.ImageEditor | Notes |
   ```

## Fallback rule
If a feature cannot be cleanly cherry-picked from ShareX.ImageEditor:
- Do not force-resolve conflicts.
- Re-implement manually using ShareX.ImageEditor code as reference only.
- Keep XerahS.Editor `develop` UI and behavior as authoritative.
- Preserve all geometry, parity, and protected UI components.
