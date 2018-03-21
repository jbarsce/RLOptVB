Imports MathNet.Numerics.LinearAlgebra

Public Class Softmax : Inherits NextOptQueryDecisionFunction

    Private Property tau As Double

    Public Sub New(minimumRLRunsPerQueryPoint As Integer, Optional tau As Double = 1)
        MyBase.New(minimumRLRunsPerQueryPoint)
        Me.tau = tau
    End Sub

    Public Overrides Function decideIfNextQuery(fTheta As List(Of Double), allObservedY As Vector(Of Double), observedY As Vector(Of Double), Optional similarTaskObservedY As Vector(Of Double) = Nothing, Optional observedYVariance As Vector(Of Double) = Nothing) As Boolean

        Dim numberOfObservedY
        If observedY IsNot Nothing Then
            numberOfObservedY = observedY.Count
        Else
            numberOfObservedY = 0
        End If

        Dim fGlobalMean
        If numberOfObservedY = 0 Then
            fGlobalMean = fTheta.Average
        Else
            fGlobalMean = observedY.Average
        End If

        Dim fThisThetaMean = fTheta.Average()

        ' If the numbers are small enough so raising them equals 0, they are rescaled
        If Math.Exp(fThisThetaMean / tau) + Math.Exp(fGlobalMean / tau) = 0 Then
            fThisThetaMean = Math.Pow(Math.Abs(fThisThetaMean), -1)
            fGlobalMean = Math.Pow(Math.Abs(fGlobalMean), -1)
        End If

        Dim randomNumber = UtilityFunctions.randomGenerator.NextDouble
        allThetaPull = allThetaPull + 1

        Dim otherThetaPullProbability = Math.Exp(fGlobalMean / tau) / (Math.Exp(fGlobalMean / tau) + Math.Exp(fThisThetaMean / tau))
        If randomNumber < otherThetaPullProbability Then
            otherThetaPull = otherThetaPull + 1
            thisThetaPull = 1
            Return False
        Else
            thisThetaPull = thisThetaPull + 1
            Return True
        End If

    End Function
End Class
