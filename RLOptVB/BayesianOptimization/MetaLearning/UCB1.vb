Imports MathNet.Numerics.LinearAlgebra

Public Class UCB1 : Inherits NextOptQueryDecisionFunction

    Public Sub New(minimumRLRunsPerQueryPoint As Integer)
        MyBase.New(minimumRLRunsPerQueryPoint)
    End Sub

    Public Overrides Function decideIfNextQuery(fTheta As List(Of Double),
                                                allObservedY As Vector(Of Double),
                                                observedY As Vector(Of Double),
                                                Optional similarTaskObservedY As Vector(Of Double) = Nothing,
                                                Optional observedYVariance As Vector(Of Double) = Nothing) As Boolean

        Dim numberOfObservedY
        If observedY IsNot Nothing Then
            numberOfObservedY = observedY.Count
        Else
            numberOfObservedY = 0
        End If

        Dim fGlobalMean
        If numberOfObservedY = 0 Then
            fGlobalMean = 0
        Else
            fGlobalMean = observedY.Average
        End If

        Dim otherThetaEligibility = fGlobalMean + Math.Sqrt(2 * (Math.Log(allThetaPull) / otherThetaPull))

        Dim fThisThetaMean = fTheta.Average()

        Dim thisThetaEligibility = fThisThetaMean + Math.Sqrt(2 * (Math.Log(allThetaPull) / thisThetaPull))

        allThetaPull = allThetaPull + 1
        If thisThetaEligibility > otherThetaEligibility Then
            thisThetaPull = thisThetaPull + 1
            Return True
        Else
            otherThetaPull = otherThetaPull + 1
            thisThetaPull = 1
            Return False
        End If
    End Function
End Class
