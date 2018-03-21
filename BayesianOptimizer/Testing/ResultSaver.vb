Imports System.Globalization
Imports System.IO

Public Class ResultSaver

    Private Shared startTime As Date
    Private Shared optimizerType As String
    Private Shared path As String = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop) + "\"

    ''' <summary>
    ''' Saves the execution of the optimizer as a csv file.
    ''' </summary>
    ''' <param name="numberOfAgentEpisodes"></param>
    ''' <param name="optimizer"></param>
    Public Shared Sub saveOptimizerResultsAsFile(numberOfAgentEpisodes As Integer, optimizer As RLHyperparameterOptimizer, Optional alsoSaveDetailedResults As Boolean = True)
        Dim filePath As String = path +
            Date.Now.Year.ToString + Date.Now.Month.ToString + System.DateTime.Now.Day.ToString +
    " - " + System.DateTime.Now.Hour.ToString + " " + System.DateTime.Now.Minute.ToString + " Simulation Results.csv"

        Try
            Dim objWriter As StreamWriter

            'If a file with the same name exists, erase it
            If My.Computer.FileSystem.FileExists(filePath) Then
                My.Computer.FileSystem.DeleteFile(filePath)
            End If

            File.Create(filePath).Dispose()

            objWriter = New StreamWriter(filePath)

            'id is the identifier of each theta vector, in the order it was queried
            Dim id As Integer = 0
            Dim amountOfHyperparameters As Integer = optimizer.hyperparameters.Count

            'Header of the document is written
            Dim header As String = """id"","

            For i As Integer = 0 To amountOfHyperparameters - 1
                header = header + """theta_" + Convert.ToInt64(i).ToString + ""","
            Next

            header = header + """mean"",""standard_deviation"",""queries"""

            objWriter.WriteLine(header)

            'Data rows are written
            For gpEpisodeNumber As Integer = 0 To optimizer.optimizationEpisodes.Count - 1
                id += 1
                Dim data_row As String = ""

                data_row += id.ToString + ","

                'Hyperparameter vector of the data row is created
                For hyperparameterNumber = 0 To amountOfHyperparameters - 1
                    data_row += optimizer.queriedX.Item(gpEpisodeNumber).Item(hyperparameterNumber).ToString(New CultureInfo("en-Us")) + ","
                Next

                'The results of the query are added into the data_row
                ''Mean
                data_row += optimizer.observedY.Item(gpEpisodeNumber).ToString(New CultureInfo("en-Us")) + ","

                ''Standard deviation
                data_row += Math.Sqrt(optimizer.observedYVariance.Item(gpEpisodeNumber)).ToString(New CultureInfo("en-Us")) + ","

                ''Number of queries
                data_row += optimizer.numberFThetaQueriesPerTheta.Item(gpEpisodeNumber).ToString(New CultureInfo("en-Us"))

                'Data row is written
                objWriter.WriteLine(data_row)
            Next

            objWriter.WriteLine()

            If alsoSaveDetailedResults Then
                saveOptimizerDetailedResultsAsFile(numberOfAgentEpisodes, optimizer)
            End If

            objWriter.Close()

        Catch ex As Exception
            MsgBox("File not saved due to an exception. Results: Xbest = " + optimizer.getBestPoint.ToString + vbCr +
                   "f*(Xbest) = " + optimizer.getBestF.ToString)
        End Try
    End Sub


    ''' <summary>
    ''' Saves the detailed execution (i.e. where the details of each episode are present) of the optimizer as a txt file
    ''' </summary>
    ''' <param name="numberOfAgentEpisodes"></param>
    ''' <param name="optimizer"></param>
    Public Shared Sub saveOptimizerDetailedResultsAsFile(numberOfAgentEpisodes As Integer, optimizer As RLHyperparameterOptimizer)

        Dim filePath As String = path +
            Date.Now.Year.ToString + Date.Now.Month.ToString + System.DateTime.Now.Day.ToString +
    " - " + System.DateTime.Now.Hour.ToString + " " + System.DateTime.Now.Minute.ToString + " Detailed Simulation Results.txt"

        Try
            Dim objWriter As StreamWriter

            'If a file with the same name exists, erase it
            If My.Computer.FileSystem.FileExists(filePath) Then
                My.Computer.FileSystem.DeleteFile(filePath)
            End If

            File.Create(filePath).Dispose()

            objWriter = New StreamWriter(filePath)

            objWriter.WriteLine("---------------  Overview  ---------------------")
            objWriter.WriteLine("")

            objWriter.WriteLine("Optimizer: " + optimizerType)

            If optimizer.isNextQueryDecided And optimizer.nextOptQueryDecisionFunction IsNot Nothing Then
                objWriter.WriteLine("Next BO query decision function: " + optimizer.nextOptQueryDecisionFunction.GetType.ToString())

                If optimizer.similarTaskObservedY IsNot Nothing Then
                    objWriter.WriteLine("Optimizer used observations of Y from similar tasks")
                End If
            End If

            objWriter.WriteLine("Number of Optimizer episodes: " + optimizer.optimizationEpisodes.Count.ToString(New CultureInfo("en-Us")))
            objWriter.WriteLine("")

            Dim bayesianOptimizer = TryCast(optimizer, BayesianOptimizer)

            If bayesianOptimizer IsNot Nothing Then
                objWriter.WriteLine("Gaussian Process Sigma^2_f: " + bayesianOptimizer.sigmaSquaredF.ToString(New CultureInfo("en-Us")))
                objWriter.WriteLine("Gaussian Process Sigma^2_n: " + bayesianOptimizer.sigmaSquaredN.ToString(New CultureInfo("en-Us")))
                objWriter.WriteLine("Gaussian Process l-vector: " + bayesianOptimizer.lVector.ToString.ToString(New CultureInfo("en-Us")))
                objWriter.WriteLine("Covariance Function: " + bayesianOptimizer.covarianceFunction.GetType.ToString())
            End If

            objWriter.WriteLine("")
            objWriter.WriteLine("RL Agent: " + optimizer.reinforcementLearningAgent.GetType.ToString)
            objWriter.WriteLine("Number of RL Agent episodes: " + optimizer.episodesToRun.ToString(New CultureInfo("en-Us")))
            objWriter.WriteLine("Number of minimum RL Agent queries per Optimizer episode: " + optimizer.minimumRLRunsPerQueryPoint.ToString(New CultureInfo("en-Us")))
            objWriter.WriteLine("Number of maximum RL Agent queries per Optimizer episode: " + optimizer.maximumRLRunsPerQueryPoint.ToString(New CultureInfo("en-Us")))
            objWriter.WriteLine("RL Agent cutoff time: " + optimizer.reinforcementLearningAgent.cutoffTime.ToString(New CultureInfo("en-Us")))
            objWriter.WriteLine("")
            objWriter.WriteLine("Finish Time: " + System.DateTime.Now)

            If Not startTime = Nothing Then
                objWriter.WriteLine("Start Time: " + startTime.ToString)
                objWriter.WriteLine("Ellapsed Time: " + (System.DateTime.Now - startTime).ToString)
            End If

            objWriter.WriteLine("")
            objWriter.WriteLine("Xbest = " + optimizer.getBestPoint.ToString().ToString(New CultureInfo("en-Us")) +
                                " at " + optimizer.observedY.AbsoluteMaximumIndex.ToString.ToString(New CultureInfo("en-Us")) +
                                " | " + "f*(Xbest) = " + optimizer.getBestF.ToString.ToString(New CultureInfo("en-Us")))

            objWriter.WriteLine("")
            objWriter.WriteLine("Results Overview")
            objWriter.WriteLine("")


            objWriter.WriteLine("f(X) Mean")
            objWriter.WriteLine("")

            For gpEpisodeNumber As Integer = 0 To optimizer.optimizationEpisodes.Count - 1
                objWriter.WriteLine(optimizer.observedY.Item(gpEpisodeNumber).ToString.ToString(New CultureInfo("en-Us")))
            Next


            objWriter.WriteLine("")
            objWriter.WriteLine("")


            objWriter.WriteLine("f(X) Standard Deviation")
            objWriter.WriteLine("")

            For gpEpisodeNumber As Integer = 0 To optimizer.optimizationEpisodes.Count - 1
                objWriter.WriteLine(Math.Sqrt(optimizer.observedYVariance.Item(gpEpisodeNumber)).ToString.ToString(New CultureInfo("en-Us")))
            Next

            objWriter.WriteLine("")
            objWriter.WriteLine("")
            objWriter.WriteLine("")
            objWriter.WriteLine("")
            objWriter.WriteLine("")

            objWriter.WriteLine("---------------  Detailed results  -------------")
            objWriter.WriteLine("")

            'If the amount of hyperparameters is four (ie not trivial nor used for plotting), displays the complete overview
            If optimizer.hyperparameters.Count > 1 Then
                For gpEpisodeNumber As Integer = 0 To optimizer.optimizationEpisodes.Count - 1
                    objWriter.WriteLine("Optimization Episode " + (gpEpisodeNumber + 1).ToString + vbCr +
                                        " - Number of queries: " +
                                        optimizer.numberFThetaQueriesPerTheta.Item(gpEpisodeNumber).ToString(New CultureInfo("en-Us")) + ", " +
                                        "  - Queried Point: (" +
                                        optimizer.queriedX.Item(gpEpisodeNumber).Item(0).ToString(New CultureInfo("en-Us")) + ", " +
                                        optimizer.queriedX.Item(gpEpisodeNumber).Item(1).ToString(New CultureInfo("en-Us")) + ", " +
                                        optimizer.queriedX.Item(gpEpisodeNumber).Item(2).ToString(New CultureInfo("en-Us")) + ", " +
                                        optimizer.queriedX.Item(gpEpisodeNumber).Item(3).ToString(New CultureInfo("en-Us")) + ") " +
                                        "|| Result: " + optimizer.observedY.Item(gpEpisodeNumber).ToString(New CultureInfo("en-Us")))

                    objWriter.WriteLine("Episode Mean")

                    For rlEpisodeNumber As Integer = 0 To numberOfAgentEpisodes - 1
                        objWriter.WriteLine(
                            Convert.ToDecimal(optimizer.optimizationEpisodes.
                            Item(gpEpisodeNumber).Item(rlEpisodeNumber)), New CultureInfo("en-Us"))
                    Next

                    objWriter.WriteLine("")
                    objWriter.WriteLine("Episode Standard Deviation")

                    For rlEpisodeNumber As Integer = 0 To numberOfAgentEpisodes - 1
                        objWriter.WriteLine(
                            Convert.ToDecimal(Math.Sqrt(optimizer.optimizationEpisodesVariances.
                            Item(gpEpisodeNumber).Item(rlEpisodeNumber))), New CultureInfo("en-Us"))
                    Next

                    objWriter.WriteLine("")
                    objWriter.WriteLine(vbCrLf)
                Next
                objWriter.Close()

            Else
                'If the amount of hyperparameters used is 1, simplifies the overview
                For gpEpisodeNumber As Integer = 0 To optimizer.optimizationEpisodes.Count - 1
                    objWriter.WriteLine("Optimization Episode " + (gpEpisodeNumber + 1).ToString + vbCr + "  - Queried Point: (" +
                                        optimizer.queriedX.Item(gpEpisodeNumber).Item(0).ToString.ToString(New CultureInfo("en-Us")) +
                                        ") " + "|| Result: " + optimizer.observedY.Item(gpEpisodeNumber).ToString.ToString(New CultureInfo("en-Us")))
                    For rlEpisodeNumber As Integer = 0 To numberOfAgentEpisodes - 1
                        objWriter.WriteLine(optimizer.optimizationEpisodes.Item(gpEpisodeNumber).Item(rlEpisodeNumber).ToString(New CultureInfo("en-Us")))
                    Next
                    objWriter.WriteLine(vbCrLf)
                Next
                objWriter.Close()

            End If

        Catch ex As Exception
            MsgBox("File not saved due to an exception. Results: Xbest = " + optimizer.getBestPoint.ToString + vbCr +
                   "f*(Xbest) = " + optimizer.getBestF.ToString)
        End Try
    End Sub

    ''' <summary>
    ''' Logs the type of the optimizer, to later include it in the detailed simulation results file.
    ''' </summary>
    ''' <param name="type"></param>
    Friend Shared Sub registerOptimizerType(type As String)
        optimizerType = type
    End Sub

    ''' <summary>
    ''' Logs the time of the beginning of the execution, so the elapsed time of the execution can be calculated and included in
    ''' the detailed simulation results file.
    ''' </summary>
    Public Shared Sub registerStartTime()
        startTime = System.DateTime.Now
    End Sub

    ''' <summary>
    ''' Saves the result of an agent execution as an csv file 
    ''' </summary>
    Public Shared Sub saveAgentResultsAsFile(agent As Agent)
        Dim filePath As String = path + Date.Now.Year.ToString + Date.Now.Month.ToString + System.DateTime.Now.Day.ToString +
            " - " + System.DateTime.Now.Hour.ToString + " " + System.DateTime.Now.Minute.ToString + " RL Simulation Results.csv"

        Try
            Dim objWriter As StreamWriter

            If My.Computer.FileSystem.FileExists(filePath) Then
                My.Computer.FileSystem.DeleteFile(filePath)
            End If

            File.Create(filePath).Dispose()

            objWriter = New StreamWriter(filePath)

            Dim stepsOfEpisode As List(Of Integer) = agent.getStepsOfEpisode

            For rlEpisode As Integer = 0 To stepsOfEpisode.Count - 1
                objWriter.WriteLine(stepsOfEpisode.Item(rlEpisode))
            Next
            objWriter.WriteLine(vbCrLf)
            objWriter.Close()

        Catch ex As Exception
            MsgBox("Error while saving the simulation")
        End Try
    End Sub

    ''' <summary>
    ''' Saves the agent execution as an csv file, given the list of averages and variances of each agent's episode.
    ''' Note: The save of the variance is not yet implemented.
    ''' </summary>
    ''' <param name="stepsOfEpisodeAverage"></param>
    ''' <param name="stepsOfEpisodeVariance"></param>
    Public Shared Sub saveAgentResultsAsFile(stepsOfEpisodeAverage As List(Of Double), stepsOfEpisodeVariance As List(Of Double))
        Dim filePath As String = path + Date.Now.Year.ToString + Date.Now.Month.ToString + System.DateTime.Now.Day.ToString +
    " - " + System.DateTime.Now.Hour.ToString + " " + System.DateTime.Now.Minute.ToString + " RL Simulation Results.csv"

        Try
            Dim objWriter As StreamWriter

            If My.Computer.FileSystem.FileExists(filePath) Then
                My.Computer.FileSystem.DeleteFile(filePath)
            End If

            File.Create(filePath).Dispose()

            objWriter = New StreamWriter(filePath)

            For rlEpisode As Integer = 0 To stepsOfEpisodeAverage.Count - 1
                objWriter.WriteLine(stepsOfEpisodeAverage.Item(rlEpisode))
            Next
            objWriter.WriteLine(vbCrLf)
            objWriter.Close()

        Catch ex As Exception
            MsgBox("Error while saving the simulation")
        End Try
    End Sub
End Class
