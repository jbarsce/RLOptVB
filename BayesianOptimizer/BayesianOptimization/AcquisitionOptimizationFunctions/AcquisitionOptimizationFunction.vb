
Imports MathNet.Numerics.LinearAlgebra

Public MustInherit Class AcquisitionOptimizationFunction

    Public MustOverride Function optimize(bayesianOptimizer As BayesianOptimizer) As Vector(Of Double)

End Class
