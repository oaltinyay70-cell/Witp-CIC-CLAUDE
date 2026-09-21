# Application error codes

All user-facing errors use a 3-digit `[XXX]` prefix (`docs/scope-v1.md` §13).

| Code | Meaning | Typical resolution |
| --- | --- | --- |
| `[101]` | Game report folder not found by automatic search | Enter the folder where WITP:AE writes `asigint_*.txt`, `*operationsreport_*.txt`, `combatreport_*.txt`, and `Combat_Events_*.txt`. |
| `[102]` | Cannot create `C:\WitP-CIC\Campaigns` | Create the install path or run with permission to write `C:\WitP-CIC`. |
| `[103]` | Cannot write the campaign file | Confirm the Campaigns directory exists and is writable; pick another vault name. |
| `[104]` | Cannot load the campaign file | Choose a different `.wic` file; the selected file may not be a WITP-CIC campaign. |
| `[105]` | A stored game path is missing | Relocate the report folder (Continue flow) or restore the game files. |
| `[106]` | Invalid vault file name | Use a name without path characters (`/ \ : * ? " < > \|`). |
| `[107]` | No campaign files in `\Campaigns` | Start a new campaign first. |
| `[108]` | A text report failed to parse during ingest | The file stays out of the vault; remaining files are still ingested. |
| `[109]` | Entered path is not a directory | Enter a folder path, not a file. |
| `[110]` | Vault name already exists | Choose a different campaign file name. Overwrite is not offered. |
| `[111]` | Campaign file is corrupt | The `.wic` magic, checksum, or payload did not match; start a new campaign from the game folder. |

`.pws` password unlock is **not** an error code in this build: the password is stored for later and is not validated (`docs/pws-inspection.md`).
