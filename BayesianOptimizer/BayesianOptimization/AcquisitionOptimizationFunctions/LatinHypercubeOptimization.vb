Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Implementation of the Latin Hypercube Sampling as an acquisition optimization function.
''' </summary>
Public Class LatinHypercubeOptimization : Inherits AcquisitionOptimizationFunction

    ''Bayesian Optimizer Information
    Dim observedY As Vector(Of Double)
    Dim hyperparameters As Vector(Of Double)
    Dim calculateMeanOfTestPoint As Func(Of Vector(Of Double), Double)
    Dim calculateVarianceOfTestPoint As Func(Of Vector(Of Double), Double)
    Dim acquisitionFunction As AcquisitionFunction

    Private ReadOnly latinHypercubeDivisions As Integer

    Public Sub New(latinHypercubeDivisions As Integer)
        Me.latinHypercubeDivisions = latinHypercubeDivisions
    End Sub

    ''' <summary>
    ''' Maximizes the (x1, x2, ..., xn) vector that are the most likely to improve the maximum of the objective function f, given the current (x,f(x)) pairs by obtaining samples in a
    ''' Latin Hypercube .
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function optimize(bayesianOptimizer As BayesianOptimizer) As Vector(Of Double)
        observedY = bayesianOptimizer.observedY
        hyperparameters = bayesianOptimizer.hyperparameters
        calculateMeanOfTestPoint = Function(Z) bayesianOptimizer.calculateMeanOfTestPoint(Z)
        calculateVarianceOfTestPoint = Function(Z) bayesianOptimizer.calculateVarianceOfTestPoint(Z)
        acquisitionFunction = bayesianOptimizer.acquisitionFunction

        Dim latinHypercube As New LatinHypercubeSampling(hyperparameters.Count, latinHypercubeDivisions)

        Dim maximumU As Double
        Dim bestPoint As Vector(Of Double) = latinHypercube.nextSample

        If observedY IsNot Nothing Then
            maximumU = acquisitionFunction.acquisition(bestPoint, calculateMeanOfTestPoint(bestPoint), calculateVarianceOfTestPoint(bestPoint), observedY.Max)
        Else
            maximumU = acquisitionFunction.acquisition(bestPoint, calculateMeanOfTestPoint(bestPoint), calculateVarianceOfTestPoint(bestPoint))
        End If

        For i As Integer = 1 To latinHypercubeDivisions - 1
            Dim newPoint As Vector(Of Double) = latinHypercube.nextSample

            Dim newSampleAcquisition As Double
            If observedY IsNot Nothing Then
                newSampleAcquisition = acquisitionFunction.acquisition(newPoint, calculateMeanOfTestPoint(newPoint), calculateVarianceOfTestPoint(newPoint), observedY.Max)
            Else
                newSampleAcquisition = acquisitionFunction.acquisition(newPoint, calculateMeanOfTestPoint(newPoint), calculateVarianceOfTestPoint(newPoint))
            End If

            If newSampleAcquisition > maximumU Then
                maximumU = newSampleAcquisition
                bestPoint = newPoint
            End If
        Next

        Return bestPoint
    End Function
End Class
