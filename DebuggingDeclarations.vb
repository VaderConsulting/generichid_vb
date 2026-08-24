Option Strict On
Option Explicit On 

Imports System.Runtime.InteropServices


Partial Friend NotInheritable Class Debugging

    Public Const FORMAT_MESSAGE_FROM_SYSTEM As Int16 = &H1000S

    <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)> _
    Shared Function FormatMessage _
            (ByVal dwFlags As Int32, _
            ByRef lpSource As Int64, _
            ByVal dwMessageId As Int32, _
            ByVal dwLanguageZId As Int32, _
            ByVal lpBuffer As String, _
            ByVal nSize As Int32, _
            ByVal Arguments As Int32) _
            As Int32
    End Function

End Class

