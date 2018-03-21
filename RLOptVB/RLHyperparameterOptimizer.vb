
Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Base class for performing a hyper-parameter optimization of a reinforcement learning agent
''' </summary>
Public MustInherit Class RLHyperparameterOptimizer

    'Objective Function f
    '-------------------------------------------------------
    'Agent that will run under different combinations of hyperparameters, returning a measure of its performance. This measure is the objective function.
    Public Property reinforcementLearningAgent As Agent
    'List that contains all the "episodes" of the optimizer run, each containing all the RL episodes run under fixed hyperparameters.
    Public Property optimizationEpisodes As New List(Of List(Of Double))
    'List that contains all the variances of each "episode" of the optimizer run. Each list is related to the same list in gaussianProcessEpisodes
    Public Property optimizationEpisodesVariances As New List(Of List(Of Double))
    'List of vector of queried points X = (x1, x2, ... , xn) on the unknown function f(X) (first element of the D previous observations (x,y)).
    Public Property queriedX As New List(Of Vector(Of Double))
    'Vector of observed f(X) based on the queried points X (second element of the D previous observations (x,y)).
    Public Property observedY As Vector(Of Double)
    'Vector of all the observed f(X). It differs from observedY in that it includes all the results of the same X query, while in observedY it is
    'stored the average of them
    Public Property allObservedY As Vector(Of Double)
    'Vector of the variances of each observed f(X) based on the queried points X.
    Public Property observedYVariance As Vector(Of Double)

    '
    '-------------------------------------------------------
    '
    'Hyperparameters
    '-------------------------------------------------------
    '
    'Vector that contains all the current hyperparameters of the reinforcement learning agent.
    Public Property hyperparameters As Vector(Of Double)
    '
    ''Reinforcement Learning
    Public Property episodesToRun As Integer

    'Minimum number of reinforcement learning agent runs per optimizer episode. This is done in order to normalize the results of the GP and to reduce the impact of randomness.
    Public Property minimumRLRunsPerQueryPoint As Integer
    'Maximum number of reinforcement learning agent runs per optimizer episode.
    Public Property maximumRLRunsPerQueryPoint As Integer
    '
    '-------------------------------------------------------
    '
    'Meta-Learning
    '-------------------------------------------------------
    '
    'Property property that determines if there is a decision cycle at every f(X) to determine if X will be queried again at f
    Public Property isNextQueryDecided As Boolean
    'Function that determines 
    Public Property nextOptQueryDecisionFunction As NextOptQueryDecisionFunction
    'ObservedY vector of a similar task, used to initialize the meta-learning layer
    Public Property similarTaskObservedY As Vector(Of Double)
    'List that stores the number of f queries per each meta-episode
    Public Property numberFThetaQueriesPerTheta As New List(Of Double)


    ''' <summary>
    ''' Main sub of each optimizer, where it is attempted to maximize the unknown function f(X) (X = (x1, x2, ..., xn)).
    ''' </summary>
    ''' <param name="numberOfEpisodes"></param>
    Public MustOverride Sub runOptimizer(numberOfEpisodes As Integer)


    ''' <summary>
    ''' Gets the point that maximizes the objective function.
    ''' </summary>
    ''' <returns></returns>
    Public Function getBestPoint() As Vector(Of Double)
        Return queriedX.ElementAt(observedY.MaximumIndex)
    End Function


    ''' <summary>
    ''' Get the best value found in the objective function.
    ''' </summary>
    ''' <returns></returns>
    Public Function getBestF() As Double
        Return observedY.Max
    End Function


    ''' <summary>
    ''' Method that adds a new point (X, y) to the queries datasetof the optimizer, by querying the reinforcement learning agent.
    ''' In order to call this method, a X query vector should be determined a-priori by the optimizer (e.g. by choosing the X that
    ''' maximizes an infill function in Bayesian optimization)
    ''' </summary>
    ''' <param name="X"></param>
    Public Overridable Sub addNewPoint(X As Vector(Of Double))
        queriedX.Add(X)
        Dim queryResult As List(Of Double) = queryX(X)

        observedY = UtilityFunctions.insertItemToVector(observedY, queryResult.Item(0))
        observedYVariance = UtilityFunctions.insertItemToVector(observedYVariance, queryResult.Item(1))

        hyperparameters = X
    End Sub


    ''' <summary>
    ''' Runs the agent several times to evaluate the unknown function f (a measure of the performance of the agent) at point X.
    ''' </summary>
    ''' <param name="X"></param>
    Protected Function queryX(X As Vector(Of Double)) As List(Of Double)
        reinforcementLearningAgent.setNewConfiguration(X)
        'amountOfRunMeasures is the sum of the measures of all the RL runs with this configuration.
        'It is computed to calculate the average.
        Dim amountOfRunMeasures As Double = 0
        'Variance of the measures (i.e.: f(theta)) of all the runs
        Dim measureVariance As Double = 0
        'stepsOfEpisodeAverage stores the average steps the agent run from the beginning to the end of each episode
        Dim stepsOfEpisodeAverage As New List(Of Double)
        'stepsOfEpisodeVariance stores the variance of the steps the agent in each episode
        Dim stepsOfEpisodeVariance As New List(Of Double)
        'List that stores the measures of each run, used to decide if perform an additional query and to calculate the variance
        Dim runMeasures As New List(Of Double)
        'This list stores the steps of each episode, to calculate the variance of the steps
        Dim stepsOfEpisode As New List(Of List(Of Double))

        Dim rlRunsPerQueryPoint = 0

        'For each query, runs the RL agent several times, to obtain the average data
        For i As Integer = 0 To maximumRLRunsPerQueryPoint - 1
            rlRunsPerQueryPoint += 1

            reinforcementLearningAgent.restartAgent()
            reinforcementLearningAgent.run(episodesToRun)
            amountOfRunMeasures = amountOfRunMeasures + reinforcementLearningAgent.getLastRunMeasure
            runMeasures.Add(reinforcementLearningAgent.getLastRunMeasure)
            stepsOfEpisode.Add(New List(Of Double))
            allObservedY = UtilityFunctions.insertItemToVector(allObservedY, reinforcementLearningAgent.getLastRunMeasure)

            'If the run is the first of the query, initializes the list
            If i = 0 Then
                For j As Integer = 0 To reinforcementLearningAgent.getStepsOfEpisode.Count - 1
                    stepsOfEpisodeAverage.Add(reinforcementLearningAgent.getStepsOfEpisode.Item(j))
                    'Variance is initialized in 0
                    stepsOfEpisodeVariance.Add(0)
                    stepsOfEpisode.Item(i).Add(reinforcementLearningAgent.getStepsOfEpisode.Item(j))
                Next
            Else
                'Otherwise, adds the result of the current query
                For j As Integer = 0 To stepsOfEpisodeAverage.Count - 1
                    stepsOfEpisode.Item(i).Add(reinforcementLearningAgent.getStepsOfEpisode.Item(j))
                    stepsOfEpisodeAverage.Item(j) = stepsOfEpisodeAverage.Item(j) + stepsOfEpisode.Item(i).Item(j)
                Next
            End If

            If isNextQueryDecided And i + 1 >= minimumRLRunsPerQueryPoint And rlRunsPerQueryPoint < maximumRLRunsPerQueryPoint Then
                Dim nextQuery = nextOptQueryDecisionFunction.decideIfNextQuery(runMeasures, allObservedY, observedY, similarTaskObservedY)
                If Not nextQuery Then
                    Exit For
                End If
            End If
        Next

        'List that stores the number of queries is updated
        numberFThetaQueriesPerTheta.Add(rlRunsPerQueryPoint)

        'Average run measure is calculated
        Dim averageRunMeasure As Double = amountOfRunMeasures / rlRunsPerQueryPoint

        'Variance of run measures are calculated
        For i As Integer = 0 To rlRunsPerQueryPoint - 1
            measureVariance = measureVariance + Math.Pow(runMeasures.Item(i) - averageRunMeasure, 2)
        Next

        measureVariance = measureVariance / rlRunsPerQueryPoint


        'Average number of steps in each episode are calculated
        For i As Integer = 0 To stepsOfEpisodeAverage.Count - 1
            stepsOfEpisodeAverage.Item(i) = stepsOfEpisodeAverage.Item(i) / rlRunsPerQueryPoint
        Next

        'The variance of the number of steps of each episode is added (division of this into the rlRunsPerQueryPoint is left)
        For i As Integer = 0 To rlRunsPerQueryPoint - 1
            For j As Integer = 0 To reinforcementLearningAgent.getStepsOfEpisode.Count - 1
                stepsOfEpisodeVariance.Item(j) += Math.Pow(stepsOfEpisode.Item(i).Item(j) - stepsOfEpisodeAverage.Item(i), 2)
            Next
        Next

        For j As Integer = 0 To reinforcementLearningAgent.getStepsOfEpisode.Count - 1
            stepsOfEpisodeVariance.Item(j) = stepsOfEpisodeVariance.Item(j) / rlRunsPerQueryPoint
        Next

        optimizationEpisodes.Add(stepsOfEpisodeAverage)
        optimizationEpisodesVariances.Add(stepsOfEpisodeVariance)

        Dim result As New List(Of Double)
        result.Add(averageRunMeasure)
        result.Add(measureVariance)

        Return result
    End Function

End Class
