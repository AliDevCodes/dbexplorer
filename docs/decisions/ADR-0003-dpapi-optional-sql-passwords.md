# ADR-0003 Opt-in encrypted SQL passwords (DPAPI)
Status: accepted; supersedes the rule "no passwords on disk" for this one case (decision D28)
Date: 2026-10

Context: the owner wants saved connections without typing the SQL password on every connect. The original rule forbade any password on disk. The app runs only on the owner's Windows user account.

Options:
1. Keep typing the password each time. Safe, but repetitive.
2. Store the password in the profile JSON. Rejected: plaintext.
3. Encrypt with Windows DPAPI (CurrentUser scope + app entropy) in a separate file, opt-in per profile, default off.
4. Own encryption with a user-entered master password. Rejected: more code to own, more failure modes.

Decision: option 3. Details: `features/credentials.md`.
- Vault file `%AppData%\FastDbExplorer\credentials.json`, keyed by profile ID, Base64 DPAPI blobs.
- The password is saved only after the server accepted it; a rejected stored password is removed.
- Changing server or user removes the stored password.
- Never shown in the UI; read only inside the connect method.

Consequences: DPAPI protects against other Windows accounts and copied files, not against malware running as the same user (documented risk). Another Windows user or PC cannot read the vault; the app shows the profile as having no stored password.

Re-evaluate when: the app is distributed to other users, or a multi-user or roaming scenario appears.
