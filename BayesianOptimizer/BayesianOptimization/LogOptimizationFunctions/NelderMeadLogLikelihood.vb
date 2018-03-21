Imports MathNet.Numerics.LinearAlgebra

Public Class NelderMeadLogLikelihood : Inherits LogLikelihoodOptimizationFunction

    'Initial point where the simplex is based
    Public ReadOnly Property initialPoint As Vector(Of Double)
    'Size of the step where the simplex is extended from the initial point to each of the dimentions
    Public ReadOnly Property simplexStepSize As Double = 1

    'Amount of GP hyperparameters to optimize
    Public ReadOnly Property dimension As Integer

    'Reflection rate
    Public ReadOnly Property alpha As Double = 1
    'Expansion rate. Must be greater than the reflection rate (i.e. gamma > alpha)
    Public ReadOnly Property gamma As Double = 2
    'Contraction rate
    Public ReadOnly Property beta As Double = 0.5
    'Shrink rate
    Public ReadOnly Property delta As Double = 0.5
    'Number to replace sigmaSquaredF when it takes a negative value
    Public ReadOnly Property defaultSigmaSquaredF As Double = 0.1
    'Number to replace sigmaSquaredN when it takes a negative value
    Public ReadOnly Property defaultSigmaSquaredN As Double = 0.1

    Protected Property bayesianOptimizer As BayesianOptimizer
    Protected Property initialSigmaSquaredF As Double
    Protected Property initialSigmaSquaredN As Double
    Protected Property initialLVector As Vector(Of Double)

    ''Termination rate criteria
    Public ReadOnly Property maximumNumberOfIterations As Integer = 100
    'Minimum difference
    Public ReadOnly Property terminationEpsilon As Double = 0.0001

    Public Sub New(numberOfGPHyperparameters As Integer, Optional initialPoint As Vector(Of Double) = Nothing)
        dimension = numberOfGPHyperparameters
        If initialPoint IsNot Nothing Then
            Me.initialPoint = initialPoint
        Else
            Me.initialPoint = Vector(Of Double).Build.Dense(numberOfGPHyperparameters, 1)
        End If

    End Sub

    Public Overrides Sub optimize(bayesianOptimizer As BayesianOptimizer)
        Me.bayesianOptimizer = bayesianOptimizer
        initialSigmaSquaredF = bayesianOptimizer.sigmaSquaredF
        initialSigmaSquaredN = bayesianOptimizer.sigmaSquaredN
        initialLVector = bayesianOptimizer.lVector.Clone
        Dim bestPoint As Vector(Of Double)

        Dim currentIteration = 0

        Dim simplexVertices As List(Of Vector(Of Double)) = buildSimplex(initialPoint)

        Dim terminate As Boolean = False
        While terminate = False
            Dim Xnew As Vector(Of Double) = searchXnew(simplexVertices)

            currentIteration += 1

            If Xnew IsNot Nothing Then
                'If the simplex has not been shrinked, then the worst element is replaced with the new Xh.
                'Otherwise, the simplex points has been replaced already.
                simplexVertices.Item(simplexVertices.IndexOf(determineWorstPoint(simplexVertices))) = Xnew
            Else
                bestPoint = determineBestPoint(simplexVertices)
                'If no value for Xnew was found, shrinks the simplex
                For i As Integer = 1 To simplexVertices.Count - 1
                    simplexVertices.Item(i) = bestPoint + delta * (simplexVertices.Item(i) - bestPoint)
                Next
            End If

            'It is defined if the running is terminated or not
            If currentIteration > maximumNumberOfIterations Or
            Math.Abs(calculateLogLikelihood(determineBestPoint(simplexVertices)) - calculateLogLikelihood(determineWorstPoint(simplexVertices))) <
            terminationEpsilon Then
                terminate = True
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
    ''' Searches for a new acceptable (i.e. a better point than X) point in the simplex and return that point if it is found. Otherwise returns Nothing.
    ''' </summary>
    ''' <param name="simplexVertices"></param>
    ''' <returns></returns>
    Protected Function searchXnew(simplexVertices As List(Of Vector(Of Double))) As Vector(Of Double)
        'Xnew is the point that (hopefully) will replace the worst point in the simplex. If this is not
        'the case, an alternative action must be done (e.g. shrink the simplex).
        Dim Xnew As Vector(Of Double) = Nothing
        'Xh is the worst point of the set i.e. the point that maximizes the log-likelihood function.
        Dim Xh As Vector(Of Double)
        'Xs is the second worst point i.e. the point with the second greatest likelihood.
        Dim Xs As Vector(Of Double)
        'Xl is the best point that minimizes the log-likelihood.
        Dim Xl As Vector(Of Double)

        Xh = determineWorstPoint(simplexVertices)

        Xs = determineSecondWorstPoint(simplexVertices, Xh)
        Xl = determineBestPoint(simplexVertices)
        Dim c As Vector(Of Double) = calculateCentroid(simplexVertices, Xh)

        Dim f_Xh As Double = calculateLogLikelihood(Xh)
        Dim f_Xs As Double = calculateLogLikelihood(Xs)
        Dim f_Xl As Double = calculateLogLikelihood(Xl)

        'Reflection
        Dim Xr = c + alpha * (c - Xh)
        Dim f_Xr As Double = calculateLogLikelihood(Xr)

        If f_Xl <= f_Xr And f_Xr < f_Xs Then
            Xnew = Xr
        Else
            'Expansion
            If f_Xr < f_Xl Then
                Dim Xe As Vector(Of Double) = c + gamma * (Xr - c)
                Dim f_Xe As Double = calculateLogLikelihood(Xe)

                If f_Xe < f_Xr Then
                    Xnew = Xe
                Else
                    Xnew = Xr
                End If
            Else
                'Contraction. At this point, it is certain that f(Xs) <= f(Xr)
                If f_Xr < f_Xh Then
                    Dim Xc As Vector(Of Double) = c + beta * (Xh - c)
                    Dim f_Xc As Double = calculateLogLikelihood(Xc)

                    If f_Xc <= f_Xr Then
                        Xnew = Xc
                    End If
                Else
                    Dim Xc As Vector(Of Double) = c + beta * (Xh - c)
                    Dim f_Xc As Double = calculateLogLikelihood(Xc)

                    If f_Xc < f_Xh Then
                        Xnew = Xc
                    End If
                End If
            End If
        End If

        Return Xnew
    End Function

    ''' <summary>
    ''' Builds the simplex based on the initial point.
    ''' </summary>
    ''' <param name="initialPoint"></param>
    ''' <returns></returns>
    Protected Function buildSimplex(initialPoint As Vector(Of Double)) As List(Of Vector(Of Double))
        Dim simplex As New List(Of Vector(Of Double))
        simplex.Add(initialPoint)

        For i As Integer = 0 To dimension - 1
            Dim unitVector As Vector(Of Double) = Vector(Of Double).Build.Dense(initialPoint.Count, 0)
            unitVector.Item(i) = 1

            simplex.Add(initialPoint + simplexStepSize * unitVector)
        Next

        Return simplex
    End Function

    ''' <summary>
    ''' Calculates c, the centroid of the simplex.
    ''' </summary>
    ''' <param name="vectors"></param>
    ''' <returns></returns>
    Private Function calculateCentroid(vectors As List(Of Vector(Of Double)), worstPoint As Vector(Of Double)) As Vector(Of Double)
        Dim dimension As Integer = vectors.Item(0).Count
        Dim summarizedVector As Vector(Of Double) = Vector(Of Double).Build.Dense(dimension, 0)

        For Each vector In vectors
            'The vertex (worst point of the simplex) it is not included in the centroid calculation
            If Not vector.Equals(worstPoint) Then
                summarizedVector = summarizedVector + vector
            End If
        Next

        Return summarizedVector / (dimension)
    End Function

    Protected Function determineWorstPoint(simplexVertices As List(Of Vector(Of Double))) As Vector(Of Double)
        Dim worstPoint As Vector(Of Double) = simplexVertices.Item(0)
        Dim worstPointF As Double = calculateLogLikelihood(worstPoint)

        For Each vertex In simplexVertices
            Dim vertexF As Double = calculateLogLikelihood(vertex)
            If vertexF > worstPointF Then
                worstPoint = vertex
                worstPointF = vertexF
            End If
        Next

        Return worstPoint
    End Function

    Protected Function determineSecondWorstPoint(simplexVertices As List(Of Vector(Of Double)), worstPoint As Vector(Of Double)) As Vector(Of Double)
        Dim sndWorstPoint As Vector(Of Double)
        Dim sndWorstPointF As Double
        If Not simplexVertices.IndexOf(worstPoint).Equals(0) Then
            sndWorstPoint = simplexVertices.Item(0)
        Else
            sndWorstPoint = simplexVertices.Item(1)
        End If
        sndWorstPointF = calculateLogLikelihood(sndWorstPoint)

        For Each vertex In simplexVertices
            If Not vertex.Equals(worstPoint) Then
                Dim vertexF As Double = calculateLogLikelihood(vertex)
                If vertexF > sndWorstPointF Then
                    sndWorstPoint = vertex
                    sndWorstPointF = vertexF
                End If
            End If
        Next

        Return sndWorstPoint
    End Function

    Protected Function determineBestPoint(simplexVertices As List(Of Vector(Of Double))) As Vector(Of Double)
        Dim bestPoint As Vector(Of Double) = simplexVertices.Item(0)
        Dim bestPointF As Double = calculateLogLikelihood(bestPoint)

        For Each vertex In simplexVertices
            Dim vertexF As Double = calculateLogLikelihood(vertex)
            If vertexF < bestPointF Then
                bestPoint = vertex
                bestPointF = vertexF
            End If
        Next

        Return bestPoint
    End Function

    ''' <summary>
    ''' Given a point, calculates the log-likelihood of this GP configuration in the bayesian optimizer.
    ''' </summary>
    ''' <param name="point"></param>
    ''' <returns></returns>
    Protected Function calculateLogLikelihood(point As Vector(Of Double)) As Double
        Dim pointSigmaSquaredF As Double
        Dim pointSigmaSquaredN As Double
        If point.Item(0) <= 1.0E-150 Then
            pointSigmaSquaredF = defaultSigmaSquaredF
        Else
            pointSigmaSquaredF = point.Item(0)
        End If

        If point.Item(1) <= 1.0E-150 Then
            pointSigmaSquaredN = defaultSigmaSquaredN
        Else
            pointSigmaSquaredN = point.Item(1)
        End If

        Dim pointLVector As Vector(Of Double) = Vector(Of Double).Build.Dense(dimension - 2)
        For i As Integer = 2 To point.Count - 1
            pointLVector.Item(i - 2) = point.Item(i)
        Next

        bayesianOptimizer.setNewGPHyperparameters(pointSigmaSquaredF, pointSigmaSquaredN, pointLVector)
        Return -bayesianOptimizer.calculateLogLikelihood
    End Function
End Class
