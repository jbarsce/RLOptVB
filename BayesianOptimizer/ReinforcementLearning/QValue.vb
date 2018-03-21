''' <summary>
''' Qvalue associates a state with an action and the value the agent thinks the pair (s,a) is worth.
''' </summary>
Public Class QValue

    Public ReadOnly Property state As State
    Public ReadOnly Property action As Action
    Public Property qValue As Double

    Public Sub New(state As State, action As Action)
        Me.state = state
        Me.action = action
        qValue = 0
    End Sub

    Public Sub New(state As State, action As Action, initialQValue As Double)
        Me.state = state
        Me.action = action
        Me.qValue = initialQValue
    End Sub

End Class
