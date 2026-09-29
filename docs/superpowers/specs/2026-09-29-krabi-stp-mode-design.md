# Krabi STP Mode — Configurable MainForm Behavior

## Goal

`Master_Blue_1` is the primary, most feature-complete branch. `KRABI_STP_2026` is
a much older fork (missing entire Master features like `ucHelp`, large parts of
`ucBackup`/`ucSetting`/`ucReport`) that also contains a handful of genuine,
Krabi-site-specific `MainForm` behaviors. The goal is to bring only those
genuine behaviors into `Master_Blue_1` as an optional mode, gated by a
persistent setting, without reverting any Master improvement or copying
Krabi's MainForm wholesale.

Scope is limited to `MainForm.cs` / `MainForm.Designer.cs` behavior. The other
144 changed files between the branches are out of scope — that divergence is
Master having grown features Krabi predates, not Krabi-specific requirements.

## Background: diff analysis

`git diff Master_Blue_1 KRABI_STP_2026` on `MainForm.cs`/`.designer.cs` is
~9,300 lines combined, but the large majority is Master-only growth after the
fork point (Delivery Order/API sync subsystem ~1150 lines, app self-update,
`chkDirectPrint`, a serial-port architecture rewrite with pluggable data
formats + port watchdog + weight-stability gating, refactored combobox
helpers, formatting-only churn). None of that is reverted or altered by this
work.

The genuine Krabi-specific behaviors, confirmed by reading the diff hunks:

1. **Short/Long Line workflow** — `rbShortLine`/`rbLongLine` radio buttons in
   a new `groupBox5`. When "short line" is selected and a car license is
   entered, the app auto-fetches that truck's last recorded weight-in from
   today's `weight` table instead of requiring a fresh scale reading
   (`getWeightInOnShortLine()`). "Long line" resets weight-in to 0. Persisted
   per-transaction via a `line_type` column.
2. **Line-type segmented daily totals** — `calTimeAndWeigtTotalByLineType()`
   tracks and persists (in a `base_setting_line` table) separate truck
   counts/weight sums per line type, shown via `lbShortTime`/
   `lbShortWeightTotal`/`lbLongTime`/`lbLongWeightTotal`, in addition to
   Master's existing combined total (`calTimeAndWeigtTotal()`, unchanged).
3. **Origin weight/qty fields** — `tbWeightOrigin`/`tbQOrigin` textboxes,
   saved to new `origin_weight`/`origin_q` columns alongside the primary
   weight/qty for a transaction.
4. **`FSettingLine` admin dialog** — opened via `btSettingLine`; configures
   the date/time cutoff (`base_setting_line_date_from`/`_time_from`) used by
   the segmented totals in (2).
5. **Default scale-user assignment** — Master hardcodes
   `tbScaleId="003"`/`tbScaleName="รุ่งฤดี"` for users with edit-weight
   permission in `resetMainForm`. Krabi instead calls `getAndSetFirstUser()`,
   which looks up the first row in the `users` table.

Explicitly **not** ported: Krabi's cancel-password check
(`checkCancelAction`) has the `FCancelPassword` prompt commented out, letting
cancellation of customer codes `09-A-001`/`09-V-001` succeed with no
password. This reads as a debug leftover, not a design requirement, and is a
security regression — Master's existing password check applies unchanged in
both modes.

The Mill combobox filter difference (`weight_type` 1/3 in Master vs. 4 in
Krabi) is site seed-data dependent, not logic — left as Master's existing
query in both modes.

## Configuration

**Setting name:** `IsKrabiSTPVersion`

**Storage:** a new flat `Key=Value` text file, `config_krabistp.txt`, in
`Utils.AppDataDir` — the same location and format already used by
`config_reportmain.txt` (see `ReportMainTemplate.cs`) and other
`config_*.txt` files. No new configuration mechanism is introduced.

