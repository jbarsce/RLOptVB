Imports MathNet.Numerics.LinearAlgebra

Public MustInherit Class Agent

    Public ReadOnly Property name As String
    'Environment where the agent is immersed.
    Protected Property environment As Gridworld
    'Maximum number of time steps where the agent will run. Reached that number, the agent will finish the episode as a failure.
    'A cutoffTime = -1 equals no maximum number of time steps. 
    Public Property cutoffTime As Double

    Public Sub New(name As String, environment As Gridworld, Optional cutoffTime As Double = -1)
        Me.name = name
        Me.environment = environment
        Me.cutoffTime = cutoffTime
    End Sub

    ''' <summary>
    ''' Runs the agent for a fixed number of episodes
    ''' </summary>
    ''' <param name="episodes">Amount of episodes the agent will run</param>
    Public MustOverride Sub run(episodes As Integer)
    ''' <summary>
    ''' Gets a measure of the last run (e.g. the sum of all the obtained reward of all episodes)
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function getLastRunMeasure() As Double
    ''' <summary>
    ''' Gets the number of steps of each episode in the current run of the agent. This list is reset every time the runAgent method is called.
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function getStepsOfEpisode() As List(Of Integer)
    ''' <summary>
    ''' Gets a list of the rewards received in each episode
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function getRewardOfEpisode() As List(Of Double)
    ''' <summary>
    ''' List that assigns 1 to each successful episode and 0 otherwise
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function getSuccessOfEpisode() As List(Of Boolean)
    ''' <summary>
    ''' Returns the number of hyperparameters of the current agent.
    ''' </summary>
    ''' <returns></returns>
    Public MustOverride Function getHyperparameterCount() As Integer
    ''' <summary>
    ''' Gives the agent a new hyperparameter configuration, of equal size of the number of hyperparameters of the current agent.
    ''' </summary>
    Public MustOverride Sub setNewConfiguration(hyperparameters As Vector(Of Double))
    ''' <summary>
    ''' Restarts all the agent's learning and its environment.
    ''' </summary>
    Public MustOverride Sub restartAgent()
End Class
