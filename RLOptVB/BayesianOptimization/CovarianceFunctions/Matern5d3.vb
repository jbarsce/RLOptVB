
Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Implementation of the Màtern 5/2 covariance kernel.
''' </summary>
Public Class Matern5d3 : Inherits CovarianceFunction

    Public Overrides Function k(Xi As Vector(Of Double), Xj As Vector(Of Double), sigmaSquaredF As Double, sigmaSquaredN As Double, lVector As Vector(Of Double)) As Double
        Dim lMatrix As Matrix(Of Double) = Matrix(Of Double).Build.Dense(Xi.Count, Xi.Count, 0)
        For i = 0 To Xi.Count - 1
            lMatrix.Item(i, i) = Math.Pow(lVector.Item(i), -2)
        Next
        Dim rSquared As Double = ((Xi.ToColumnMatrix - Xj.ToColumnMatrix).Transpose * lMatrix * (Xi - Xj)).Item(0)

        If Xi.Equals(Xj) Then
            Return sigmaSquaredF * (1 + Math.Sqrt(5) * Math.Sqrt(rSquared) + (5 / 3) * rSquared ^ 2) * Math.E ^ (-Math.Sqrt(5) * Math.Sqrt(rSquared)) + sigmaSquaredN
        Else
            Return sigmaSquaredF * (1 + Math.Sqrt(5) * Math.Sqrt(rSquared) + (5 / 3) * rSquared ^ 2) * Math.E ^ (-Math.Sqrt(5) * Math.Sqrt(rSquared))
        End If
    End Function
End Class
