

Imports MathNet.Numerics.LinearAlgebra

Public Class BayesianOptimizer : Inherits RLHyperparameterOptimizer

    '-------------------------------------------------------
    '
    'Gaussian Process
    '-------------------------------------------------------
    'm, or u (mu) sub-zero is the vector that contains the prior mean for the objective function.
    Public Property u0 As Vector(Of Double)
    'The prior mean assumed in the GP. In this version, prior mean is the scalar equivalent of u0.
    Public Property priorMean As Double
    'K is the Kernel Matrix.
    Public Property K As Matrix(Of Double)
    '(K + sigmaI)^-1, is the inverted K matrix that takes in account the noise. This matrix is stored to avoid unnecessary matrix invertions when
    'the matrix order becomes too large.
    Public Property invertedK As Matrix(Of Double)
    '
    '-------------------------------------------------------
    '
    'Hyperparameters
    '-------------------------------------------------------
    '
    ''Gaussian Process
    'Function that determines the covariance between two points (vectors)
    Public Property covarianceFunction As CovarianceFunction
    'Function that optimizes the log-likelihood of the Gaussian Process
    Public Property logLikelihoodOptimizationFunction As LogLikelihoodOptimizationFunction
    'l-vector is the vector of the hyperparameters that tune the GP in an anisotropic model
    Public Property lVector As Vector(Of Double)
    'σ^2-sub-f, or sigma-squared-sub-f is the variance of the (noise free) signal f, hyperparameter of the GP.
    Public Property sigmaSquaredF As Double
    'σ^2-sub-n, or sigma-squared-sub-n is the noise level, hyperparameter of the GP. It is the variance of the noise, i.e., y = f(X) + ε, where ε ~ N(0, σ^2-sub-n)
    Public Property sigmaSquaredN As Double
    '
    '-------------------------------------------------------
    '
    'Acquisition Function (also called infill function)
    '-------------------------------------------------------
    'The selected acquisition function to be used
    Public Property acquisitionFunction As AcquisitionFunction
    'The function selected to optimize the acquisition function
    Public Property acquisitionOptimizationFunction As AcquisitionOptimizationFunction


    ''' <summary>
    ''' Initializes the Bayesian Optimizer.
    ''' </summary>
    ''' <param name="priorMean">The prior mean of all the hyperparameters.</param>
    ''' <param name="sigmaSquaredF">Variance of the noiseless signal f.</param>
    ''' <param name="sigmaSquaredN">Noise level.</param>
    Public Sub New(reinforcementLearningAgent As Agent,
                   acquisitionFunction As AcquisitionFunction,
                   acquisitionOptimizationFunction As AcquisitionOptimizationFunction,
                   covarianceFunction As CovarianceFunction,
                   lVector As Vector(Of Double),
                   Optional logLikelihoodOptimizationFunction As LogLikelihoodOptimizationFunction = Nothing,
                   Optional priorMean As Double = 0,
                   Optional sigmaSquaredF As Double = 0.8,
                   Optional sigmaSquaredN As Double = 0.17,
                   Optional episodesToRunRLAgent As Integer = 50,
                   Optional latinHypercubeTrainingSteps As Integer = 0,
                   Optional maximumRLRunsPerQueryPoint As Integer = 5,
                   Optional isNextQueryDecided As Boolean = False,
                   Optional minimumRLRunsPerQueryPoint As Integer = 1,
                   Optional nextBOQueryDecisionFunction As String = "UCB1Tuned",
                   Optional similarTaskObservedY As Vector(Of Double) = Nothing,
                   Optional nextBOQueryDecisionFunctionParameter As Double = 0.2)

        hyperparameters = Vector(Of Double).Build.Sparse(reinforcementLearningAgent.getHyperparameterCount)
        Me.reinforcementLearningAgent = reinforcementLearningAgent
        Me.sigmaSquaredN = sigmaSquaredN
        Me.sigmaSquaredF = sigmaSquaredF
        episodesToRun = episodesToRunRLAgent
        Me.acquisitionFunction = acquisitionFunction
        Me.covarianceFunction = covarianceFunction
        Me.lVector = lVector
        Me.priorMean = priorMean
        Me.acquisitionOptimizationFunction = acquisitionOptimizationFunction
        Me.logLikelihoodOptimizationFunction = logLikelihoodOptimizationFunction
        Me.maximumRLRunsPerQueryPoint = maximumRLRunsPerQueryPoint
        Me.minimumRLRunsPerQueryPoint = minimumRLRunsPerQueryPoint
        Me.isNextQueryDecided = isNextQueryDecided
        Me.similarTaskObservedY = similarTaskObservedY

        Select Case nextBOQueryDecisionFunction
            Case "UCB1TunedStandarized"
                Me.nextOptQueryDecisionFunction = New UCB1TunedStandarized(minimumRLRunsPerQueryPoint)
                Exit Select
            Case "UCB1Tuned"
                Me.nextOptQueryDecisionFunction = New UCB1Tuned(minimumRLRunsPerQueryPoint)
                Exit Select
            Case "UCB1"
                Me.nextOptQueryDecisionFunction = New UCB1(minimumRLRunsPerQueryPoint)
                Exit Select
            Case "UCB1Variance"
                Me.nextOptQueryDecisionFunction = New UCB1Variance(minimumRLRunsPerQueryPoint)
                Exit Select
            Case "EpsilonGreedy"
                Me.nextOptQueryDecisionFunction = New EpsilonGreedy(minimumRLRunsPerQueryPoint, nextBOQueryDecisionFunctionParameter)
                Exit Select
            Case "Softmax"
                Me.nextOptQueryDecisionFunction = New Softmax(minimumRLRunsPerQueryPoint, nextBOQueryDecisionFunctionParameter)
                Exit Select
            Case ""
                Exit Select
            Case Else
                Throw New NotSupportedException
        End Select

        trainBO(latinHypercubeTrainingSteps)
    End Sub

    ''' <summary>
    ''' Trains the BO by adding a representative sample of points in it. This is done to reduce the high variance that appears in the first BO episodes, increasing the quality
    ''' of the process by improving the performance of acquisition function.
    ''' </summary>
    ''' <param name="latinHypercubeTrainingSteps"></param>
    Private Sub trainBO(latinHypercubeTrainingSteps As Integer)
        Dim latinHypercube As New LatinHypercubeSampling(hyperparameters.Count, latinHypercubeTrainingSteps)
        Dim latinHypercubeSamples As New List(Of Vector(Of Double))

        For i As Integer = 0 To latinHypercubeTrainingSteps - 1
            addNewPoint(latinHypercube.nextSample)
        Next
    End Sub

    ''' <summary>
    ''' Runs the BO algorithm for a certain number of episodes (points to test the function Y), in attempt to maximize the unknown function f(X) (X = (x1, x2, ..., xn)).
    ''' </summary>
    Public Overrides Sub runOptimizer(numberOfEpisodes As Integer)
        For i As Integer = 0 To numberOfEpisodes - 1
            Dim pointToSample As Vector(Of Double) = Vector(Of Double).Build.Sparse(hyperparameters.Count) 'Build the new vector with lenght = quantity of RL hyperparameters
            pointToSample = acquisitionOptimizationFunction.optimize(Me)
            addNewPoint(pointToSample)
        Next
    End Sub

    ''' <summary>
    ''' Adds a new point to the Gaussian Process. Adds y to Y, adds the new m, the new variance and recalculaes the K matrix.
    ''' This sub is public so a priori query points can be added to the GP, if desired.
    ''' </summary>
    Public Overrides Sub addNewPoint(X As Vector(Of Double))
        queriedX.Add(X)
        Dim queryResult As List(Of Double) = queryX(X)

        observedY = UtilityFunctions.insertItemToVector(observedY, queryResult.Item(0))
        observedYVariance = UtilityFunctions.insertItemToVector(observedYVariance, queryResult.Item(1))

        u0 = UtilityFunctions.insertItemToVector(u0, priorMean)
        hyperparameters = X
        addNewPointToK(X)
        optimizeGPHyperparameters()
    End Sub

    ''' <summary>
    ''' Adds a new (X, y) pair to the Gaussian Process. Instead of query the agent to obtain y, it directly adds it.
    ''' </summary>
    ''' <param name="X"></param>
    Public Sub addNewXYPair(X As Vector(Of Double), y As Double)
        queriedX.Add(X)
        observedY = UtilityFunctions.insertItemToVector(observedY, y)

        u0 = UtilityFunctions.insertItemToVector(u0, priorMean)
        hyperparameters = X
        addNewPointToK(X)
        optimizeGPHyperparameters()
    End Sub


    ''' <summary>
    ''' Calculates the mean Un(Z) of a test point Z in the current surrogate function generated by the GP, as shown in Shahriari et. al. 2015 p. 10.
    ''' </summary>
    ''' <returns></returns>
    Public Function calculateMeanOfTestPoint(Z As Vector(Of Double)) As Double
        'If there is no points queried, returns the prior mean
        If queriedX.Count = 0 Then
            Return priorMean
        Else
            'This vector contains each (k(Z, X1), k(Z, X2), ..., k(Z, Xn))
            Dim zetaCovarianceRowVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(queriedX.Count)

            For i As Integer = 0 To queriedX.Count - 1
                zetaCovarianceRowVector.Item(i) = covarianceFunction.k(Z, queriedX.Item(i), sigmaSquaredF, sigmaSquaredN, lVector)
            Next

            Return priorMean + (zetaCovarianceRowVector * invertedK * (observedY - u0))
        End If
    End Function

    ''' <summary>
    ''' Calculates the variance (sigma-squared) sigma^2n(Z) of a test point Z in the current surrogate function generated by the GP, as shown in Shahriari et. al. 2015 p. 10.
    ''' </summary>
    ''' <returns></returns>
    Public Function calculateVarianceOfTestPoint(Z As Vector(Of Double)) As Double
        'If no points have been queried yet, returns the noise level.
        If queriedX.Count = 0 Then
            Return 0
        Else

            'This vector contains each (k(Z, X1), k(Z, X2), ..., k(Z, Xn))
            Dim zetaCovarianceRowVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(queriedX.Count)
            Dim zetaCovarianceColumnVector As Vector(Of Double) = Vector(Of Double).Build.Sparse(queriedX.Count)

            For i As Integer = 0 To queriedX.Count - 1
                zetaCovarianceRowVector.Item(i) = covarianceFunction.k(Z, queriedX.Item(i), sigmaSquaredF, sigmaSquaredN, lVector)
            Next

            For i As Integer = 0 To queriedX.Count - 1
                zetaCovarianceColumnVector.Item(i) = covarianceFunction.k(queriedX.Item(i), Z, sigmaSquaredF, sigmaSquaredN, lVector)
            Next

            Dim zetaCovarianceColumnMatrix As Matrix(Of Double) = zetaCovarianceColumnVector.ToColumnMatrix
            Dim variance As Double = covarianceFunction.k(Z, Z, sigmaSquaredF, sigmaSquaredN, lVector) - (zetaCovarianceRowVector * invertedK * zetaCovarianceColumnMatrix).Item(0)
            If variance >= 0 Then
                Return variance
            Else
                Return 0
            End If
        End If
    End Function

    ''' <summary>
    ''' Calculates the log-likelihood of the Gaussian Process, as seen in Rasmussen and Williams (2006) p. 19.
    ''' </summary>
    ''' <returns></returns>
    Public Function calculateLogLikelihood() As Double
        Dim identityMatrix As Matrix(Of Double) = Matrix(Of Double).Build.DiagonalIdentity(K.RowCount)
        Dim n As Double = K.RowCount

        Return -0.5 * ((observedY - u0).ToRowMatrix * invertedK * (observedY - u0).ToColumnMatrix).Item(0, 0) - 0.5 * Math.Log((K + sigmaSquaredN * identityMatrix).Determinant) - (n / 2) * Math.Log(2 * Math.PI)

    End Function

    ''' <summary>
    ''' Change the hyperparameters of the GP, recalculating all the K matrix with the new values.
    ''' </summary>
    Public Sub setNewGPHyperparameters(sigmaSquaredF As Double, sigmaSquaredN As Double, lVector As Vector(Of Double))
        Me.sigmaSquaredF = sigmaSquaredF
        Me.sigmaSquaredN = sigmaSquaredN
        Me.lVector = lVector

        'K matrix is cleared
        K.Clear()

        For i As Integer = 0 To queriedX.Count - 1
            For j As Integer = 0 To queriedX.Count - 1
                K.Item(i, j) = covarianceFunction.k(queriedX.Item(i), queriedX.Item(j), sigmaSquaredF, sigmaSquaredN, lVector)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Adds a new tested point to the covariance matrix and to the vector of queried points.
    ''' </summary>
    ''' <param name="X">New tested point to incorporate in K</param>
    Private Sub addNewPointToK(X As Vector(Of Double))
        If K IsNot Nothing Then
            Dim newKColumn As Vector(Of Double) = Vector(Of Double).Build.Sparse(queriedX.Count - 1, 0)
            'newkrow is the vector that adds k(X,X) for the new point X.
            Dim newKRow As Vector(Of Double) = Vector(Of Double).Build.Sparse(queriedX.Count, 0)

            For i As Integer = 0 To queriedX.Count - 2
                newKColumn.Item(i) = covarianceFunction.k(queriedX.Item(i), X, sigmaSquaredF, sigmaSquaredN, lVector)
            Next

            For i As Integer = 0 To queriedX.Count - 1
                newKRow.Item(i) = covarianceFunction.k(X, queriedX.Item(i), sigmaSquaredF, sigmaSquaredN, lVector)
            Next

            newKRow.Item(queriedX.Count - 1) = covarianceFunction.k(X, X, sigmaSquaredF, sigmaSquaredN, lVector)

            K = K.InsertColumn(K.ColumnCount, newKColumn)
            K = K.InsertRow(K.RowCount, newKRow)

            Dim identityMatrix As Matrix(Of Double) = Matrix(Of Double).Build.DiagonalIdentity(K.RowCount)
            invertedK = K.Inverse
        Else
            'If X is the first point added to the matrix, K is initialized
            K = Matrix(Of Double).Build.Sparse(1, 1, covarianceFunction.k(X, X, sigmaSquaredF, sigmaSquaredN, lVector))

            invertedK = K.Inverse
        End If
    End Sub

    ''' <summary>
    ''' Optimizes the hyperparameters of the Gaussian Process, recalculating the K matrix according to the best parameters found.
    ''' </summary>
    Private Sub optimizeGPHyperparameters()
        If logLikelihoodOptimizationFunction IsNot Nothing Then
            logLikelihoodOptimizationFunction.optimize(Me)
            setNewGPHyperparameters(logLikelihoodOptimizationFunction.sigmaSquaredF, logLikelihoodOptimizationFunction.sigmaSquaredN, logLikelihoodOptimizationFunction.lVector)
        End If
    End Sub
End Class