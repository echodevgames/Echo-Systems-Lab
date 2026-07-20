---
title: Checkpoint Build Plan Format
project: Echo Systems Lab
type: workflow-template
tags:
  - unity
  - portfolio
  - systems-design
  - checkpoint
  - workflow
---
# Checkpoint Build Plan Format

Use this note as the source format for future Echo Systems Lab implementation checkpoints.

When asking for a new build step, use language like:

> Use the Checkpoint Build Plan format.

Or:

> Can you break this down in the same Checkpoint Build Plan format?

---

# Purpose

The Checkpoint Build Plan format is used to keep each development milestone structured, implementation-focused, expandable, testable, commit-ready, and portfolio-ready.

Each checkpoint should answer:

- What are we building?
- Why does it matter for the overall system?
- What files, scripts, prefabs, and scene objects are needed?
- What order should the work happen in?
- What is the goal line?
- How do we test it?
- What should be committed when it works?
- What should go into the devlog?
- Does this milestone deserve a portfolio system page?
- How does this strengthen the portfolio?

---

# Standard Workflow

Each major Echo Systems Lab phase should follow this loop:

```text
1. PLAN
   - Build plan
   - Scripts
   - Inspector setup
   - Scene setup
   - Testing checklist

2. TEST / COMMIT / PUSH / DEVLOG
   - Test the feature in Unity
   - Fix bugs
   - Confirm completion checklist
   - Commit to Git
   - Push to remote
   - Write devlog

3. SYSTEM PAGE
   - Create or update portfolio HTML page
   - Add screenshots / diagrams
   - Add metadata / SEO
   - Link from systems.html
   - Confirm all navigation works