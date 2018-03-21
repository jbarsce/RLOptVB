

Public Class TestRandomSearch

    Private Const OPTIMIZER_RUNS As Integer = 30

    Public Sub randomSearchOptimizerWithSuccessMeasure()

        'Number of Bayesian Optimizer runs
        Const OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("RandomSearch")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400
            Dim numberOfAgentEpisodes As Integer = 50

            Dim randomSearchOptimizer As New RandomSearchOptimizer(agent)
            randomSearchOptimizer.runOptimizer(OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, randomSearchOptimizer)
        Next
    End Sub


    Public Sub randomSearchOptimizerStepsPerEpisodeMeasure()

        'Number of Bayesian Optimizer runs
        Const OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("RandomSearch")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400
            Dim numberOfAgentEpisodes As Integer = 50

            Dim randomSearchOptimizer As New RandomSearchOptimizer(agent)
            randomSearchOptimizer.runOptimizer(OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, randomSearchOptimizer)
        Next
    End Sub



    ''' <summary>
    ''' Creates a blocking maze environment, such as Sutton and Barto, 1998
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function createABlockingMaze() As Gridworld
        Dim environment As New Gridworld(6, 9)
        Dim timeOfFirstBlock As Integer = 25

        environment.setInitialState(0, 8)
        environment.setFinalState(5, 3)
        environment.setStateReward(5, 3, 1)

        environment.disableState(3, 0, True)
        environment.disableState(3, 1, True)
        environment.disableState(3, 2, True)
        environment.disableState(3, 3, True)
        environment.disableState(3, 4, True)
        environment.disableState(3, 5, True)
        environment.disableState(3, 6, True)
        environment.disableState(3, 7, True)
        environment.disableStateOnCondition(3, 8, timeOfFirstBlock)
        environment.enableStateOnCondition(3, 0, timeOfFirstBlock)
        Return environment
    End Function

    ''' <summary>
    ''' Complex blocking maze with two blocking stages: the first block happens after the 15th episode; the optimal amount of steps from S to G
    ''' is unchanged at 17. The second block happens in the 30th episode and blocks the upper maze at the time that unlocks a state to enable the lower maze.
    ''' The idea of this test is to test the agent in a setting where a very low epsilon can (hypotetically) be worse.
    ''' 
    ''' A possible modification is to later change the final state.
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function createADoubleBlockingMaze() As Gridworld
        Dim environment As New Gridworld(10, 10)
        Dim timeOfFirstBlock As Integer = 15
        Dim timeOfSecondBlock As Integer = 30

        environment.setInitialState(2, 0)
        environment.setFinalState(0, 9)
        environment.setStateReward(0, 9, 1)

        'Initially disabled states
        environment.disableState(0, 0, True)
        environment.disableState(8, 0, True)
        environment.disableState(1, 1, True)
        environment.disableState(2, 1, True)
        environment.disableState(3, 1, True)
        environment.disableState(6, 1, True)
        environment.disableState(9, 2, True)
        environment.disableState(1, 3, True)
        environment.disableState(8, 3, True)
        environment.disableState(4, 4, True)
        environment.disableState(7, 4, True)
        environment.disableState(3, 5, True)
        environment.disableState(5, 5, True)
        environment.disableState(8, 6, True)
        environment.disableState(5, 7, True)
        environment.disableState(6, 7, True)
        environment.disableState(7, 7, True)
        environment.disableState(0, 8, True)
        environment.disableState(1, 8, True)
        environment.disableState(2, 8, True)
        environment.disableState(4, 8, True)
        environment.disableState(7, 8, True)
        environment.disableState(9, 8, True)
        environment.disableState(4, 9, True)

        'First block
        environment.enableStateOnCondition(0, 0, timeOfFirstBlock)
        environment.disableStateOnCondition(4, 0, timeOfFirstBlock)

        'Second block
        environment.disableStateOnCondition(2, 7, timeOfSecondBlock)
        environment.disableStateOnCondition(3, 7, timeOfSecondBlock)
        environment.disableStateOnCondition(4, 7, timeOfSecondBlock)
        environment.enableStateOnCondition(4, 8, timeOfSecondBlock)

        Return environment
    End Function

End Class
