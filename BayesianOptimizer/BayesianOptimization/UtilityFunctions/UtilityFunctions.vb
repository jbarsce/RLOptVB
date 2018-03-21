Imports MathNet.Numerics.LinearAlgebra
Imports MathNet.Numerics.Statistics

Public Class UtilityFunctions


    'Random generator, to use the same seed for the duration of the run
    Public Shared Property randomGenerator As New Random()


    ''' <summary>
    ''' Given a vector of double and a scalar, returns a new vector containing the scalar as a new item added at the last position of the vector.
    ''' </summary>
    ''' <param name="targetVector">The vector where the scalar is to be inserted</param>
    ''' <param name="itemToInsert">Scalar item to be inserted in the vector</param>
    ''' <returns></returns>
    Public Shared Function insertItemToVector(targetVector As Vector(Of Double), itemToInsert As Double) As Vector(Of Double)
        If targetVector IsNot Nothing Then
            Dim newObservedYVector As Vector(Of Double) = Vector(Of Double).Build.Dense(targetVector.Count + 1, 0)
            For i As Integer = 0 To targetVector.Count - 1
                newObservedYVector.Item(i) = targetVector.Item(i)
            Next

            newObservedYVector.Item(targetVector.Count) = itemToInsert
            Return newObservedYVector
        Else
            'If the item to insert is the first to be inserted in the target vector, inicializes the vector.
            Return Vector(Of Double).Build.Dense(1, itemToInsert)
        End If
    End Function

    ''' <summary>
    ''' Functions that performs z-score normalization to lists
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Shared Function zScoreNormalization(X As List(Of Double)) As List(Of Double)
        Dim newX As New List(Of Double)

        Dim mean = X.Mean
        Dim std = X.StandardDeviation

        For Each element In X
            newX.Add((element - mean) / std)
        Next

        Return newX
    End Function


    ''' <summary>
    ''' Functions that performs z-score normalization to lists
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Shared Function zScoreNormalization(X As List(Of Double), mean As Double, std As Double) As List(Of Double)
        Dim newX As New List(Of Double)

        For Each element In X
            newX.Add((element - mean) / std)
        Next

        Return newX
    End Function


    ''' <summary>
    ''' Functions that performs z-score normalization to vectors
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Shared Function zScoreNormalization(X As Vector(Of Double)) As Vector(Of Double)
        Dim newX As Vector(Of Double) = Vector(Of Double).Build.Dense(X.Count)

        Dim mean = X.Mean
        Dim std = X.StandardDeviation

        Dim i = 0
        For Each element In X
            newX.Item(i) = (element - mean) / std
            i += 1
        Next

        Return newX
    End Function


    ''' <summary>
    ''' Functions that performs z-score normalization to vectors
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Shared Function zScoreNormalization(X As Vector(Of Double), mean As Double, std As Double) As Vector(Of Double)
        Dim newX As Vector(Of Double) = Vector(Of Double).Build.Dense(X.Count)

        Dim i = 0
        For Each element In X
            newX.Item(i) = (element - mean) / std
            i += 1
        Next

        Return newX
    End Function

    ''' <summary>
    ''' Inserts X2 elements into X1 vector
    ''' </summary>
    ''' <param name="X1"></param>
    ''' <param name="X2"></param>
    Public Shared Function appendVectors(X1 As Vector(Of Double), X2 As Vector(Of Double))
        If X1 IsNot Nothing Then
            For Each elementX2 In X2
                X1 = insertItemToVector(X1, elementX2)
            Next
            Return X1
        Else
            Return X2
        End If
    End Function

End Class
