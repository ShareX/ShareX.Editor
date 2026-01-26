# jaex Branch Integration Tracking

This folder contains all documentation related to integrating changes from the `jaex` branch into `develop`.

## Current Status

All currently approved `jaex` batches (JX-001 through JX-006) are integrated into `develop` as of 2026-01-26.
No pending integration batches remain at this time.

## Folder Structure

- **completed/** - Completed integration batches (JX-001, JX-002, etc.)
- **ui-snapshots/** - UI state snapshots after each integration batch
- **jaex_feature_candidates.md** - Feature candidates for future integration

## Integration Rules

**These rules are mandatory** (see [AGENTS.md](../../AGENTS.md)):

1. **Never merge** the `jaex` branch
   - Only use `git cherry-pick -x`
   - One logical feature per integration batch

2. **develop is the source of truth**
   - If a conflict occurs, prefer `develop` behaviour
   - Re-apply only the minimal logic required from `jaex`

3. **UI is protected surface area**
   - Do not remove, rename, or hide any existing UI items
   - This explicitly includes all `MenuItem` entries
   - UI changes must be additive unless a task explicitly states otherwise

4. **Mandatory comparison before changes**
   - Compare `develop` and `jaex` using:
     - `git log develop..jaex`
     - `git range-diff develop...jaex`
   - Summarise candidate commits before applying them

5. **Mandatory verification after changes**
   - Build must succeed with zero new warnings
   - All protected UI items must still exist and be wired
   - If UI is touched, create or update a UI snapshot file

6. **Provenance is required**
   - Always use `git cherry-pick -x`
   - PRs must list the original `jaex` commit SHAs

7. **If clean cherry-pick is not possible**
   - Do not force resolve
   - Re-implement manually using `jaex` as reference only

## Integration Workflow

1. Review `jaex_feature_candidates.md` for next feature
2. Run comparison: `git log develop..jaex` and `git range-diff develop...jaex`
3. Create new integration document: `jaex-JX-XXX.md` in `completed/`
4. Cherry-pick commits: `git cherry-pick -x <commit-sha>`
5. Build and verify: `dotnet build` with zero warnings
6. Create UI snapshot in `ui-snapshots/` (if UI changed)
7. Commit with format: `[vX.Y.Z] [Type] Description`
8. Update this README with integration notes

## Integration History

| Batch | Date | Description | Commits |
|-------|------|-------------|---------|
| JX-006 | 2026-01-26 | License header updates | See completed/jaex-JX-006.md |
| JX-005 | 2026-01-26 | Effect annotation robustness | See completed/jaex-JX-005.md |
| JX-004 | 2026-01-26 | Annotation interaction and state fixes | See completed/jaex-JX-004.md |
| JX-003 | 2026-01-26 | Text and speech balloon editing UX | See completed/jaex-JX-003.md |
| JX-002 | 2026-01-26 | Editor toolbar layout updates and pin-to-screen | See completed/jaex-JX-002.md |
| JX-001 | 2026-01-26 | Annotation styling controls and color picker UI | See completed/jaex-JX-001.md |

---

**Failure to follow these rules is a defect.**
