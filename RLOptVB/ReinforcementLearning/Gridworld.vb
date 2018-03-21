Public Class Gridworld
    Implements Environment

    Private _initialState As State
    Public Property initialState As State
        Private Set(value As State)
            _initialState = value
        End Set
        Get
            Return _initialState
        End Get
    End Property

    Private _actualState As State
    Public Property actualState As State
        Private Set(value As State)
            _actualState = value
        End Set
        Get
            Return _actualState
        End Get
    End Property


    'List of the possible actions the agent can take in the gridworld
    Public ReadOnly Property actions As IList(Of Action)
    'List of all the (s,a,s')
    Public ReadOnly Property transitions As IList(Of Transition)
    Public ReadOnly Property numberOfRows As Integer
    Public ReadOnly Property numberOfColumns As Integer
    'The time steps elapsed in the gridworld in all episodes since the last reset of the environment changes.
    Public Property timeStepsElapsed As Integer = 0
    'List of states pending to be disabled when a certain condition is met
    Public Property statesToDisable As New List(Of State)
    'List of states pending to be enabled when a certain condition is met
    Public Property statesToEnable As New List(Of State)

    'Constants that determines the wind function used in this gridworld (if any)
    Public ReadOnly Property windFunction As Integer
    Public Const NOWIND As Integer = 0
    Public Const DETERMINISTICWIND As Integer = 1
    Public Const STOCHASTICWIND As Integer = 2


    ''' <summary>
    ''' Creates a n x m gridworld for the new Environment. All the states and actions have the default settings (enabled, noninitial and nonfinal).
    ''' </summary>
    ''' <param name="numberOfRows"></param>
    ''' <param name="numberOfColumns"></param>
    Public Sub New(numberOfRows As Integer, numberOfColumns As Integer, Optional windFunction As Integer = 0)

        actions = New List(Of Action)
        transitions = New List(Of Transition)
        Me.numberOfRows = numberOfRows
        Me.numberOfColumns = numberOfColumns

        Dim upAction As New Action("up")
        Dim downAction As New Action("down")
        Dim leftAction As New Action("left")
        Dim rightAction As New Action("right")

        actions.Add(upAction)
        actions.Add(downAction)
        actions.Add(leftAction)
        actions.Add(rightAction)

        Dim statesMatrix As New List(Of List(Of State))

        For r As Integer = 0 To numberOfRows - 1

            Dim stateRow As New List(Of State)

            For c As Integer = 0 To numberOfColumns - 1
                Dim newState As New State(r.ToString + "," + c.ToString, True, False, False)
                stateRow.Add(newState)
            Next

            statesMatrix.Add(stateRow)
        Next

        For r As Integer = 0 To numberOfRows - 1
            Dim stateRow As List(Of State) = statesMatrix.Item(r)

            For c As Integer = 0 To numberOfColumns - 1

                Dim currentState As State = stateRow.Item(c)


                Dim stateToTransitionUpAction As State = determineStateToTransitionInGridworld(upAction, currentState, numberOfRows, numberOfColumns, statesMatrix)
                Dim stateToTransitionDownAction As State = determineStateToTransitionInGridworld(downAction, currentState, numberOfRows, numberOfColumns, statesMatrix)
                Dim stateToTransitionLeftAction As State = determineStateToTransitionInGridworld(leftAction, currentState, numberOfRows, numberOfColumns, statesMatrix)
                Dim stateToTransitionRightAction As State = determineStateToTransitionInGridworld(rightAction, currentState, numberOfRows, numberOfColumns, statesMatrix)

                Dim transitionUp As New Transition(currentState, upAction, stateToTransitionUpAction)
                Dim transitionDown As New Transition(currentState, downAction, stateToTransitionDownAction)
                Dim transitionLeft As New Transition(currentState, leftAction, stateToTransitionLeftAction)
                Dim transitionRight As New Transition(currentState, rightAction, stateToTransitionRightAction)

                transitions.Add(transitionUp)
                transitions.Add(transitionDown)
                transitions.Add(transitionLeft)
                transitions.Add(transitionRight)
            Next
        Next

        'Wind function is not implemented yet
        Me.windFunction = windFunction
    End Sub

    Private Function determineStateToTransitionInGridworld(action As Action, currentState As State, numberOfRows As Integer, numberOfColumns As Integer, statesMatrix As List(Of List(Of State))) As State

        Dim currentStateRow As Integer = getStateRow(currentState)
        Dim currentStateColumn As Integer = getStateColumn(currentState)

        Dim targetStateName As String = ""
        Dim targetState As State = Nothing

        If action.name.Equals("up") Then
            targetStateName = (currentStateRow - 1).ToString + "," + currentStateColumn.ToString
        End If
        If action.name.Equals("down") Then
            targetStateName = (currentStateRow + 1).ToString + "," + currentStateColumn.ToString
        End If
        If action.name.Equals("left") Then
            targetStateName = currentStateRow.ToString + "," + (currentStateColumn - 1).ToString
        End If
        If action.name.Equals("right") Then
            targetStateName = currentStateRow.ToString + "," + (currentStateColumn + 1).ToString
        End If


        For Each row As List(Of State) In statesMatrix
            For Each state As State In row
                If state.name = targetStateName Then
                    Return state
                End If
            Next
        Next

        Return currentState
    End Function

    Friend Sub enableStateOnCondition(row As Integer, column As Integer, timeStepsElapsed As Integer)
        Dim targetState As State = findState(row, column)
        targetState.enableAtTimeStep = timeStepsElapsed
        statesToEnable.Add(targetState)
    End Sub

    Friend Sub disableStateOnCondition(row As Integer, column As Integer, timeStepsElapsed As Integer)
        Dim targetState As State = findState(row, column)
        targetState.disableAtTimeStep = timeStepsElapsed
        statesToDisable.Add(targetState)
    End Sub

    Public Function getStateRow(currentState As State) As Integer
        Dim commaIndex As String = currentState.name.LastIndexOf(",")
        Return currentState.name.Substring(0, commaIndex)
    End Function

    Public Function getStateColumn(currentState As State) As Integer
        Dim commaIndex As String = currentState.name.LastIndexOf(",")
        Return currentState.name.Substring(commaIndex + 1)
    End Function

    Public Sub setInitialState(initialStateRow As Integer, initialStateColumn As Integer)
        Dim previousInitialState As State = initialState

        Dim targetState As State = findState(initialStateRow, initialStateColumn)
        targetState.isInitial = True
        initialState = targetState

        If previousInitialState Is Nothing Then
            actualState = targetState
        End If
    End Sub

    Public Sub setFinalState(finalStateRow As Integer, finalStateColumn As Integer)
        Dim targetState As State = findState(finalStateRow, finalStateColumn)
        targetState.isFinal = True
    End Sub

    Public Sub disableState(stateRow As Integer, stateColumn As Integer, isDisablePermanent As Boolean)
        Dim targetState As State = findState(stateRow, stateColumn)
        targetState.isEnabled = False

        If isDisablePermanent Then
            targetState.isEnabledAtStart = False
        End If
    End Sub

    Public Sub enableState(stateRow As Integer, stateColumn As Integer)
        Dim targetState As State = findState(stateRow, stateColumn)
        targetState.isEnabled = True
    End Sub

    Public Sub setStateReward(stateRow As Integer, stateColumn As Integer, newReward As Double)
        Dim targetState As State = findState(stateRow, stateColumn)
        targetState.reward = newReward
    End Sub

    ''' <summary>
    ''' Searches for a state given a row and column. If no state is found, returns nothing.
    ''' </summary>
    ''' <param name="stateRow"></param>
    ''' <param name="stateColumn"></param>
    ''' <returns></returns>
    Public Function findState(stateRow As Integer, stateColumn As Integer) As State
        Dim targetStateName As String = stateRow.ToString + "," + stateColumn.ToString

        For Each transition In transitions
            If transition.stateFrom.name.Equals(targetStateName) Then
                Return transition.stateFrom
            End If
            If transition.stateTo.name.Equals(targetStateName) Then
                Return transition.stateTo
            End If
        Next

        Return Nothing
    End Function

    Private Function findNextState(stateFromRow As Integer, stateFromColumn As Integer, action As Action) As State
        Dim stateFromName As String = stateFromRow.ToString + "," + stateFromColumn.ToString

        For Each transition In transitions
            If transition.stateFrom.name.Equals(stateFromName) And transition.action.Equals(action) Then
                Return transition.stateTo
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Applies an action issued by an agent.
    ''' </summary>
    Public Sub takeAction(action As Action, episodeNumber As Integer) Implements Environment.takeAction
        Dim listOfStepsToEnable As New List(Of State)
        Dim listOfStepsToDisable As New List(Of State)

        For Each stateToEnable In statesToEnable
            If stateToEnable.enableAtTimeStep = episodeNumber Then
                enableState(getStateRow(stateToEnable), getStateColumn(stateToEnable))
                listOfStepsToEnable.Add(stateToEnable)
            End If
        Next
        For Each stateToDisable In statesToDisable
            If stateToDisable.disableAtTimeStep = episodeNumber Then
                disableState(getStateRow(stateToDisable), getStateColumn(stateToDisable), False)
                listOfStepsToDisable.Add(stateToDisable)
            End If
        Next

        For Each stepToEnable In listOfStepsToEnable
            statesToEnable.Remove(stepToEnable)
        Next

        For Each stepToDisable In listOfStepsToDisable
            statesToDisable.Remove(stepToDisable)
        Next

        Dim nextState As State = findNextState(getStateRow(actualState), getStateColumn(actualState), action)

        If nextState.isEnabled Then
            actualState = findNextState(getStateRow(actualState), getStateColumn(actualState), action)
        End If


    End Sub

    ''' <summary>
    ''' Restarts the gridworld to its initial, default state.
    ''' </summary>
    Friend Sub restartEnvironment(Optional restartEnvironmentChanges = False)
        actualState = initialState

        If restartEnvironmentChanges Then
            timeStepsElapsed = 0

            For Each transition In transitions
                Dim stateFrom As State = transition.stateFrom

                If stateFrom.isEnabledAtStart And Not (stateFrom.isEnabled) And statesToDisable.IndexOf(stateFrom) = -1 Then
                    stateFrom.isEnabled = True
                    statesToDisable.Add(stateFrom)
                Else
                    If Not (stateFrom.isEnabledAtStart) And stateFrom.isEnabled And statesToEnable.IndexOf(stateFrom) = -1 Then
                        stateFrom.isEnabled = False
                        statesToEnable.Add(stateFrom)
                    End If
                End If

                Dim stateTo As State = transition.stateTo

                If stateTo.isEnabledAtStart And Not (stateTo.isEnabled) And statesToDisable.IndexOf(stateTo) = -1 Then
                    stateTo.isEnabled = True
                    statesToDisable.Add(stateTo)
                Else
                    If Not (stateTo.isEnabledAtStart) And stateTo.isEnabled And statesToEnable.IndexOf(stateTo) = -1 Then
                        stateTo.isEnabled = False
                        statesToEnable.Add(stateTo)
                    End If
                End If
            Next
        End If
    End Sub
End Class
