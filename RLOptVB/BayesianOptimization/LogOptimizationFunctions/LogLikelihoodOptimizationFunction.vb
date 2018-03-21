
Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Function that optimizes the Log Likelihood of the Gaussian Process. It includes distinct functions that those used to optimize the
''' acquisition function due to the increased complexity of the latter functions, that normally include gradients, compared to the
''' complexity of the former functions, that are normally cheap sampling functions.
''' </summary>
Public MustInherit Class LogLikelihoodOptimizationFunction

    Public Property sigmaSquaredF As Double
    Public Property sigmaSquaredN As Double
    Public Property lVector As Vector(Of Double)


    ''' <summary>
    ''' Given a Gaussian Process, optimizes its hyperparameters. Once finished, the results can be extracted from the sigmaSquaredF,
    ''' sigmaSquaredN and lVector properties.
    ''' </summary>
    Public MustOverride Sub optimize(bayesianOptimizer As BayesianOptimizer)
End Class
