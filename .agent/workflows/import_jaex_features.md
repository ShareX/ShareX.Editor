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
Identify and manually integrate high-value features from ShareX.ImageEditor into the `origin/develop` branch of XerahS.Editor.

**Integration approach:**
- All features are implemented via **manual re-implementation** in XerahS.Editor code.
- ShareX.ImageEditor source code is used as a **reference only**; never cherry-picked.
- Features that enhance existing behavior are adopted (with manual implementation).
- Features that do not enhance existing behavior are **ignored** (not implemented).

**Non-negotiable:**
- No UI regressions allowed.
- All menu items must remain intact, including:
  - `Import Preset...`
  - `Export Preset...`
- No removal or breaking of functionality.

XerahS.Editor is our primary destination repository. ShareX.ImageEditor is an independently maintained reference repository. We integrate features based on their value, not their availability.

## Documentation placement
All integration documentation lives under `docs/jaex-integration/`:

- **Feature candidates:** `docs/jaex-integration/jaex_feature_candidates.md` — List of features proposed for integration, with approval tracking.
- **UI snapshots (baseline):** `docs/ui_snapshot_develop.md` — Current state of develop branch UI. Keep as single source of truth per AGENTS.md. Also store a copy under `docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md` for workflow scoping.
- **UI snapshots (post-integration):** `docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md` — UI state after integrating each feature.
- **State tracking:** `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` — SHA of the newest integrated commit from ShareX.ImageEditor, for resume. Also maintain `docs/jaex-integration/completed/features_log.md` for provenance.
- **Readiness signal:** `integrate/ready.md` (repository root) — Created when an integration branch is ready for PR.

## State tracking and resume
Maintain a feature integration log for tracking and resume:

- **Log file:** `docs/jaex-integration/completed/features_log.md`
- **Entry format:** `| Date | Feature ID | Feature Name | Implementation Type | Notes |`
- **Purpose:** Track which features have been integrated, for quick reference when resuming work.

**Usage:**
- When starting work: review the log to see which features have already been integrated.
- If resuming at JX-012: do **not** re-implement JX-001 through JX-011 (already completed).
- After merging a feature: add an entry to the log.

**Note:** Unlike the cherry-pick workflow, this is **not** a resume gate. Manual implementation is self-contained per feature and does not depend on commit SHAs. The log is purely for human reference and to avoid duplicate work.

### Resume gate (required)
Before starting integration of a new feature batch:

1. Review `docs/jaex-integration/completed/features_log.md` to see which features have already been integrated.
2. Do **not** re-implement any feature already listed in the log.
3. Only proceed with features that are not yet in the log (i.e., marked as Rejected or not yet processed).

Unlike cherry-pick workflows, manual implementation does not depend on commit SHAs or base references. The log is purely for human tracking.

### Skip already-integrated features (required)
Before starting implementation of a feature:

1. Check `docs/jaex-integration/completed/features_log.md` for the feature ID.
2. If the feature already has a log entry (Approved: Y and merged), **skip it**—do not re-implement.
3. Only implement features that are:
   - Approved (Y)
   - **Not** already in the log as merged

This prevents duplicate work when resuming across sessions.

## Phase 0. Feature discovery and approval gate
No code changes are allowed until a high level feature list is produced and explicitly approved.

### 0.1 Identify candidate changes
Determine what features from ShareX.ImageEditor are worth integrating:

1. **Review the reference repository:** Visit ShareX.ImageEditor to understand recent features and changes.
2. **Assess value:** Determine which features would enhance XerahS.Editor's functionality.
3. **Document features:** List candidates with clear descriptions of what they do and why they are valuable.

Unlike cherry-pick workflows, there is **no git-based comparison**. Instead, rely on:
- Code review of ShareX.ImageEditor's recent commits
- Feature descriptions and pull requests in that repository
- Manual assessment of what would benefit XerahS.Editor

