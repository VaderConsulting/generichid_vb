Option Strict On
Option Explicit On 
Imports System.Runtime.InteropServices

Partial Friend NotInheritable Class DeviceManagement

    'from dbt.h

    Public Const DBT_DEVICEARRIVAL As Int32 = &H8000
    Public Const DBT_DEVICEREMOVECOMPLETE As Int32 = &H8004
    Public Const DBT_DEVTYP_DEVICEINTERFACE As Int32 = 5
    Public Const DBT_DEVTYP_HANDLE As Int32 = 6
    Public Const DEVICE_NOTIFY_ALL_INTERFACE_CLASSES As Int32 = 4
    Public Const DEVICE_NOTIFY_SERVICE_HANDLE As Int32 = 1
    Public Const DEVICE_NOTIFY_WINDOW_HANDLE As Int32 = 0
    Public Const WM_DEVICECHANGE As Int32 = &H219

    'from setupapi.h

    Public Const DIGCF_PRESENT As Int16 = &H2S
    Public Const DIGCF_DEVICEINTERFACE As Int16 = &H10S

    'There are two declarations for the DEV_BROADCAST_DEVICEINTERFACE class.

    'Use this in the call to RegisterDeviceNotification() and
    'in checking dbch_devicetype in a DEV_BROADCAST_HDR structure.

    <StructLayout(LayoutKind.Sequential)> _
    Public Class DEV_BROADCAST_DEVICEINTERFACE
        Public dbcc_size As Int32
        Public dbcc_devicetype As Int32
        Public dbcc_reserved As Int32
        Public dbcc_classguid As Guid
        Public dbcc_name As Int16
    End Class

    'Use this to read the dbcc_name string and classguid.

    <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Unicode)> _
       Public Class DEV_BROADCAST_DEVICEINTERFACE_1
        Public dbcc_size As Int32
        Public dbcc_devicetype As Int32
        Public dbcc_reserved As Int32
        <MarshalAs(UnmanagedType.ByValArray, ArraySubType:=UnmanagedType.U1, SizeConst:=16)> _
        Public dbcc_classguid() As Byte
        <MarshalAs(UnmanagedType.ByValArray, sizeconst:=255)> _
        Public dbcc_name() As Char
    End Class

    <StructLayout(LayoutKind.Sequential)> _
    Public Class DEV_BROADCAST_HANDLE
        Public dbch_size As Int32
        Public dbch_devicetype As Int32
        Public dbch_reserved As Int32
        Public dbch_handle As Int32
        Public dbch_hdevnotify As Int32
    End Class

    <StructLayout(LayoutKind.Sequential)> _
    Public Class DEV_BROADCAST_HDR
        Public dbch_size As Int32
        Public dbch_devicetype As Int32
        Public dbch_reserved As Int32
    End Class

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure SP_DEVICE_INTERFACE_DATA
        Dim cbSize As Int32
        Dim InterfaceClassGuid As System.Guid
        Dim Flags As Int32
        Dim Reserved As Int32
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure SP_DEVICE_INTERFACE_DETAIL_DATA
        Dim cbSize As Int32
        Dim DevicePath As String
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure SP_DEVINFO_DATA
        Dim cbSize As Int32
        Dim ClassGuid As System.Guid
        Dim DevInst As Int32
        Dim Reserved As Int32
    End Structure

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Shared Function RegisterDeviceNotification _
        (ByVal hRecipient As IntPtr, _
        ByVal NotificationFilter As IntPtr, _
        ByVal Flags As Int32) _
        As IntPtr
    End Function

    <DllImport("setupapi.dll", SetLastError:=True)> _
    Shared Function SetupDiCreateDeviceInfoList _
        (ByRef ClassGuid As System.Guid, _
        ByVal hwndParent As Int32) _
        As Int32
    End Function

    <DllImport("setupapi.dll", SetLastError:=True)> _
    Shared Function SetupDiDestroyDeviceInfoList _
        (ByVal DeviceInfoSet As IntPtr) _
        As Int32
    End Function

    <DllImport("setupapi.dll", SetLastError:=True)> _
    Shared Function SetupDiEnumDeviceInterfaces _
        (ByVal DeviceInfoSet As IntPtr, _
        ByVal DeviceInfoData As Int32, _
        ByRef InterfaceClassGuid As System.Guid, _
        ByVal MemberIndex As Int32, _
        ByRef DeviceInterfaceData As SP_DEVICE_INTERFACE_DATA) _
        As Boolean
    End Function

    <DllImport("setupapi.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Shared Function SetupDiGetClassDevs _
        (ByRef ClassGuid As System.Guid, _
        ByVal Enumerator As String, _
        ByVal hwndParent As Int32, _
        ByVal Flags As Int32) _
        As IntPtr
    End Function

    <DllImport("setupapi.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
    Shared Function SetupDiGetDeviceInterfaceDetail _
        (ByVal DeviceInfoSet As IntPtr, _
        ByRef DeviceInterfaceData As SP_DEVICE_INTERFACE_DATA, _
        ByVal DeviceInterfaceDetailData As IntPtr, _
        ByVal DeviceInterfaceDetailDataSize As Int32, _
        ByRef RequiredSize As Int32, _
        ByVal DeviceInfoData As IntPtr) _
        As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True)> _
    Shared Function UnregisterDeviceNotification _
        (ByVal Handle As IntPtr) _
    As Boolean
    End Function

End Class

