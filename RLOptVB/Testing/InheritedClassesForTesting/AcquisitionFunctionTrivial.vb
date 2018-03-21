
Imports MathNet.Numerics.LinearAlgebra

Public Class AcquisitionFunctionTrivial : Inherits AcquisitionFunction

    Public Overrides Function acquisition(X As Vector(Of Double), posteriorMean As Double, posteriorVariance As Double, Optional bestF As Double = 0) As Double
        Return X.Item(0) * -1 - Math.Abs(X.Item(1) - 0.68)
    End Function
End Class
