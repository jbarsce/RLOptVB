
Imports MathNet.Numerics.LinearAlgebra

Public Class TestBayesianOptimizer
    Private Const BAYESIAN_OPTIMIZER_RUNS As Integer = 30


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

    ''' <summary>
    ''' Creates a blocking maze environment
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function createABlockingMazeChangedReward() As Gridworld
        Dim environment As New Gridworld(4, 6)

        environment.setInitialState(0, 0)
        environment.setFinalState(3, 5)
        environment.setFinalState(0, 4)
        environment.setStateReward(3, 5, 1)

        environment.disableState(2, 0, True)
        environment.disableState(2, 1, True)
        environment.disableState(2, 2, True)
        environment.disableState(2, 3, True)
        environment.disableState(2, 4, True)
        environment.disableStateOnCondition(2, 5, 400)
        environment.enableStateOnCondition(2, 0, 400)
        Return environment
    End Function


    Public Sub testBayesianOptimizerStepsPerEpisodeMeasureAndUCB1()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="UCB1")
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for steps-per-episode f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerStepsPerEpisodeMeasureAndUCB1Tuned()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="UCB1Tuned")
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    Public Sub testBayesianOptimizerStepsPerEpisodeMeasureAndUCB1Variance()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="UCB1Variance")
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerSuccessMeasureAndUCB1Tuned()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="UCB1Tuned")
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerSuccessMeasureAndUCB1()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="UCB1")
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerSuccessMeasureAndGreedy()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="EpsilonGreedy",
                                                           nextBOQueryDecisionFunctionParameter:=0)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerStepPerEpisodeMeasureAndSoftmax()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="Softmax", nextBOQueryDecisionFunctionParameter:=1)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerSuccessMeasureAndEGreedy()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="EpsilonGreedy",
                                                           nextBOQueryDecisionFunctionParameter:=0.2)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerStepPerEpisodeMeasureAndGreedy()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="EpsilonGreedy",
                                                           nextBOQueryDecisionFunctionParameter:=0)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned
    ''' </summary>
    Public Sub testBayesianOptimizerStepPerEpisodeMeasureAndEpsilonGreedy()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="EpsilonGreedy",
                                                           nextBOQueryDecisionFunctionParameter:=0.2)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer meta-learning test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned.
    ''' This example initially starts from a blocking gridworld and then reuses the bandits training in the double-blocking gridworld.
    ''' </summary>
    Public Sub testBayesianOptimizerSuccessMeasureAndSoftmaxMLSmallGToDBG()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1

            ' Blocking gridworld environment

            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createABlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 100

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 3

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="Softmax")
            bayesianOptimizer.runOptimizer(10) ' The small environment is run 10 times

            Dim observedY = bayesianOptimizer.observedY

            runDoubleBlockingGridworldBO(observedY)
        Next
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer meta-learning test for success measured f(X) and EI acquisition function and contextual bandits with UCB1-Tuned.
    ''' This example initially starts from a blocking gridworld and then reuses the bandits training in the double-blocking gridworld.
    ''' </summary>
    Public Sub testBayesianOptimizerSuccessMeasureAndSoftmaxMLSameSmallGToDBG()

        ' Blocking gridworld environment
        Dim environment As Gridworld = createABlockingMaze()

        Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
        agent.cutoffTime = 100

        Dim initialM As Double = 0
        Dim sigmaSquaredF As Double = 0.8
        Dim sigmaSquaredN As Double = 0.17
        Dim numberOfAgentEpisodes As Integer = 50

        Dim acqEpsilon As Double = 0
        Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

        Dim directIterations As Integer = 10
        Dim includeGPInHyperparameters As Boolean = False
        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
        Dim covarianceFunction As New SquaredExponentialARD()
        Dim numberOfLHS As Integer = 0
        Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
        Dim logLikelihoodOptimizationFunction = Nothing
        Dim rlRunsPerQueryPoint As Integer = 3

        Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="Softmax")
        bayesianOptimizer.runOptimizer(30) ' The small environment is run 10 times

        Dim observedY = bayesianOptimizer.observedY

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1

            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")

            runDoubleBlockingGridworldBO(observedY)
        Next
    End Sub


    Public Sub testBayesianOptimizerSuccessMeasureAndSoftmaxMLSameSmallGToDBGTransferGoodX()

        ResultSaver.registerStartTime()
        ' Blocking gridworld environment
        Dim environment As Gridworld = createABlockingMaze()

        Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
        agent.cutoffTime = 100

        Dim initialM As Double = 0
        Dim sigmaSquaredF As Double = 0.8
        Dim sigmaSquaredN As Double = 0.17
        Dim numberOfAgentEpisodes As Integer = 50

        Dim acqEpsilon As Double = 0
        Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

        Dim directIterations As Integer = 10
        Dim includeGPInHyperparameters As Boolean = False
        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
        Dim covarianceFunction As New SquaredExponentialARD()
        Dim numberOfLHS As Integer = 0
        Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
        Dim logLikelihoodOptimizationFunction = Nothing
        Dim rlRunsPerQueryPoint As Integer = 3

        Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="Softmax")
        bayesianOptimizer.runOptimizer(30) ' The small environment is run 10 times

        Dim observedY = bayesianOptimizer.observedY
        Dim queriedX = bayesianOptimizer.queriedX

        ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1

            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")

            runDoubleBlockingGridworldBO(observedY, queriedX)
        Next
    End Sub


    Public Sub testBayesianOptimizerSuccessMeasureAndSoftmaxMLSameSmallGToDBGTransferWhereBestYIsPriorMean()

        ResultSaver.registerStartTime()
        ' Blocking gridworld environment
        Dim environment As Gridworld = createABlockingMaze()

        Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
        agent.cutoffTime = 100

        Dim initialM As Double = 0
        Dim sigmaSquaredF As Double = 0.8
        Dim sigmaSquaredN As Double = 0.17
        Dim numberOfAgentEpisodes As Integer = 50

        Dim acqEpsilon As Double = 0
        Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

        Dim directIterations As Integer = 10
        Dim includeGPInHyperparameters As Boolean = False
        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
        Dim covarianceFunction As New SquaredExponentialARD()
        Dim numberOfLHS As Integer = 0
        Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
        Dim logLikelihoodOptimizationFunction = Nothing
        Dim rlRunsPerQueryPoint As Integer = 3

        Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
                isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="Softmax")
        bayesianOptimizer.runOptimizer(30) ' The small environment is run 10 times

        Dim observedY = bayesianOptimizer.observedY
        Dim queriedX = bayesianOptimizer.queriedX

        ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1

            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")

            runDoubleBlockingGridworldBO(observedY, queriedX)
        Next
    End Sub


    Public Sub runDoubleBlockingGridworldBO(Optional similarTaskObservedY As Vector(Of Double) = Nothing,
                                            Optional similarTaskQueriedX As List(Of Vector(Of Double)) = Nothing)
        Dim environment As Gridworld = createADoubleBlockingMaze()

        Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
        agent.cutoffTime = 400

        Dim initialM As Double = similarTaskObservedY.Maximum
        Dim sigmaSquaredF As Double = 0.8
        Dim sigmaSquaredN As Double = 0.17
        Dim numberOfAgentEpisodes As Integer = 50

        Dim acqEpsilon As Double = 0
        Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

        Dim directIterations As Integer = 10
        Dim includeGPInHyperparameters As Boolean = False
        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
        Dim covarianceFunction As New SquaredExponentialARD()
        Dim numberOfLHS As Integer = 0
        Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
        Dim logLikelihoodOptimizationFunction = Nothing
        Dim rlRunsPerQueryPoint As Integer = 5

        Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
            initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint,
            isNextQueryDecided:=True, minimumRLRunsPerQueryPoint:=2, nextBOQueryDecisionFunction:="Softmax", similarTaskObservedY:=similarTaskObservedY)

        bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

        ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
    End Sub


    ''' <summary>
    ''' A fully functional bayesian optimizer test for success measured f(X) and EI acquisition function
    ''' </summary>
    Public Sub testBayesianOptimizerWithSuccessMeasure()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambdaSuccessMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub

    ''' <summary>
    ''' A fully functional bayesian optimizer test for steps-per-episode f(X) and EI acquisition function
    ''' </summary>
    Public Sub testBayesianOptimizerStepsPerEpisodeMeasure()

        'Number of Bayesian Optimizer runs
        Const BAYESIAN_OPTIMIZER_INSTANCES As Integer = 10

        For i As Integer = 0 To BAYESIAN_OPTIMIZER_INSTANCES - 1
            ResultSaver.registerStartTime()
            ResultSaver.registerOptimizerType("BayesianOptimizer")
            Dim environment As Gridworld = createADoubleBlockingMaze()

            Dim agent As New SarsaLambda("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)
            agent.cutoffTime = 400

            Dim initialM As Double = 0
            Dim sigmaSquaredF As Double = 0.8
            Dim sigmaSquaredN As Double = 0.17
            Dim numberOfAgentEpisodes As Integer = 50

            Dim acqEpsilon As Double = 0
            Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

            Dim directIterations As Integer = 10
            Dim includeGPInHyperparameters As Boolean = False
            Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, -0.124217096829483)
            Dim covarianceFunction As New SquaredExponentialARD()
            Dim numberOfLHS As Integer = 0
            Dim acquisitionOptimizationFunction As New LatinHypercubeOptimization(directIterations)
            Dim logLikelihoodOptimizationFunction = Nothing
            Dim rlRunsPerQueryPoint As Integer = 5

            Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
                initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, numberOfLHS, rlRunsPerQueryPoint)
            bayesianOptimizer.runOptimizer(BAYESIAN_OPTIMIZER_RUNS)

            ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
        Next
    End Sub

    ''' <summary>
    ''' Dummy Bayesian Optimizer acquisition function test case in order to test the optimization of the acquisition function (e.g. to test the DIRECT algorithm). 
    ''' The best X found in the acquisition function should be that whose approaches the most to the dummy acq function. 
    ''' </summary>
    Public Sub testBayesianOptimizerTrivialAcquisition()
        Dim environment As Gridworld = createABlockingMaze()

        Dim agent As New SarsaLambdaAgentOnlyTwoParameters("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)

        Dim initialM As Double = 0
        Dim sigmaSquaredF As Double = 1
        Dim sigmaSquaredN As Double = 1
        Dim numberOfAgentEpisodes As Integer = 100

        Dim acqEpsilon As Double = 0
        Dim acquisitionFunction As New AcquisitionFunctionTrivial()

        Dim directIterations As Integer = 10
        Dim dividingRectanglesEpsilon As Double = 0
        Dim dividingRectanglesProbabilityToExplore As Double = 0
        Dim includeGPInHyperparameters As Boolean = False
        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, 0.5)
        Dim covarianceFunction As New SquaredExponentialIsotropic()
        Dim acquisitionOptimizationFunction As New DividingRectangles(directIterations, dividingRectanglesEpsilon)
        Dim logLikelihoodOptimizationFunction = Nothing
        Dim rlRunsPerQueryPoint As Integer = 1

        Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
            initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, 0, rlRunsPerQueryPoint)
        bayesianOptimizer.runOptimizer(100)

        ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)
    End Sub

    ''' <summary>
    ''' Test case that uses a very simple agent measure (i.e. f(X) the agent returns) in order to test if the GP convergence is correct
    ''' </summary>
    Public Sub testBayesianOptimizerTestAgentMeasure()

        Dim environment As Gridworld = createABlockingMaze()

        Dim agent As New SarsaLambdaAgentSimpleMeasure("GridworldAgent", 0.1, 0.25, 0.85, 0.5, environment)

        Dim initialM As Double = 0
        Dim sigmaSquaredF As Double = 1
        Dim sigmaSquaredN As Double = 1
        Dim numberOfAgentEpisodes As Integer = 100

        Dim acqEpsilon As Double = 0
        Dim acquisitionFunction As New ExpectedImprovement(acqEpsilon)

        Dim directIterations As Integer = 3
        Dim dividingRectanglesEpsilon As Double = 0
        Dim dividingRectanglesProbabilityToExplore As Double = 0
        Dim includeGPInHyperparameters As Boolean = False
        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(4, 0.5)
        Dim covarianceFunction As New SquaredExponentialIsotropic()
        Dim acquisitionOptimizationFunction As New DividingRectangles(directIterations, dividingRectanglesEpsilon)
        Dim logLikelihoodOptimizationFunction = Nothing
        Dim rlRunsPerQueryPoint As Integer = 1

        Dim bayesianOptimizer As New BayesianOptimizer(agent, acquisitionFunction, acquisitionOptimizationFunction, covarianceFunction, lVector, logLikelihoodOptimizationFunction,
            initialM, sigmaSquaredF, sigmaSquaredN, numberOfAgentEpisodes, 0, rlRunsPerQueryPoint)
        bayesianOptimizer.runOptimizer(100)

        ResultSaver.saveOptimizerResultsAsFile(numberOfAgentEpisodes, bayesianOptimizer)

    End Sub

End Class
