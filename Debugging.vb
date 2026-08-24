Option Strict On
Option Explicit On



''' <summary>
''' Used only in Debug.Write statements.
''' </summary>
''' 
Partial Friend NotInheritable Class Debugging

    ''' <summary>
    ''' Get text that describes the result of an API call.
    ''' </summary>
    ''' 
    ''' <param name="FunctionName"> the name of the API function. </param>
    ''' 
    ''' <returns>
    ''' The text.
    ''' </returns>
    ''' 
    Friend Function ResultOfAPICall _
        (ByRef functionName As String) _
        As String

        Dim bytes As Int32
        Dim resultCode As Int32
        Dim resultString As String = ""

        resultString = New String(Chr(0), 129)

        'Returns the result code for the last API call.

        resultCode = System.Runtime.InteropServices.Marshal.GetLastWin32Error

        'Get the result message that corresponds to the code.

        bytes = FormatMessage _
            (FORMAT_MESSAGE_FROM_SYSTEM, _
            0, _
            resultCode, _
            0, _
            resultString, _
            128, _
            0)

        'Subtract two characters from the message to strip the CR and LF.

        If bytes > 2 Then
            resultString = resultString.Remove(bytes - 2, 2)
        End If

        'Create the string to return.

        resultString = vbCrLf & functionName & vbCrLf & _
            "Result = " & resultString & vbCrLf

        Return resultString

    End Function
End Class

