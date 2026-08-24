Option Strict On
Option Explicit On

Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices
Imports GenericHid.FileIO

''' <summary>
''' For communicating with HID-class USB devices.
''' The ReportIn class handles Input reports and Feature reports that carry data to the host.
''' The ReportOut class handles Output reports and Feature reports that that carry data to the device.
''' Other routines retrieve information about and configure the HID.
''' </summary>
''' 
Partial Friend NotInheritable Class Hid

    ' Used in error messages.

    Const MODULE_NAME As String = "Hid"

    Friend Capabilities As HIDP_CAPS
    Friend DeviceAttributes As HIDD_ATTRIBUTES

    ' For viewing results of API calls in debug.write statements:

    Shared MyDebugging As New Debugging()

    ''' <summary>
    ''' For reports the device sends to the host.
    ''' </summary>

    Friend MustInherit Class ReportIn

        ''' <summary>
        ''' Each class that handles reading reports defines a Read method for reading 
        ''' a type of report. Read is declared as a Sub rather
        ''' than as a Function because asynchronous reads use a callback method 
        ''' that can access parameters passed by ByRef but not Function return values.
        ''' </summary>

        Friend MustOverride Sub Read _
            (ByVal hidHandle As SafeFileHandle, _
            ByVal readHandle As SafeFileHandle, _
            ByVal writeHandle As SafeFileHandle, _
            ByRef myDeviceDetected As Boolean, _
            ByRef readBuffer() As Byte, _
            ByRef success As Boolean)

    End Class

    ''' <summary>
    '''  'For reading Feature reports.
    ''' </summary>

    Friend Class InFeatureReport
        Inherits ReportIn

        ''' <summary>
        ''' reads a Feature report from the device.
        ''' </summary>
        ''' 
        ''' <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
        ''' <param name="readHandle"> the handle for reading Input reports from the device. </param>
        ''' <param name="writeHandle"> the handle for writing Output reports to the device. </param>
        ''' <param name="myDeviceDetected"> tells whether the device is currently attached.</param>
        ''' <param name="inFeatureReportBuffer"> contains the requested report.</param>
        ''' <param name="success"> read success</param>
        Friend Overrides Sub Read _
            (ByVal hidHandle As SafeFileHandle, _
            ByVal readHandle As SafeFileHandle, _
            ByVal writeHandle As SafeFileHandle, _
            ByRef myDeviceDetected As Boolean, _
            ByRef inFeatureReportBuffer() As Byte, _
            ByRef success As Boolean)

            Try
                ' ***
                ' API function: HidD_GetFeature
                ' Attempts to read a Feature report from the device.

                ' Requires:
                ' A handle to a HID
                ' A pointer to a buffer containing the report ID and report
                ' The size of the buffer. 

                ' Returns: true on success, false on failure.
                ' ***

                success = HidD_GetFeature _
                   (hidHandle, _
                   inFeatureReportBuffer(0), _
                   inFeatureReportBuffer.Length)

                Debug.Print("HidD_GetFeature success = " & success)

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Sub
    End Class

    ''' <summary>
    ''' For reading Input reports via control transfers
    ''' </summary>

    Friend Class InputReportViaControlTransfer
        Inherits ReportIn

        ''' <summary>
        ''' reads an Input report from the device using a control transfer.
        ''' </summary>
        ''' 
        ''' 
        ''' <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
        ''' <param name="readHandle"> the handle for reading Input reports from the device. </param>
        ''' <param name="writeHandle"> the handle for writing Output reports to the device. </param>
        ''' <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
        ''' <param name="inputReportBuffer"> contains the requested report. </param>
        ''' <param name="success"> read success </param>
        ''' 
        Friend Overrides Sub Read _
            (ByVal hidHandle As SafeFileHandle, _
            ByVal readHandle As SafeFileHandle, _
            ByVal writeHandle As SafeFileHandle, _
            ByRef myDeviceDetected As Boolean, _
            ByRef inputReportBuffer() As Byte, _
            ByRef success As Boolean)

            Try

                ' ***
                ' API function: HidD_GetInputReport

                ' Purpose: Attempts to read an Input report from the device using a control transfer.
                ' Supported under Windows XP and later only.

                ' Requires:
                ' A handle to a HID
                ' A pointer to a buffer containing the report ID and report
                ' The size of the buffer. 

                ' Returns: true on success, false on failure.
                ' ***

                success = HidD_GetInputReport _
                   (hidHandle, _
                   inputReportBuffer(0), _
                   inputReportBuffer.Length)

                Debug.Print("HidD_GetInputReport success = " & success)

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Sub

    End Class

    ''' <summary>
    ''' For reading Input reports.
    ''' </summary>

    Friend Class InputReportViaInterruptTransfer
        Inherits ReportIn

        Dim readyForOverlappedTransfer As Boolean ' initialize to false

        ''' <summary>
        ''' closes open handles to a device.
        ''' </summary>
        ''' 
        ''' <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
        ''' <param name="readHandle"> the handle for reading Input reports from the device. </param>
        ''' <param name="writeHandle"> the handle for writing Output reports to the device. </param>
        Friend Sub CancelTransfer _
            (ByVal hidHandle As SafeFileHandle, _
            ByVal readHandle As SafeFileHandle, _
            ByVal writeHandle As SafeFileHandle)

            Try
                ' ***
                ' API function: CancelIo

                ' Purpose: Cancels a call to ReadFile

                ' Accepts: the device handle.

                ' Returns: True on success, False on failure.
                ' ***

                CancelIo(readHandle)

                Debug.WriteLine("************ReadFile error*************")
                Debug.WriteLine(MyDebugging.ResultOfAPICall("CancelIo"))
                Debug.WriteLine("")

                ' The failure may have been because the device was removed,
                ' so close any open handles and
                ' set myDeviceDetected=False to cause the application to
                ' look for the device on the next attempt.

                If (Not hidHandle.IsInvalid) Then
                    hidHandle.Close()
                End If

                If (Not readHandle.IsInvalid) Then
                    readHandle.Close()
                End If

                If (Not writeHandle.IsInvalid) Then
                    writeHandle.Close()
                End If

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Sub

        ''' <summary>
        ''' Creates an event object for the overlapped structure used with 
        ''' ReadFile. Called before the first call to ReadFile.
        ''' </summary>
        ''' 
        ''' <param name="hidOverlapped"> the overlapped structure </param>
        ''' <param name="eventObject"> the event object </param>
        Friend Sub PrepareForOverlappedTransfer _
            (ByRef hidOverlapped As OVERLAPPED, _
            ByRef eventObject As SafeWaitHandle)

            Try
                ' ***
                ' API function: CreateEvent

                ' Purpose: Creates an event object for the overlapped structure used with ReadFile.

                ' Accepts:
                ' A security attributes structure.
                ' Manual Reset = False (The system automatically resets the state to nonsignaled 
                ' after a waiting thread has been released.)
                ' Initial state = True (signaled)
                ' An event object name (optional)

                ' Returns: a handle to the event object
                ' ***

                eventObject = CreateEvent(Nothing, Convert.ToInt32(False), Convert.ToInt32(True), "")

                ' Debug.WriteLine(MyDebugging.ResultOfAPICall("CreateEvent"))

                ' Set the members of the overlapped structure.

                hidOverlapped.Offset = 0
                hidOverlapped.OffsetHigh = 0
                hidOverlapped.hEvent = eventObject
                readyForOverlappedTransfer = True

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Sub

        ''' <summary>
        ''' reads an Input report from the device using interrupt transfers.
        ''' </summary>
        ''' 
        ''' <param name="hidHandle"> the handle for learning about the device and exchanging Feature reports. </param>
        ''' <param name="readHandle"> the handle for reading Input reports from the device. </param>
        ''' <param name="writeHandle"> the handle for writing Output reports to the device. </param>
        ''' <param name="myDeviceDetected"> tells whether the device is currently attached. </param>
        ''' <param name="inputReportBuffer"> contains the requested report. </param>
        ''' <param name="success"> read success </param>
        Friend Overrides Sub Read _
            (ByVal hidHandle As SafeFileHandle, _
              ByVal readHandle As SafeFileHandle, _
              ByVal writeHandle As SafeFileHandle, _
              ByRef myDeviceDetected As Boolean, _
              ByRef inputReportBuffer() As Byte, _
              ByRef success As Boolean)

            Dim eventObject As SafeWaitHandle = Nothing
            Dim HidOverlapped As OVERLAPPED = Nothing
            Dim numberOfBytesRead As Int32
            Dim result As Int32

            Try
                ' If it's the first attempt to read, set up the overlapped structure for ReadFile.

                If readyForOverlappedTransfer = False Then
                    PrepareForOverlappedTransfer(HidOverlapped, eventObject)
                End If

                ' ***
                ' API function: ReadFile
                ' Purpose: Attempts to read an Input report from the device.

                ' Accepts:
                ' A device handle returned by CreateFile
                ' (for overlapped I/O, CreateFile must have been called with FILE_FLAG_OVERLAPPED),
                ' A pointer to a buffer for storing the report.
                ' The Input report length in bytes returned by HidP_GetCaps,
                ' A pointer to a variable that will hold the number of bytes read. 
                ' An overlapped structure whose hEvent member is set to an event object.

                ' Returns: the report in ReadBuffer.

                ' The overlapped call returns immediately, even if the data hasn't been received yet.

                ' To read multiple reports with one ReadFile, increase the size of ReadBuffer
                ' and use NumberOfBytesRead to determine how many reports were returned.
                ' Use a larger buffer if the application can't keep up with reading each report
                ' individually. 
                ' ***

                result = ReadFile _
                    (readHandle, _
                    inputReportBuffer(0), _
                    inputReportBuffer.Length, _
                    numberOfBytesRead, _
                    HidOverlapped)

                Debug.WriteLine("waiting for ReadFile")

                ' API function: WaitForSingleObject

                ' Purpose: waits for at least one report or a timeout.
                ' Used with overlapped ReadFile.

                ' Accepts:
                ' An event object created with CreateEvent
                ' A timeout value in milliseconds.

                ' Returns: A result code.

                result = WaitForSingleObject(eventObject, 3000)

                ' Find out if ReadFile completed or timeout.

                Select Case result
                    Case WAIT_OBJECT_0

                        ' ReadFile has completed

                        success = True
                        Debug.WriteLine("ReadFile completed successfully.")
                    Case WAIT_TIMEOUT

                        ' Cancel the operation on timeout

                        CancelTransfer(hidHandle, readHandle, writeHandle)
                        Debug.WriteLine("Readfile timeout")
                        success = False
                        myDeviceDetected = False
                    Case Else

                        ' Cancel the operation on other error.

                        CancelTransfer(hidHandle, readHandle, writeHandle)
                        Debug.WriteLine("Readfile undefined error")
                        success = False
                        myDeviceDetected = False
                End Select

                If result = 0 Then
                    success = True
                Else
                    success = False
                End If

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Sub
    End Class

    ''' <summary>
    ''' For reports the host sends to the device.
    ''' </summary>

    Friend MustInherit Class ReportOut

        ''' <summary>
        ''' Each class that handles writing reports defines a Write method for 
        ''' writing a type of report.
        ''' </summary>
        ''' 
        ''' <param name="reportBuffer"> contains the report ID and report data. </param>
        '''  <param name="deviceHandle"> handle to the device.  </param>
        ''' 
        ''' <returns>
        '''  True on success. False on failure.
        ''' </returns>
        ''' 
        Friend MustOverride Function Write _
            (ByVal reportBuffer() As Byte, _
            ByVal deviceHandle As SafeFileHandle) _
            As Boolean

    End Class

    ''' <summary>
    ''' For Feature reports the host sends to the device.
    ''' </summary>
    Class OutFeatureReport
        Inherits ReportOut

        ''' <summary>
        ''' writes a Feature report to the device.
        ''' </summary>
        ''' 
        ''' <param name="outFeatureReportBuffer"> contains the report ID and report data. </param>
        ''' <param name="hidHandle"> handle to the device.  </param>
        ''' 
        ''' <returns>
        '''  True on success. False on failure.
        ''' </returns>
        Friend Overrides Function Write _
            (ByVal outFeatureReportBuffer() As Byte, _
            ByVal hidHandle As SafeFileHandle) _
            As Boolean

            Dim success As Boolean

            Try
                ' ***
                ' API function: HidD_SetFeature

                ' Purpose: Attempts to send a Feature report to the device.

                ' Accepts:
                ' A handle to a HID
                ' A pointer to a buffer containing the report ID and report
                ' The size of the buffer. 

                ' Returns: true on success, false on failure.
                ' ***

                success = HidD_SetFeature _
                    (hidHandle, _
                    outFeatureReportBuffer(0), _
                    outFeatureReportBuffer.Length)

                Debug.Print("HidD_SetFeature success = " & success)

                Return success

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Function

    End Class

    ''' <summary>
    ''' For writing Output reports via control transfers
    ''' </summary>

    Class OutputReportViaControlTransfer
        Inherits ReportOut

        ''' <summary>
        ''' writes an Output report to the device using a control transfer.
        ''' </summary>
        ''' 
        ''' <param name="outputReportBuffer"> contains the report ID and report data. </param>
        ''' <param name="hidHandle"> handle to the device.  </param>
        ''' 
        ''' <returns>
        '''  True on success. False on failure.
        ''' </returns>
        Friend Overrides Function Write _
                (ByVal outputReportBuffer() As Byte, _
                ByVal hidHandle As SafeFileHandle) _
                As Boolean

            Dim success As Boolean

            Try
                ' ***
                ' API function: HidD_SetOutputReport

                ' Purpose: 
                ' Attempts to send an Output report to the device using a control transfer.
                ' Requires Windows XP or later.

                ' Accepts:
                ' A handle to a HID
                ' A pointer to a buffer containing the report ID and report
                ' The size of the buffer. 

                ' Returns: true on success, false on failure.
                ' ***

                success = HidD_SetOutputReport _
                    (hidHandle, _
                    outputReportBuffer(0), _
                    outputReportBuffer.Length)

                Debug.Print("HidD_SetOutputReport success = " & success)

                Return success

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Function

    End Class

    ''' <summary>
    ''' For Output reports the host sends to the device.
    ''' Uses interrupt or control transfers depending on the device and OS.
    ''' </summary>

    Class OutputReportViaInterruptTransfer
        Inherits ReportOut

        ''' <summary>
        ''' writes an Output report to the device.
        ''' </summary>
        ''' 
        ''' <param name="outputReportBuffer"> contains the report ID and report data. </param>
        ''' <param name="writeHandle"> handle to the device.  </param>
        ''' 
        ''' <returns>
        '''  True on success. False on failure.
        ''' </returns>
        Friend Overrides Function Write _
            (ByVal outputReportBuffer() As Byte, _
            ByVal writeHandle As SafeFileHandle) _
            As Boolean

            Dim numberOfBytesWritten As Int32
            Dim success As Boolean

            Try
                ' The host will use an interrupt transfer if the the HID has an interrupt OUT
                ' endpoint (requires USB 1.1 or later) AND the OS is NOT Windows 98 Gold (original version). 
                ' Otherwise the the host will use a control transfer.
                ' The application doesn't have to know or care which type of transfer is used.

                numberOfBytesWritten = 0

                ' ***
                ' API function: WriteFile

                ' Purpose: writes an Output report to the device.

                ' Accepts:
                ' A handle returned by CreateFile
                ' An integer to hold the number of bytes written.

                ' Returns: True on success, False on failure.
                ' ***

                success = WriteFile _
                        (writeHandle, _
                        outputReportBuffer(0), _
                        outputReportBuffer.Length, _
                        numberOfBytesWritten, _
                        0)

                Debug.Print("WriteFile success = " & success)

                If Not (success) Then

                    If (Not writeHandle.IsInvalid) Then
                        writeHandle.Close()
                    End If
                End If

                Return success

            Catch ex As Exception
                DisplayException(MODULE_NAME, ex)
                Throw
            End Try

        End Function

    End Class

    ''' <summary>
    ''' Remove any Input reports waiting in the buffer.
    ''' </summary>
    ''' 
    ''' <param name="hidHandle"> a handle to a device.   </param>
    ''' 
    ''' <returns>
    ''' True on success, False on failure.
    ''' </returns>
    Friend Function FlushQueue _
        (ByVal hidHandle As SafeFileHandle) _
        As Boolean

        Dim success As Boolean

        Try
            ' ***
            ' API function: HidD_FlushQueue

            ' Purpose: Removes any Input reports waiting in the buffer.

            ' Accepts: a handle to the device.

            ' Returns: True on success, False on failure.
            ' ***

            success = HidD_FlushQueue(hidHandle)

            Return success

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

    End Function

    ''' <summary>
    ''' Retrieves a structure with information about a device's capabilities. 
    ''' </summary>
    ''' 
    ''' <param name="hidHandle"> a handle to a device. </param>
    ''' 
    ''' <returns>
    ''' An HIDP_CAPS structure.
    ''' </returns>
    Friend Function GetDeviceCapabilities _
        (ByVal hidHandle As SafeFileHandle) _
        As HIDP_CAPS

        Dim preparsedDataBytes(29) As Byte
        Dim preparsedDataString As String
        Dim preparsedDataPointer As IntPtr
        Dim result As Int32
        Dim success As Boolean
        Dim valueCaps(1023) As Byte '(the array size is a guess)

        Try

            ' ***
            ' API function: HidD_GetPreparsedData

            ' Purpose: retrieves a pointer to a buffer containing information about the device's capabilities.
            ' HidP_GetCaps and other API functions require a pointer to the buffer.

            ' Requires: 
            ' A handle returned by CreateFile.
            ' A pointer to a buffer.

            ' Returns:
            ' True on success, False on failure.
            ' ***

            success = HidD_GetPreparsedData(hidHandle, preparsedDataPointer)

            ' Copy the data at PreparsedDataPointer into a byte array.

            preparsedDataString = System.Convert.ToBase64String(preparsedDataBytes)

            ' ***
            ' API function: HidP_GetCaps

            ' Purpose: find out a device's capabilities.
            ' For standard devices such as joysticks, you can find out the specific
            ' capabilities of the device.
            ' For a custom device where the software knows what the device is capable of,
            ' this call may be unneeded.

            ' Accepts:
            ' A pointer returned by HidD_GetPreparsedData
            ' A pointer to a HIDP_CAPS structure.

            ' Returns: True on success, False on failure.
            ' ***

            result = HidP_GetCaps(preparsedDataPointer, Capabilities)
            If (result <> 0) Then

                Debug.WriteLine("")

                Debug.WriteLine("  Usage: " & Hex(Capabilities.Usage))
                Debug.WriteLine("  Usage Page: " & Hex(Capabilities.UsagePage))
                Debug.WriteLine("  Input Report Byte Length: " & Capabilities.InputReportByteLength)
                Debug.WriteLine("  Output Report Byte Length: " & Capabilities.OutputReportByteLength)
                Debug.WriteLine("  Feature Report Byte Length: " & Capabilities.FeatureReportByteLength)
                Debug.WriteLine("  Number of Link Collection Nodes: " & Capabilities.NumberLinkCollectionNodes)
                Debug.WriteLine("  Number of Input Button Caps: " & Capabilities.NumberInputButtonCaps)
                Debug.WriteLine("  Number of Input Value Caps: " & Capabilities.NumberInputValueCaps)
                Debug.WriteLine("  Number of Input Data Indices: " & Capabilities.NumberInputDataIndices)
                Debug.WriteLine("  Number of Output Button Caps: " & Capabilities.NumberOutputButtonCaps)
                Debug.WriteLine("  Number of Output Value Caps: " & Capabilities.NumberOutputValueCaps)
                Debug.WriteLine("  Number of Output Data Indices: " & Capabilities.NumberOutputDataIndices)
                Debug.WriteLine("  Number of Feature Button Caps: " & Capabilities.NumberFeatureButtonCaps)
                Debug.WriteLine("  Number of Feature Value Caps: " & Capabilities.NumberFeatureValueCaps)
                Debug.WriteLine("  Number of Feature Data Indices: " & Capabilities.NumberFeatureDataIndices)

                ' ***
                ' API function: HidP_GetValueCaps

                ' Purpose: retrieves a buffer containing an array of HidP_ValueCaps structures.
                ' Each structure defines the capabilities of one value.
                ' This application doesn't use this data.

                ' Accepts:
                ' A report type enumerator from hidpi.h,
                ' A pointer to a buffer for the returned array,
                ' The NumberInputValueCaps member of the device's HidP_Caps structure,
                ' A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.

                ' Returns: True on success, False on failure.
                ' ***

                result = HidP_GetValueCaps _
                    (HidP_Input, _
                    valueCaps(0), _
                    Capabilities.NumberInputValueCaps, _
                    preparsedDataPointer)

                '(To use this data, copy the ValueCaps byte array into an array of structures.)

                ' ***
                ' API function: HidD_FreePreparsedData

                ' Purpose: frees the buffer reserved by HidD_GetPreparsedData.

                ' Accepts: A pointer to the PreparsedData structure returned by HidD_GetPreparsedData.

                ' Returns: True on success, False on failure.
                ' ***

                success = HidD_FreePreparsedData(preparsedDataPointer)

            End If

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

        Return Capabilities

    End Function

    ''' <summary>
    ''' Creates a 32-bit Usage from the Usage Page and Usage ID. 
    ''' Determines whether the Usage is a system mouse or keyboard.
    ''' Can be modified to detect other Usages.
    ''' </summary>
    ''' 
    ''' <param name="MyCapabilities"> a HIDP_CAPS structure retrieved with HidP_GetCaps. </param>
    ''' 
    ''' <returns>
    ''' A string describing the Usage.
    ''' </returns>
    Friend Function GetHidUsage _
        (ByVal MyCapabilities As HIDP_CAPS) _
        As String

        Dim usage As Int32
        Dim usageDescription As String = ""

        Try
            ' Create32-bit Usage from Usage Page and Usage ID.

            usage = MyCapabilities.UsagePage * 256 + MyCapabilities.Usage

            If usage = Convert.ToInt32(&H102) Then usageDescription = "mouse"
            If usage = Convert.ToInt32(&H106) Then usageDescription = "keyboard"

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

        Return usageDescription

    End Function

    ''' <summary>
    ''' Retrieves the number of Input reports the host can store.
    ''' </summary>
    ''' 
    ''' <param name="hidDeviceObject"> a handle to a device  </param>
    ''' <param name="numberOfInputBuffers"> an integer to hold the returned value. </param>
    ''' 
    ''' <returns>
    ''' True on success, False on failure.
    ''' </returns>
    Friend Function GetNumberOfInputBuffers _
      (ByVal hidDeviceObject As SafeFileHandle, _
      ByRef numberOfInputBuffers As Int32) _
      As Boolean

        Dim success As Boolean

        Try

            If Not (IsWindows98Gold()) Then
                ' ***
                ' API function: HidD_GetNumInputBuffers

                ' Purpose: retrieves the number of Input reports the host can store.
                ' Not supported by Windows 98 Gold.
                ' If the buffer is full and another report arrives, the host drops the 
                ' ldest report.

                ' Accepts: a handle to a device and an integer to hold the number of buffers. 

                ' Returns: True on success, False on failure.
                ' ***

                success = HidD_GetNumInputBuffers _
                     (hidDeviceObject, _
                     numberOfInputBuffers)

            Else

                ' Under Windows 98 Gold, the number of buffers is fixed at 2.

                numberOfInputBuffers = 2
                success = True
            End If

            Return success

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

    End Function

    ''' <summary>
    ''' sets the number of input reports the host will store.
    ''' Requires Windows XP or later.
    ''' </summary>
    ''' 
    ''' <param name="hidDeviceObject"> a handle to the device.</param>
    ''' <param name="numberBuffers"> the requested number of input reports.  </param>
    ''' 
    ''' <returns>
    ''' True on success. False on failure.
    ''' </returns>
    Friend Function SetNumberOfInputBuffers _
         (ByVal hidDeviceObject As SafeFileHandle, _
         ByVal numberBuffers As Int32) _
         As Boolean

        Dim success As Boolean

        Try
            If Not (IsWindows98Gold()) Then

                ' ***
                ' API function: HidD_SetNumInputBuffers

                ' Purpose: Sets the number of Input reports the host can store.
                ' If the buffer is full and another report arrives, the host drops the 
                ' oldest report.

                ' Requires:
                ' A handle to a HID
                ' An integer to hold the number of buffers. 

                ' Returns: true on success, false on failure.
                ' ***

                HidD_SetNumInputBuffers _
                     (hidDeviceObject, _
                     numberBuffers)

                Return success

            Else
                ' Not supported under Windows 98 Gold.

                Return False
            End If

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

    End Function

    ''' <summary>
    ''' Find out if the current operating system is Windows XP or later.
    ''' (Windows XP or later is required for HidD_GetInputReport and HidD_SetInputReport.)
    ''' </summary>
    Friend Function IsWindowsXpOrLater() As Boolean

        Try
            Dim myEnvironment As OperatingSystem = Environment.OSVersion

            ' Windows XP is version 5.1.

            Dim versionXP As New System.Version(5, 1)

            If (Version.op_GreaterThanOrEqual(myEnvironment.Version, versionXP) = True) Then
                Debug.Write("The OS is Windows XP or later.")
                Return True
            Else
                Debug.Write("The OS is earlier than Windows XP.")
                Return False
            End If

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

    End Function

    ''' <summary>
    ''' Find out if the current operating system is Windows 98 Gold (original version).
    ''' Windows 98 Gold does not support the following:
    ''' Interrupt OUT transfers (WriteFile uses control transfers and Set_Report).
    ''' HidD_GetNumInputBuffers and HidD_SetNumInputBuffers
    ''' (Not yet tested on a Windows 98 Gold system.)
    ''' </summary>
    Friend Function IsWindows98Gold() As Boolean

        Try
            Dim myEnvironment As OperatingSystem = Environment.OSVersion

            ' Windows 98 Gold is version 4.10 with a build number less than 2183.

            Dim version98SE As New System.Version(4, 10, 2183)

            If (Version.op_LessThan(myEnvironment.Version, version98SE) = True) Then
                Debug.Write("The OS is Windows 98 Gold.")
                Return True
            Else
                Debug.Write("The OS is more recent than Windows 98 Gold.")
                Return False
            End If

        Catch ex As Exception
            DisplayException(MODULE_NAME, ex)
            Throw
        End Try

    End Function

    ''' <summary>
    ''' Provides a central mechanism for exception handling.
    ''' Displays a message box that describes the exception.
    ''' </summary>
    ''' 
    ''' <param name="moduleName">  the module where the exception occurred. </param>
    ''' <param name="e"> the exception </param>
    Shared Sub DisplayException(ByVal moduleName As String, ByVal e As Exception)

        Dim message As String
        Dim caption As String

        ' Create an error message.

        message = "Exception: " & e.Message & ControlChars.CrLf & _
        "Module: " & moduleName & ControlChars.CrLf & _
         "Method: " & e.TargetSite.Name

        caption = "Unexpected Exception"

        MessageBox.Show(message, caption, MessageBoxButtons.OK)
        Debug.Write(message)

    End Sub

End Class
