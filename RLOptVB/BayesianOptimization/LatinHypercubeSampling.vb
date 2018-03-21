

Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Class that implements the Latin Hypercube Sampling algorithm. A Latin Hypercube is the generalization on several dimensions of the Latin Square, a NxN square where N elements are selected
''' in such a way that an element is never selected more than once in any row or column.
''' </summary>
Public Class LatinHypercubeSampling

    Public ReadOnly Property numberOfDivisions As Integer
    Public Property samplesRemaining As Integer
    Public ReadOnly Property numberOfVariables As Integer
    Public ReadOnly Property minimumIntervalRange As Integer
    Public ReadOnly Property maximumIntervalRange As Integer
    'Latin hypercube is a list that contain the information of all the dimensions. Each dimension contains a list of all the starts of the division that haven't been sampled yet.
    'Whenever a division is sampled, it is retired from the list.
    Private ReadOnly Property latinHypercube As New List(Of List(Of Double))
    Private ReadOnly Property divisionDelta As Double

    ''' <summary>
    ''' Initializes the latin hypercube, given the number of samples that will be done (this equals to the number of divisions on each dimension), the number of variables (dimensions) 
    ''' considered, and a minimum and maximum interval range, if dimensions are normalized in a range distinct than [0.0, 1.0]
    ''' </summary>
    ''' <param name="numberOfSamples"></param>
    ''' <param name="numberOfVariables"></param>
    ''' <param name="minimumIntervalRange"></param>
    ''' <param name="maximumIntervalRange"></param>
    Public Sub New(numberOfVariables As Integer, numberOfSamples As Integer, Optional minimumIntervalRange As Integer = 0, Optional maximumIntervalRange As Integer = 1)
        numberOfDivisions = numberOfSamples
        samplesRemaining = numberOfSamples
        Me.numberOfVariables = numberOfVariables
        Me.minimumIntervalRange = minimumIntervalRange
        Me.maximumIntervalRange = maximumIntervalRange

        divisionDelta = (maximumIntervalRange - minimumIntervalRange) / numberOfSamples

        For numberOfDimension As Integer = 0 To numberOfVariables - 1
            Dim dimensionInformation As New List(Of Double)
            latinHypercube.Add(dimensionInformation)

            Dim divisionStart As Double = minimumIntervalRange

            For division As Integer = 0 To numberOfDivisions - 1
                dimensionInformation.Add(divisionStart)
                divisionStart += minimumIntervalRange + divisionDelta
            Next
        Next
    End Sub

    ''' <summary>
    ''' Samples the latin hypercube, if there are points remaining to sample. Otherwise returns Nothing.
    ''' </summary>
    ''' <returns>The next sample of the Latin Hypercube</returns>
    Public Function nextSample() As Vector(Of Double)
        If samplesRemaining > 0 Then
            Dim sample As Vector(Of Double) = Vector(Of Double).Build.Dense(numberOfVariables, 0)
            Dim random As New Random

            'A sample will be selected for each dimension
            For numberOfDimension As Integer = 0 To numberOfVariables - 1
                Dim currentDimension As List(Of Double) = latinHypercube.Item(numberOfDimension)

                Dim selectedDivisionStart As Double = currentDimension.Item(random.Next(0, currentDimension.Count))

                Dim selectedNumber As Double = selectedDivisionStart + random.NextDouble() * divisionDelta

                sample.Item(numberOfDimension) = selectedNumber

                'Removes the selected start from the list
                currentDimension.Remove(selectedDivisionStart)
            Next

            samplesRemaining -= 1

            Return sample
        Else
            Return Nothing
        End If
    End Function

End Class