### 0.1b Local state sanity check (required)
Before implementing any features:
- Ensure you are on a clean working tree: `git status -sb`
- Ensure `origin/develop` is current: `git fetch origin`
- Ensure `jaex` is current: `git fetch jaex` (for reference purposes)
- Verify no stale local integration branches exist from earlier sessions.

### 0.2 Produce feature list for approval
Create `docs/jaex-integration/jaex_feature_candidates.md`.

For each feature include:
- Feature ID `JX-###`
- Feature name
- One sentence user visible outcome
- Primary areas touched in ShareX.ImageEditor (as reference)
- Commit SHAs and messages from ShareX.ImageEditor
- Risk rating: Low / Medium / High
- **Behavior value:** Does this feature enhance existing XerahS.Editor behavior? (Y/N)
  - If **Yes**: Recommend for integration (manual implementation)
  - If **No**: Recommend for rejection (ignore)
- UI impact statement (from ShareX.ImageEditor source code)
  - Whether `.axaml` files are touched
  - Whether menus or commands are touched
  - Explicit statement about `Import Preset...` and `Export Preset...`
  - **Note:** These are only references; XerahS.Editor UI will be manually adapted

Interactive approval (chat):

- The agent will read `docs/jaex-integration/jaex_feature_candidates.md`, display the candidate features in chat, and prompt for approval decisions.
- Supported responses:
   - "Approve all" (approves every JX feature listed)
   - "Approve all (except JX-###, JX-###)"
   - "Approve: JX-###, JX-###; Reject: JX-###, JX-###"
   - Explicit per-feature decisions (approve/reject) by JX ID
- After you reply, the agent will update the approval table in `docs/jaex-integration/jaex_feature_candidates.md` to reflect the final decisions.
   - The table format remains:
    
      | Feature ID | Name | Risk | Enhances Behavior | Approved (Y/N) | Notes |
      | --- | --- | --- | --- | --- | --- |

### 0.3 Stop point
After creating `docs/jaex_feature_candidates.md`:
- Do not cherry pick
- Do not modify source
- Wait for explicit approval per feature

## Hard rules
- **All features are manually implemented** in XerahS.Editor code. Never cherry-pick from ShareX.ImageEditor.
- ShareX.ImageEditor source code is reference material only.
- `origin/develop` is the authoritative source of truth for XerahS.Editor.
- No UI removals, renames, or hiding. No regressions.
- Zero new warnings or broken builds.
- Target framework must remain cross-platform (e.g., `net10.0`, NOT `net10.0-windows`).
- Features that do not enhance behavior in XerahS.Editor are ignored (not implemented).
- Always preserve XerahS.Editor's geometry, parity logic, and protected UI components.
- Only approved features are implemented. Rejected features are disregarded entirely.

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

3. **Manually implement the feature in XerahS.Editor.**
   - Review the ShareX.ImageEditor source code for the feature (reference in candidates list).
   - Translate the feature into XerahS.Editor's codebase, respecting all architecture, geometry, parity, and UI constraints.
   - Do not copy-paste code directly; adapt and refactor as needed.
   - Ensure all changes follow XerahS.Editor's coding standards and patterns.

4. **Preserve all existing behavior and UI.**
   - Do not remove or rename any menu items.
   - Maintain all `Import Preset...` and `Export Preset...` functionality.
   - Do not alter geometry, layout, or parity helpers.
   - If the feature conflicts with existing behavior: prefer XerahS.Editor's behavior.
   - **Protected components:** `AnnotationToolbar.axaml` and `EditorToolsPanel.axaml` must only be modified to add new features; never remove or restructure existing UI.

5. **Test thoroughly before PR.**
   - Compile with zero warnings.
   - Run all unit tests.
   - Verify the feature works as intended.
   - Perform manual UI smoke test.