**Default:** `false` (Master/Standard behavior) whenever the file is
missing, unreadable, or contains an unparseable value. Reading never throws;
failures fall back to Standard mode silently, mirroring
`ReportMainTemplate.GetSelectedTemplateNumber()`'s error handling.

**Access class:** a new static class, `KrabiStpMode` (own file,
`KrabiStpMode.cs`), with:

```csharp
public static bool IsEnabled { get; }      // reads config_krabistp.txt, cached
public static bool Save(bool enabled);     // writes config_krabistp.txt
```

`Globals.IsKrabiSTPVersion` becomes a thin pass-through property to
`KrabiStpMode.IsEnabled`, so all `MainForm` code reads one name
(`Globals.IsKrabiSTPVersion`) consistent with how other cross-cutting flags
are already exposed off `Globals`.

## Settings UI (`ucSetting`)

- Add a checkbox `chkKrabiStpMode`, label "Krabi STP Mode (Port Version)",
  placed in the existing settings layout near other admin-only toggles.
- Visibility/enable gated by `Globals.isPermissionAddSetting()`, matching the
  existing pattern used for backup-config and report-template controls (see
  `ucSetting.cs` around `btnSaveBackupConfig_Click`).
- On load: `chkKrabiStpMode.Checked = KrabiStpMode.IsEnabled`.
- On save: `KrabiStpMode.Save(chkKrabiStpMode.Checked)`, then show a message
  box: "เปลี่ยนโหมดแล้ว กรุณาปิดโปรแกรมและเปิดใหม่เพื่อให้มีผลกับหน้าจอหลัก"
  (mode changed, restart required for MainForm to apply it).
- No live-apply. Switching mid-session risks corrupting an in-progress
  weighing transaction, the line-total state, or the visible control set —
  the requirements explicitly call for restart-gating when live switching
  isn't safe, and here it isn't.

## MainForm changes

### Designer (additive only)

Add the following controls, ported from Krabi's designer file, placed in
currently unused space on the form (no existing Master control is moved,
resized, re-anchored, or removed):

- `groupBox5` (GroupBox) containing `rbShortLine`, `rbLongLine`
- `tbWeightOrigin`, `tbQOrigin` (TextBox)
- `btSettingLine` (Button)
- `lbShortTime`, `lbShortWeightTotal`, `lbLongTime`, `lbLongWeightTotal`
  (Label)

All added with `Visible = false` at design time; visibility is set at
runtime by `ApplyMainFormMode()`.

### Runtime mode application

One new method, called once from `MainForm_Load` after existing
initialization:

```csharp
private void ApplyMainFormMode()
{
    bool krabi = Globals.IsKrabiSTPVersion;
    groupBox5.Visible = krabi;
    btSettingLine.Visible = krabi;
    lbShortTime.Visible = lbShortWeightTotal.Visible = krabi;
    lbLongTime.Visible = lbLongWeightTotal.Visible = krabi;
    // tbWeightOrigin/tbQOrigin visibility follows groupBox5's association
}
```

### Business logic — branch at the decision point, don't duplicate flows

- **Weight-in on short line**: in the existing weight-in read/save path,
  add `if (Globals.IsKrabiSTPVersion && rbShortLine.Checked) { GetWeightInOnShortLine(...); return; }`
  ahead of the normal scale-read logic. `GetWeightInOnShortLine()` and
  `SetDataLineTypeToRB`/`GetLineTypeRadioValue` are added as new methods,
  ported from Krabi, only invoked under this guard.
