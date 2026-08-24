# generichid_vb

VB.NET WinForms GenericHid sample (Jan Axelson 2.4) that finds an attached USB HID by vendor and product IDs (form defaults 0925/1299), reads HID capabilities, and exchanges Input, Output, and Feature reports. FrmMain uses RegisterDeviceNotification and WM_DEVICE_CHANGE for attach/remove, and reads Input reports asynchronously via a Delegate with BeginInvoke so the UI thread does not block when the HID buffer is empty. Hid.vb wraps hid.dll and file I/O with SafeFileHandle; DeviceManagement.vb locates devices by GUID. The original Lakeview Research license in README/source still applies.

**Source last updated:** 2008-06-22  
**Language:** VB.NET  
**Target:** not recorded  
**Output:** WinExe

## What it is

VB.NET WinForms GenericHid sample (Jan Axelson 2.4) that finds an attached USB HID by vendor and product IDs (form defaults 0925/1299), reads HID capabilities, and exchanges Input, Output, and Feature reports. FrmMain uses RegisterDeviceNotification and WM_DEVICE_CHANGE for attach/remove, and reads Input reports asynchronously via a Delegate with BeginInvoke so the UI thread does not block when the HID buffer is empty. Hid.vb wraps hid.dll and file I/O with SafeFileHandle; DeviceManagement.vb locates devices by GUID. The original Lakeview Research license in README/source still applies.

## Solution structure

| Project | Language | Path |
|---------|----------|------|
| `GenericHid` | VB.NET | `GenericHid.vbproj` |

## How to open

Open `GenericHid.sln` in Visual Studio.

## Attribution and provenance

- **Assembly company:** Lakeview Research
- **Assembly copyright:** c. 1999-2005 by Jan Axelson

## License

Original license terms apply where recorded in the tree or package metadata. This repository does not claim authorship. See `THIRD_PARTY_NOTICES.md`.
