Public Class State

    Public ReadOnly Property name As String
    Public Property reward As Double
    Public Property isEnabled As Boolean
    Public Property isInitial As Boolean
    Public Property isFinal As Boolean
    Public Property disableAtTimeStep As Integer
    Public Property enableAtTimeStep As Integer
    Public Property isEnabledAtStart As Boolean
    Public Property hasWind As Boolean

    Public Sub New(name As String, isEnabled As Boolean, isInitial As Boolean, isFinal As Boolean, Optional hasWind As Boolean = False)
        Me.name = name
        Me.isEnabled = isEnabled
        isEnabledAtStart = isEnabled
        Me.isInitial = isInitial
        Me.isFinal = isFinal
        Me.hasWind = hasWind
        reward = 0
        disableAtTimeStep = -1
        enableAtTimeStep = -1
    End Sub
End Class
