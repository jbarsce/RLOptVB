
Imports MathNet.Numerics.Distributions
Imports MathNet.Numerics.LinearAlgebra

Public Class ExpectedImprovement : Inherits AcquisitionFunction

    Public Sub New(acqEpsilon As Double)
        Me.acqEpsilon = acqEpsilon
    End Sub

    'The epsilon of the acquisition function - minimum value that the new maximum u(Xnewmax) > u(Xcurrentmax) should exceed to become the current maximum, ie u(Xnewmax) > u(Xcurrentmax) + epsilon.
    'Not to be confused with the epsilon of an ε-greedy function.
    Public Property acqEpsilon As Double

    Public Overrides Function acquisition(X As Vector(Of Double), posteriorMean As Double, posteriorVariance As Double, Optional bestF As Double = 0) As Double
        Dim zeta As Double
        If posteriorVariance = 0 Then
            zeta = 0
        Else
            zeta = (posteriorMean - bestF - acqEpsilon) / Math.Sqrt(posteriorVariance)
        End If

        Return ((posteriorMean - bestF - acqEpsilon) * Normal.CDF(0, 1, zeta) + Math.Sqrt(posteriorVariance) * Normal.PDF(0, 1, zeta))
    End Function
End Class
