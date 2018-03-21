Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Test agent that only modifies one of its hyperparameters (epsilon). Its purpose is to graph the GP behavior in order to get a better understanding of it.
''' </summary>
Public Class SarsaLambdaAgentBinaryOnlyOneParameter : Inherits SarsaLambdaSuccessMeasure

    Public Const alphaHyperparameter As Integer = 0
    Public Const epsilonHyperparameter As Integer = 1
    Public Const gammaHyperparameter As Integer = 2
    Public Const lambdaHyperparameter As Integer = 3

    Public Property indexOfParameterToOptimize As Integer = epsilonHyperparameter

    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, lambda As Double, environment As Gridworld)
        MyBase.New(name, alpha, epsilon, gamma, lambda, environment)
    End Sub

    Public Overrides Function getHyperparameterCount() As Integer
        Return 1
    End Function

    Public Overrides Sub setNewConfiguration(hyperparameters As Vector(Of Double))
        Select Case indexOfParameterToOptimize
            Case alphaHyperparameter
                alpha = hyperparameters.Item(0)
                Exit Select
            Case epsilonHyperparameter
                explorationRate = hyperparameters.Item(0)
                Exit Select
            Case gammaHyperparameter
                gamma = hyperparameters.Item(0)
                Exit Select
            Case lambdaHyperparameter
                lambda = hyperparameters.Item(0)
                Exit Select
        End Select
    End Sub
End Class

