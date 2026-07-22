## 1. Constrain workflow authority

- [x] 1.1 Set a read-only top-level permissions baseline in `prerelease.yml`; grant `contents: write` and `pull-requests: write` only to the pre-release publishing job and `contents: write` only to the cleanup job.
- [x] 1.2 Set a read-only top-level permissions baseline in `sync-release-notes.yml`; grant `contents: write` only to the changelog synchronization job.
- [x] 1.3 Confirm `build-and-test.yml` keeps all existing publishing write scopes exclusively on its trusted `publish` job and does not add write authority to build-and-test jobs.
- [x] 1.4 Configure and verify the repository GitHub Actions default workflow token permission as read-only.

## 2. Validate release behavior and assurance evidence

- [x] 2.1 Validate workflow syntax and inspect the affected jobs to confirm each release, tag, changelog, package, OIDC, and PR-label operation retains exactly its required permission.
- [x] 2.2 Update `docs/openssf-assurance.md` with alert #13's required scope, trusted trigger boundary, owner, validation evidence, and exit criterion; distinguish it from resolved alerts.
- [ ] 2.3 Trigger or await a fresh Scorecard analysis and record the resulting status of alerts #13, #14, and #15 without claiming an unobserved result.

## 3. Verify the Best Practices outcome

- [ ] 3.1 Submit the existing truthful `.bestpractices.json` evidence through the supported Best Practices flow for project 13670.
- [ ] 3.2 Retrieve the public project record after processing and record its timestamp, badge level, and any remaining required criteria in assurance evidence.
- [ ] 3.3 Verify the README Best Practices badge continues to resolve to project 13670 and reflects the service-reported status.
