Option Strict On
Option Explicit On

Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices

''' <summary>
''' API declarations relating to file I/O.
''' </summary>

Friend NotInheritable Class FileIO

    Public Const FILE_FLAG_OVERLAPPED As Int32 = &H40000000
    Public Const FILE_SHARE_READ As Int16 = &H1S
    Public Const FILE_SHARE_WRITE As Int16 = &H2S
    Public Const GENERIC_READ As Int32 = &H80000000
    Public Const GENERIC_WRITE As Int32 = &H40000000
    Public Const INVALID_HANDLE_VALUE As Int32 = -1
    Public Const OPEN_EXISTING As Int16 = 3
    Public Const WAIT_TIMEOUT As Int32 = &H102
    Public Const WAIT_OBJECT_0 As Int16 = 0

    <StructLayout(LayoutKind.Sequential)> _
    Public Structure OVERLAPPED
        Dim Internal As Int32
        Dim InternalHigh As Int32
        Dim Offset As Int32
        Dim OffsetHigh As Int32
        Dim hEvent As SafeWaitHandle
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Public Class SECURITY_ATTRIBUTES
        Dim nLength As Int32
        Dim lpSecurityDescriptor As Int32
        Dim bInheritHandle As Int32
    End Class

    <DllImport("kernel32.dll", SetLastError:=True)> _
    Shared Function CancelIo _
        (ByVal hFile As SafeFileHandle) _
            As Int32
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Shared Function CreateEvent _
        (ByVal SecurityAttributes As SECURITY_ATTRIBUTES, _
        ByVal bManualReset As Int32, _
        ByVal bInitialState As Int32, _
        ByVal lpName As String) _
        As SafeWaitHandle
    End Function

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Shared Function CreateFile _
        (ByVal lpFileName As String, _
        ByVal dwDesiredAccess As Int32, _
        ByVal dwShareMode As Int32, _
        ByVal lpSecurityAttributes As SECURITY_ATTRIBUTES, _
        ByVal dwCreationDisposition As Int32, _
        ByVal dwFlagsAndAttributes As Int32, _
        ByVal hTemplateFile As Int32) _
        As SafeFileHandle
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)> _
    Shared Function ReadFile _
        (ByVal hFile As SafeFileHandle, _
        ByRef lpBuffer As Byte, _
        ByVal nNumberOfBytesToRead As Int32, _
        ByRef lpNumberOfBytesRead As Int32, _
        ByRef lpOverlapped As OVERLAPPED) _
        As Int32
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)> _
    Shared Function WaitForSingleObject _
        (ByVal hHandle As SafeWaitHandle, _
        ByVal dwMilliseconds As Int32) _
        As Int32
    End Function

    <DllImport("kernel32.dll", SetLastError:=True)> _
    Shared Function WriteFile _
        (ByVal hFile As SafeFileHandle, _
        ByRef lpBuffer As Byte, _
        ByVal nNumberOfBytesToWrite As Int32, _
        ByRef lpNumberOfBytesWritten As Int32, _
        ByVal lpOverlapped As Int32) _
        As Boolean
    End Function

End Class

