

Imports MathNet.Numerics.LinearAlgebra

Public Class RadialBasisFunction : Inherits CovarianceFunction

    Public Overrides Function k(Xi As Vector(Of Double), Xj As Vector(Of Double), sigmaSquaredF As Double, sigmaSquaredN As Double, lVector As Vector(Of Double)) As Double
        Dim squaredEuclideanDistance As Double = 0

        For i = 0 To Xi.Count - 1
            squaredEuclideanDistance = squaredEuclideanDistance + Math.Pow(Xi.Item(i) - Xj.Item(i), 2)
        Next

        If Xi.Equals(Xj) Then
            Return Math.E ^ (-(squaredEuclideanDistance / (2 * sigmaSquaredF))) + sigmaSquaredN
        Else
            Return Math.E ^ (-(squaredEuclideanDistance / (2 * sigmaSquaredF)))
        End If
    End Function
End Class
