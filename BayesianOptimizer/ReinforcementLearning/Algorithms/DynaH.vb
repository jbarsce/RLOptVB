Imports MathNet.Numerics.LinearAlgebra

Public Class DynaH : Inherits TDAgent

    'Parameters of the agent
    Protected Property alpha As Double
    'Exploration rate is known as epsilon in e-greedy and thau in softmax policies
    Protected Property gamma As Double
    'Model(S,A) is a matrix structure that contains a 
    Protected Property model As New List(Of ModelStateAction)

    Protected Property n As Integer

    Private Const numberOfHyperparameters As Integer = 4

    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, n As Integer, environment As Gridworld, Optional cutoffTime As Double = -1,
                   Optional selectedPolicy As Integer = EGREEDY)
        MyBase.New(name, environment, cutoffTime)

        Me.alpha = alpha
        explorationRate = epsilon
        Me.gamma = gamma
        Me.n = n

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

                For j As Integer = 0 To n - 1
                    Dim sPrev As State = s
                    Dim aPrev As Action = getMaximumDistanceAction(s)

                    If aPrev Is Nothing Then
                        sPrev = getRandomPreviousVisitedState()
                        aPrev = getRandomPreviouslyTakenAction(sPrev)
                    Else
                        If Not getModelStateAction(sPrev, aPrev).visited Then
                            sPrev = getRandomPreviousVisitedState()
                            aPrev = getRandomPreviouslyTakenAction(sPrev)
                        End If
                    End If

                    Dim prevModelSA = getModelStateAction(sPrev, aPrev)
                    Dim rPrev As Double = prevModelSA.reward
                    Dim sPlusPrev As State = prevModelSA.sPlus

                    aMax = determineBestAction(sPrev)
                    getQPair(sPrev, aPrev).qValue = getQPair(sPrev, aPrev).qValue + alpha * (rPrev + gamma * getQPair(sPlusPrev, aMax).qValue - getQPair(sPrev, aPrev).qValue)
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

    ''' <summary>
    ''' Hypothesis used in Dyna-H: gets the action whom next state are farthest from the goal, starting in state s.
    ''' Such next state is taken from the model. If no next state has been experienced i.e. there is no sPlus for
    ''' all the available actions, returns Nothing.
    ''' </summary>
    ''' <param name="state"></param>
    ''' <returns></returns>
    Private Function getMaximumDistanceAction(state As State) As Action
        Dim maximumDistanceActions As New List(Of Action)
        Dim maximumDistance As Double

        For Each action In environment.actions
            Dim modelStateAction As ModelStateAction = getModelStateAction(state, action)
            If modelStateAction.sPlus IsNot Nothing Then
                If maximumDistanceActions.Count = 0 Then
                    maximumDistanceActions.Add(action)
                    maximumDistance = calculateDistanceFromGoal(getModelStateAction(state, maximumDistanceActions.Item(0)).sPlus)
                Else
                    Dim actionDistance As Double = calculateDistanceFromGoal(getModelStateAction(state, action).sPlus)
                    If actionDistance > maximumDistance Then
                        maximumDistanceActions.Clear()
                        maximumDistanceActions.Add(action)
                    Else
                        If actionDistance = maximumDistance Then
                            maximumDistanceActions.Add(action)
                        End If
                    End If
                End If
            End If
        Next

        If maximumDistanceActions.Count > 0 Then
            Return maximumDistanceActions.Item(randomGenerator.Next(0, maximumDistanceActions.Count))
        Else
            Return Nothing
        End If
    End Function

    Private Function calculateDistanceFromGoal(s As State) As Double
        Dim sPoint As Vector(Of Double) = Vector(Of Double).Build.Dense(2, 0)
        Dim goalPoint As Vector(Of Double) = Vector(Of Double).Build.Dense(2, 0)

        sPoint.Item(0) = environment.getStateRow(s)
        sPoint.Item(1) = environment.getStateColumn(s)

        Dim goalState As State = getGoalState()
        goalPoint.Item(0) = environment.getStateRow(goalState)
        goalPoint.Item(1) = environment.getStateColumn(goalState)

        Return (sPoint - goalPoint).SumMagnitudes ^ 2
    End Function

    Private Function getGoalState() As State
        For Each transition In environment.transitions
            If transition.stateTo.isFinal Then
                Return transition.stateTo
            End If
        Next

        Return Nothing
    End Function

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
    End Sub

    Public Overrides Function getHyperparameterCount() As Integer
        Return numberOfHyperparameters
    End Function
End Class