6. **Branch isolation rule (critical):**
   - Do **NOT** merge the feature branch into `develop` locally before creating the PR.
   - The PR must contain the actual feature diff (non-empty merge).
   - If `origin/develop` has moved since branching: rebase/merge it into the *feature branch only*, resolve conflicts there, then open/refresh the PR.
   - Do **NOT** push or merge `develop` ahead of the feature PR.

## Mandatory verification
After implementing changes:

1. **Compile and verify no warnings.**
   - Run: `dotnet build`
   - Ensure **zero warnings** (warnings as errors mode is enabled)

2. **Run tests.**
   - Run: `dotnet test` (or equivalent)
   - Ensure all tests pass

3. **Review code changes.**
   - `git diff develop...HEAD --stat` — Verify reasonable change scope
   - `git diff develop...HEAD -- src/**/*.axaml src/**/*.axaml.cs` — Verify UI changes are intentional

4. **Verify protected UI items.**
   - `git grep -n "Import Preset" src/`
   - `git grep -n "Export Preset" src/`
   - Confirm these items still exist and bindings remain intact

5. **Verify Image Effects compatibility** (if applicable).
   - Ensure all `ImageEffect` classes have public parameterless constructors.
   - Verify standard effects are present: `Border`, `Glow`, `Outline`, `Reflection`, `Shadow`, `Slice`, `TornEdge`.
   - Verify they are registered in the `AvailableEffects` list.

6. **Manual smoke test.**
   - Test the feature end-to-end in the running application.
   - Verify no side effects or regressions in other features.

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
- Feature name and one-paragraph description of the implementation
- Reference source in ShareX.ImageEditor (commit SHAs, files reviewed)
- Files modified in XerahS.Editor (manual implementation locations)
- Tests run and results
- UI changes made (if any)
- Verification checklist (warnings, tests, protected components, etc.)

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
Feature: <feature-id> <feature-name>
```
(Example: `Feature: JX-042 Advanced blur effect`)

**PR description must include:**
- Feature summary and user-visible behavior
- Reference to ShareX.ImageEditor source (commit SHAs, files reviewed)
- List of files modified in XerahS.Editor (manual implementation)
- Verification results:
  - Build status (zero warnings)
  - Test results (all passed)
  - UI verification (no regressions, all protected items intact)
  - Manual smoke test results
- Links to UI snapshots (before and after, if applicable)

## Merge and cleanup
- Merge the PR into `develop` (do not pre-merge locally).
- Verify the merge introduces the actual feature diff (no empty merges).
- Delete the feature branch after PR merge.
- Do not delete any other branches.
- Prioritize merging once all checks pass.

### Update state marker (after merge)
After the feature batch is merged into `develop`:

1. Identify the feature ID integrated (e.g., JX-042).
2. Append an entry to `docs/jaex-integration/completed/features_log.md`:
   ```
   | Date | Feature ID | Feature Name | Implementation Type | Notes |
   ```
   Example: `| 2026-02-08 | JX-042 | Advanced blur effect | Manual implementation | Successfully integrated, no conflicts |`

3. The marker file `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` is **not used** in manual implementation (it was for cherry-pick workflow). You may delete it if it exists or leave it as historical record.

## Implementation reference
When implementing a feature from ShareX.ImageEditor:

1. **Study the source:** Read the ShareX.ImageEditor source code for the feature.
2. **Understand the intent:** Determine what the feature does and why it is valuable.
3. **Adapt to XerahS.Editor:** Implement the feature using XerahS.Editor's architecture, patterns, and constraints.
4. **Do not copy-paste:** Refactor and integrate the code into XerahS.Editor's style and structure.
5. **Preserve all constraints:**
   - Geometry and parity helpers must not be bypassed.
   - Protected UI components must not be replaced.
   - All menu items must remain present and functional.
   - Target framework must remain cross-platform.

Manual implementation allows us to:
- Maintain full control over XerahS.Editor's architecture.
- Avoid importing unwanted dependencies or design patterns.
- Ensure features integrate cleanly with existing code.
- Preserve all constraints and protections.
