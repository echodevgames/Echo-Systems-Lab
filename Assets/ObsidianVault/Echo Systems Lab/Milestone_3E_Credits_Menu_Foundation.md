# Milestone 3E - Credits Menu Foundation

## 1. Milestone Title

Milestone 3E - Credits Menu Foundation

---

## 2. Goal Line

Main Menu and Pause Menu can open a reusable Credits Menu.

The Credits Menu reads from a CreditsDatabase ScriptableObject, spawns rows for each credit entry, displays project/tool/asset/license information, supports scrolling, and returns cleanly to the menu that opened it.

---

## 3. Naming / Scope Notes

Use Credits Menu as a shared UI system.

This menu should work from:

- Main Menu
- Pause Menu
- Future settings/info menus

Credits should not be hardcoded into UI text objects.

Credits should be data-driven through ScriptableObjects so third-party assets, tools, fonts, music, sounds, and special thanks can be added without rewriting code.

---

## 4. System Pieces

1. CreditsDatabase ScriptableObject
2. CreditEntry serializable data class
3. CreditCategory enum
4. CreditsMenuUI
5. CreditsRowUI
6. Credits row prefab
7. MainMenuController integration
8. PauseMenuController integration
9. Scroll View layout setup
10. Back button return flow

---

## 5. Folder Setup

```text
Assets/Scripts/UI/Credits/
Assets/ScriptableObjects/UI/Credits/
Assets/Prefabs/UI/Elements/
Assets/Prefabs/UI/Menus/
```

Suggested assets:

```text
CreditsDatabase_EchoSystemsLab
Canvas_CreditsMenuUI
CreditsRowUIRoot
```

---

## 6. Numbered Implementation Steps

### Step 1 - Create CreditsDatabase

Path:

```text
Assets/Scripts/UI/Credits/CreditsDatabase.cs
```

Purpose:

CreditsDatabase stores all credits in one reusable data asset.

It includes:

- Credits title
- Credits intro text
- List of CreditEntry objects

Credit categories:

```text
Project
AssetPack
Tool
Font
Music
Sound
SpecialThanks
Other
```

Why this matters:

This keeps credits expandable and portfolio-safe. Third-party asset attribution can be added cleanly without scattering text across scenes.

---

### Step 2 - Create CreditEntry

CreditEntry stores:

```text
showInMenu
category
title
creator
roleOrUsage
sourceName
sourceUrl
licenseNotes
additionalNotes
```

Why this matters:

Each credit can hold enough information for attribution, documentation, and future export into a credits page, website, or portfolio note.

---

### Step 3 - Create CreditsRowUI

Path:

```text
Assets/Scripts/UI/Credits/CreditsRowUI.cs
```

Purpose:

CreditsRowUI receives one CreditEntry and fills row text fields.

Text references:

```text
CategoryText
TitleText
CreatorText
UsageText
SourceText
LicenseText
NotesText
```

Why this matters:

The row prefab becomes reusable, clean, and independent from the menu logic.

---

### Step 4 - Create CreditsMenuUI

Path:

```text
Assets/Scripts/UI/Credits/CreditsMenuUI.cs
```

Purpose:

CreditsMenuUI handles:

- Opening credits
- Closing credits
- Remembering which menu panel opened it
- Building rows from CreditsDatabase
- Clearing old rows before rebuilding
- Forcing layout refresh

Important methods:

```text
Open()
OpenFrom(GameObject panelToReturnTo)
Close()
BuildCredits()
ClearRows()
```

Why this matters:

The same credits menu can be opened from multiple places without duplicating canvases or logic.

---

### Step 5 - Create Credits Row Prefab

Prefab name:

```text
CreditsRowUIRoot
```

Required components:

```text
RectTransform
CreditsRowUI
Layout Element
Canvas Group optional
```

Child text objects:

```text
CategoryText
TitleText
CreatorText
UsageText
SourceText
LicenseText
NotesText
```

Important layout notes:

- Row prefab should have a real preferred height.
- Do not leave row height at 0.
- Text objects should have readable anchors.
- Parent Content object should have Vertical Layout Group.
- Content object should have Content Size Fitter with Vertical Fit set to Preferred Size.