- **Segmented totals**: `CalTimeAndWeightTotalByLineType()` (renamed from
  Krabi's `calTimeAndWeigtTotalByLineType` to fix the typo) is called
  alongside — not instead of — Master's existing `calTimeAndWeigtTotal()`,
  guarded by `if (Globals.IsKrabiSTPVersion)`.
- **Origin weight/qty**: read from `tbWeightOrigin`/`tbQOrigin` and included
  in the save parameters only `if (Globals.IsKrabiSTPVersion)`; the columns
  are otherwise left null/unused.
- **`FSettingLine` dialog**: `btSettingLine_Click` opens it; the button
  itself is only visible in Krabi mode, so no additional guard needed inside
  the handler.
- **Default scale-user assignment**: extract both branches into one method:

```csharp
private void AssignDefaultScaleUser()
{
    if (Globals.IsKrabiSTPVersion)
        GetAndSetFirstUser();   // Krabi: first row in users table
    else
    {
        tbScaleId.Text = "003";
        tbScaleName.Text = "รุ่งฤดี";
    }
}
```
  called from `resetMainForm` in place of the current hardcoded assignment.
- **Cancel-password**: no change. Master's `checkCancelAction` behavior
  applies in both modes.

## Database

New objects, all additive (nullable columns / new table), needed only when
Krabi mode is used:

- `weight.line_type` (nullable)
- `weight.origin_weight`, `weight.origin_q` (nullable)
- `base_setting_line` table (line-type cutoff config + running totals)

These are created via an idempotent migration (`CREATE TABLE IF NOT EXISTS`,
`ADD COLUMN IF NOT EXISTS` equivalent for the target DB engine) that runs
unconditionally at application startup, not gated on `IsKrabiSTPVersion`.
Rationale: nullable, unused columns are harmless on Standard-mode
installations, and running the migration unconditionally means an operator
can enable Krabi mode later without needing a DBA to run a separate script
first.

## Files touched

- `SerialPortListener/KrabiStpMode.cs` (new)
- `SerialPortListener/Globals.cs` (add `IsKrabiSTPVersion` pass-through)
- `SerialPortListener/MainForm.cs` (add `ApplyMainFormMode`, ported Krabi
  methods behind guards, `AssignDefaultScaleUser`)
- `SerialPortListener/MainForm.Designer.cs` (add Krabi-only controls,
  hidden by default)
- `SerialPortListener/ucSetting.cs` / `ucSetting.Designer.cs` (add
  `chkKrabiStpMode` checkbox + save/load wiring)
- `SerialPortListener/FSettingLine.cs` / `.Designer.cs` (new, ported from
  Krabi largely as-is — it's a self-contained admin dialog with no Master
  equivalent to reconcile)
- DB migration script (new, additive only)

No other files are modified. `Master_Blue_1` itself is not modified directly
— all work happens on a new branch `feature/krabi-stp-mode` created from it.

## Test matrix

1. **Standard mode** (`IsKrabiSTPVersion=false`): MainForm behaves exactly as
   Master_Blue_1 today — Krabi controls hidden, no line-type logic invoked,
   scale-user hardcoded to 003, existing Delivery Order / self-update /
   direct-print / serial-architecture features all unaffected.
2. **Krabi mode** (`=true`): short/long line radios visible and functional,
   segmented totals compute and persist, origin weight/qty save correctly,
   `FSettingLine` dialog opens and its cutoff setting affects the totals,
   scale-user defaults via `GetAndSetFirstUser()`.
3. **Restart persistence**: toggle false→true, restart, confirm Krabi mode;
   toggle true→false, restart, confirm Standard mode.
4. **Existing Master features regression check** in both modes: Delivery
   Order flow, app self-update check, direct-print toggle, serial port
   stability-gated Read buttons, port watchdog — unaffected by the setting.
5. **Permission**: `chkKrabiStpMode` only editable by
   `Globals.isPermissionAddSetting()`-authorized users; setting persists
   across restart; corrupt/missing config file falls back to Standard mode
   without crashing.

## Explicitly out of scope

- Any of the other 143 changed files between the branches (ucHelp, ucBackup,
  ucReport, ucSetting business logic beyond the new checkbox, datasets,
  installer projects).
- Reverting any Master-only feature (Delivery Order, self-update,
  `chkDirectPrint`, serial architecture rewrite) — these are strict
  improvements over Krabi's older code and are kept as-is in both modes.
- Porting Krabi's disabled cancel-password check.
- Mill combobox filter difference — left as Master's existing query.
