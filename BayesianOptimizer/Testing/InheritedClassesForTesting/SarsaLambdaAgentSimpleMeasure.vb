Public Class SarsaLambdaAgentSimpleMeasure : Inherits SarsaLambda

    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, lambda As Double, environment As Gridworld)
        MyBase.New(name, alpha, epsilon, gamma, lambda, environment)
    End Sub

    Public Overrides Function getLastRunMeasure() As Double
        Dim epsilonDistance As Double = (Math.Abs(0.6 - explorationRate))
        Return alpha * -1 + epsilonDistance * -1 + gamma * 3 + lambda * -2 + 5
    End Function
End Class
