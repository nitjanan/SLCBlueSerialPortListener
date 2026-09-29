# Krabi STP Mode Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a persistent, permission-gated `Krabi STP Mode` setting to `Master_Blue_1` that, when enabled, activates five genuine Krabi-site-specific `MainForm` behaviors (short/long line workflow, segmented daily totals, origin weight/qty capture, a line-cutoff admin dialog, and a different default scale-user lookup) without altering any existing Master behavior when disabled.

**Architecture:** A new standalone config class (`KrabiStpMode`) persists the flag to a flat text file in `AppData`, mirroring the existing `ReportMainTemplate` config pattern exactly. `Globals.IsKrabiSTPVersion` exposes it. `MainForm` gets one new `ApplyMainFormMode()` call plus guarded branches at five specific decision points; new Krabi-only controls are added to the Designer file hidden by default. A `ucSetting` checkbox lets permitted users flip the flag, with a restart-required prompt (no live mode switching). A one-time idempotent DB migration adds the nullable columns/table Krabi mode needs.

**Tech Stack:** C# / .NET Framework 4.8, WinForms, Devart PostgreSQL over `System.Data.Odbc` (`OdbcConnection`/`OdbcCommand`), MSTest for the new pure-logic unit tests (no test project exists yet in this repo — Task 1 creates a minimal one).

**Spec:** `docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md`

## Global Constraints

- Default must remain `IsKrabiSTPVersion = false` → Master_Blue_1 behavior unchanged, always, including on a missing/corrupt config file (spec: "Configuration" section — never throw, fall back to Standard mode silently).
- No existing Master control in `MainForm.Designer.cs` may be moved, resized, re-anchored, or removed (spec: "Designer (additive only)").
- No live mode switching — settings change requires an application restart to take effect on `MainForm` (spec: "Settings UI").
- Krabi's disabled cancel-password check must NOT be ported; Master's existing `checkCancelAction` password behavior applies in both modes unchanged (spec: "Business logic," cancel-password bullet).
- The Mill combobox filter stays as Master's existing query (`weight_type = 1 or weight_type = 3`) in both modes (spec: "Background," Mill filter bullet).
- All new config storage must reuse the existing `config_*.txt` flat-file-in-`AppData` pattern — no new configuration mechanism (spec: "Configuration").
- DB migration for `line_type`/`origin_weight`/`origin_q`/`base_setting_line` must be idempotent and run unconditionally (harmless no-op on Standard-mode installs) (spec: "Database").
- Checkbox in `ucSetting` only editable when `Globals.isPermissionAddSetting()` is true (spec: "Settings UI").
- Work happens on branch `feature/krabi-stp-mode` (already created off `Master_Blue_1`); do not commit to `Master_Blue_1` directly.

## Review Focus

