
Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Class that contains the function used to calculate the covariance between two vectors
''' </summary>
Public MustInherit Class CovarianceFunction

    Public MustOverride Function k(Xi As Vector(Of Double), Xj As Vector(Of Double), sigmaSquaredF As Double, sigmaSquaredN As Double, lVector As Vector(Of Double)) As Double

End Class
