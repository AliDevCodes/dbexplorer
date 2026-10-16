# Saved connections & optional password storage

> Branch: `feature/saved-connection-credentials` (from `feature/nav-shell` @ `c761d3b`). **Status: written, NOT compiled/run by the AI** (no .NET in the AI sandbox). Owner must build + test on Windows.
> **This document supersedes Master Context §2 rule 4 and decision D4** ("no passwords on disk"): there is now an *opt-in*, encrypted exception. Everything else in those rules still holds.

## What already existed (unchanged)
Named profiles with friendly names, stable IDs, uniqueness check, save/delete/new, Connections screen, shell profile switcher, legacy JSON migration, `connections.json` without passwords.

## What this change adds
1. **Per-profile option "Remember password"** (SQL auth only, **off by default**). Persisted as the boolean `RememberPassword` in `connections.json` (a flag, not a secret).
2. **`ICredentialVault`** (Application) + **`DpapiCredentialVault`** (Infrastructure): password encrypted with Windows DPAPI, `CurrentUser` scope, plus app entropy; stored as Base64 in `%AppData%\FastDbExplorer\credentials.json`, keyed by profile ID. Atomic write (temp file + move). Copying the file to another PC/account makes decryption fail, which is treated as "not saved" (user is asked again).
3. **Connect flow** (`ConnectionViewModel.ConnectProfileAsync`):
   - option on + vault has password -> no prompt, connect directly;
   - otherwise the usual prompt; a typed password is stored **only after the server accepted it**;
   - stored password rejected (`DatabaseErrorKind.LoginFailed`) -> entry deleted, clear Persian message, next connect prompts again.
4. **Cleanup rules**: option turned off, profile deleted, or server/auth/user changed on save -> vault entry removed.
5. **UI**: checkbox + note in the SQL-auth block; small lock icon on remembered profiles in the list; updated password notes (new `Localization/CredentialStrings.cs`; old `ShellStrings.SqlPasswordPromptNote` / `ProfilePasswordStorageNote` are now unused).

## Previous vs new behaviour
| | Before | After |
|---|---|---|
| SQL password | prompted on every connect, memory only | same by default; opt-in: kept encrypted, reused automatically |
| `connections.json` | no secrets | no secrets (+ `RememberPassword` flag) |
| Windows auth | no password | unchanged (option hidden, never stored) |

## Files
New: `Application/Abstractions/CredentialAbstractions.cs`, `Infrastructure/DpapiCredentialVault.cs`, `Wpf/Localization/CredentialStrings.cs`, `tests/.../CredentialVaultTests.cs`, this doc.
Modified: `Domain/Models.cs` (`SavedConnection.RememberPassword`), `Infrastructure/*.csproj` (package), `Wpf/ViewModels/ConnectionViewModel.cs`, `Wpf/Views/ConnectionsView.xaml`, `Wpf/App.xaml.cs` (DI line), `CLAUDE.md`.

## Decisions to record in DECISIONS.md on merge
- **Opt-in encrypted password storage** (DPAPI, current user) replaces the blanket "no passwords on disk" rule. Default stays off. Reason: owner asked for persistent credentials; DPAPI avoids plain text and avoids custom crypto.
- **New dependency** `System.Security.Cryptography.ProtectedData 10.*` (floating like the other packages; pin after first restore). Alternative without a package: P/Invoke `CryptProtectData`.

## Security notes
- DPAPI protects against other users/PCs and file theft, **not** against malware running as the same Windows user. Do not enable it on shared accounts.
- Plaintext password exists only in memory while connecting/connected (as before). Byte buffers are zeroed after use; the .NET `string` itself cannot be wiped.
- Not covered: key rotation/versioning (entropy string has `.v1` suffix to allow it later).

## Manual test checklist
1. `dotnet build FastDbExplorer.slnx` and `dotnet test` (new `CredentialVaultTests`).
2. Existing profiles load; checkbox is off for all.
3. SQL profile: tick "remember" -> Connect -> password prompt -> success -> Disconnect -> Connect again: **no prompt**.
4. Open `connections.json`: `"RememberPassword": true`, no password. Open `credentials.json`: Base64 only, plaintext password absent.
5. Untick + Save -> next connect prompts; entry gone from `credentials.json`.
6. Make the stored password wrong (change it on the server) -> connect fails with login error + "stored password was cleared" message -> next connect prompts.
7. Change server or user name of a remembered profile + Save -> entry removed.
8. Delete a remembered profile -> entry removed.
9. Windows-auth profile: checkbox hidden, nothing stored.
10. Shell profile switcher on a remembered profile connects without a prompt.
11. Check light/dark theme and RTL of the checkbox, notes and lock icon (glyph `E72E`).
12. Copy `credentials.json` to another Windows account: profile behaves as "not saved" (prompt, no crash).

## Known risks / not verified
- Whole change uncompiled. Likely trouble spots: package restore (`10.*`), `Segoe Fluent Icons` fallback glyph, `SqlPasswordWindow` text may still claim the password is not kept (not inspected).
- Failed vault write is silent (connect still works, user is simply asked again next time).

## Rollback
Delete the branch, or revert the commit. `credentials.json` can be deleted at any time; `RememberPassword` in `connections.json` is ignored by older builds.
