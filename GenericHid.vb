Option Strict On
Option Explicit On

'''<summary>
''' Runs the application.
'''</summary> 
''' 
Public Class GenericHid

    Friend Shared FrmMy As FrmMain

    '''<summary>
    ''' Displays the application's main form.
    '''</summary> 

    Public Shared Sub Main()
        FrmMy = New FrmMain()
        Application.Run(FrmMy)
    End Sub

End Class



