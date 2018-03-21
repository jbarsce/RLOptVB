Imports MathNet.Numerics.LinearAlgebra
Imports MathNet.Numerics.Statistics.Statistics

Public Class UCB1Variance : Inherits NextOptQueryDecisionFunction

    Public Sub New(minimumRLRunsPerQueryPoint As Integer)
        MyBase.New(minimumRLRunsPerQueryPoint)
    End Sub

    Public Overrides Function decideIfNextQuery(fTheta As List(Of Double),
                                                allObservedY As Vector(Of Double),
                                                observedY As Vector(Of Double),
                                                Optional similarTaskObservedYVariance As Vector(Of Double) = Nothing,
                                                Optional observedYVariance As Vector(Of Double) = Nothing) As Boolean

        ' The number of all the observedY of a similar task
        Dim numberOfSimilarTaskObservedY = 0
        If similarTaskObservedYVariance IsNot Nothing Then
            numberOfSimilarTaskObservedY = similarTaskObservedYVariance.Count

            ' If the vector of all the observedY on a similar task is not empty, it appends to the observedY vector
            If observedYVariance IsNot Nothing Then
                observedYVariance = UtilityFunctions.appendVectors(similarTaskObservedYVariance, observedYVariance)
            Else
                observedYVariance = similarTaskObservedYVariance
            End If
        End If

        Dim numberOfAllObservedYVariances = allObservedY.Count + numberOfSimilarTaskObservedY

        Dim numberOfObservedYVariances
        If observedYVariance IsNot Nothing Then
            numberOfObservedYVariances = observedYVariance.Count
        Else
            numberOfObservedYVariances = 0
        End If

        Dim fVarianceGlobalMean
        If numberOfObservedYVariances = 0 Then
            fVarianceGlobalMean = 0
        Else
            fVarianceGlobalMean = observedYVariance.Average
        End If


        Dim otherThetaEligibility = fVarianceGlobalMean + Math.Sqrt(2 * (Math.Log(allThetaPull) / otherThetaPull))

        Dim fThisThetaVariance = fTheta.Variance

        Dim thisThetaEligibility = fThisThetaVariance + Math.Sqrt(2 * (Math.Log(allThetaPull) / thisThetaPull))


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
