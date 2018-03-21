Imports MathNet.Numerics.LinearAlgebra

Public Class EpsilonGreedy : Inherits NextOptQueryDecisionFunction

    Private Property epsilon As Double

    Public Sub New(minimumRLRunsPerQueryPoint As Integer, Optional epsilon As Double = 0.2)
        MyBase.New(minimumRLRunsPerQueryPoint)
        Me.epsilon = epsilon
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

        Dim randomNumber = UtilityFunctions.randomGenerator.NextDouble
        Dim exploit = randomNumber > epsilon

        allThetaPull = allThetaPull + 1
        If exploit Then
            If fGlobalMean >= fThisThetaMean Then
                otherThetaPull = otherThetaPull + 1
                thisThetaPull = 1
                Return False
            Else
                thisThetaPull = thisThetaPull + 1
                Return True
            End If
        Else
            If fGlobalMean >= fThisThetaMean Then
                thisThetaPull = thisThetaPull + 1
                Return True
            Else
                otherThetaPull = otherThetaPull + 1
                thisThetaPull = 1
                Return False
            End If
        End If

    End Function
End Class
