
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

4. Mandatory comparison before changes.
   - Compare `develop` and `jaex` using:
     - `git log develop..jaex`
     - `git range-diff develop...jaex`
   - Summarise candidate commits before applying them.

5. Mandatory verification after changes.
   - Build must succeed with zero new warnings.
   - All protected UI items must still exist and be wired.
   - If UI is touched, create or update a UI snapshot file in `docs/`.

6. Provenance is required.
   - Always use `git cherry-pick -x`.
   - PRs must list the original `jaex` commit SHAs.

7. If clean cherry pick is not possible.
   - Do not force resolve.
   - Re implement manually using `jaex` as reference only.

Failure to follow these rules is a defect.

8. Finalisation and Merge.
   - Once the integration branch `integrate/jaex-JX-*` is verified:
     - Create a corresponding file named `integrate/jaex-JX-*.md` to signal readiness.
     - Create a Pull Request targeting `develop`.
     - Prioritize merging the PR into `develop`.
     - Delete the `integrate/jaex-JX-*` branch after merge.
