Imports MathNet.Numerics.LinearAlgebra
Imports MathNet.Numerics.Statistics.Statistics

Public Class UCB1TunedStandarized : Inherits NextOptQueryDecisionFunction

    Public Sub New(minimumRLRunsPerQueryPoint As Integer)
        MyBase.New(minimumRLRunsPerQueryPoint)
    End Sub

    ''' <summary>
    ''' Function used to decide if performing a new query of the objective function f under a certain configuration has any value.
    ''' This implementation uses a UCB1-Tuned Contextual Bandit. It also performs a z-score standarization over observedY and standarizes
    ''' all other observations based on this first standarization.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function decideIfNextQuery(fTheta As List(Of Double),
                                                allObservedY As Vector(Of Double),
                                                observedY As Vector(Of Double),
                                                Optional similarTaskObservedY As Vector(Of Double) = Nothing,
                                                Optional observedYVariance As Vector(Of Double) = Nothing) As Boolean

        ' TODO update from the pull number bugfix pending

        ' total number Of times f(theta) has been queried, included all the internal queries In query_f
        ' aka "t" In Kuleshov 14'. Employed to know how many times all the arms were pulled.
        Dim numberOfAllObservedY = allObservedY.Count

        ' ------ ObservedY queries (aka BO queries) ----------

        Dim numberOfObservedY
        If observedY IsNot Nothing Then
            numberOfObservedY = observedY.Count
        Else
            numberOfObservedY = 0
        End If

        Dim numberOfBOQuery = numberOfObservedY + 1
        Dim fGlobalMean
        If numberOfObservedY < 2 Then
            fGlobalMean = 0
        Else
            fGlobalMean = observedY.Average
        End If

        Dim fGlobalVariance
        If numberOfObservedY < 2 Then
            fGlobalVariance = 1
        Else
            fGlobalVariance = StandardDeviation(observedY)
        End If

        Dim fGlobaStandarizedlMean = 0
        Dim fGlobalStandarizedVariance = 1

        Dim otherQueryEligibility = fGlobaStandarizedlMean + Math.Sqrt((Math.Log(numberOfAllObservedY) / numberOfBOQuery) *
                                                            Math.Min(0.25, fGlobalStandarizedVariance + Math.Sqrt(2 * Math.Log(numberOfAllObservedY) / numberOfBOQuery)))

        ' ------ This query ----------------------------------

        Dim fThetaStandarized = UtilityFunctions.zScoreNormalization(fTheta, fGlobalMean, fGlobalVariance)

        Dim fThisThetaNumberQuery = fThetaStandarized.Count
        Dim fThisThetaMean = fThetaStandarized.Average()
        Dim fThisThetaVariance = fThetaStandarized.Variance()

        Dim thisThetaEligibility = fThisThetaMean + Math.Sqrt((Math.Log(numberOfAllObservedY) / fThisThetaNumberQuery) *
                                                            Math.Min(0.25, fThisThetaVariance + Math.Sqrt(2 * Math.Log(numberOfAllObservedY) / fThisThetaNumberQuery)))

        Return thisThetaEligibility > otherQueryEligibility

    End Function

End Class
