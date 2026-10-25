# Lessons Learned

Bugs and mistakes already made. Do not repeat them. Each lesson says what happened, the rule, and where it is enforced.

## Inherited (from the original master context)
- **L1 Blank screen:** a View without a code-behind that calls `InitializeComponent()` builds but renders empty. Check every new view.
- **L2 Namespace clash:** `FastDbExplorer.Application` shadows `Application`. In WPF code write `System.Windows.Application` or an alias.
- **L3 localhost vs 127.0.0.1:** localhost can connect through shared memory; 127.0.0.1 needs TCP/IP in SQL Server Configuration Manager and a service restart. Named instance: `127.0.0.1\SQLEXPRESS`; port: `127.0.0.1,1433`.
- **L4 Guard false positive:** a column named `[Update]` or `[Delete]` must not trigger the read-only guard. Identifiers are stripped before checking.
- **L5 Sandbox shell:** the AI sandbox uses `sh`; no brace expansion. Create directories explicitly.
- **L6 ComboBox and DataGrid** need custom templates for dark mode.
- **L7 WebView2 airspace:** it is a native window; WPF cannot draw over it. Use panels beside it or hide it for state screens.
- **L8 Style hacks:** no implicit Style in `Border.Resources` to style the outer border. Use `Border.Style` with `BasedOn` and `DataTrigger`. Inverse bool to Visibility uses a `DataTrigger`, not a null converter.
- **L9 Static checks the AI can run:** XML well-formed, code-behind present, StaticResource keys exist, every `Strings.X` used in XAML exists.

## From the phase of 2026-10 (found in the owner's runs)
- **N1 Two-way mirror properties loop.** Two TwoWay RadioButtons bound to two properties that notify each other caused an endless loop and a StackOverflowException (the process dies; it cannot be caught). Rule: one source of truth; derived properties are read-only or ignore writes; never call a setter from a notification of the same pair. Fixed in commit 1d1a132 and 8416448; ADR-0005.
- **N2 ComboBox gives null.** SelectedItem can become null when the item list changes. Rule: a setter that must have a value reverts null to a default; getters use `?.`. Fixed in 15038c3; the rule itself now lives in Domain (ADR-0007).
- **N3 Never dispose WebView2 inside the event that asked for it** (click, Closing, Unloaded). Queue the disposal, expose a Disposed task, close the window only after it. ADR-0004.
- **N4 Persian text with numbers.** Wrap each number in an isolated LTR run so that "32.22 کیلومتر" reads correctly.
- **N5 Missing usings** broke the build three times. Before pushing a new file, check every namespace it uses.
- **N6 Generator errors** (CS0759, CS0103 for ObservableProperty members) mean the build failed earlier; delete bin and obj and rebuild. Do not edit the code for this.
- **N7 Stale copies.** A ZIP folder is not a git repository: git pull fails and old code runs. Verify the commit (`git log --oneline -1`) before testing.
- **N8 Disk full** makes Visual Studio fail with copy errors. Free space before debugging.
- **N9 Blank map after returning** came from rebuilding WebView2 on every visit. The map view is created once and only hidden or shown.
- **N10 Untested view models.** Every UI failure of the phase appeared only in the running app, because no test reaches a view model.

## Added by the standards audit (2026-10-25)
- **N11 Search before create.** Two test files were added without checking the existing test folder; they duplicated `CoordinateLayerTests` and `CoordinateRenderingTests`. Rule: list the folder and grep the names before adding a test, a utility or a class (coding-standards: search before create).
- **N12 A fix needs a regression test.** The StackOverflow and NullReference fixes have no regression test yet (debugging: add regression coverage). Rule: a fix in Domain gets a Domain test; a fix in a view model gets a test once ADR-0007 moves its rule.
- **N13 Untested commits.** Many commits were pushed without a build. Rule: say "not compiled" in the commit message and in `.ai/current-state.md`; the owner builds before the next slice.
- **N14 Stale documentation.** The module map and feature map said that some areas had no tests when they did. Rule: check claims against the folder before writing them down (ai-documentation: avoid drift).
