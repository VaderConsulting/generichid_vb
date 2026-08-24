Option Strict On
Option Explicit On

Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices


Partial Friend NotInheritable Class Hid

    ' API declarations for HID communications.

    ' from hidpi.h
    ' Typedef enum defines a set of integer constants for HidP_Report_Type

    Public Const HidP_Input As Int16 = 0
    Public Const HidP_Output As Int16 = 1
    Public Const HidP_Feature As Int16 = 2

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure HIDD_ATTRIBUTES
        Dim Size As Int32
        Dim VendorID As Int16
        Dim ProductID As Int16
        Dim VersionNumber As Int16
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure HIDP_CAPS
        Dim Usage As Int16
        Dim UsagePage As Int16
        Dim InputReportByteLength As Int16
        Dim OutputReportByteLength As Int16
        Dim FeatureReportByteLength As Int16
        <MarshalAs(UnmanagedType.ByValArray, SizeConst:=17)> Dim Reserved() As Int16
        Dim NumberLinkCollectionNodes As Int16
        Dim NumberInputButtonCaps As Int16
        Dim NumberInputValueCaps As Int16
        Dim NumberInputDataIndices As Int16
        Dim NumberOutputButtonCaps As Int16
        Dim NumberOutputValueCaps As Int16
        Dim NumberOutputDataIndices As Int16
        Dim NumberFeatureButtonCaps As Int16
        Dim NumberFeatureValueCaps As Int16
        Dim NumberFeatureDataIndices As Int16

    End Structure

    ' If IsRange is false, UsageMin is the Usage and UsageMax is unused.
    ' If IsStringRange is false, StringMin is the string index and StringMax is unused.
    ' If IsDesignatorRange is false, DesignatorMin is the designator index and DesignatorMax is unused.

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure HidP_Value_Caps
        Dim UsagePage As Int16
        Dim ReportID As Byte
        Dim IsAlias As Int32
        Dim BitField As Int16
        Dim LinkCollection As Int16
        Dim LinkUsage As Int16
        Dim LinkUsagePage As Int16
        Dim IsRange As Int32
        Dim IsStringRange As Int32
        Dim IsDesignatorRange As Int32
        Dim IsAbsolute As Int32
        Dim HasNull As Int32
        Dim Reserved As Byte
        Dim BitSize As Int16
        Dim ReportCount As Int16
        Dim Reserved2 As Int16
        Dim Reserved3 As Int16
        Dim Reserved4 As Int16
        Dim Reserved5 As Int16
        Dim Reserved6 As Int16
        Dim LogicalMin As Int32
        Dim LogicalMax As Int32
        Dim PhysicalMin As Int32
        Dim PhysicalMax As Int32
        Dim UsageMin As Int16
        Dim UsageMax As Int16
        Dim StringMin As Int16
        Dim StringMax As Int16
        Dim DesignatorMin As Int16
        Dim DesignatorMax As Int16
        Dim DataIndexMin As Int16
        Dim DataIndexMax As Int16
    End Structure

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_FlushQueue _
        (ByVal HidDeviceObject As SafeFileHandle) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_FreePreparsedData _
        (ByRef PreparsedData As IntPtr) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_GetAttributes _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef Attributes As HIDD_ATTRIBUTES) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_GetFeature _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef lpReportBuffer As Byte, _
        ByVal ReportBufferLength As Int32) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_GetInputReport _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef lpReportBuffer As Byte, _
        ByVal ReportBufferLength As Int32) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Sub HidD_GetHidGuid _
        (ByRef HidGuid As System.Guid)
    End Sub

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_GetNumInputBuffers _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef NumberBuffers As Int32) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_GetPreparsedData _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef PreparsedData As IntPtr) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_SetFeature _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef lpReportBuffer As Byte, _
        ByVal ReportBufferLength As Int32) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_SetNumInputBuffers _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByVal NumberBuffers As Int32) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidD_SetOutputReport _
        (ByVal HidDeviceObject As SafeFileHandle, _
        ByRef lpReportBuffer As Byte, _
        ByVal ReportBufferLength As Int32) _
        As Boolean
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidP_GetCaps _
        (ByVal PreparsedData As IntPtr, _
        ByRef Capabilities As HIDP_CAPS) _
        As Int32
    End Function

    <DllImport("hid.dll", SetLastError:=True)> _
    Shared Function HidP_GetValueCaps _
        (ByVal ReportType As Int16, _
        ByRef ValueCaps As Byte, _
        ByRef ValueCapsLength As Int16, _
        ByVal PreparsedData As IntPtr) _
        As Int32
    End Function

End Class