---

### Step 6 - Create Canvas_CreditsMenuUI

Suggested hierarchy:

```text
Canvas_CreditsMenuUI
└── CreditsMenuRoot
    ├── CreditsMenu_BG
    ├── TitleText
    ├── IntroText
    ├── Scroll View
    │   └── Viewport
    │       └── Content
    │           └── CreditsRowUIRoot(Clone)
    └── BackButton
```

CreditsMenuUI references:

```text
Credits Root = CreditsMenuRoot
Title Text = TitleText
Intro Text = IntroText
Row Parent = Content
Row Prefab = CreditsRowUIRoot
Back Button = BackButton
Credits Database = CreditsDatabase_EchoSystemsLab
```

---

### Step 7 - Integrate with MainMenuController

MainMenuController references:

```text
mainMenuPanel
creditsMenuUI
creditsButton
```

Credits button should call:

```text
creditsMenuUI.OpenFrom(mainMenuPanel)
```

Goal:

The main menu hides, credits opens, and Back returns to the main menu.

---

### Step 8 - Integrate with PauseMenuController

PauseMenuController references:

```text
mainPausePanel
creditsMenuUI
creditsButton
```

Credits button should call:

```text
creditsMenuUI.OpenFrom(mainPausePanel)
```

Goal:

The pause panel hides, credits opens, and Back returns to the pause panel.

---

### Step 9 - Layout Debugging Checklist

If rows do not appear, check:

```text
CreditsDatabase has entries
Each entry has Show In Menu enabled
CreditsMenuUI has CreditsDatabase assigned
Row Parent points to Scroll View Content
Row Prefab points to CreditsRowUIRoot prefab
CreditsRowUIRoot has CreditsRowUI script
CreditsRowUIRoot has visible child TMP text
Content has Vertical Layout Group
Content has Content Size Fitter
Row prefab has non-zero preferred height
```

If rows spawn but overlap:

```text
Check Vertical Layout Group spacing
Check Row Prefab Layout Element preferred height
Check Content Size Fitter vertical fit
Check child anchors
```

---

## 7. Unity Setup Checklist

Main Menu:

```text
Canvas_MainMenu
- MainMenuController
- CreditsButton
- Canvas_CreditsMenuUI
```

Pause Menu:

```text
Canvas_PauseMenu
- PauseMenuController
- CreditsButton
- Canvas_CreditsMenuUI
```

Credits Database:

```text
CreditsDatabase_EchoSystemsLab
- Echo Systems Lab project entry
- Unity entry
- Third-party asset pack entries
- Font entries
- Tool entries
- Placeholder art/audio notes
```

---

## 8. Goal Line / Completion Checklist

1. Main Menu Credits button opens Credits Menu.
2. Pause Menu Credits button opens Credits Menu.
3. Credits Menu reads from CreditsDatabase.
4. Credits rows spawn into Scroll View Content.
5. Rows show category, title, creator, source, license, and notes.
6. Scroll View can scroll through entries.
7. Back button returns to the menu that opened credits.
8. Credits menu can be reused in Hub and TargetRangeTrial.
9. No hardcoded credit rows are required in the scene.
10. System is ready for adding asset pack credits over time.

---

## 9. Suggested Commit

```bash
git add .
git commit -m "Add reusable credits menu foundation"
```

Optional tag:

```bash
git tag milestone-3e-credits-menu-foundation
```

---

## 10. Design Notes

Credits are data-driven through CreditsDatabase.

CreditsMenuUI does not need to know whether it was opened from the Main Menu or Pause Menu. It simply stores a returnPanel and restores it on close.

This makes the credits menu reusable across scenes.

The row prefab should remain simple and readable. The visual polish can come later once the final UI theme settles.

---

## 11. Portfolio Value

This checkpoint demonstrates:

- Data-driven UI
- ScriptableObject content database
- Reusable menu architecture
- Dynamic row spawning
- Scroll View layout setup
- Attribution and licensing awareness
- Main Menu and Pause Menu integration

This is a strong portfolio milestone because it shows responsible use of third-party content and scalable UI architecture.
