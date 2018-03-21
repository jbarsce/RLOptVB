

Imports MathNet.Numerics.LinearAlgebra


''' <summary>
'''  This class performs an optimization of hyper-parameters by doing a random search
''' </summary>
Public Class RandomSearchOptimizer : Inherits RLHyperparameterOptimizer

    Dim hypersphereRadius As Double

    Public Sub New(reinforcementLearningAgent As Agent,
                   Optional hypersphereRadius As Double = 0.15,
                   Optional episodesToRunRLAgent As Integer = 50,
                   Optional maximumRLRunsPerQueryPoint As Integer = 5,
                   Optional minimumRLRunsPerQueryPoint As Integer = 1)

        Me.hypersphereRadius = hypersphereRadius
        hyperparameters = Vector(Of Double).Build.Sparse(reinforcementLearningAgent.getHyperparameterCount)
        Me.reinforcementLearningAgent = reinforcementLearningAgent
        episodesToRun = episodesToRunRLAgent
        Me.maximumRLRunsPerQueryPoint = maximumRLRunsPerQueryPoint
        Me.minimumRLRunsPerQueryPoint = minimumRLRunsPerQueryPoint

    End Sub

    Public Overrides Sub runOptimizer(numberOfEpisodes As Integer)

        ' First random point is determined and queried
        Dim initialPoint As Vector(Of Double) = Nothing
        For hyperparameter As Integer = 0 To hyperparameters.Count - 1
            initialPoint = UtilityFunctions.insertItemToVector(initialPoint, UtilityFunctions.randomGenerator.NextDouble)
        Next

        addNewPoint(initialPoint)
        Dim currentMaximum = getBestF()

        ' Next points are queried in the hypersphere surrounding the maximum (that is why it starts at 1)
        For i As Integer = 1 To numberOfEpisodes - 1
            Dim newPoint As Vector(Of Double) = Nothing

            For hyperparameter As Integer = 0 To hyperparameters.Count - 1
                ' the new point element in the dimension of the current hyper-parameter
                Dim newPointElementOfDimension = currentMaximum + (2 * (UtilityFunctions.randomGenerator.NextDouble - 0.5)) * hypersphereRadius

                ' point is capped at (0, 1)
                If newPointElementOfDimension > 0.999999 Then
                    newPointElementOfDimension = 0.999999
                End If

                If newPointElementOfDimension < 0.000001 Then
                    newPointElementOfDimension = 0.000001
                End If

                newPoint = UtilityFunctions.insertItemToVector(newPoint, UtilityFunctions.randomGenerator.NextDouble)
            Next

            addNewPoint(newPoint)
            currentMaximum = getBestF()
        Next

    End Sub
End Class
