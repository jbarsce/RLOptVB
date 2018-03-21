Imports MathNet.Numerics.LinearAlgebra

Public Class DynaQPlus : Inherits TDAgent

    'Parameters of the agent
    Protected Property alpha As Double
    'Exploration rate is known as epsilon in e-greedy and thau in softmax policies
    Protected Property gamma As Double
    'Model(S,A) is a matrix structure that contains a 
    Protected Property model As New List(Of ModelStateAction)

    Protected Property n As Integer
    Protected Property k As Double

    Private Const numberOfHyperparameters As Integer = 5

    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, n As Integer, k As Double, environment As Gridworld, Optional cutoffTime As Double = -1,
                   Optional selectedPolicy As Integer = EGREEDY)
        MyBase.New(name, environment, cutoffTime)

        Me.alpha = alpha
        explorationRate = epsilon
        Me.gamma = gamma
        Me.n = n
        Me.k = k

        For Each transition In environment.transitions
            'the current setting assumes deterministic environment (for probabilistic environment, it must be chequed that a pair (s,a) is not added twice)
            Dim qValue As New QValue(transition.stateFrom, transition.action)

            qValues.Add(qValue)

            'Model is initialized
            Dim modelSA As New ModelStateAction(transition.stateFrom, transition.action)
            model.Add(modelSA)
        Next

        MyBase.selectedPolicy = selectedPolicy
    End Sub

    Public Overrides Sub run(episodes As Integer)
        Dim s As State
        Dim a As Action
        Dim sPlus As State
        Dim r As Double
        stepsOfEpisode = New List(Of Integer)
        rewardOfEpisode = New List(Of Double)
        sucessOfEpisode = New List(Of Boolean)
        Dim listOfStepsPerEpisode As New List(Of Integer)

        For episode As Integer = 0 To episodes - 1
            Dim stepsOfEpisode As Integer = 0
            Dim rewardOfEpisode As Double = 0

            'Initializes the episode
            environment.restartEnvironment()

            s = environment.actualState

            Do While Not environment.actualState.isFinal

                a = nextActionFromPolicy()

                environment.takeAction(a, episode)
                sPlus = environment.actualState
                r = sPlus.reward

                rewardOfEpisode += r

                Dim aMax As Action = determineBestAction(s)

                'if sPlus is a final state, then its q(s,a) is considered as zero.
                getQPair(s, a).qValue = getQPair(s, a).qValue + alpha * (r + gamma * getQPair(sPlus, aMax).qValue - getQPair(s, a).qValue)

                Dim modelSA = getModelStateAction(s, a)
                modelSA.sPlus = sPlus
                modelSA.reward = r
                modelSA.visited = True

                updateTimeStepsSinceLastTransition(s, a)

                For j As Integer = 0 To n - 1
                    Dim sPrev As State = getRandomPreviousVisitedState()
                    Dim aPrev As Action = getRandomPreviouslyTakenAction(sPrev)

                    Dim thau = findTimeStepsSinceLastTransition(sPrev, aPrev)
                    Dim prevModelSA = getModelStateAction(sPrev, aPrev)
                    Dim rPrev = prevModelSA.reward
                    Dim sPlusPrev As State = prevModelSA.sPlus

                    aMax = determineBestAction(sPrev)
                    getQPair(sPrev, aPrev).qValue = getQPair(sPrev, aPrev).qValue + alpha * (rPrev + k * Math.Sqrt(thau) + gamma * getQPair(sPlusPrev, aMax).qValue - getQPair(sPrev, aPrev).qValue)
                Next

                s = sPlus
                stepsOfEpisode = stepsOfEpisode + 1

                'If cutoff time is reached, agent's current episode is terminated even if the final state has been reached
                If stepsOfEpisode = cutoffTime Then
                    Exit Do
                End If
            Loop

            Me.stepsOfEpisode.Add(stepsOfEpisode)
            Me.rewardOfEpisode.Add(rewardOfEpisode)

            'If the last state of the current episode is the final state, then the episode is added as successful; otherwise it is computed as a failure.
            'It does not support non-episodic tasks for the moment.
            If environment.actualState.isFinal Then
                sucessOfEpisode.Add(True)
            Else
                sucessOfEpisode.Add(False)
            End If
        Next

        calculateValueMatrix()
    End Sub

    Private Function findTimeStepsSinceLastTransition(s As State, a As Action) As Double
        For Each transition In environment.transitions
            If transition.stateFrom.Equals(s) And transition.action.Equals(a) Then
                Return transition.timeStepsSinceLastTry
            End If
        Next

        Return Nothing
    End Function

    Private Sub updateTimeStepsSinceLastTransition(s As State, a As Action)
        'All the transitions but the last tried (s,a) pair are increased by one
        For Each transition In environment.transitions
            If transition.stateFrom.Equals(s) And transition.action.Equals(a) Then
                transition.timeStepsSinceLastTry = 0
            Else
                transition.timeStepsSinceLastTry += 1
            End If
        Next
    End Sub

    Private Function getModelStateAction(s As State, a As Action) As ModelStateAction
        For Each modelSA In model
            If modelSA.state.Equals(s) And modelSA.action.Equals(a) Then
                Return modelSA
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Gets a random previously visited, non final state.
    ''' </summary>
    ''' <returns></returns>
    Private Function getRandomPreviousVisitedState() As State
        Dim listOfVisitedStates As New List(Of State)

        For Each modelSA In model
            If modelSA.visited And Not listOfVisitedStates.Contains(modelSA.state) And Not modelSA.state.isFinal Then
                listOfVisitedStates.Add(modelSA.state)
            End If
        Next

        Return listOfVisitedStates.Item(randomGenerator.Next(0, listOfVisitedStates.Count))
    End Function

    Private Function getRandomPreviouslyTakenAction(s As State) As Action
        Dim listOfActionsTakenInS As New List(Of Action)

        For Each modelSA In model
            If modelSA.state.Equals(s) And modelSA.visited And Not listOfActionsTakenInS.Contains(modelSA.action) Then
                listOfActionsTakenInS.Add(modelSA.action)
            End If
        Next

        Return listOfActionsTakenInS.Item(randomGenerator.Next(0, listOfActionsTakenInS.Count))
    End Function

    Public Overrides Sub restartAgent()
        qValues.Clear()
        valueMatrix.Clear()
        environment.restartEnvironment(True)

        For Each transition In environment.transitions
            'the current setting assumes deterministic environment (for probabilistic environment, it must be chequed that a pair (s,a) is not added twice)
            Dim qValue As New QValue(transition.stateFrom, transition.action)

            qValues.Add(qValue)
        Next
    End Sub

    Public Overrides Sub setNewConfiguration(hyperparameters As Vector(Of Double))
        alpha = hyperparameters.Item(0)
        explorationRate = hyperparameters.Item(1)
        gamma = hyperparameters.Item(2)
        n = hyperparameters.Item(3)
        k = hyperparameters.Item(4)
    End Sub

    Public Overrides Function getHyperparameterCount() As Integer
        Return numberOfHyperparameters
    End Function
End Class
