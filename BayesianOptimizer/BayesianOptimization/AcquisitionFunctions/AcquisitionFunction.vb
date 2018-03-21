Imports MathNet.Numerics.LinearAlgebra

Public MustInherit Class AcquisitionFunction

    Public MustOverride Function acquisition(X As Vector(Of Double), posteriorMean As Double, posteriorVariance As Double, Optional bestF As Double = Nothing) As Double

End Class
