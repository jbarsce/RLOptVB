Imports MathNet.Numerics.Distributions
Imports MathNet.Numerics.SpecialFunctions
Imports MathNet.Numerics.Integration

Imports MathNet.Numerics.LinearAlgebra

Public Class ExpectedImprovementBinary : Inherits AcquisitionFunction


    Public Property acqEpsilon As Double

    Public Sub New(acqEpsilon As Double)
        Me.acqEpsilon = acqEpsilon
    End Sub


    Public Overrides Function acquisition(X As Vector(Of Double), posteriorMean As Double, posteriorVariance As Double, Optional bestF As Double = 0) As Double
        'For those rare cases when the posterior variance is 0 or the best F found equals 1 (i.e. the improvement cannot be matched)
        If posteriorVariance = 0 Or bestF = 1 Then
            Return 0
        Else
            Dim functionToIntegrate As Func(Of Double, Double) = Function(z) (binarySquish(z) - bestF) * Normal.PDF(posteriorMean, Math.Sqrt(posteriorVariance), z)

            If bestF <> 0 Then
                'Approaches the integral of the product of the response function times the probability of pi
                Return DoubleExponentialTransformation.Integrate(functionToIntegrate, binarySquishInverse(bestF), Int32.MaxValue, 0.000001)
            Else
                Return DoubleExponentialTransformation.Integrate(functionToIntegrate, Int32.MinValue, Int32.MaxValue, 0.000001)
            End If
        End If
    End Function

    Private Function binarySquish(x As Double) As Double
        '(Erf(x / Sqrt(2)) + 1) / 2 equals to the CDF of z
        Return (Erf(x / Math.Sqrt(2)) + 1) / 2
    End Function

    Private Function binarySquishInverse(z As Double) As Double
        'Sqrt(2) * ErfInv(2 * z - 1) equals to the inverse of the CDF, also known as the normal quantile function or probit function
        Return Math.Sqrt(2) * ErfInv(2 * z - 1)
    End Function

End Class
