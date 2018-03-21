Module Main

    ''' <summary>
    ''' Main class of the solution, from where the simulations are called.
    ''' </summary>
    Public Sub Main()

        Dim testClassRandomSearch = New TestRandomSearch()
        testClassRandomSearch.randomSearchOptimizerWithSuccessMeasure()
        testClassRandomSearch.randomSearchOptimizerStepsPerEpisodeMeasure()

        Dim testClassGridSearch = New TestGridSearch()
        testClassGridSearch.gridSearchOptimizerStepsPerEpisodeMeasure()
        testClassGridSearch.gridSearchOptimizerWithSuccessMeasure()

        Dim testClass = New TestBayesianOptimizer()
        testClass.testBayesianOptimizerStepPerEpisodeMeasureAndSoftmax()

        testClass = New TestBayesianOptimizer()
        testClass.testBayesianOptimizerStepPerEpisodeMeasureAndGreedy()

        testClass = New TestBayesianOptimizer()
        testClass.testBayesianOptimizerStepPerEpisodeMeasureAndEpsilonGreedy()

        testClass = New TestBayesianOptimizer()
        testClass.testBayesianOptimizerStepsPerEpisodeMeasureAndUCB1()

        testClass = New TestBayesianOptimizer()
        testClass.testBayesianOptimizerStepsPerEpisodeMeasureAndUCB1Tuned()

        testClass = New TestBayesianOptimizer()
        testClass.testBayesianOptimizerStepsPerEpisodeMeasureAndUCB1Variance()

    End Sub


    ''' <summary>
    ''' Randomizes the contents of the list using Fisher–Yates shuffle (a.k.a. Knuth shuffle).
    ''' Credit to Paul Lammertsma for his answer at
    ''' https://stackoverflow.com/questions/554587/is-there-an-easy-way-to-randomize-a-list-in-vb-net#554626
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list"></param>
    ''' <returns>Randomized result</returns>
    ''' <remarks></remarks>
    <Runtime.CompilerServices.Extension()>
    Function Randomize(Of T)(ByVal list As List(Of T)) As List(Of T)
        Dim rand = UtilityFunctions.randomGenerator
        Dim temp As T
        Dim indexRand As Integer
        Dim indexLast As Integer = list.Count - 1
        For index As Integer = 0 To indexLast
            indexRand = rand.Next(index, indexLast)
            temp = list(indexRand)
            list(indexRand) = list(index)
            list(index) = temp
        Next index
        Return list
    End Function

End Module
