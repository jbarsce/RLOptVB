
Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Test agent that only modifies two of its hyperparameters. Its purpose is to test what happens when the calculation is reduced, either to test a function optimizer more
''' easily or to do a sensitivity test.
''' </summary>
Public Class SarsaLambdaAgentOnlyTwoParameters : Inherits SarsaLambda

    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, lambda As Double, environment As Gridworld)
        MyBase.New(name, alpha, epsilon, gamma, lambda, environment)
    End Sub

    Public Overrides Function getHyperparameterCount() As Integer
        Return 2
    End Function

    Public Overrides Sub setNewConfiguration(hyperparameters As Vector(Of Double))
        alpha = hyperparameters.Item(0)
        explorationRate = hyperparameters.Item(1)
    End Sub
End Class
