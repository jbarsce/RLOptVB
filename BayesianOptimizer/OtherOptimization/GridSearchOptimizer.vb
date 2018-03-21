
Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' This class performs an optimization of hyper-parameters by doing an exhaustive, grid search over the hyper-parameter space
''' </summary>
Public Class GridSearchOptimizer : Inherits RLHyperparameterOptimizer


    Public Sub New(reinforcementLearningAgent As Agent,
                   Optional episodesToRunRLAgent As Integer = 50,
                   Optional maximumRLRunsPerQueryPoint As Integer = 5,
                   Optional minimumRLRunsPerQueryPoint As Integer = 1)

        hyperparameters = Vector(Of Double).Build.Sparse(reinforcementLearningAgent.getHyperparameterCount)
        Me.reinforcementLearningAgent = reinforcementLearningAgent
        episodesToRun = episodesToRunRLAgent
        Me.maximumRLRunsPerQueryPoint = maximumRLRunsPerQueryPoint
        Me.minimumRLRunsPerQueryPoint = minimumRLRunsPerQueryPoint
    End Sub

    Public Overrides Sub runOptimizer(numberOfEpisodes As Integer)
        ' Note: this method was implemented in order to test how grid search performs, so
        ' the values of the hyperparameters in the grid are established manually.
        ' It is not properly tested, so use it at your own risk.

        Dim queriesCounter = 1

        For iteration As Integer = 0 To 1

            Dim alphaValues = New List(Of Double)
            Dim epsilonValues = New List(Of Double)
            Dim gammaValues = New List(Of Double)
            Dim lambdaValues = New List(Of Double)

            ' 0.25 is added in order to avoid starting at 0 (and, therefore, to sample a more diverse set of values)
            For i As Integer = 0 To 1
                alphaValues.Add(i * (1 / 2) + 0.25)
                epsilonValues.Add(i * (1 / 2) + 0.25)
                gammaValues.Add(i * (1 / 2) + 0.25)
                lambdaValues.Add(i * (1 / 2) + 0.25)
            Next

            alphaValues = alphaValues.Randomize()
            epsilonValues = epsilonValues.Randomize()
            gammaValues = gammaValues.Randomize()
            lambdaValues = lambdaValues.Randomize()

            ' Vectors are loaded and then queried
            For alphaIndex As Integer = 0 To alphaValues.Count - 1
                Dim zeroDVectorToQuery As Vector(Of Double) = Nothing
                Dim oneDVectorToQuery = UtilityFunctions.insertItemToVector(zeroDVectorToQuery, alphaValues(alphaIndex))

                For epsilonIndex As Integer = 0 To epsilonValues.Count - 1
                    Dim twoDVectorToQuery = UtilityFunctions.insertItemToVector(oneDVectorToQuery, epsilonValues(epsilonIndex))

                    For gammaIndex As Integer = 0 To gammaValues.Count - 1
                        Dim threeDVectorToQuery = UtilityFunctions.insertItemToVector(twoDVectorToQuery, gammaValues(gammaIndex))

                        For lambdaIndex As Integer = 0 To lambdaValues.Count - 1
                            Dim fourDVectorToQuery = UtilityFunctions.insertItemToVector(threeDVectorToQuery, lambdaValues(lambdaIndex))
                            addNewPoint(fourDVectorToQuery)
                            queriesCounter = queriesCounter + 1
                            If queriesCounter = 30 Then
                                Exit Sub
                            End If
                        Next
                    Next
                Next
            Next
        Next
    End Sub


End Class
