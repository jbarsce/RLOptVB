Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Class that implements the generic behavior of a Temporal-Difference Reinforcement Learning Algorithm
''' </summary>
Public MustInherit Class TDAgent : Inherits Agent

    'Matrix qπ and vπ
    Public Property qValues As New List(Of QValue)
    Public Property valueMatrix As Matrix(Of Double)
    'Exploration rate is known as epsilon in e-greedy and thau in softmax policies
    Protected Property explorationRate As Double

    'Selected agent policy
    Protected Property selectedPolicy As Integer
    Protected Const EGREEDY As Integer = 0
    'SOFTMAX is not yet implemented
    Protected Const SOFTMAX As Integer = 1

    ''Measure of the last run
    'Number of steps the agent performed in each episode
    Public Property stepsOfEpisode As New List(Of Integer)
    'List of boolean values that indicates which episodes were successful
    Public Property sucessOfEpisode As New List(Of Boolean)
    'List of the rewards received in every episode
    Public Property rewardOfEpisode As New List(Of Double)

    'Random generator, to use the same seed for the duration of the run
    Protected Property randomGenerator = UtilityFunctions.randomGenerator

    Public Sub New(name As String, environment As Gridworld, Optional cutoffTime As Double = -1)
        MyBase.New(name, environment, cutoffTime)

        valueMatrix = CreateMatrix.Dense(Of Double)(environment.numberOfRows, environment.numberOfColumns)
    End Sub

    ''' <summary>
    ''' Returns the expected sucess of each episode.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function getLastRunMeasure() As Double
        Dim totalSteps As Double = 0

        For Each [step] In stepsOfEpisode
            totalSteps += [step]
        Next

        Return -totalSteps / stepsOfEpisode.Count
    End Function

    Public Overrides Function getRewardOfEpisode() As List(Of Double)
        Return rewardOfEpisode
    End Function

    Public Overrides Function getStepsOfEpisode() As List(Of Integer)
        Return stepsOfEpisode
    End Function

    Public Overrides Function getSuccessOfEpisode() As List(Of Boolean)
        Return sucessOfEpisode
    End Function

    ''' <summary>
    ''' Takes a new action from the chosen policy and under the current q values.
    ''' </summary>
    ''' <returns></returns>
    Protected Function nextActionFromPolicy() As Action
        If selectedPolicy.Equals(EGREEDY) Then

            Dim randomNumberOfEpsilon As Double = randomGenerator.NextDouble()

            'Exploratory action is chosen
            If randomNumberOfEpsilon < explorationRate Then
                Dim randomNumberOfAction As Integer = randomGenerator.Next(0, environment.actions.Count)
                Return environment.actions.Item(randomNumberOfAction)
            Else
                'Greedy action is instead chosen

                Dim greedyAction As Action
                greedyAction = determineBestAction(environment.actualState)

                Return greedyAction
            End If
        End If

        If selectedPolicy.Equals(SOFTMAX) Then
            'Note that thau is unsquished (i.e. it is transformed from [0,1] to [0, Infinity])
            Dim temperature As Double = Math.Sqrt(2) * MathNet.Numerics.SpecialFunctions.ErfInv(2 * (explorationRate + 1) / 2 - 1)

            'The sum of the Softmax denominator is computed
            Dim totalActionNormalization As Double = 0
            Dim accumulatedActionProbability As Double = 0

            For Each action In environment.actions
                totalActionNormalization = totalActionNormalization + Math.Pow(Math.E, getQPair(environment.actualState, action).qValue / temperature)
            Next

            Dim randomNumber As Double = randomGenerator.NextDouble

            For Each action In environment.actions
                Dim actionProbability As Double = Math.Pow(Math.E, getQPair(environment.actualState, action).qValue / temperature) / totalActionNormalization
                'For the rare cases where the probability of an action is infinity or not a number (i.e, where their Q value is either infinity or
                'a big number that is divided for a very low temperature)
                If Double.IsInfinity(actionProbability) Or Double.IsNaN(actionProbability) Then
                    Return action
                End If
                accumulatedActionProbability = accumulatedActionProbability + actionProbability

                If randomNumber <= accumulatedActionProbability Then
                    Return action
                End If
            Next
        End If
        Return Nothing
    End Function

    ''' <summary>
    ''' Determines the best action in the environment's actual state
    ''' </summary>
    ''' <returns></returns>
    Protected Function determineBestAction(s As State) As Action
        Dim bestAction As Action
        Dim bestActions As New List(Of Action)

        For Each a As Action In environment.actions
            If bestActions.Count = 0 Then
                bestActions.Add(a)
            Else
                If getQPair(s, a).qValue > getQPair(s, bestActions.Item(0)).qValue Then
                    bestActions.Clear()
                    bestActions.Add(a)
                ElseIf getQPair(s, a).qValue >= getQPair(s, bestActions.Item(0)).qValue Then
                    bestActions.Add(a)
                End If
            End If
        Next

        If bestActions.Count = 1 Then
            bestAction = bestActions.Item(0)
        Else
            bestAction = bestActions.Item(randomGenerator.Next(0, bestActions.Count))
        End If

        Return bestAction
    End Function

    Protected Sub calculateValueMatrix()

        For i As Integer = 0 To valueMatrix.RowCount - 1
            For j As Integer = 0 To valueMatrix.ColumnCount - 1
                Dim qValuesForThisState As New List(Of Double)
                Dim maxQValue As Double = -1000
                Dim numberOfMaxQValues As Integer = 1

                For Each action In environment.actions
                    qValuesForThisState.Add(getQPair(environment.findState(i, j), action).qValue)
                    If getQPair(environment.findState(i, j), action).qValue >= maxQValue Then
                        If getQPair(environment.findState(i, j), action).qValue > maxQValue Then
                            maxQValue = getQPair(environment.findState(i, j), action).qValue
                            numberOfMaxQValues = 1
                        Else
                            numberOfMaxQValues += 1
                        End If
                    End If
                Next

                valueMatrix.Item(i, j) = maxQValue
            Next
        Next

    End Sub

    Protected Function getQPair(s As State, a As Action) As QValue
        For Each qValue In qValues
            If qValue.state.Equals(s) And qValue.action.Equals(a) Then
                Return qValue
            End If
        Next
        Return Nothing
    End Function
End Class
