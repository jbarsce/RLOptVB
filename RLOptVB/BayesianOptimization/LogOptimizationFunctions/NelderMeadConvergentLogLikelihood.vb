Imports MathNet.Numerics.LinearAlgebra

''' <summary>
''' Implementation of the modification of the Nelder-Mead algorithm as proposed by Price et.al. 2001. This modification resolves several convergence problems,
''' while mantaining the core of the Nelder Mead algorithm.
''' </summary>
Public Class NelderMeadConvergentLogLikelihood : Inherits NelderMeadLogLikelihood
    'm counts the number of the quasi-minimal frames
    Dim m As Integer = 1
    'v is a constant number that is the power of h
    Dim v As Double = 4.5
    'N is a positive constant that defines the epsilon value
    Dim N As Double = 1
    'h is raised to v and multiplied by N to calculate epsilon
    Dim h As Double = 1
    'k divides h to recalculate epsilon
    Dim k As Double = 4
    'k0 is a constant used to determine if convergence condition (5) is hold
    Dim k0 As Double = 1
    'thau is a small number that checks if convergence condition (2) is hold
    Dim thau As Double = 1.0E-17

    Public Sub New(numberOfGPHyperparameters As Integer, Optional initialPoint As Vector(Of Double) = Nothing)
        MyBase.New(numberOfGPHyperparameters, initialPoint)
    End Sub

    Public Overrides Sub optimize(bayesianOptimizer As BayesianOptimizer)
        Me.bayesianOptimizer = bayesianOptimizer
        initialSigmaSquaredF = bayesianOptimizer.sigmaSquaredF
        initialSigmaSquaredN = bayesianOptimizer.sigmaSquaredN
        initialLVector = bayesianOptimizer.lVector.Clone
        Dim bestPoint As Vector(Of Double)
        'First step
        '---------------
        'j is the number of the current iteration of the algorithm
        Dim j = 1

        Dim simplexVertices As List(Of Vector(Of Double)) = buildSimplex(initialPoint)
        Dim terminateOptimization As Boolean = False

        Dim epsilon As Double = N * (h ^ v)

        While Not terminateOptimization
            'Second step
            '----------------
            Dim terminateNelderMead As Boolean = False
            While Not terminateNelderMead

                Dim worstFValue As Double = calculateLogLikelihood(determineWorstPoint(simplexVertices))

                Dim Xnew As Vector(Of Double) = searchXnew(simplexVertices)
                'If Xnew is undefined, the NM optimization is terminated as there is no shrink step in that algorithm
                If Xnew Is Nothing Then
                    terminateNelderMead = True
                    Exit While
                End If
                simplexVertices.Item(simplexVertices.IndexOf(determineWorstPoint(simplexVertices))) = Xnew
                simplexVertices = orderSimplexVerticesByFunctionValue(simplexVertices)

                Dim newWorstFValue As Double = calculateLogLikelihood(determineWorstPoint(simplexVertices))

                'Stopping conditions are met?
                If Math.Abs(calculateLogLikelihood(simplexVertices.First) - calculateLogLikelihood(simplexVertices.Last)) < terminationEpsilon Then
                    terminateOptimization = True
                    Exit While
                ElseIf worstFValue - newWorstFValue > epsilon Then
                    terminateNelderMead = True
                Else
                    j = j + 1
                End If
            End While


            'Checks if the stopping condition is met
            If terminateOptimization = True Then
                bestPoint = determineBestPoint(simplexVertices)
                Exit While
            End If

            simplexVertices = orderSimplexVerticesByFunctionValue(simplexVertices)
            Dim X0 As Vector(Of Double) = determineBestPoint(simplexVertices)

            Dim basisSum As Vector(Of Double) = Vector(Of Double).Build.Dense(dimension, 0)

            'The basis is created as the V matrix
            Dim VMatrix As Matrix(Of Double) = Nothing

            For i As Integer = 1 To simplexVertices.Count - 1
                Dim Xi As Vector(Of Double) = simplexVertices.Item(i)
                If VMatrix Is Nothing Then
                    VMatrix = Xi.ToColumnMatrix
                    basisSum = basisSum + (Xi - X0) / h
                Else
                    VMatrix = VMatrix.Append(Xi.ToColumnMatrix)
                    basisSum = basisSum + (Xi - X0) / h
                End If
            Next
            'Last element of the basis is added
            Dim Vnplus1 As Vector(Of Double) = -((gamma - alpha) / (alpha * (dimension + 1))) * basisSum
            VMatrix = VMatrix.Append(Vnplus1.ToColumnMatrix)
            'VMatrix without Xn+1 is used to check the (2) convergence conditions
            Dim VMatrixWithoutVnplus1 = VMatrix.RemoveColumn(VMatrix.ColumnCount - 1)

            Dim hasReshaped As Boolean = False
            Dim toBeReshaped As Boolean = False

            'Third step
            '---------------------
            'Condition (2), part I
            If VMatrixWithoutVnplus1.Determinant < thau Then
                toBeReshaped = True
            Else
                'Condition (2), part II
                For vectorIndex As Integer = 0 To VMatrix.ColumnCount - 2
                    'Checks if sumMagnitudes equals |X|
                    If VMatrixWithoutVnplus1.Column(vectorIndex).SumMagnitudes > k0 Then
                        toBeReshaped = True
                        Exit For
                    End If
                Next

                'Condition (5)
                'UpperK is the greatest value between K0 and ((gamma - alpha) * K0) / alpha
                Dim UpperK As Double = Math.Max(k0, ((gamma - alpha) * k0) / alpha)
                For vectorIndex As Integer = 0 To VMatrix.ColumnCount - 1
                    If VMatrix.Column(vectorIndex).SumMagnitudes > UpperK Then
                        toBeReshaped = True
                        Exit For
                    End If
                Next
            End If

            If toBeReshaped Then
                VMatrix = reshapeVMatrix(VMatrixWithoutVnplus1, k0)
                VMatrixWithoutVnplus1.Append(Vnplus1.ToColumnMatrix)
                hasReshaped = True
                toBeReshaped = False
            End If

            'Fourth step
            '-----------------
            Dim Xp As Vector(Of Double) = X0 - h * Vnplus1
            Dim fp As Double = calculateLogLikelihood(Xp)

            'Fifth step
            '-----------------
            Dim quasiMinimalFrame As Boolean = True
            While quasiMinimalFrame
                'x is the center point of the frame
                Dim x As Vector(Of Double) = calculateFrameCenterPoint(VMatrixWithoutVnplus1)
                'It is checked if the actual frame is a quasi-minimal frame.
                For vectorIndex As Integer = 0 To VMatrix.ColumnCount - 1
                    Dim v As Vector(Of Double) = VMatrix.Column(vectorIndex)
                    If calculateLogLikelihood(x + h * v) + epsilon <= calculateLogLikelihood(x) Then
                        quasiMinimalFrame = False
                        Exit While
                    End If
                Next

                Dim Zm As Vector(Of Double) = X0
                If Not hasReshaped Then
                    VMatrix = reshapeVMatrix(VMatrixWithoutVnplus1, k0)
                    VMatrixWithoutVnplus1.Append(Vnplus1.ToColumnMatrix)
                    hasReshaped = True
                    toBeReshaped = False
                Else
                    h = h / k
                    epsilon = N * (h ^ v)
                End If

                j = j + 1
            End While

            'Sixth step
            '----------------
            Dim newX0 As Vector(Of Double)
            If calculateLogLikelihood(X0) <= calculateLogLikelihood(Xp) Then
                newX0 = X0
            Else
                newX0 = Xp
            End If

            'The worst point of the simplex is replaced for the new X0
            simplexVertices.Item(simplexVertices.IndexOf(determineWorstPoint(simplexVertices))) = newX0
            simplexVertices = orderSimplexVerticesByFunctionValue(simplexVertices)

            If Math.Abs(calculateLogLikelihood(simplexVertices.First) - calculateLogLikelihood(simplexVertices.Last)) < terminationEpsilon Then
                terminateOptimization = True
            End If
        End While

        bestPoint = determineBestPoint(simplexVertices)

        Dim lVector As Vector(Of Double) = Vector(Of Double).Build.Dense(dimension - 2)
        For i As Integer = 2 To bestPoint.Count - 1
            lVector.Item(i - 2) = bestPoint.Item(i)
        Next

        sigmaSquaredF = bestPoint.Item(0)
        sigmaSquaredN = bestPoint.Item(1)

        If sigmaSquaredF <= 0 Then
            sigmaSquaredF = defaultSigmaSquaredF
        End If

        If sigmaSquaredN <= 0 Then
            sigmaSquaredN = defaultSigmaSquaredN
        End If

        Me.lVector = lVector

        'Bayesian Optimizer hyperparameters return to its initial state
        bayesianOptimizer.setNewGPHyperparameters(initialSigmaSquaredF, initialSigmaSquaredN, initialLVector)
    End Sub

    ''' <summary>
    ''' Returns the center point of the frame
    ''' </summary>
    ''' <param name="VMatrix"></param>
    ''' <returns></returns>
    Private Function calculateFrameCenterPoint(VMatrix As Matrix(Of Double)) As Vector(Of Double)
        Dim x As Vector(Of Double) = Vector(Of Double).Build.Dense(dimension, 0)

        For vectorIndex As Integer = 0 To VMatrix.ColumnCount - 1
            x = x + VMatrix.Column(vectorIndex)
        Next

        Return x / VMatrix.ColumnCount
    End Function

    ''' <summary>
    ''' Reshapes the V matrix to ensure it keeps the algorithm convergence
    ''' </summary>
    ''' <param name="VMatrix"></param>
    Private Shared Function reshapeVMatrix(VMatrix As Matrix(Of Double), K0 As Double)
        Dim qrFactorization As Factorization.QR(Of Double) = VMatrix.QR()

        Dim Q As Matrix(Of Double) = qrFactorization.Q
        Dim scaledQ As Matrix(Of Double) = Nothing
        Dim R As Matrix(Of Double) = qrFactorization.R

        Dim RAverage As Double = 0
        For i As Integer = 0 To VMatrix.ColumnCount - 1
            RAverage = RAverage + R.Item(i, i)
        Next
        RAverage = RAverage * (1 / VMatrix.ColumnCount)

        For i As Integer = 0 To Q.ColumnCount - 1

            'Di is the scale factor of the Q matrix. It is scaling by multiplying the matrix with Di
            Dim Di As Double = Math.Sign(R.Item(i, i)) * Math.Min(K0, Math.Max(Math.Abs(R.Item(i, i)), RAverage / 10))
            Dim Qi As Vector(Of Double) = Q.Column(i).Clone * Di

            If i = 0 Then
                scaledQ = Qi.ToColumnMatrix
            Else
                scaledQ = scaledQ.Append(Qi.ToColumnMatrix)
            End If
        Next

        Return scaledQ * R
    End Function

    ''' <summary>
    ''' Orders a list of vectors (a simplex) by their log-likelihood function value
    ''' </summary>
    ''' <param name="simplexVertices"></param>
    ''' <returns></returns>
    Private Function orderSimplexVerticesByFunctionValue(simplexVertices As List(Of Vector(Of Double))) As List(Of Vector(Of Double))
        If Not simplexVertices.Count = 0 Then
            Dim X0 As Vector(Of Double) = determineBestPoint(simplexVertices)
            Dim orderedSimplexVertices As New List(Of Vector(Of Double))
            orderedSimplexVertices.Add(X0)

            Dim X0Found As Boolean = False
            Dim simplexVerticesNonX0 As New List(Of Vector(Of Double))
            For vectorIndex As Integer = 0 To simplexVertices.Count - 1
                If Not simplexVertices.Item(vectorIndex).Equals(X0) Or X0Found Then
                    simplexVerticesNonX0.Add(simplexVertices.Item(vectorIndex))
                Else
                    X0Found = True
                End If
            Next

            If Not simplexVerticesNonX0.Count = 0 Then
                Dim orderedSimplexVerticesNonX0 = orderSimplexVerticesByFunctionValue(simplexVerticesNonX0)
                For Each vector In orderedSimplexVerticesNonX0
                    orderedSimplexVertices.Add(vector)
                Next

                Return orderedSimplexVertices
            Else
                Return orderedSimplexVertices
            End If
        Else
            Return Nothing
        End If
    End Function
End Class
