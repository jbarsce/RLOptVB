
Public Class TestAgentGridworld
    'Each "RL Meta Episode" is a meta episode that consists of an environment restart, and a fixed set of agent's episodes.
    Private Const rlMetaEpisodes As Integer = 5
    Private Const rlEpisodes As Integer = 50
    Private Const rlRunsPerQueryPoint As Integer = 20

    ''' <summary>
    ''' Sarsa(Lambda) agent that behaves as the agent in bayesian optimizer but without
    ''' the bayesian optimizer. The simulation generates a file detailing the avg number
    ''' of steps in each of the agent's episodes, in order to get a detailed view of the
    ''' learning curve.
    ''' </summary>
    Public Sub testAgentSarsaLambdaWithAverageAndVariance()
        Const alpha As Double = 0.3

        Const epsilon As Double = 0.1

        Const gamma As Double = 0.9

        Const lambda As Double = 0.5


        Dim environment As Gridworld = TestBayesianOptimizer.createADoubleBlockingMaze()
        Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", alpha, epsilon, gamma, lambda, environment)
        agent.cutoffTime = 400
        'amountOfRunMeasures is the sum of the measures of all the RL runs with this configuration.
        'It is computed to calculate the average.
        Dim amountOfRunMeasures As Double = 0
        'List that stores the measures of each run, used to calculate the variance
        Dim runsMeasures As New List(Of Double)
        'This list stores the steps of each episode, to calculate the variance of the steps
        Dim stepsOfEpisode As New List(Of List(Of Double))
        'stepsOfEpisodeAverage stores the average steps the agent run from the beginning to the end of each episode
        Dim stepsOfEpisodeAverage As New List(Of Double)
        'stepsOfEpisodeVariance stores the variance of the steps the agent in each episode
        Dim stepsOfEpisodeVariance As New List(Of Double)
        'Variance of the measures (i.e.: f(theta)) of all the runs
        Dim measureVariance As Double = 0

        'List similar to stepsOfEpisode but that counts whether an episode was successful or not
        Dim successOfEpisode As New List(Of List(Of Double))
        'successAverage stores the success average (from 0 to 1) of each run from the beginning to the end of each episode
        Dim successAverage As New List(Of Double)

        'For each query, runs the RL agent several times, to obtain the average data
        For i As Integer = 0 To rlRunsPerQueryPoint - 1
            agent.restartAgent()
            agent.run(rlEpisodes)
            amountOfRunMeasures = amountOfRunMeasures + agent.getLastRunMeasure
            runsMeasures.Add(agent.getLastRunMeasure)
            stepsOfEpisode.Add(New List(Of Double))
            successOfEpisode.Add(New List(Of Double))

            'If the run is the first of the query, initializes the list
            If i = 0 Then
                For j As Integer = 0 To agent.getStepsOfEpisode.Count - 1
                    stepsOfEpisodeAverage.Add(agent.getStepsOfEpisode.Item(j))

                    'Variance is initialized in 0
                    stepsOfEpisodeVariance.Add(0)
                    stepsOfEpisode.Item(i).Add(agent.getStepsOfEpisode.Item(j))
                Next

                Dim successOfQueryEpisodes = agent.getSuccessOfEpisode
                For Each queryEpisode In successOfQueryEpisodes
                    If queryEpisode Then
                        successAverage.Add(1)
                    Else
                        successAverage.Add(0)
                    End If
                Next
            Else
                'Otherwise, adds the result of the current query
                For j As Integer = 0 To stepsOfEpisodeAverage.Count - 1
                    stepsOfEpisode.Item(i).Add(agent.getStepsOfEpisode.Item(j))
                    stepsOfEpisodeAverage.Item(j) = stepsOfEpisodeAverage.Item(j) + stepsOfEpisode.Item(i).Item(j)
                Next

                Dim successOfQueryEpisodes = agent.getSuccessOfEpisode
                For episodeNumber As Integer = 0 To successOfQueryEpisodes.Count - 1
                    If successOfQueryEpisodes.Item(episodeNumber) Then
                        successAverage.Item(episodeNumber) += 1
                    End If
                Next
            End If
        Next

        'Average number of steps in each episode are calculated
        For i As Integer = 0 To stepsOfEpisodeAverage.Count - 1
            stepsOfEpisodeAverage.Item(i) = stepsOfEpisodeAverage.Item(i) / rlRunsPerQueryPoint
            successAverage.Item(i) = successAverage.Item(i) / rlRunsPerQueryPoint
        Next

        'The variance of the number of steps of each episode is added (division of this into the rlRunsPerQueryPoint is left)
        For i As Integer = 0 To rlRunsPerQueryPoint - 1
            For j As Integer = 0 To agent.getStepsOfEpisode.Count - 1
                stepsOfEpisodeVariance.Item(j) += Math.Pow(stepsOfEpisode.Item(i).Item(j) - stepsOfEpisodeAverage.Item(i), 2)
            Next
        Next

        For j As Integer = 0 To agent.getStepsOfEpisode.Count - 1
            stepsOfEpisodeVariance.Item(j) = stepsOfEpisodeVariance.Item(j) / rlRunsPerQueryPoint
        Next


        ResultSaver.saveAgentResultsAsFile(successAverage, stepsOfEpisodeVariance)
    End Sub

    Public Sub testAgentSarsaLambda()
        For j As Integer = 0 To rlMetaEpisodes - 1
            Dim environment As Gridworld = TestBayesianOptimizer.createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.499647, 0.103915, 0.562122, 0.54239, environment)
            agent.cutoffTime = 400

            agent.restartAgent()
            agent.run(rlEpisodes)

            Dim steps As New List(Of Double)
            Dim acumulatedSteps As Double = 0
            Dim currentEpisode = 0

            For Each stepOfEpisode In agent.stepsOfEpisode
                acumulatedSteps += stepOfEpisode
                currentEpisode += 1
                steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
            Next


            'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

            ResultSaver.saveAgentResultsAsFile(agent)
        Next
    End Sub

    Public Sub testAgentSarsa()
        Dim environment As Gridworld = createSimpleGridworld()

        Dim agent As New Sarsa("GridworldAgent", 0.1, 0.2, 0.85, environment)

        agent.restartAgent()
        agent.run(rlEpisodes)

        Dim b As String = ""

        For i As Integer = 0 To agent.stepsOfEpisode.Count - 1
            b = b + vbCrLf + agent.stepsOfEpisode.Item(i).ToString
        Next

        Dim steps As New List(Of Double)
        Dim acumulatedSteps As Double = 0
        Dim currentEpisode = 0

        For Each stepOfEpisode In agent.stepsOfEpisode
            acumulatedSteps += stepOfEpisode
            currentEpisode += 1
            steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
        Next


        'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

        ResultSaver.saveAgentResultsAsFile(agent)

    End Sub

    Public Sub testAgentQLambda()
        Dim environment As Gridworld = createSimpleGridworld()

        Dim agent As New QLambda("GridworldAgent", 0.1, 0.2, 0.85, 0.5, environment)

        agent.restartAgent()
        agent.run(rlEpisodes)

        Dim b As String = ""

        For i As Integer = 0 To agent.stepsOfEpisode.Count - 1
            b = b + vbCrLf + agent.stepsOfEpisode.Item(i).ToString
        Next

        Dim steps As New List(Of Double)
        Dim acumulatedSteps As Double = 0
        Dim currentEpisode = 0

        For Each stepOfEpisode In agent.stepsOfEpisode
            acumulatedSteps += stepOfEpisode
            currentEpisode += 1
            steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
        Next

        'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

        ResultSaver.saveAgentResultsAsFile(agent)

    End Sub

    Public Sub testAgentQLearning()
        Dim environment As Gridworld = createSimpleGridworld()

        Dim agent As New QLearning("GridworldAgent", 0.1, 0.2, 0.85, environment)

        agent.restartAgent()
        agent.run(rlEpisodes)

        Dim b As String = ""

        For i As Integer = 0 To agent.stepsOfEpisode.Count - 1
            b = b + vbCrLf + agent.stepsOfEpisode.Item(i).ToString
        Next

        Dim steps As New List(Of Double)
        Dim acumulatedSteps As Double = 0
        Dim currentEpisode = 0

        For Each stepOfEpisode In agent.stepsOfEpisode
            acumulatedSteps += stepOfEpisode
            currentEpisode += 1
            steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
        Next


        'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

        ResultSaver.saveAgentResultsAsFile(agent)

    End Sub

    Public Sub testAgentDynaQ()
        Dim environment As Gridworld = createSimpleGridworld()

        Dim agent As New DynaQ("GridworldAgent", 0.1, 0.2, 0.85, 20, environment)

        agent.restartAgent()
        agent.run(rlEpisodes)

        Dim b As String = ""

        For i As Integer = 0 To agent.stepsOfEpisode.Count - 1
            b = b + vbCrLf + agent.stepsOfEpisode.Item(i).ToString
        Next

        Dim steps As New List(Of Double)
        Dim acumulatedSteps As Double = 0
        Dim currentEpisode = 0

        For Each stepOfEpisode In agent.stepsOfEpisode
            acumulatedSteps += stepOfEpisode
            currentEpisode += 1
            steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
        Next


        'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

        ResultSaver.saveAgentResultsAsFile(agent)

    End Sub

    Public Sub testAgentDynaH()
        Dim environment As Gridworld = createSimpleGridworld()

        Dim agent As New DynaH("GridworldAgent", 0.1, 0.2, 0.85, 20, environment)

        agent.restartAgent()
        agent.run(rlEpisodes)

        Dim b As String = ""

        For i As Integer = 0 To agent.stepsOfEpisode.Count - 1
            b = b + vbCrLf + agent.stepsOfEpisode.Item(i).ToString
        Next

        Dim steps As New List(Of Double)
        Dim acumulatedSteps As Double = 0
        Dim currentEpisode = 0

        For Each stepOfEpisode In agent.stepsOfEpisode
            acumulatedSteps += stepOfEpisode
            currentEpisode += 1
            steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
        Next


        'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

        ResultSaver.saveAgentResultsAsFile(agent)

    End Sub

    Public Sub testAgentDynaQPlus()
        Dim environment As Gridworld = createSimpleGridworld()
        Dim metaEpisodes As New List(Of List(Of Integer))

        Dim agent As New DynaQPlus("GridworldAgent", 0.1, 0.2, 0.85, 20, 0.0001, environment)
        agent.restartAgent()

        For i As Integer = 0 To rlMetaEpisodes - 1
            agent.restartAgent()
            agent.run(rlEpisodes)
            metaEpisodes.Add(agent.stepsOfEpisode)
        Next

        Dim b As String = ""

        For i As Integer = 0 To agent.stepsOfEpisode.Count - 1
            b = b + vbCrLf + agent.stepsOfEpisode.Item(i).ToString
        Next

        Dim steps As New List(Of Double)
        Dim acumulatedSteps As Double = 0
        Dim currentEpisode = 0

        For Each stepOfEpisode In agent.stepsOfEpisode
            acumulatedSteps += stepOfEpisode
            currentEpisode += 1
            steps.Add((stepOfEpisode + acumulatedSteps) / currentEpisode)
        Next


        'MsgBox(agent.valueMatrix.ToString(1000000, 10000000))

        ResultSaver.saveAgentResultsAsFile(agent)
    End Sub



    Private Shared Function createSimpleGridworld() As Gridworld
        Dim environment As New Gridworld(4, 6)

        environment.setInitialState(0, 0)
        environment.setFinalState(3, 5)
        environment.setStateReward(3, 5, 1)

        environment.disableState(2, 0, True)
        environment.disableState(2, 1, True)
        environment.disableState(2, 2, True)
        environment.disableState(2, 3, True)
        environment.disableState(2, 4, True)
        environment.disableStateOnCondition(2, 5, 200)
        environment.enableStateOnCondition(2, 0, 200)
        Return environment
    End Function
End Class
