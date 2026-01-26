---
description: Import selected features from jaex into develop without losing UI items
---

# Task: Import selected features from `jaex` into `develop` without losing UI items

## Repository scope
This task applies only to the ShareX.Editor repository.

Repository URL  
https://github.com/ShareX/ShareX.Editor

All commands and analysis must be executed inside this repository.

## Branch locations and access
- `develop` and `master` are long lived branches on the `origin` remote.
- `jaex` is a feature branch on the same GitHub repository.

Branch reference  
- Remote branch name: `origin/jaex`  
- Branch URL: https://github.com/ShareX/ShareX.Editor/tree/jaex  

Do not assume `jaex` exists locally.

## Mandatory setup
No work may proceed until `origin/jaex` is available locally.

1. Verify remotes.
   - `git remote -v`

2. Ensure `origin` points to:
   - `https://github.com/ShareX/ShareX.Editor.git`

3. Fetch all branches.
   - `git fetch origin`

4. Verify branch availability.
   - `git branch -r`

`origin/jaex` must be present.  
If not present, stop and report this as a blocking issue.

## Branch safety rules
The following actions are strictly forbidden.

- Do not delete the `jaex` branch.
- Do not rename the `jaex` branch.
- Do not force push to `jaex`.
- Do not modify history on `jaex`.
- Do not open pull requests targeting `jaex`.

`jaex` is a read only upstream reference branch.  
It must remain unchanged at all times.

## Goal
Periodically import high value features from `origin/jaex` into `origin/develop` with minimal risk.

UI regressions are not allowed.  
This explicitly includes all menu items such as:
- `Import Preset...`
- `Export Preset...`

## Documentation placement
All documentation artifacts produced by this workflow must live under [docs/jaex-integration](ShareX.Editor/docs/jaex-integration), respecting its folder structure.

- Feature candidate lists: [docs/jaex-integration/jaex_feature_candidates.md](ShareX.Editor/docs/jaex-integration/jaex_feature_candidates.md)
- UI snapshots (develop baseline): keep [docs/ui_snapshot_develop.md](ShareX.Editor/docs/ui_snapshot_develop.md) per AGENTS.md, and also store a copy under [docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md](ShareX.Editor/docs/jaex-integration/ui-snapshots/ui_snapshot_develop.md)
- UI snapshots (after feature): [docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md](ShareX.Editor/docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md)
- State markers and provenance: [docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt](ShareX.Editor/docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt) and [docs/jaex-integration/completed/features_log.md](ShareX.Editor/docs/jaex-integration/completed/features_log.md)

Note: The readiness signal file [integrate/jaex-JX-<feature-id>.md](ShareX.Editor/integrate/jaex-JX-<feature-id>.md) remains at the repository root as a non-documentation marker per AGENTS.md.

## State tracking and resume
To avoid re-processing older `jaex` commits across runs, maintain a simple state marker:

- Marker path: `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt`
- Content: single `origin/jaex` commit SHA corresponding to the newest commit integrated in the last completed batch.

Usage:
- When starting comparisons in Phase 0, if the marker file exists, use its SHA as the BASE reference; otherwise use `origin/develop`.
- Update the marker after each successful merge to record the newest processed `jaex` commit SHA.
- Optionally maintain `docs/jaex-integration/completed/features_log.md` with rows: `Date | Feature ID | Name | Jaex SHAs | Notes` for provenance.

## Phase 0. Feature discovery and approval gate
No code changes are allowed until a high level feature list is produced and explicitly approved.

### 0.1 Identify candidate changes
Determine the BASE for comparison:

- If `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` exists, set `<BASE>` to its SHA.
- Otherwise set `<BASE>` to `origin/develop`.

Run the following comparisons using `<BASE>`:

- `git log --oneline <BASE>..origin/jaex`
- `git range-diff <BASE>...origin/jaex`
- `git diff --name-only <BASE>...origin/jaex`

Group commits into coherent user visible features.

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

- The agent will read [docs/jaex-integration/jaex_feature_candidates.md](ShareX.Editor/docs/jaex-integration/jaex_feature_candidates.md), display the candidate features in chat, and prompt for approval decisions.
- Supported responses:
   - "Approve all" (approves every JX feature listed)
   - "Approve all (except JX-###, JX-###)"
   - "Approve: JX-###, JX-###; Reject: JX-###, JX-###"
   - Explicit per-feature decisions (approve/reject) by JX ID
- After you reply, the agent will update the approval table in [docs/jaex-integration/jaex_feature_candidates.md](ShareX.Editor/docs/jaex-integration/jaex_feature_candidates.md) to reflect the final decisions.
   - The table format remains:
    
      | Feature ID | Name | Risk | UI touched | Approved (Y/N) | Notes |
      | --- | --- | --- | --- | --- | --- |

### 0.3 Stop point
After creating `docs/jaex_feature_candidates.md`:
- Do not cherry pick
- Do not modify source
- Wait for explicit approval per feature

## Hard rules
- Never merge `jaex`
- Use `git cherry-pick -x` only
- One approved feature per integration batch
- `develop` is the source of truth
- No UI removals, renames, or hiding; no regressions
- No new warnings
- No broken builds
 - If any conflict occurs, prefer `develop` behaviour across code and assets

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
   - `git checkout -b integrate/jaex-<feature-id> develop`

2. Apply commits.
   - `git cherry-pick -x <sha1> <sha2> ...`

3. Conflict resolution rules:
- Preserve existing UI from `develop`
- Re apply only logic changes from `jaex`
- Do not remove or rename menu items
 - If conflicts arise, prefer `develop` behaviour; do not hide UI

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

## UI regression gate
Before PR:

1. Create `docs/jaex-integration/ui-snapshots/ui_snapshot_after_<feature-id>.md`
2. Compare against `docs/ui_snapshot_develop.md`
3. Confirm no removals or regressions
4. Perform manual UI smoke test

## Readiness signal
Once the integration branch is verified end-to-end:

1. Create readiness marker file in the repository root:
   - `integrate/jaex-JX-<feature-id>.md`
   - The existence of this file signals that the branch is ready for PR.

2. Proceed to create the PR targeting `develop`.

## Pull request requirements
PR title  
`Import jaex feature: <feature-id> <feature-name>`

PR description must include:
- Cherry picked commit SHAs
- Summary of behaviour changes
- Explicit UI confirmation
- Links to UI snapshots

## Merge and cleanup
- Merge the PR into `develop` (do not pre-merge locally).
- Delete `integrate/jaex-<feature-id>` branch only after PR merge.
- Do not delete any other branches.
- Prioritise merging the PR once checks pass.
- Ensure the PR merge actually introduces the feature diff (no “empty” merges).

### Update state marker
After merging the feature batch into `develop`:

1. Identify the newest `jaex` commit SHA used in this batch's cherry-picks.
2. Write that SHA to `docs/jaex-integration/completed/LAST_PROCESSED_JAEX_COMMIT.txt` (overwrite with the single SHA).
3. Append an entry to `docs/jaex-integration/completed/features_log.md` capturing: date, `JX-###`, feature name, list of `jaex` SHAs, and any notes.

## Fallback rule
If a feature cannot be cleanly cherry picked:
- Do not force resolve
- Re implement manually
- Use `jaex` as reference only
- Keep `develop` UI as authoritative
