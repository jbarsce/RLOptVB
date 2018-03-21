Imports MathNet.Numerics.LinearAlgebra

Public MustInherit Class NextOptQueryDecisionFunction

    Public ReadOnly Property minimumRLRunsPerQueryPoint As Integer

    ' total number Of times f(theta) has been queried, included all the internal queries In query_f
    ' aka "t" In Kuleshov 14'. Employed to know how many times all the arms were pulled.
    Public Property allThetaPull As Integer = 1

    Public Property otherThetaPull As Integer = 1
    Public Property thisThetaPull As Integer = 1

    Public Sub New(minimumRLRunsPerQueryPoint As Integer)
        Me.minimumRLRunsPerQueryPoint = minimumRLRunsPerQueryPoint
    End Sub

    Public MustOverride Function decideIfNextQuery(fTheta As List(Of Double),
                                               allObservedY As Vector(Of Double),
                                               observedY As Vector(Of Double),
                                               Optional similarTaskObservedY As Vector(Of Double) = Nothing,
                                               Optional observedYVariance As Vector(Of Double) = Nothing) As Boolean

End Class
