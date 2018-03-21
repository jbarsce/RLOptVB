Imports MathNet.Numerics.LinearAlgebra

Public Class QLambda : Inherits TDAgent

    'Parameters of the agent
    Protected Property alpha As Double
    Protected Property gamma As Double
    Protected Property lambda As Double

    Private Const numberOfHyperparameters As Integer = 4

    'Complementary matrix
    Public Property zValues As New List(Of ZValue)

    ''' <summary>
    ''' Initializes the SarsaLambda agent by setting an e-greedy policy
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="epsilon"></param>
    ''' <param name="environment"></param>
    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, lambda As Double, environment As Gridworld, Optional cutoffTime As Double = -1,
                   Optional selectedPolicy As Integer = EGREEDY)
        MyBase.New(name, environment, cutoffTime)

        Me.alpha = alpha
        explorationRate = epsilon
        Me.gamma = gamma
        Me.lambda = lambda

        MyBase.selectedPolicy = selectedPolicy

        For Each transition In environment.transitions
            'the current setting assumes deterministic environment (for probabilistic environment, it must be chequed that a pair (s,a) is not added twice)
            Dim qValue As New QValue(transition.stateFrom, transition.action)
            Dim zValue As New ZValue(transition.stateFrom, transition.action)

            qValues.Add(qValue)
            zValues.Add(zValue)
        Next
    End Sub

    Public Overrides Sub run(episodes As Integer)

        Dim s As State
        Dim a As Action
        Dim sPlus As State
        Dim aPlus As Action
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
            a = nextActionFromPolicy()

            Do While Not environment.actualState.isFinal

                environment.takeAction(a, episode)
                sPlus = environment.actualState
                r = sPlus.reward
                aPlus = nextActionFromPolicy()

                rewardOfEpisode += r
                Dim delta As Double = 0

                'if sPlus is a final state, then its q(s,a) is considered as zero.

                delta = r + gamma * getQPair(sPlus, aPlus).qValue - getQPair(s, a).qValue

                getZValue(s, a).zValue = 1

                For Each qPair In qValues
                    qPair.qValue = qPair.qValue + alpha * delta * getZValue(qPair.state, qPair.action).zValue
                Next

                For Each zPair In zValues
                    If Not zPair.state.Equals(s) Then
                        zPair.zValue = gamma * lambda * getZValue(zPair.state, zPair.action).zValue
                    End If
                Next

                s = sPlus
                a = aPlus
                stepsOfEpisode = stepsOfEpisode + 1

                'If cutoff time is reached, agent's current episode is terminated even if the final state has been reached
                If stepsOfEpisode = cutoffTime Then
                    Exit Do
                End If
            Loop

            Me.stepsOfEpisode.Add(stepsOfEpisode)
            Me.rewardOfEpisode.Add(rewardOfEpisode)

            'If the last state of the current episode is the final state, then the episode is added as successful; otherwise it is computed as a failure.
            If environment.actualState.isFinal Then
                sucessOfEpisode.Add(True)
            Else
                sucessOfEpisode.Add(False)
            End If
        Next

        calculateValueMatrix()
    End Sub

    Private Function getZValue(s As State, a As Action) As ZValue
        For Each zValue In zValues
            If zValue.state.Equals(s) And zValue.action.Equals(a) Then
                Return zValue
            End If
        Next
        Return Nothing
    End Function

    Public Overrides Function getHyperparameterCount() As Integer
        Return numberOfHyperparameters
    End Function

    Public Overrides Sub setNewConfiguration(hyperparameters As Vector(Of Double))
        alpha = hyperparameters.Item(0)
        explorationRate = hyperparameters.Item(1)
        gamma = hyperparameters.Item(2)
        lambda = hyperparameters.Item(3)
    End Sub

    Public Overrides Sub restartAgent()
        qValues.Clear()
        zValues.Clear()
        valueMatrix.Clear()
        environment.restartEnvironment(True)

        For Each transition In environment.transitions
            'the current setting assumes deterministic environment (for probabilistic environment, it must be chequed that a pair (s,a) is not added twice)
            Dim qValue As New QValue(transition.stateFrom, transition.action)
            Dim zValue As New ZValue(transition.stateFrom, transition.action)

            qValues.Add(qValue)
            zValues.Add(zValue)
        Next
    End Sub

    Public Overrides Function getStepsOfEpisode() As List(Of Integer)
        Return stepsOfEpisode
    End Function

    Public Overrides Function getRewardOfEpisode() As List(Of Double)
        Return rewardOfEpisode
    End Function

    Public Overrides Function getSuccessOfEpisode() As List(Of Boolean)
        Return sucessOfEpisode
    End Function
End Class
