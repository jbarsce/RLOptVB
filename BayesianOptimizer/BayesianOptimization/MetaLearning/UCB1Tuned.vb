Imports MathNet.Numerics.LinearAlgebra
Imports MathNet.Numerics.Statistics.Statistics

Public Class UCB1Tuned : Inherits NextOptQueryDecisionFunction

    Public Sub New(minimumRLRunsPerQueryPoint As Integer)
        MyBase.New(minimumRLRunsPerQueryPoint)
    End Sub

    ''' <summary>
    ''' Function used to decide if performing a new query of the objective function f under a certain configuration has any value.
    ''' This implementation uses a UCB1-Tuned Contextual Bandit.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function decideIfNextQuery(fTheta As List(Of Double),
                                                allObservedY As Vector(Of Double),
                                                observedY As Vector(Of Double),
                                                Optional similarTaskObservedY As Vector(Of Double) = Nothing,
                                                Optional observedYVariance As Vector(Of Double) = Nothing) As Boolean

        If similarTaskObservedY IsNot Nothing Then

            ' If the vector of all the observedY on a similar task is not empty, it appends to the observedY vector
            If observedY IsNot Nothing Then
                observedY = UtilityFunctions.appendVectors(similarTaskObservedY, observedY)
            Else
                observedY = similarTaskObservedY
            End If
        End If


        Dim numberOfObservedY
        If observedY IsNot Nothing Then
            numberOfObservedY = observedY.Count
        Else
            numberOfObservedY = 0
        End If

        Dim fGlobalMean
        If observedY Is Nothing Then
            fGlobalMean = 0
        Else
            fGlobalMean = observedY.Average
        End If

        Dim fGlobalVariance
        If numberOfObservedY < 2 Then
            fGlobalVariance = 0
        Else
            fGlobalVariance = Variance(observedY)
        End If

        Dim otherThetaEligibility = fGlobalMean + Math.Sqrt((Math.Log(allThetaPull) / otherThetaPull) *
                                                            Math.Min(0.25, fGlobalVariance + Math.Sqrt(2 * Math.Log(allThetaPull) / otherThetaPull)))

        Dim fThisThetaMean = fTheta.Average()
        Dim fThisThetaVariance = fTheta.Variance

        Dim thisThetaEligibility = fThisThetaMean + Math.Sqrt((Math.Log(allThetaPull) / thisThetaPull) *
                                                            Math.Min(0.25, fThisThetaVariance + Math.Sqrt(2 * Math.Log(allThetaPull) / thisThetaPull)))


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
