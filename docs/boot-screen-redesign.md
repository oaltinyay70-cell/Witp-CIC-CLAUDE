# ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC REDESIGN SPECIFICATION

Use this document to edit and specify the exact visual layout, ASCII frames, labels, data fields, and instructions for the **Boot Diagnostic Screen**.

---

## 1. Updated Wireframe Incorporating Ozgur's Comments

```text
════════════════════════════════════════════════════════════════════════════════
[ ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC ]
════════════════════════════════════════════════════════════════════════════════

>> SYSTEM INITIALIZATION: OK
>> COLD-START DIAGNOSTICS COMPLETE. ARCHITECTURE: 2.028-ALPHA-ROCHEFORT
>> AUTHORITATIVE BASE REGISTRY: 1,330 BASES LOADED
>> NAVAL DOCTRINE HULL TYPES: 60 CLASSES COMPILED
>> 6 HEURISTICS READY
>> 10 GRAPHIC SCREENS READY

  ██████╗  ██████╗  ██████╗██╗  ██╗███████╗███████╗ ██████╗ ██████╗ ████████╗
  ██╔══██╗██╔═══██╗██╔════╝██║  ██║██╔════╝██╔════╝██╔═══██╗██╔══██╗╚══██╔══╝
  ██████╔╝██║   ██║██║     ███████║█████╗  █████╗  ██║   ██║██████╔╝   ██║   
  ██╔══██╗██║   ██║██║     ██╔══██║██╔══╝  ██╔══╝  ██║   ██║██╔══██╗   ██║   
  ██║  ██║╚██████╔╝╚██████╗██║  ██║███████╗██║     ╚██████╔╝██║  ██║   ██║   
  ╚═╝  ╚═╝ ╚═════╝  ╚═════╝╚═╝  ╚═╝╚══════╝╚═╝      ╚═════╝ ╚═╝  ╚═╝   ╚═╝   

════════════════════════════════════════════════════════════════════════════════
  [ SYSTEM READY ]
  PRESS ANY KEY OR [ENTER] TO ACCESS OPERATIONAL THEATER DECK
════════════════════════════════════════════════════════════════════════════════
```

---

## 2. Option B (Compact Block ASCII Alternative)

If you prefer a 3-line or single-line stylized font:

```text
════════════════════════════════════════════════════════════════════════════════
[ ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC ]
════════════════════════════════════════════════════════════════════════════════

>> SYSTEM INITIALIZATION: OK
>> COLD-START DIAGNOSTICS COMPLETE. ARCHITECTURE: 2.028-ALPHA-ROCHEFORT
>> AUTHORITATIVE BASE REGISTRY: 1,330 BASES LOADED
>> NAVAL DOCTRINE HULL TYPES: 60 CLASSES COMPILED
>> 6 HEURISTICS READY
>> 10 GRAPHIC SCREENS READY

   █▀█ █▀█ █▀▀ █ █ █▀▀ █▀▀ █▀█ █▀█ ▀█▀   █▀▀ █▀█ █▀█ █▀█
   █▀▄ █▄█ █▄▄ █▀█ ██▄ █▀  █▄█ █▀▄  █    ██▄ █▄█ █▄█ █▄█

════════════════════════════════════════════════════════════════════════════════
  [ SYSTEM READY ]
  PRESS ANY KEY OR [ENTER] TO ACCESS OPERATIONAL THEATER DECK
════════════════════════════════════════════════════════════════════════════════
```

---

## 3. Global System Directives Recorded

1. **System Name Rebranding:**
   - Every reference to `MUTHUR 6000` is being transitioned to **`ROCHEFORT 6000`**.
   - Window Title: `ROCHEFORT 6000 // PACIFIC CIC`
   - Header Badge: `[ ROCHEFORT 6000 // PACIFIC CIC ]`
   - Boot Screen Header: `[ ROCHEFORT 6000 // SYSTEM BOOT DIAGNOSTIC ]`
   - Context Menus & Tooltips: Updated to `Close ROCHEFORT Console`.

2. **Diagnostic Lines Added:**
   - `>> 6 HEURISTICS READY`
   - `>> 10 GRAPHIC SCREENS READY`

3. **Block Deletion:**
   - Deleted the `PACIFIC CAMPAIGN TELEMETRY HARVEST` header and its 5 sub-lines entirely from the Boot Screen.

4. **Big Block ASCII Art:**
   - Inserted big block ASCII art for **ROCHEFORT** in the vacated section.
