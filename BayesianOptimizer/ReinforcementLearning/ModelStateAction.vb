
''' <summary>
''' Model(s,a) stores the values of the reward and s' of the model for the state s and action a.
''' </summary>
Public Class ModelStateAction

    Public ReadOnly Property state As State
    Public ReadOnly Property action As Action

    Public Property reward As Double
    Public Property sPlus As State
    Public Property visited As Boolean = False

    Public Sub New(state As State, action As Action, Optional initialReward As Double = 0, Optional initialSPlus As State = Nothing)
        Me.state = state
        Me.action = action

        reward = initialReward
        sPlus = initialSPlus
    End Sub

End Class
