Imports MathNet.Numerics.Distributions
Imports MathNet.Numerics.SpecialFunctions
Imports MathNet.Numerics.Integration

Public Class TestSandbox

    Public Sub testLatinHypercube()

        Dim lhs As New LatinHypercubeSampling(4, 4)

        Dim a = lhs.nextSample()
        Dim b = lhs.nextSample()
        Dim c = lhs.nextSample()
        Dim d = lhs.nextSample()
        Dim e = lhs.nextSample
    End Sub

    Public Sub testIntegration()

        Dim bestY As Double = 0.9411
        Dim mu As Double = 9.3017
        Dim s2 As Double = Math.Sqrt(55.6019)


        Dim binarySquish As Func(Of Double, Double) = Function(z) ((Erf(z / Math.Sqrt(2)) + 1) / 2)
        Dim binarySquishInverse As Func(Of Double, Double) = Function(z) Math.Sqrt(2) * ErfInv(2 * z - 1)

        Dim functionToIntegrate As Func(Of Double, Double) = Function(z) (binarySquish(z) - bestY) * Normal.PDF(mu, s2, z)

        Dim r4 = DoubleExponentialTransformation.Integrate(functionToIntegrate, binarySquishInverse(bestY), Integer.MaxValue, 0.000001)
    End Sub

    Public Sub testSandbox()

    End Sub

End Class
