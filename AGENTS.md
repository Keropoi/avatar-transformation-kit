# Agent instructions

Read `Documentation~/AGENT_SETUP.md` before installing or modifying a user's Unity project.

- Use the shipped generator and explicit recipe. Do not rewrite animations based on guesses.
- Default to the core recipe and user-selected A/B groups; enable product-specific adapters only when explicitly needed.
- Keep external halo creator authorization marked pending until the author supplies a completed record. Do not invent product links or permissions.
- Keep paid assets, derived materials/meshes/controllers, audio, scenes, builds and validation outputs local.
- Install only onto the selected, prepared source avatar via `KitInstaller`; preserve the source.
- Version or binding mismatches are failures to report, not permission to disable checks silently.
- Do not upload an avatar or publish a user's project as part of installation.
- Run the requested install and validation. Report what passed, failed, and what still needs visual/game validation.
- In a consuming Unity project, this nested file may not be automatically loaded. The user's prompt should explicitly reference `Documentation~/AGENT_SETUP.md`.
