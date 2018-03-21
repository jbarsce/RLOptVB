
Imports MathNet.Numerics.LinearAlgebra

Public Class DividingRectangles : Inherits AcquisitionOptimizationFunction

    ''Bayesian Optimizer Information
    Dim queriedX As List(Of Vector(Of Double))
    Dim observedY As Vector(Of Double)
    Dim hyperparameters As Vector(Of Double)
    Dim calculateMeanOfTestPoint As Func(Of Vector(Of Double), Double)
    Dim calculateVarianceOfTestPoint As Func(Of Vector(Of Double), Double)
    Dim acquisitionFunction As AcquisitionFunction

    'Subset of queriedAcqX that include all the potentially optimal rectangle centers.
    Public Property potentiallyOptimalRectangleCenter As New List(Of Vector(Of Double))
    'Complement of potentiallyOptimalRectangleCenter that contains the delta of all the points considered.
    Public Property potentiallyOptimalRectangleDelta As New List(Of Vector(Of Double))
    'Number of iterations that the DIRECT algorithm will run before returning the best point X, according to the acquisition function
    Public Property dividingRectanglesIterations As Integer
    'A threshold (% of the current maximum) that a new maximum expected improvement must surpass in order to become the new maximum
    Public Property dividingRectanglesEpsilon As Double
    'Chance, in the dividing rectangles algorithm, to get an a-priori suboptimal rectangle to be divided and sampled
    Public Property dividingRectanglesProbabilityToExplore As Double

    'List of vector of queried points X = (x1, x2, ... , xn) on the expected improvement acquisition function, u(X).
    Public Property queriedAcqX As New List(Of Vector(Of Double))
    'Vector of observed u(X) based on queried points on the acquisition function.
    Public Property observedAcqY As Vector(Of Double)
    'List of the points left to be sampled in the dividing rectangles approach.
    Public Property pointsLeftToSample As New List(Of Vector(Of Double))



    Public Sub New(dividingRectanglesIterations As Integer, dividingRectanglesEpsilon As Double, Optional dividingRectanglesProbabilityToExplore As Double = 0)
        Me.dividingRectanglesIterations = dividingRectanglesIterations
        Me.dividingRectanglesEpsilon = dividingRectanglesEpsilon
        Me.dividingRectanglesProbabilityToExplore = dividingRectanglesProbabilityToExplore
    End Sub

    ''' <summary>
    ''' Maximizes the (x1, x2, ..., xn) vector that are the most likely to improve the maximum of the objective function f, given the current (x,f(x)) pairs.
    ''' It uses the Dividing Rectangles (DIRECT) method to maximize the vector (Jones et. al. 1993).
    ''' 
    ''' Note: this method consider that the range of all hyperparameters is normalized in [0.0 , 1.0]
    ''' </summary>
    Public Overrides Function optimize(bayesianOptimizer As BayesianOptimizer) As Vector(Of Double)
        queriedX = bayesianOptimizer.queriedX
        observedY = bayesianOptimizer.observedY
        hyperparameters = bayesianOptimizer.hyperparameters
        calculateMeanOfTestPoint = Function(Z) bayesianOptimizer.calculateMeanOfTestPoint(Z)
        calculateVarianceOfTestPoint = Function(Z) bayesianOptimizer.calculateVarianceOfTestPoint(Z)
        acquisitionFunction = bayesianOptimizer.acquisitionFunction

        'If the method is called for the first time, returns the center point (as this is the point that will be returned if the method finishes normally)
        If queriedX.Count = 0 Then
            'As the hypercube is normalized in [0.0 , 1.0], the center point in each dimension is 0.5
            Dim centerPoint As Double = 0.5
            Return Vector(Of Double).Build.Sparse(hyperparameters.Count, centerPoint)
        End If

        'Clear the contents of the last expected improvement execution
        Dim alternativePotentiallyOptimal As New List(Of Vector(Of Double))
        potentiallyOptimalRectangleCenter.Clear()
        potentiallyOptimalRectangleDelta.Clear()
        pointsLeftToSample.Clear()
        queriedAcqX.Clear()

        If observedAcqY IsNot Nothing Then
            observedAcqY = Nothing
        End If

        Dim bestPoint As Vector(Of Double)

        Dim maximumU As Double
        Dim currentIteration As Integer = 0

        'As the hypercube is normalized in [0.0 , 1.0], the center point in each dimension is 0.5
        Dim centerPointScalar As Double = 0.5

        'Initial value is the center point of the hypercube
        Dim initialVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count, centerPointScalar)
        Dim initialDeltaVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count, 1 / 3)

        bestPoint = initialVector

        If observedY.Count = 0 Then
            maximumU = acquisitionFunction.acquisition(initialVector, calculateMeanOfTestPoint(initialVector), calculateVarianceOfTestPoint(initialVector))
        Else
            maximumU = acquisitionFunction.acquisition(initialVector, calculateMeanOfTestPoint(initialVector), calculateVarianceOfTestPoint(initialVector), observedY.Max)
        End If
        queriedAcqX.Add(initialVector)
        observedAcqY = insertItemToVector(observedAcqY, maximumU)

        potentiallyOptimalRectangleCenter.Add(bestPoint)
        potentiallyOptimalRectangleDelta.Add(Vector(Of Double).Build.Sparse(hyperparameters.Count, 1 / 3))

        '"Divides" the rectangles by adding two points to sample for each dimension
        For i As Integer = 0 To hyperparameters.Count - 1

            'ZeroDeltaVector is the vector where all its values are zero except the value of the dimension to test
            Dim zeroDeltaVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count, 0)
            zeroDeltaVector.Item(i) = initialDeltaVector.Item(i)

            Dim upperVectorToAdd As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count, centerPointScalar) + zeroDeltaVector
            Dim lowerVectorToAdd As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count, centerPointScalar) - zeroDeltaVector

            pointsLeftToSample.Add(upperVectorToAdd)
            pointsLeftToSample.Add(lowerVectorToAdd)

            potentiallyOptimalRectangleCenter.Add(upperVectorToAdd)
            potentiallyOptimalRectangleDelta.Add(initialDeltaVector)
            potentiallyOptimalRectangleCenter.Add(lowerVectorToAdd)
            potentiallyOptimalRectangleDelta.Add(initialDeltaVector)
        Next

        bestPoint = findBestPoint(bestPoint, maximumU)

        currentIteration += 1
        pointsLeftToSample.Clear()


        While currentIteration < dividingRectanglesIterations
            'Divides a new subrectangle based on the function values
            Dim newPotentiallyOptimalRectangleCenter As New List(Of Vector(Of Double))
            Dim newPotentiallyOptimalRectangleDeltas As New List(Of Vector(Of Double))

            findNewPotentiallyOptimalRectangles(alternativePotentiallyOptimal, currentIteration, newPotentiallyOptimalRectangleCenter, newPotentiallyOptimalRectangleDeltas)

            potentiallyOptimalRectangleCenter.Clear()
            potentiallyOptimalRectangleDelta.Clear()

            For i As Integer = 0 To newPotentiallyOptimalRectangleCenter.Count - 1
                Dim currentCenterPoint As Vector(Of Double) = newPotentiallyOptimalRectangleCenter.Item(i)
                Dim newDelta As Vector(Of Double) = newPotentiallyOptimalRectangleDeltas.Item(i)

                Dim centerDelta As Vector(Of Double) = Vector(Of Double).Build.Dense(hyperparameters.Count, 0)
                For m As Integer = 0 To hyperparameters.Count - 1
                    centerDelta.Item(m) = newDelta.Item(m) / 3
                Next

                potentiallyOptimalRectangleCenter.Add(currentCenterPoint)
                potentiallyOptimalRectangleDelta.Add(centerDelta)

                For j As Integer = 0 To hyperparameters.Count - 1
                    Dim currentCenterPointScalar As Double = currentCenterPoint.Item(j)

                    Dim deltaVector As Vector(Of Double) = Vector(Of Double).Build.Dense(hyperparameters.Count, 0)
                    For k As Integer = 0 To hyperparameters.Count - 1
                        deltaVector.Item(k) = newDelta.Item(k) / 3
                    Next


                    Dim zeroDeltaVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count, 0)
                    zeroDeltaVector.Item(j) = deltaVector.Item(j)

                    Dim upperVectorToAdd As Vector(Of Double) = currentCenterPoint + zeroDeltaVector
                    Dim lowerVectorToAdd As Vector(Of Double) = currentCenterPoint - zeroDeltaVector

                    pointsLeftToSample.Add(upperVectorToAdd)
                    pointsLeftToSample.Add(lowerVectorToAdd)

                    potentiallyOptimalRectangleCenter.Add(upperVectorToAdd)
                    potentiallyOptimalRectangleDelta.Add(deltaVector)
                    potentiallyOptimalRectangleCenter.Add(lowerVectorToAdd)
                    potentiallyOptimalRectangleDelta.Add(deltaVector)
                Next
            Next

            bestPoint = findBestPoint(bestPoint, maximumU)

            currentIteration += 1
        End While


        Dim a = acquisitionFunction.acquisition(bestPoint, calculateMeanOfTestPoint(bestPoint), calculateVarianceOfTestPoint(bestPoint), observedY.Max)


        If queriedX.IndexOf(bestPoint) < 0 Then
            Return bestPoint
        Else
            'If the best point has already been queried, it takes the second best point
            While queriedX.IndexOf(bestPoint) >= 0
                If alternativePotentiallyOptimal.Count > 0 Then
                    Dim random As New Random
                    bestPoint = alternativePotentiallyOptimal.Item(random.Next(0, alternativePotentiallyOptimal.Count - 1))
                Else
                    Dim bestPointIndex As Double = observedAcqY.MaximumIndex
                    observedAcqY.Item(bestPointIndex) = -10000
                    bestPoint = queriedAcqX.Item(observedAcqY.MaximumIndex)
                End If
            End While
            Return bestPoint
        End If
    End Function

    ''' <summary>
    ''' Finds the best point, given the vector and the value of the current best point 
    ''' </summary>
    ''' <param name="maximumU"></param>
    Private Function findBestPoint(bestPoint As Vector(Of Double), maximumU As Double)

        For Each point In pointsLeftToSample
            Dim currentExpectedImprovement As Double
            If observedY.Count = 0 Then
                currentExpectedImprovement = AcquisitionFunction.acquisition(point, calculateMeanOfTestPoint(point), calculateVarianceOfTestPoint(point))
            Else
                currentExpectedImprovement = AcquisitionFunction.acquisition(point, calculateMeanOfTestPoint(point), calculateVarianceOfTestPoint(point), observedY.Max)
            End If
            queriedAcqX.Add(point)
            observedAcqY = insertItemToVector(observedAcqY, currentExpectedImprovement)
            If currentExpectedImprovement > maximumU Then
                maximumU = currentExpectedImprovement
                bestPoint = point
            End If
        Next

        Return bestPoint
    End Function

    ''' <summary>
    ''' Determines the current potentially optimal center and rectangles and alternative potentially optimal rectangles
    ''' </summary>
    ''' <param name="alternativePotentiallyOptimal"></param>
    ''' <param name="currentIteration"></param>
    ''' <param name="newPotentiallyOptimalRectangleCenter"></param>
    ''' <param name="newPotentiallyOptimalRectangleDeltas"></param>
    Private Sub findNewPotentiallyOptimalRectangles(alternativePotentiallyOptimal As List(Of Vector(Of Double)), currentIteration As Integer, newPotentiallyOptimalRectangleCenter As List(Of Vector(Of Double)), newPotentiallyOptimalRectangleDeltas As List(Of Vector(Of Double)))
        'Determines which of the previous potentially optimal rectangle center are now potentially optimal to be subdivided into new rectangles
        Dim bestAcqY = observedAcqY.Max
        For i As Integer = 0 To potentiallyOptimalRectangleCenter.Count - 1
            Dim isCurrentPotentiallyOptimal As Boolean = True
            If observedEIValue(potentiallyOptimalRectangleCenter.Item(i)) <
                            bestAcqY + dividingRectanglesEpsilon * Math.Abs(bestAcqY) Then
                isCurrentPotentiallyOptimal = False
                Dim random As New Random()
                If random.NextDouble < dividingRectanglesProbabilityToExplore And currentIteration = dividingRectanglesIterations - 1 Then
                    alternativePotentiallyOptimal.Add(potentiallyOptimalRectangleCenter.Item(i))
                End If
            End If
            If isCurrentPotentiallyOptimal Then
                newPotentiallyOptimalRectangleCenter.Add(potentiallyOptimalRectangleCenter.Item(i))
                newPotentiallyOptimalRectangleDeltas.Add(potentiallyOptimalRectangleDelta.Item(i))
            End If
        Next
    End Sub

    ''' <summary>
    ''' Returns the observed expected improvement function value for a previously queried vector
    ''' </summary>
    ''' <param name="vector">Previously queried vector of double</param>
    ''' <returns></returns>
    Private Function observedEIValue(vector As Vector(Of Double)) As Double
        Dim vectorIndex As Integer = queriedAcqX.IndexOf(vector)
        Return observedAcqY.Item(vectorIndex)
    End Function

    ''' <summary>
    ''' Given a vector of double and a scalar, returns a new vector containing the scalar as a new item added at the last position of the vector.
    ''' </summary>
    ''' <param name="targetVector">The vector where the scalar is to be inserted</param>
    ''' <param name="itemToInsert">Scalar item to be inserted in the vector</param>
    ''' <returns></returns>
    Private Function insertItemToVector(targetVector As Vector(Of Double), itemToInsert As Double) As Vector(Of Double)
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
End Class