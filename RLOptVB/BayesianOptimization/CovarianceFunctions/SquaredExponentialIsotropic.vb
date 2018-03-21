

Imports MathNet.Numerics.LinearAlgebra

Public Class SquaredExponentialIsotropic : Inherits CovarianceFunction

    Public Overrides Function k(Xi As Vector(Of Double), Xj As Vector(Of Double), sigmaSquaredF As Double, sigmaSquaredN As Double, lVector As Vector(Of Double)) As Double
        Dim euclideanDistance As Double = 0

        For i = 0 To Xi.Count - 1
            euclideanDistance = euclideanDistance + Math.Pow(Xi.Item(i) - Xj.Item(i), 2)
        Next

        euclideanDistance = Math.Sqrt(euclideanDistance)

        Dim partialResult As Double = -(1 / (2 * lVector.Item(0))) * euclideanDistance

        If Xi.Equals(Xj) Then
            Return sigmaSquaredF * Math.Pow(Math.E, partialResult) + sigmaSquaredN
        Else
            Return sigmaSquaredF * Math.Pow(Math.E, partialResult)
        End If
    End Function
End Class
