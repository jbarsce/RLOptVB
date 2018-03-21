
Public Class ZValue

    Public ReadOnly Property state As State
    Public ReadOnly Property action As Action
    Public Property zValue As Double

    Public Sub New(state As State, action As Action)
        Me.state = state
        Me.action = action
        zValue = 0
    End Sub

End Class
