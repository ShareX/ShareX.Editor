
## Branch integration rules (jaex → develop)

These rules are mandatory.

1. Never merge the `jaex` branch.
   - Only use `git cherry-pick -x`.
   - One logical feature per integration batch.

2. `develop` is the source of truth.
   - If a conflict occurs, prefer `develop` behaviour.
   - Re apply only the minimal logic required from `jaex`.

3. UI is protected surface area.
   - Do not remove, rename, or hide any existing UI items.
   - This explicitly includes all `MenuItem` entries such as:
     - `Import Preset...`
     - `Export Preset...`
   - UI changes must be additive unless a task explicitly states otherwise.

4. Mandatory setup before any comparison or changes.
   - Verify remotes: `git remote -v`
   - Ensure `origin` is `https://github.com/ShareX/ShareX.Editor.git`
   - Fetch: `git fetch origin`
   - Verify `origin/jaex` exists: `git branch -r`
   - If `origin/jaex` is missing, stop and report as blocking.

5. Mandatory comparison before changes.
   - Compare `origin/develop` and `origin/jaex` using:
     - `git log --oneline origin/develop..origin/jaex`
     - `git range-diff origin/develop...origin/jaex`
     - `git diff --name-only origin/develop...origin/jaex`
   - Summarise candidate commits before applying them.

6. Phase 0 approval gate.
   - Create `docs/jaex_feature_candidates.md` with:
     - Feature ID JX-###, name, 1-line user outcome
     - Primary areas touched
     - Commit SHAs and messages
     - Risk rating (Low/Medium/High)
     - UI impact statement (axaml touched, menus/commands touched, explicit Import/Export Preset status)
     - Recommended import method (clean cherry-pick, cherry-pick with conflicts, manual re-implementation)
     - Approval table with Approved (Y/N)
   - Stop after writing the file and wait for explicit approval per feature.

7. Mandatory verification after changes.
   - Build must succeed with zero new warnings.
   - All protected UI items must still exist and be wired.
   - If UI is touched, create or update a UI snapshot file in `docs/`.

8. Provenance is required.
   - Always use `git cherry-pick -x`.
   - PRs must list the original `jaex` commit SHAs.

9. If clean cherry pick is not possible.
   - Do not force resolve.
   - Re implement manually using `jaex` as reference only.

Failure to follow these rules is a defect.

10. Finalisation and Merge.
   - Once the integration branch `integrate/jaex-JX-*` is verified:
     - Create a corresponding file named `integrate/jaex-JX-*.md` to signal readiness.
     - Create a Pull Request targeting `develop`.
     - Prioritize merging the PR into `develop`.
     - Delete the `integrate/jaex-JX-*` branch after merge.

11. Branch safety rules.
    - Do not delete, rename, force-push, or modify history on `jaex`.
    - Do not open PRs targeting `jaex`.

12. Preflight UI protection (before importing any approved feature).
    - Create or update `docs/ui_snapshot_develop.md` with:
      - Full main menu structure
      - All `MenuItem` headers and bindings
      - Confirmation of `Import Preset...` and `Export Preset...`
    - Capture evidence using:
      - `git grep -n "MenuItem" src/`
      - `git grep -n "Import Preset" src/`
      - `git grep -n "Export Preset" src/`

13. Feature integration process.
    - Create branch: `git checkout -b integrate/jaex-<feature-id> develop`
    - Apply commits: `git cherry-pick -x <sha1> <sha2> ...`
    - Conflict rules: preserve develop UI, re-apply minimal logic, do not remove/rename menu items.

14. Mandatory verification steps after applying a feature.
    - `git diff --stat develop...HEAD`
    - `git diff develop...HEAD -- src/**/*.axaml src/**/*.axaml.cs src/**/Views/**`
    - `git grep -n "Import Preset" src/`
    - `git grep -n "Export Preset" src/`

15. UI regression gate before PR.
    - Create `docs/ui_snapshot_after_<feature-id>.md`
    - Compare with `docs/ui_snapshot_develop.md`
    - Confirm no removals/regressions
    - Perform manual UI smoke test

16. PR requirements.
    - Title: `Import jaex feature: <feature-id> <feature-name>`
    - Description includes: cherry-picked SHAs, behavior summary, explicit UI confirmation, links to UI snapshots.
