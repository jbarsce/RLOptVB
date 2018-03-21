
Imports MathNet.Numerics.LinearAlgebra

Public Class LatinHypercubeLogLikelihood : Inherits LogLikelihoodOptimizationFunction

    Public Property numberOfGPVariables As Integer = 3
    Public Property numberOfDivisions As Integer = 100
    Public Property minimumRange As Integer = 0
    Public Property maximumRange As Integer = 1

    Public Overrides Sub optimize(bayesianOptimizer As BayesianOptimizer)
        Dim maximumLogLikelihood = bayesianOptimizer.calculateLogLikelihood
        Dim bestVector As Vector(Of Double) = Vector(Of Double).Build.Dense(New Double() {bayesianOptimizer.sigmaSquaredF, bayesianOptimizer.sigmaSquaredN, bayesianOptimizer.lVector.Item(0)})

        'Optimizes the GP hyperparameters
        Dim lhs As New LatinHypercubeSampling(numberOfGPVariables, numberOfDivisions, minimumRange, maximumRange)

        For i As Integer = 0 To numberOfDivisions - 1
            Dim configVector As Vector(Of Double) = lhs.nextSample
            bayesianOptimizer.setNewGPHyperparameters(configVector.Item(0), configVector.Item(1), Vector(Of Double).Build.Dense(1, configVector.Item(2)))
            Dim newLogLikelihood As Double = bayesianOptimizer.calculateLogLikelihood
            If newLogLikelihood > maximumLogLikelihood Then
                maximumLogLikelihood = newLogLikelihood
                bestVector = configVector
            End If
        Next

        sigmaSquaredF = bestVector.Item(0)
        sigmaSquaredN = bestVector.Item(1)
        Dim bestLVector = Vector(Of Double).Build.Dense(numberOfGPVariables - 2)

        For i As Integer = 2 To numberOfGPVariables - 1
            bestLVector.Item(i - 2) = bestVector.Item(i)
        Next

        lVector = bestLVector
    End Sub
End Class
