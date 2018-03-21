
Public Class SarsaLambdaSuccessMeasure : Inherits SarsaLambda

    Public Sub New(name As String, alpha As Double, epsilon As Double, gamma As Double, lambda As Double, environment As Gridworld)
        MyBase.New(name, alpha, epsilon, gamma, lambda, environment)
    End Sub

    'The last run measure is overriden; returns the number of successful episodes / the total number of episodes.
    Public Overrides Function getLastRunMeasure() As Double
        Dim sucessfulEpisodes As Integer = 0

        For Each episodeResult In sucessOfEpisode
            If episodeResult = True Then
                sucessfulEpisodes += 1
            End If
        Next

        Return sucessfulEpisodes / stepsOfEpisode.Count
    End Function
End Class
