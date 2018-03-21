Public Class Transition

    Public Property stateFrom As State
    Public Property action As Action
    Public Property stateTo As State
    'Some algorithms (e.g. Dyna-Q+) need to know how much time ellapsed since the last try of the state-action combination
    Public Property timeStepsSinceLastTry As Integer = 0

    Public Sub New(stateFrom As State, action As Action, stateTo As State)
        Me.stateFrom = stateFrom
        Me.action = action
        Me.stateTo = stateTo
    End Sub

End Class