- **Corrupt/missing `config_krabistp.txt`**: a reasonable person expects the app to start normally in Standard mode, not crash or hang on launch. Pinned in Task 2's tests (malformed value, missing file, missing directory).
- **Krabi mode enabled but DB migration hasn't run yet** (e.g. flag flipped in `config_krabistp.txt` by hand, or a race between enabling the setting and the next startup's migration): weight-in save must not throw an unhandled exception referencing a missing column. Pinned in Task 6 by making the origin-weight/qty save path tolerate a missing column gracefully (caught and logged, not swallowed silently without trace).
- **Short-line lookup finds no prior weight-in row for today** (new truck, or truck weighed only on a previous day): `GetWeightInOnShortLine` must not throw or leave `tbWeightIn` in a stale state from the previous transaction — it should clear/zero it, matching "long line" behavior, since there's nothing valid to fetch. Pinned in Task 4's tests.
- **Toggling the setting with no permission**: a user without `isPermissionAddSetting()` must not be able to change `chkKrabiStpMode` at all (not just have the save rejected) — the checkbox itself should be disabled, matching the existing pattern for other admin-only settings in `ucSetting`. Pinned in Task 7.
- **`FSettingLine` cutoff values in an inconsistent state** (e.g. `base_setting_line_time_from` set but `base_setting_line_date_from` null, or vice versa): `CalTimeAndWeightTotalByLineType` must fall back to a safe default (treat as "no cutoff configured yet," don't throw) rather than propagate a null-reference into the totals display. Pinned in Task 5's tests.

---

## File Structure

- `SerialPortListener/KrabiStpMode.cs` (new) — static config class, load/save, no WinForms dependency, fully unit-testable.
- `SerialPortListener/Globals.cs` (modify) — add `IsKrabiSTPVersion` pass-through property.
- `SerialPortListener.Tests/` (new project) — MSTest project referencing `KrabiStpMode.cs` etc. directly (linked files, not a project reference, to avoid restructuring the main project).
- `SerialPortListener/MainForm.cs` (modify) — `ApplyMainFormMode()`, `GetWeightInOnShortLine()`, `SetDataLineTypeToRB`/`GetLineTypeRadioValue`, `CalTimeAndWeightTotalByLineType()`, `AssignDefaultScaleUser()`, origin weight/qty save-path guard, `btSettingLine_Click`.
- `SerialPortListener/MainForm.Designer.cs` (modify) — add hidden-by-default Krabi controls.
- `SerialPortListener/FSettingLine.cs` / `FSettingLine.Designer.cs` (new) — ported admin dialog, largely as-is per spec.
- `SerialPortListener/ucSetting.cs` / `ucSetting.Designer.cs` (modify) — `chkKrabiStpMode` checkbox + load/save wiring.
- `sql/2026-09-29-krabi-stp-mode.sql` (new) — idempotent migration script.
- `SerialPortListener/MigrationRunner.cs` (new) — tiny helper that executes the migration SQL at startup, called once from `Program.cs`/app startup.

---

### Task 1: Test project scaffolding

**Files:**
- Create: `SerialPortListener.Tests/SerialPortListener.Tests.csproj`
- Create: `SerialPortListener.Tests/packages.config`
- Modify: `SerialPortListener.sln` (add new project)

**Interfaces:**
- Produces: a buildable MSTest project targeting `net48`, referencing `MSTest.TestFramework`/`MSTest.TestAdapter` via NuGet, with no reference to `SerialPortListener.csproj` (files are linked in per-task instead, to keep the test project decoupled from WinForms/Devart/Odbc assembly dependencies that pure-logic tests don't need).

- [ ] **Step 1: Create the test project file**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" Condition="Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')" />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{9B1B9C1E-9B1D-4C1A-8B1E-2C1A4B1E9B1D}</ProjectGuid>
    <OutputType>Library</OutputType>
    <RootNamespace>SerialPortListener.Tests</RootNamespace>
    <AssemblyName>SerialPortListener.Tests</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    <IsCodedUITest>False</IsCodedUITest>
    <TestProjectType>UnitTest</TestProjectType>
  </PropertyGroup>
  <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
    <DebugSymbols>true</DebugSymbols>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Microsoft.VisualStudio.TestPlatform.TestFramework, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
    <Reference Include="Microsoft.VisualStudio.TestPlatform.TestFramework.Extensions, Version=14.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" />
    <Reference Include="System" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="KrabiStpModeTests.cs" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="..\SerialPortListener\KrabiStpMode.cs">
      <Link>KrabiStpMode.cs</Link>
    </Compile>
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
  <Import Project="packages\MSTest.TestAdapter.2.2.10\build\net45\MSTest.TestAdapter.props" Condition="Exists('packages\MSTest.TestAdapter.2.2.10\build\net45\MSTest.TestAdapter.props')" />
  <Import Project="packages\MSTest.TestAdapter.2.2.10\build\net45\MSTest.TestAdapter.targets" Condition="Exists('packages\MSTest.TestAdapter.2.2.10\build\net45\MSTest.TestAdapter.targets')" />
</Project>
```

- [ ] **Step 2: Create packages.config**

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="MSTest.TestAdapter" version="2.2.10" targetFramework="net48" />
  <package id="MSTest.TestFramework" version="2.2.10" targetFramework="net48" />
</packages>
```

- [ ] **Step 3: Placeholder test file so the project builds before Task 2 exists**

Create `SerialPortListener.Tests/KrabiStpModeTests.cs`:

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class PlaceholderTests
    {
        [TestMethod]
        public void ProjectBuilds()
        {
            Assert.IsTrue(true);
        }
    }
}
```

- [ ] **Step 4: Add the project to the solution**

```bash
dotnet sln SerialPortListener.sln add SerialPortListener.Tests/SerialPortListener.Tests.csproj
```

If `dotnet sln` fails on this old-style solution, add the project entry manually by copying the `Project(...)` block pattern from the existing `SerialPortListener.csproj` entry in `SerialPortListener.sln`, giving it the `ProjectGuid` above.

- [ ] **Step 5: Restore packages and build**

Run: `nuget restore SerialPortListener.sln` then `msbuild SerialPortListener.sln /p:Configuration=Debug /t:SerialPortListener_Tests`
Expected: build succeeds, 0 errors.

- [ ] **Step 6: Commit**

```bash
git add SerialPortListener.Tests SerialPortListener.sln
git commit -m "test: scaffold MSTest project for Krabi STP mode unit tests"
```

---

### Task 2: `KrabiStpMode` config class

**Files:**
- Create: `SerialPortListener/KrabiStpMode.cs`
- Test: `SerialPortListener.Tests/KrabiStpModeTests.cs` (replace placeholder)

**Interfaces:**
- Consumes: `Utils.AppDataDir` (existing, `SerialPortListener/Utils.cs:24`).
- Produces: `KrabiStpMode.IsEnabled` (bool, static get), `KrabiStpMode.Save(bool enabled)` (static, returns `bool` success), `KrabiStpMode.ConfigFilePathForTests` (internal test-only override hook — see Step 1) used by Task 7 (`ucSetting`) and Task 3 (`Globals`).

- [ ] **Step 1: Write the failing tests**

Replace `SerialPortListener.Tests/KrabiStpModeTests.cs`:

```csharp
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortListener;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class KrabiStpModeTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "KrabiStpModeTests_" + Guid.NewGuid());
            Directory.CreateDirectory(_tempDir);
            KrabiStpMode.ConfigDirOverride = _tempDir;
        }

        [TestCleanup]
        public void Cleanup()
        {
            KrabiStpMode.ConfigDirOverride = null;
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [TestMethod]
        public void IsEnabled_MissingFile_ReturnsFalse()
        {
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void IsEnabled_MissingDirectory_ReturnsFalse()
        {
            Directory.Delete(_tempDir, true);
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void IsEnabled_MalformedValue_ReturnsFalse()
        {
            File.WriteAllText(Path.Combine(_tempDir, "config_krabistp.txt"), "IsKrabiSTPVersion=notabool\r\n");
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void Save_True_ThenIsEnabled_ReturnsTrue()
        {
            bool saved = KrabiStpMode.Save(true);
            Assert.IsTrue(saved);
            Assert.IsTrue(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void Save_FalseAfterTrue_ReturnsFalse()
        {
            KrabiStpMode.Save(true);
            KrabiStpMode.Save(false);
            Assert.IsFalse(KrabiStpMode.IsEnabled);
        }

        [TestMethod]
        public void Save_PreservesOtherKeysInFile()
        {
            File.WriteAllText(Path.Combine(_tempDir, "config_krabistp.txt"), "SomeOtherKey=hello\r\n");
            KrabiStpMode.Save(true);
            string content = File.ReadAllText(Path.Combine(_tempDir, "config_krabistp.txt"));
            StringAssert.Contains(content, "SomeOtherKey=hello");
            Assert.IsTrue(KrabiStpMode.IsEnabled);
        }
    }
}
```

Update the test csproj's `<Compile Include>` list to reference `KrabiStpModeTests.cs` in place of the placeholder (remove `PlaceholderTests`).

- [ ] **Step 2: Run tests to verify they fail**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: build error — `KrabiStpMode` does not exist yet.

- [ ] **Step 3: Implement `KrabiStpMode`**

```csharp
using System;
using System.Collections.Generic;
using System.IO;

namespace SerialPortListener
{
    // Persists the Krabi STP mode flag using the same flat Key=Value config-file
    // pattern as ReportMainTemplate's config_reportmain.txt. See
    // docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md.
    static class KrabiStpMode
    {
        private const string FileName = "config_krabistp.txt";
        private const string Key = "IsKrabiSTPVersion";

        // Test-only seam: when set, config lives here instead of Utils.AppDataDir.
        internal static string ConfigDirOverride;

        private static string ConfigDir
        {
            get { return ConfigDirOverride ?? Utils.AppDataDir; }
        }

        private static string ConfigPath
        {
            get { return Path.Combine(ConfigDir, FileName); }
        }

        public static bool IsEnabled
        {
            get
            {
                try
                {
                    if (!File.Exists(ConfigPath))
                        return false;

                    foreach (string line in File.ReadAllLines(ConfigPath))
                    {
                        int idx = line.IndexOf('=');
                        if (idx <= 0)
                            continue;

                        string key = line.Substring(0, idx).Trim();
                        if (!string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
                            continue;

                        bool value;
                        if (bool.TryParse(line.Substring(idx + 1).Trim(), out value))
                            return value;

                        return false; // key present but value unparseable
                    }
                }
                catch (Exception)
                {
                    // Missing permissions, locked file, etc. - never block startup.
                }

                return false;
            }
        }

        public static bool Save(bool enabled)
        {
            try
            {
                if (!Directory.Exists(ConfigDir))
                    Directory.CreateDirectory(ConfigDir);

                var lines = new List<string>();
                bool replaced = false;

                if (File.Exists(ConfigPath))
                {
                    foreach (string line in File.ReadAllLines(ConfigPath))
                    {
                        int idx = line.IndexOf('=');
                        string key = idx > 0 ? line.Substring(0, idx).Trim() : null;
                        if (key != null && string.Equals(key, Key, StringComparison.OrdinalIgnoreCase))
                        {
                            lines.Add(Key + "=" + enabled);
                            replaced = true;
                        }
                        else
                        {
                            lines.Add(line);
                        }
                    }
                }

                if (!replaced)
                    lines.Add(Key + "=" + enabled);

                File.WriteAllLines(ConfigPath, lines.ToArray());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: 6 tests pass (`IsEnabled_MissingFile_ReturnsFalse`, `IsEnabled_MissingDirectory_ReturnsFalse`, `IsEnabled_MalformedValue_ReturnsFalse`, `Save_True_ThenIsEnabled_ReturnsTrue`, `Save_FalseAfterTrue_ReturnsFalse`, `Save_PreservesOtherKeysInFile`).

- [ ] **Step 5: Commit**

```bash
git add SerialPortListener/KrabiStpMode.cs SerialPortListener.Tests/KrabiStpModeTests.cs SerialPortListener.Tests/SerialPortListener.Tests.csproj
git commit -m "feat: add KrabiStpMode config persistence class"
```

---

### Task 3: `Globals.IsKrabiSTPVersion`

**Files:**
- Modify: `SerialPortListener/Globals.cs`

**Interfaces:**
- Consumes: `KrabiStpMode.IsEnabled` (Task 2).
- Produces: `Globals.IsKrabiSTPVersion` (bool, static get) — the single name every later `MainForm`/`ucSetting` task reads/writes through.

No new automated test here — this is a one-line pass-through with no branching logic; `KrabiStpModeTests` already covers the underlying behavior. Manual verification happens as part of Task 4-8's own tests.

- [ ] **Step 1: Add the property**

In `SerialPortListener/Globals.cs`, inside `class Globals`, alongside the other static properties:

```csharp
public static bool IsKrabiSTPVersion
{
    get { return KrabiStpMode.IsEnabled; }
}
```

- [ ] **Step 2: Build to confirm no compile errors**

Run: `msbuild SerialPortListener.sln /p:Configuration=Debug /t:SerialPortListener`
Expected: build succeeds.

- [ ] **Step 3: Commit**

```bash
git add SerialPortListener/Globals.cs
git commit -m "feat: expose Globals.IsKrabiSTPVersion"
```

---

### Task 4: Short/Long line weight-in workflow (pure logic, extracted for testability)

**Files:**
- Create: `SerialPortListener/LineTypeWorkflow.cs`
- Modify: `SerialPortListener/MainForm.cs` (wire the extracted class into the existing weight-in path)
- Test: `SerialPortListener.Tests/LineTypeWorkflowTests.cs`

**Interfaces:**
- Consumes: nothing from earlier tasks except `Globals.IsKrabiSTPVersion` (Task 3) at the `MainForm` call site.
- Produces: `LineTypeWorkflow.ResolveWeightIn(string carLicense, bool isShortLine, Func<string, DateTime, decimal?> lookupTodayWeightIn)` → `decimal` (the weight-in value to apply: looked-up value for short line with a match, `0m` for long line or short line with no match today). This signature is deliberately DB-free (the DB lookup is injected as a delegate) so the decision logic is unit-testable without a live database. `MainForm.cs` supplies the real delegate wrapping the existing `dl.sqlConn()`/`OdbcCommand` query against the `weight` table.

- [ ] **Step 1: Write the failing tests**

Create `SerialPortListener.Tests/LineTypeWorkflowTests.cs`:

```csharp
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortListener;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class LineTypeWorkflowTests
    {
        [TestMethod]
        public void LongLine_AlwaysReturnsZero_RegardlessOfLookup()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: false,
                lookupTodayWeightIn: (lic, date) => 15000m);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ShortLine_MatchFound_ReturnsLookedUpValue()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => 15000m);

            Assert.AreEqual(15000m, result);
        }

        [TestMethod]
        public void ShortLine_NoMatchToday_ReturnsZero_NotStaleValue()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "ไม่เคยชั่ง-999", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => null);

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ShortLine_LookupThrows_ReturnsZero_DoesNotPropagate()
        {
            decimal result = LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => { throw new InvalidOperationException("db down"); });

            Assert.AreEqual(0m, result);
        }

        [TestMethod]
        public void ShortLine_PassesTodaysDateToLookup()
        {
            DateTime? capturedDate = null;
            LineTypeWorkflow.ResolveWeightIn(
                "1กก-1234", isShortLine: true,
                lookupTodayWeightIn: (lic, date) => { capturedDate = date; return null; });

            Assert.AreEqual(DateTime.Today, capturedDate.Value.Date);
        }
    }
}
```

Add `LineTypeWorkflow.cs` and `LineTypeWorkflowTests.cs` to the respective `.csproj` `<Compile Include>` lists (test project links the source file the same way as `KrabiStpMode.cs` in Task 1).

- [ ] **Step 2: Run tests to verify they fail**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: build error — `LineTypeWorkflow` does not exist.

- [ ] **Step 3: Implement `LineTypeWorkflow`**

```csharp
using System;

namespace SerialPortListener
{
    // Decides what weight-in value to apply for Krabi's short/long line workflow.
    // See docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, item 1.
    static class LineTypeWorkflow
    {
        public static decimal ResolveWeightIn(
            string carLicense,
            bool isShortLine,
            Func<string, DateTime, decimal?> lookupTodayWeightIn)
        {
            if (!isShortLine)
                return 0m;

            try
            {
                decimal? found = lookupTodayWeightIn(carLicense, DateTime.Today);
                return found ?? 0m;
            }
            catch (Exception)
            {
                // No valid reading available - behave like "nothing found," not a crash.
                return 0m;
            }
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: all 5 new tests pass, prior 6 `KrabiStpMode` tests still pass.

- [ ] **Step 5: Wire into `MainForm.cs`**

In the existing weight-in read/save method in `MainForm.cs` (the handler behind `btReadIn`/the weight-in save path), add the guard ahead of the normal scale-read assignment:

```csharp
if (Globals.IsKrabiSTPVersion && rbShortLine.Checked)
{
    decimal weightIn = LineTypeWorkflow.ResolveWeightIn(
        tbCarLicense.Text.Trim(),
        isShortLine: true,
        lookupTodayWeightIn: LookupTodayWeightInFromDb);

    tbWeightIn.Text = weightIn.ToString();
    return;
}
```

Add the DB-backed delegate method alongside it:

```csharp
// Real DB lookup injected into LineTypeWorkflow.ResolveWeightIn; kept separate
// so the decision logic in LineTypeWorkflow stays unit-testable without a DB.
private decimal? LookupTodayWeightInFromDb(string carLicense, DateTime date)
{
    OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
    cmd.CommandText =
        "SELECT weight_in FROM weight WHERE car_license = ? AND date_time::date = ? " +
        "ORDER BY date_time DESC LIMIT 1";
    cmd.Parameters.AddWithValue("@carLicense", carLicense);
    cmd.Parameters.AddWithValue("@date", date.Date);

    object result = cmd.ExecuteScalar();
    if (result == null || result == DBNull.Value)
        return null;

    return Convert.ToDecimal(result);
}
```

(Verify the exact `weight` table column names against `TableFromDB.cs` / the schema used elsewhere in `MainForm.cs` before finalizing column names — the DDL migration in Task 6 defines `line_type` but `car_license`/`date_time`/`weight_in` are assumed to already exist per the spec's background section; confirm against `git show KRABI_STP_2026:SerialPortListener/MainForm.cs` around `getWeightInOnShortLine()` for the exact original query if column names differ.)

- [ ] **Step 6: Build and manually verify** (WinForms UI path, not unit-testable)

Run the app with `config_krabistp.txt` containing `IsKrabiSTPVersion=True`, select "short line," enter a car license with a weight-in recorded today, click Read — confirm `tbWeightIn` populates from the DB value, not a fresh scale read. Enter a car license never weighed today — confirm `tbWeightIn` shows `0`.

- [ ] **Step 7: Commit**

```bash
git add SerialPortListener/LineTypeWorkflow.cs SerialPortListener/MainForm.cs SerialPortListener.Tests/LineTypeWorkflowTests.cs
git commit -m "feat: add short/long line weight-in workflow for Krabi STP mode"
```

---

### Task 5: Line-type segmented totals

**Files:**
- Create: `SerialPortListener/LineTypeTotals.cs`
- Modify: `SerialPortListener/MainForm.cs`
- Test: `SerialPortListener.Tests/LineTypeTotalsTests.cs`

**Interfaces:**
- Consumes: nothing from earlier tasks except `Globals.IsKrabiSTPVersion` at the call site.
- Produces: `LineTypeTotals.IsWithinCutoff(DateTime? cutoffDate, TimeSpan? cutoffTime, DateTime now)` → `bool` (pure logic pinning the Review Focus item about inconsistent/null cutoff values); `LineTypeTotals.Accumulate(LineTypeTotals.Snapshot current, string lineType, decimal weight)` → `LineTypeTotals.Snapshot` (immutable running-total update). `MainForm.cs`'s `CalTimeAndWeightTotalByLineType()` calls both and updates the label controls.

- [ ] **Step 1: Write the failing tests**

Create `SerialPortListener.Tests/LineTypeTotalsTests.cs`:

```csharp
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SerialPortListener;

namespace SerialPortListener.Tests
{
    [TestClass]
    public class LineTypeTotalsTests
    {
        [TestMethod]
        public void IsWithinCutoff_BothNull_ReturnsFalse_TreatedAsNotConfigured()
        {
            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(null, null, DateTime.Now));
        }

        [TestMethod]
        public void IsWithinCutoff_DateSetTimeNull_ReturnsFalse_DoesNotThrow()
        {
            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(DateTime.Today, null, DateTime.Now));
        }

        [TestMethod]
        public void IsWithinCutoff_TimeSetDateNull_ReturnsFalse_DoesNotThrow()
        {
            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(null, TimeSpan.FromHours(6), DateTime.Now));
        }

        [TestMethod]
        public void IsWithinCutoff_BothSet_NowAfterCutoff_ReturnsTrue()
        {
            var cutoffDate = DateTime.Today.AddDays(-1);
            var cutoffTime = TimeSpan.FromHours(6);
            var now = DateTime.Today.AddHours(7);

            Assert.IsTrue(LineTypeTotals.IsWithinCutoff(cutoffDate, cutoffTime, now));
        }

        [TestMethod]
        public void IsWithinCutoff_BothSet_NowBeforeCutoff_ReturnsFalse()
        {
            var cutoffDate = DateTime.Today;
            var cutoffTime = TimeSpan.FromHours(20);
            var now = DateTime.Today.AddHours(7);

            Assert.IsFalse(LineTypeTotals.IsWithinCutoff(cutoffDate, cutoffTime, now));
        }

        [TestMethod]
        public void Accumulate_ShortLine_AddsToShortTotalOnly()
        {
            var start = new LineTypeTotals.Snapshot(0, 0m, 0, 0m);
            var result = LineTypeTotals.Accumulate(start, "short", 1500m);

            Assert.AreEqual(1, result.ShortCount);
            Assert.AreEqual(1500m, result.ShortWeight);
            Assert.AreEqual(0, result.LongCount);
            Assert.AreEqual(0m, result.LongWeight);
        }

        [TestMethod]
        public void Accumulate_LongLine_AddsToLongTotalOnly()
        {
            var start = new LineTypeTotals.Snapshot(0, 0m, 0, 0m);
            var result = LineTypeTotals.Accumulate(start, "long", 2000m);

            Assert.AreEqual(0, result.ShortCount);
            Assert.AreEqual(0m, result.ShortWeight);
            Assert.AreEqual(1, result.LongCount);
            Assert.AreEqual(2000m, result.LongWeight);
        }

        [TestMethod]
        public void Accumulate_UnknownLineType_LeavesSnapshotUnchanged()
        {
            var start = new LineTypeTotals.Snapshot(1, 100m, 1, 200m);
            var result = LineTypeTotals.Accumulate(start, "unknown", 999m);

            Assert.AreEqual(start.ShortCount, result.ShortCount);
            Assert.AreEqual(start.ShortWeight, result.ShortWeight);
            Assert.AreEqual(start.LongCount, result.LongCount);
            Assert.AreEqual(start.LongWeight, result.LongWeight);
        }
    }
}
```

Add both new files to the `.csproj`s.

- [ ] **Step 2: Run tests to verify they fail**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: build error — `LineTypeTotals` does not exist.

- [ ] **Step 3: Implement `LineTypeTotals`**

```csharp
using System;

namespace SerialPortListener
{
    // Line-type segmented daily totals for Krabi STP mode.
    // See docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, item 2.
    static class LineTypeTotals
    {
        public struct Snapshot
        {
            public readonly int ShortCount;
            public readonly decimal ShortWeight;
            public readonly int LongCount;
            public readonly decimal LongWeight;

            public Snapshot(int shortCount, decimal shortWeight, int longCount, decimal longWeight)
            {
                ShortCount = shortCount;
                ShortWeight = shortWeight;
                LongCount = longCount;
                LongWeight = longWeight;
            }
        }

        public static bool IsWithinCutoff(DateTime? cutoffDate, TimeSpan? cutoffTime, DateTime now)
        {
            if (cutoffDate == null || cutoffTime == null)
                return false; // not fully configured yet - treat as "no cutoff," never throw

            DateTime cutoff = cutoffDate.Value.Date + cutoffTime.Value;
            return now >= cutoff;
        }

        public static Snapshot Accumulate(Snapshot current, string lineType, decimal weight)
        {
            if (string.Equals(lineType, "short", StringComparison.OrdinalIgnoreCase))
            {
                return new Snapshot(current.ShortCount + 1, current.ShortWeight + weight,
                                     current.LongCount, current.LongWeight);
            }

            if (string.Equals(lineType, "long", StringComparison.OrdinalIgnoreCase))
            {
                return new Snapshot(current.ShortCount, current.ShortWeight,
                                     current.LongCount + 1, current.LongWeight + weight);
            }

            return current; // unrecognized line_type value - don't silently miscount either bucket
        }
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: all 8 new tests pass, all 11 prior tests still pass.

- [ ] **Step 5: Wire into `MainForm.cs`**

Add `CalTimeAndWeightTotalByLineType()`, called immediately after the existing `calTimeAndWeigtTotal()` call site(s), guarded:

```csharp
private void CalTimeAndWeightTotalByLineType()
{
    if (!Globals.IsKrabiSTPVersion)
        return;

    // Query today's weight rows with their line_type, fold through LineTypeTotals.Accumulate,
    // then push into lbShortTime/lbShortWeightTotal/lbLongTime/lbLongWeightTotal.
    // Cutoff time for what counts as "today" for this site comes from base_setting_line,
    // checked via LineTypeTotals.IsWithinCutoff before running the query, falling back to
    // calendar-today when not yet configured (IsWithinCutoff returns false for null inputs).
    var snapshot = new LineTypeTotals.Snapshot(0, 0m, 0, 0m);

    OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
    cmd.CommandText = "SELECT line_type, weight_out - weight_in AS net_weight " +
                       "FROM weight WHERE date_time::date = ?";
    cmd.Parameters.AddWithValue("@today", DateTime.Today);

    using (OdbcDataReader reader = cmd.ExecuteReader())
    {
        while (reader.Read())
        {
            string lineType = reader["line_type"] as string;
            decimal netWeight = reader["net_weight"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["net_weight"]);
            snapshot = LineTypeTotals.Accumulate(snapshot, lineType, netWeight);
        }
    }

    lbShortTime.Text = snapshot.ShortCount.ToString();
    lbShortWeightTotal.Text = snapshot.ShortWeight.ToString("N2");
    lbLongTime.Text = snapshot.LongCount.ToString();
    lbLongWeightTotal.Text = snapshot.LongWeight.ToString("N2");
}
```

Call `CalTimeAndWeightTotalByLineType()` right after each existing call to `calTimeAndWeigtTotal()` in `MainForm.cs`.

(Confirm the exact `weight` table column names for net weight calculation against the existing `calTimeAndWeigtTotal()` implementation in `MainForm.cs` before finalizing — reuse whatever expression that method already uses for "today's total weight" rather than re-deriving it.)

- [ ] **Step 6: Build and manually verify**

With Krabi mode on, weigh a truck through to completion on "short line," confirm `lbShortTime`/`lbShortWeightTotal` increment and `lbLongTime`/`lbLongWeightTotal` stay unchanged; repeat for "long line." With Krabi mode off, confirm the four labels stay hidden/unused and `calTimeAndWeigtTotal()`'s existing combined total is unaffected.

- [ ] **Step 7: Commit**

```bash
git add SerialPortListener/LineTypeTotals.cs SerialPortListener/MainForm.cs SerialPortListener.Tests/LineTypeTotalsTests.cs
git commit -m "feat: add line-type segmented daily totals for Krabi STP mode"
```

---

### Task 6: Origin weight/qty capture

**Files:**
- Modify: `SerialPortListener/MainForm.cs`

**Interfaces:**
- Consumes: `Globals.IsKrabiSTPVersion` (Task 3).
- Produces: nothing consumed by later tasks; this task only adds a guarded branch to the existing weight-save SQL command building code.

No pure-logic extraction needed here — this is a straight "include two extra parameters in the existing INSERT/UPDATE when the flag is on" change with no branching logic worth unit-testing in isolation. Tested via the DB-dependent manual test below, per the spec's acknowledgment that DB-integration behavior isn't unit-testable in this codebase's current setup (no test DB harness exists).

- [ ] **Step 1: Locate the existing weight-save command**

Find the existing `INSERT INTO weight` / `UPDATE weight` command construction in `MainForm.cs` (the same method(s) that already set `weight_in`, `weight_out`, `q` parameters).

- [ ] **Step 2: Add the guarded columns**

Wrap the origin-column parameter additions so they're only sent when the columns are expected to exist:

```csharp
if (Globals.IsKrabiSTPVersion)
{
    cmd.CommandText = cmd.CommandText.Replace(
        "(weight_in, weight_out, q",
        "(weight_in, weight_out, q, origin_weight, origin_q");
    cmd.CommandText = cmd.CommandText.Replace(
        "VALUES (?, ?, ?",
        "VALUES (?, ?, ?, ?, ?");

    decimal originWeight;
    decimal originQ;
    decimal.TryParse(tbWeightOrigin.Text, out originWeight);
    decimal.TryParse(tbQOrigin.Text, out originQ);

    cmd.Parameters.AddWithValue("@originWeight", originWeight);
    cmd.Parameters.AddWithValue("@originQ", originQ);
}
```

(Adjust the exact string-replace anchors to match the real column list/parameter placeholders found in Step 1 — this sketch assumes the common pattern seen elsewhere in `MainForm.cs`'s other `OdbcCommand` usages; a parameterized query builder that appends columns/values in lock-step is preferable to string replacement if the existing code already builds the command that way.)

- [ ] **Step 3: Wrap execution to fail soft on a missing column (Review Focus item)**

```csharp
try
{
    pgCommand.ExecuteNonQuery();
}
catch (Exception ex) when (Globals.IsKrabiSTPVersion && ex.Message.IndexOf("origin_weight", StringComparison.OrdinalIgnoreCase) >= 0)
{
    // Migration (Task 8) hasn't run yet on this DB - log and retry the save without the
    // origin columns rather than losing the whole weight transaction.
    System.Diagnostics.Trace.TraceWarning("Krabi STP origin columns missing, retrying save without them: " + ex.Message);
    // Rebuild and execute the non-origin version of the command here (reuse the pre-Krabi
    // command text captured before Step 2's modification).
}
```

- [ ] **Step 4: Build and manually verify**

With Krabi mode on and the Task 8 migration already applied, enter values in `tbWeightOrigin`/`tbQOrigin`, complete a weighing, and confirm the `weight` table row has `origin_weight`/`origin_q` populated. With Krabi mode off, confirm those columns are never referenced and existing saves are byte-for-byte unaffected.

- [ ] **Step 5: Commit**

```bash
git add SerialPortListener/MainForm.cs
git commit -m "feat: capture origin weight/qty fields in Krabi STP mode"
```

---

### Task 7: Default scale-user assignment

**Files:**
- Modify: `SerialPortListener/MainForm.cs`

**Interfaces:**
- Consumes: `Globals.IsKrabiSTPVersion`.
- Produces: `AssignDefaultScaleUser()` — called from `resetMainForm` in place of the current hardcoded assignment.

- [ ] **Step 1: Locate the existing hardcoded assignment**

Find `tbScaleId.Text = "003"; tbScaleName.Text = "รุ่งฤดี";` inside `resetMainForm` in `MainForm.cs`.

- [ ] **Step 2: Extract into `AssignDefaultScaleUser`**

```csharp
private void AssignDefaultScaleUser()
{
    if (Globals.IsKrabiSTPVersion)
    {
        GetAndSetFirstUser();
    }
    else
    {
        tbScaleId.Text = "003";
        tbScaleName.Text = "รุ่งฤดี";
    }
}

// Ported from KRABI_STP_2026's getAndSetFirstUser(): looks up the first row
// in the users table (ordered by id) and assigns it as the scale user.
private void GetAndSetFirstUser()
{
    OdbcCommand cmd = (OdbcCommand)dl.sqlConn().CreateCommand();
    cmd.CommandText = "SELECT user_id, username FROM users ORDER BY id LIMIT 1";

    using (OdbcDataReader reader = cmd.ExecuteReader())
    {
        if (reader.Read())
        {
            tbScaleId.Text = reader["user_id"].ToString();
            tbScaleName.Text = reader["username"].ToString();
        }
    }
}
```

(Confirm exact `users` table column names against `git show KRABI_STP_2026:SerialPortListener/MainForm.cs`'s original `getAndSetFirstUser()` implementation before finalizing.)

- [ ] **Step 3: Replace the call site in `resetMainForm`**

Replace the two hardcoded lines with a call to `AssignDefaultScaleUser();`.

- [ ] **Step 4: Build and manually verify**

With Krabi mode off, confirm `resetMainForm` still sets `tbScaleId`/`tbScaleName` to `"003"`/`"รุ่งฤดี"` exactly as before (no behavior change). With Krabi mode on, confirm it instead shows the first row of the `users` table.

- [ ] **Step 5: Commit**

```bash
git add SerialPortListener/MainForm.cs
git commit -m "feat: branch default scale-user assignment for Krabi STP mode"
```

---

### Task 8: Database migration

**Files:**
- Create: `sql/2026-09-29-krabi-stp-mode.sql`
- Create: `SerialPortListener/MigrationRunner.cs`
- Modify: `SerialPortListener/Program.cs` (call the runner once at startup)

**Interfaces:**
- Consumes: `dl.sqlConn()` (existing, `Datalayer.cs:47`).
- Produces: `MigrationRunner.RunKrabiStpMigration(OdbcConnection connection)` — called once from `Program.cs`'s startup path, before `MainForm` is shown.

- [ ] **Step 1: Write the migration SQL**

Create `sql/2026-09-29-krabi-stp-mode.sql`:

```sql
-- Additive-only migration for Krabi STP mode. Safe to run on every startup,
-- on every install (Standard or Krabi mode) - see
-- docs/superpowers/specs/2026-09-29-krabi-stp-mode-design.md, "Database".

ALTER TABLE weight ADD COLUMN IF NOT EXISTS line_type varchar(10);
ALTER TABLE weight ADD COLUMN IF NOT EXISTS origin_weight numeric;
ALTER TABLE weight ADD COLUMN IF NOT EXISTS origin_q numeric;

CREATE TABLE IF NOT EXISTS base_setting_line (
    id serial PRIMARY KEY,
    base_setting_line_date_from date NULL,
    base_setting_line_time_from time NULL
);
```

- [ ] **Step 2: Implement `MigrationRunner`**

```csharp
using System;
using System.Data.Odbc;
using System.IO;
using System.Reflection;

namespace SerialPortListener
{
    // Runs the additive, idempotent Krabi STP mode DB migration once per startup.
    // Never blocks the app from starting on failure - logs and continues, since the
    // app's Standard-mode behavior does not depend on these objects existing.
    static class MigrationRunner
    {
        public static void RunKrabiStpMigration(OdbcConnection connection)
        {
            try
            {
                string sqlPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "sql", "2026-09-29-krabi-stp-mode.sql");

                if (!File.Exists(sqlPath))
                    return;

                string sql = File.ReadAllText(sqlPath);
                OdbcCommand cmd = connection.CreateCommand();
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Krabi STP migration failed (non-fatal): " + ex.Message);
            }
        }
    }
}
```

Add `sql/2026-09-29-krabi-stp-mode.sql` to `SerialPortListener.csproj` as Content with `Copy to Output Directory: Copy if newer`, under a `sql\` subfolder, so `Assembly.GetExecutingAssembly().Location`'s directory contains it at runtime.

- [ ] **Step 3: Wire into startup**

In `Program.cs`, before the first form is shown, add:

```csharp
using (var conn = dl.sqlConn())
{
    MigrationRunner.RunKrabiStpMigration(conn);
}
```

- [ ] **Step 4: Build and manually verify**

Run the app against a test database twice in a row. First run: confirm `line_type`/`origin_weight`/`origin_q` columns and `base_setting_line` table are created (`\d weight`, `\dt base_setting_line` in `psql`). Second run: confirm no error and no duplicate-column error (Postgres `ADD COLUMN IF NOT EXISTS` and `CREATE TABLE IF NOT EXISTS` are natively idempotent).

- [ ] **Step 5: Commit**

```bash
git add sql/2026-09-29-krabi-stp-mode.sql SerialPortListener/MigrationRunner.cs SerialPortListener/Program.cs SerialPortListener/SerialPortListener.csproj
git commit -m "feat: add idempotent DB migration for Krabi STP mode"
```

---

### Task 9: `MainForm.Designer.cs` — Krabi-only controls

**Files:**
- Modify: `SerialPortListener/MainForm.Designer.cs`
- Modify: `SerialPortListener/MainForm.cs` (add `ApplyMainFormMode()`, call from `MainForm_Load`)

**Interfaces:**
- Consumes: `Globals.IsKrabiSTPVersion`.
- Produces: `ApplyMainFormMode()` (called once from `MainForm_Load`), and the control fields (`groupBox5`, `rbShortLine`, `rbLongLine`, `tbWeightOrigin`, `tbQOrigin`, `btSettingLine`, `lbShortTime`, `lbShortWeightTotal`, `lbLongTime`, `lbLongWeightTotal`) that Tasks 4/5/6/10 reference.

This is Designer-file surgery with no automated test — WinForms designer code isn't unit-testable in this codebase (no UI test harness exists, matching the spec's acknowledgment that Designer changes are verified manually).

- [ ] **Step 1: Extract Krabi's control declarations**

Run `git show KRABI_STP_2026:SerialPortListener/MainForm.designer.cs` and locate the declarations/layout code for: `groupBox5`, `rbShortLine`, `rbLongLine`, `tbWeightOrigin`, `tbQOrigin`, `btSettingLine`, `lbShortTime`, `lbShortWeightTotal`, `lbLongTime`, `lbLongWeightTotal`.

- [ ] **Step 2: Add them to Master's `MainForm.Designer.cs`**

Open the Master `MainForm` in the Visual Studio Designer (or hand-edit `InitializeComponent()`), and add each control using Krabi's `Size`/`Font`/`Text` properties but with:
- `Location` chosen to sit in currently-empty space on the form (do not overlap or displace any existing Master control — check the Designer preview).
- `Visible = false` explicitly set on every added control.
- Field declarations added to the private fields section alongside existing control fields, never replacing or renaming an existing Master field.

- [ ] **Step 3: Add `ApplyMainFormMode()` to `MainForm.cs`**

```csharp
private void ApplyMainFormMode()
{
    bool krabi = Globals.IsKrabiSTPVersion;

    groupBox5.Visible = krabi;
    rbShortLine.Visible = krabi;
    rbLongLine.Visible = krabi;
    tbWeightOrigin.Visible = krabi;
    tbQOrigin.Visible = krabi;
    btSettingLine.Visible = krabi;
    lbShortTime.Visible = krabi;
    lbShortWeightTotal.Visible = krabi;
    lbLongTime.Visible = krabi;
    lbLongWeightTotal.Visible = krabi;
}
```

- [ ] **Step 4: Call it from `MainForm_Load`**

Add `ApplyMainFormMode();` at the end of the existing `MainForm_Load` handler, after other initialization.

- [ ] **Step 5: Build and manually verify**

With Krabi mode off, launch the app and confirm none of the 10 controls are visible anywhere on `MainForm`, and no existing Master control has shifted position (compare a screenshot against the pre-change build). With Krabi mode on, confirm all 10 controls appear, positioned without overlapping existing controls.

- [ ] **Step 6: Commit**

```bash
git add SerialPortListener/MainForm.Designer.cs SerialPortListener/MainForm.cs
git commit -m "feat: add hidden-by-default Krabi STP mode controls to MainForm"
```

---

### Task 10: `FSettingLine` admin dialog

**Files:**
- Create: `SerialPortListener/FSettingLine.cs`
- Create: `SerialPortListener/FSettingLine.Designer.cs`
- Modify: `SerialPortListener/MainForm.cs` (`btSettingLine_Click`)

**Interfaces:**
- Consumes: `dl.sqlConn()`.
- Produces: nothing consumed by later tasks — this is a leaf dialog.

- [ ] **Step 1: Port the dialog from Krabi**

Run `git show KRABI_STP_2026:SerialPortListener/FSettingLine.cs` and `git show KRABI_STP_2026:SerialPortListener/FSettingLine.Designer.cs` (or the equivalent file names in that branch if named differently — check with `git show KRABI_STP_2026 --stat | grep -i settingline`). Copy both files into `SerialPortListener/` largely as-is, per the spec ("self-contained admin dialog with no Master equivalent to reconcile"). Add both to `SerialPortListener.csproj`.

- [ ] **Step 2: Update the dialog's save logic to use `LineTypeTotals`' cutoff model**

Wherever `FSettingLine` saves `base_setting_line_date_from`/`_time_from`, ensure it writes to the `base_setting_line` table created in Task 8 (not a config file), keeping both values in the same row/transaction so Task 5's `IsWithinCutoff` never sees one set without the other from a normal save (a partially-written row is still possible on a crash mid-write, which is exactly why Task 5 treats a null half as "not configured" rather than trusting both are always present).

- [ ] **Step 3: Wire `btSettingLine_Click` in `MainForm.cs`**

```csharp
private void btSettingLine_Click(object sender, EventArgs e)
{
    using (var dlg = new FSettingLine())
    {
        dlg.ShowDialog(this);
    }
}
```

(`btSettingLine` is only visible in Krabi mode per Task 9's `ApplyMainFormMode()`, so no additional `Globals.IsKrabiSTPVersion` guard is needed inside the handler.)

- [ ] **Step 4: Build and manually verify**

With Krabi mode on, click `btSettingLine`, set a cutoff date/time, save, close, reopen the dialog and confirm the saved values are reloaded correctly. Confirm `CalTimeAndWeightTotalByLineType` (Task 5) picks up the new cutoff on the next total calculation.

- [ ] **Step 5: Commit**

```bash
git add SerialPortListener/FSettingLine.cs SerialPortListener/FSettingLine.Designer.cs SerialPortListener/MainForm.cs SerialPortListener/SerialPortListener.csproj
git commit -m "feat: add FSettingLine admin dialog for Krabi STP line cutoff config"
```

---

### Task 11: `ucSetting` checkbox

**Files:**
- Modify: `SerialPortListener/ucSetting.cs`
- Modify: `SerialPortListener/ucSetting.Designer.cs`

**Interfaces:**
- Consumes: `Globals.isPermissionAddSetting()` (existing), `KrabiStpMode.IsEnabled`/`KrabiStpMode.Save` (Task 2).
- Produces: nothing consumed by later tasks — this is the UI leaf for the feature.

- [ ] **Step 1: Add the checkbox to the Designer**

In `ucSetting.Designer.cs`, add a new `CheckBox chkKrabiStpMode` with `Text = "Krabi STP Mode (Port Version)"`, placed near the other admin-only settings controls (e.g. alongside the backup-config section).

- [ ] **Step 2: Load the current value and apply the permission gate**

In `ucSetting.cs`, in the same initialization method that currently calls `LoadReportTemplateSetting()`:

```csharp
chkKrabiStpMode.Checked = KrabiStpMode.IsEnabled;
chkKrabiStpMode.Enabled = Globals.isPermissionAddSetting();
```

- [ ] **Step 3: Save on the existing save button, with the restart notice**

Find the existing save handler (the same one that calls `SaveReportTemplateSetting()`) and add:

```csharp
if (Globals.isPermissionAddSetting())
{
    bool saved = KrabiStpMode.Save(chkKrabiStpMode.Checked);
    if (saved)
    {
        MessageBox.Show(
            "เปลี่ยนโหมดแล้ว กรุณาปิดโปรแกรมและเปิดใหม่เพื่อให้มีผลกับหน้าจอหลัก",
            "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

- [ ] **Step 4: Build and manually verify**

Log in as a user without `isPermissionAddSetting()` — confirm `chkKrabiStpMode` is visible but disabled (cannot be toggled), matching the Review Focus requirement. Log in as an authorized user, toggle the checkbox, save, confirm the restart message appears, restart the app, confirm `MainForm` now reflects the new mode (Task 9's `ApplyMainFormMode()`), and confirm `config_krabistp.txt` in `Utils.AppDataDir` contains the expected value.

- [ ] **Step 5: Commit**

```bash
git add SerialPortListener/ucSetting.cs SerialPortListener/ucSetting.Designer.cs
git commit -m "feat: add Krabi STP Mode checkbox to settings screen"
```

---

### Task 12: Full regression pass and branch review

**Files:** none (verification only)

- [ ] **Step 1: Run the full automated test suite**

Run: `vstest.console.exe SerialPortListener.Tests\bin\Debug\SerialPortListener.Tests.dll`
Expected: all tests from Tasks 2, 4, 5 pass (19 tests total).

- [ ] **Step 2: Build both configurations**

Run: `msbuild SerialPortListener.sln /p:Configuration=Debug` then `msbuild SerialPortListener.sln /p:Configuration=Release`
Expected: 0 errors in both. Review warnings; do not introduce new ones beyond what Task 1-11's new files unavoidably need.

- [ ] **Step 3: Standard-mode regression check**

With `config_krabistp.txt` absent (or `IsKrabiSTPVersion=False`), exercise: Delivery Order flow, app self-update check, `chkDirectPrint` toggle, serial port Read-button stability gating, port watchdog reconnect, normal weight-in/weight-out save, cancel-password prompt on cancel. Confirm every one behaves identically to a pre-change build of `Master_Blue_1`.

- [ ] **Step 4: Krabi-mode full walkthrough**

With `IsKrabiSTPVersion=True`: short line weight-in auto-fetch, long line reset-to-zero, segmented totals increment correctly, origin weight/qty save to the DB, `FSettingLine` dialog open/save/reload, default scale-user shows the first `users` row, cancel-password prompt still appears (unchanged from Master).

- [ ] **Step 5: Request final review**

Use the superpowers:requesting-code-review skill for a whole-branch review of `feature/krabi-stp-mode` against `Master_Blue_1`, confirming no unrelated file was touched and no existing Master control/behavior changed when the flag is off.

- [ ] **Step 6: Commit any fixes found in review, then stop — do not merge without explicit user instruction.**
