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

## Phase 0. Feature discovery and approval gate
No code changes are allowed until a high level feature list is produced and explicitly approved.

### 0.1 Identify candidate changes
Run the following comparisons:

- `git log --oneline origin/develop..origin/jaex`
- `git range-diff origin/develop...origin/jaex`
- `git diff --name-only origin/develop...origin/jaex`

Group commits into coherent user visible features.

### 0.2 Produce feature list for approval
Create `docs/jaex_feature_candidates.md`.

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

Include an approval table:

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
- No UI removals or regressions
- No new warnings
- No broken builds

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

## Mandatory verification
After applying changes:

1. Review impact.
   - `git diff --stat develop...HEAD`

2. Review UI diffs.
   - `git diff develop...HEAD -- src/**/*.axaml src/**/*.axaml.cs src/**/Views/**`

3. Verify protected UI.
   - `git grep -n "Import Preset" src/`
   - `git grep -n "Export Preset" src/`

4. Build and test.
- Run standard build
- Run tests
- Ensure zero new warnings

## UI regression gate
Before PR:

1. Create `docs/ui_snapshot_after_<feature-id>.md`
2. Compare against `docs/ui_snapshot_develop.md`
3. Confirm no removals or regressions
4. Perform manual UI smoke test

## Pull request requirements
PR title  
`Import jaex feature: <feature-id> <feature-name>`

PR description must include:
- Cherry picked commit SHAs
- Summary of behaviour changes
- Explicit UI confirmation
- Links to UI snapshots

## Merge and cleanup
- Merge into `develop`
- Delete `integrate/jaex-<feature-id>` branch only
- Do not delete any other branches

## Fallback rule
If a feature cannot be cleanly cherry picked:
- Do not force resolve
- Re implement manually
- Use `jaex` as reference only
- Keep `develop` UI as authoritative
