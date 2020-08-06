Imports rhapsody2
Imports System.IO
Imports System.Windows

Class Rpy_Software_Model_Controller

    Private Rhapsody_App As RPApplication = Nothing
    Private Model As Rpy_Software_Model

    Public Sub New()
        ' Get the opened Rhapsody application.
        Me.Rhapsody_App = DirectCast(GetObject(Nothing, "Rhapsody2.Application"), RPApplication)
    End Sub


    ' Export the Rhapsody model in a xml file.
    ' The Rhapsody model select element shall be the root (Rhapsody project).
    ' The user is asked to choose the directory where the xml file will be created.
    Public Sub Export_Rpy_Soft_Model_To_Xml()

        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Export software model to xml..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created xml file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Get model from Rhapsody
            Rhapsody_App.writeToOutputWindow("out", "Get model from Rhapsody...")
            Me.Model = New Rpy_Software_Model
            Me.Model.Load_From_Rhapsody_Model(rpy_sw_mdl)
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            ' Select xml file directory
            Dim output_directory As String
            output_directory = Select_Directory(Rhapsody_App, "xml file")
            If output_directory = "" Then
                Rhapsody_App.writeToOutputWindow("out", " no directory selected." & vbCrLf)
            Else
                ' Open XML file
                Rhapsody_App.writeToOutputWindow("out", "Create xml file...")
                Dim file_name As String = rpy_sw_mdl.name & "_" & date_str & ".xml"
                Dim file_path As String = output_directory & "\" & file_name
                Dim file_stream As New FileStream(file_path, FileMode.Create)

                ' Serialize model
                Me.Model.Create_Xml(file_stream)

                file_stream.Close()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                Rhapsody_App.writeToOutputWindow("out", "xml file full path : " & file_path & vbCrLf)
            End If

        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Export end." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))

    End Sub


    Public Sub Merge_Rpy_Soft_Model()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Merge software model..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then

            ' Get XML file
            Rhapsody_App.writeToOutputWindow("out", "Get XML file to merge...")
            Dim xml_file_path As String
            xml_file_path = Select_File("Select XML file", "XML file|*.xml")
            If xml_file_path = "" Then
                Rhapsody_App.writeToOutputWindow("out", "no file selected." & vbCrLf)
                Rhapsody_App.writeToOutputWindow("OUT", "Merge end." & vbCrLf)
                Exit Sub
            Else
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)
            End If

            ' Get model from Rhapsody
            Rhapsody_App.writeToOutputWindow("out", "Get model from Rhapsody...")
            Me.Model = New Rpy_Software_Model
            Me.Model.Load_From_Rhapsody_Model(rpy_sw_mdl)
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            ' Check model
            Rhapsody_App.writeToOutputWindow("out", "Check model...")
            Me.Model.Check_Consistency()
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            If Me.Model.Has_Error Then
                Rhapsody_App.writeToOutputWindow("out",
                    "Model has errors, cannot perform merge." & vbCrLf)
            Else
                Rhapsody_App.writeToOutputWindow("out", "Get software model from XML file...")
                ' Open XML file
                Dim file_stream As New FileStream(xml_file_path, FileMode.Open)
                ' Deserialize XML file
                Dim soft_mdl_from_file As New Rpy_Software_Model
                Dim deserialization_status As Boolean
                deserialization_status = soft_mdl_from_file.Load_From_Xml_File(file_stream)
                ' Close stuff
                file_stream.Close()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)
                If deserialization_status = False Then
                    Rhapsody_App.writeToOutputWindow("out",
                        "Invalid xml file, cannot perform merge." & vbCrLf)
                Else
                    Me.Model.Merge(soft_mdl_from_file)
                End If
            End If

        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Merge end." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub


    Public Sub Check_Rpy_Soft_Model()

        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Check software model consistency..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created csv file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Get model from Rhapsody
            Rhapsody_App.writeToOutputWindow("out", "Get model from Rhapsody...")
            Me.Model = New Rpy_Software_Model
            Me.Model.Load_From_Rhapsody_Model(rpy_sw_mdl)
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            ' Check model
            Rhapsody_App.writeToOutputWindow("out", "Check model...")
            Me.Model.Check_Consistency()
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            ' Create report
            ' Select csv file directory
            Dim output_directory As String
            output_directory = Select_Directory(Rhapsody_App, "model consistency report file")
            If output_directory = "" Then
                Rhapsody_App.writeToOutputWindow("out", " no directory selected." & vbCrLf)
            Else
                ' Open csv file
                Rhapsody_App.writeToOutputWindow("out", "Create report file...")
                Dim file_name As String =
                    rpy_sw_mdl.name & "_Consistency_Report_" & date_str & ".csv"
                Dim file_path As String = output_directory & "\" & file_name

                Dim file_stream As New StreamWriter(file_path, False)

                Me.Model.Generate_Consistency_Report(file_stream)

                file_stream.Close()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                Rhapsody_App.writeToOutputWindow(
                    "out",
                    "Consistency report file full path : " & file_path & vbCrLf)
            End If


        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Export end." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))

    End Sub


    Public Sub Compute_Rpy_Soft_Model_Metrics()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Compute software model metrics..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Get model from Rhapsody
            Rhapsody_App.writeToOutputWindow("out", "Get model from Rhapsody...")
            Me.Model = New Rpy_Software_Model
            Me.Model.Load_From_Rhapsody_Model(rpy_sw_mdl)
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            ' Check model
            Rhapsody_App.writeToOutputWindow("out", "Check model...")
            Me.Model.Check_Consistency()
            Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

            If Me.Model.Has_Error Then
                Rhapsody_App.writeToOutputWindow("out",
                    "Model has errors, cannot compute metrics." & vbCrLf)
            Else
                Rhapsody_App.writeToOutputWindow("out", "Compute model metrics...")
                Me.Model.Compute_Metrics()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

    '            Create report
                ' Select txt file directory
                Dim output_directory As String
                output_directory = Select_Directory(Rhapsody_App, "model metrics report file")
                If output_directory = "" Then
                    Rhapsody_App.writeToOutputWindow("out", " no directory selected." & vbCrLf)
                Else
                    ' Open txt file
                    Rhapsody_App.writeToOutputWindow("out", "Create report file...")
                    Dim file_name As String =
                        rpy_sw_mdl.name & "_Metrics_Report_" & date_str & ".txt"
                    Dim file_path As String = output_directory & "\" & file_name

                    Dim file_stream As New StreamWriter(file_path, False)

                    Me.Model.Generate_Metrics_Report(file_stream)

                    file_stream.Close()
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    Rhapsody_App.writeToOutputWindow(
                        "out",
                        "Metrics report file full path : " & file_path & vbCrLf)
                End If
            End If

        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Metrics computation end." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub


    Public Sub Generate_All()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Generate all documents..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created files
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Select txt file directory
            Dim output_directory As String
            output_directory = Select_Directory(Rhapsody_App, "all documents")
            If output_directory = "" Then
                Rhapsody_App.writeToOutputWindow("out", " no directory selected." & vbCrLf)
            Else

                '----------------------------------------------------------------------------------'
                ' Model extraction
                ' Get model from Rhapsody
                Rhapsody_App.writeToOutputWindow("out", "Get model from Rhapsody...")
                Me.Model = New Rpy_Software_Model
                Me.Model.Load_From_Rhapsody_Model(rpy_sw_mdl)
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                ' Open XML file
                Rhapsody_App.writeToOutputWindow("out", "Create xml file...")
                Dim file_name As String = rpy_sw_mdl.name & "_" & date_str & ".xml"
                Dim file_path As String = output_directory & "\" & file_name
                Dim file_stream As New FileStream(file_path, FileMode.Create)

                ' Serialize model
                Me.Model.Create_Xml(file_stream)

                file_stream.Close()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                Rhapsody_App.writeToOutputWindow("out",
                    "xml file full path : " & file_path & vbCrLf)

                '----------------------------------------------------------------------------------'
                ' Consistency check
                ' Check model
                Rhapsody_App.writeToOutputWindow("out", "Check model...")
                Me.Model.Check_Consistency()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                ' Open csv file
                Rhapsody_App.writeToOutputWindow("out", "Create report file...")
                file_name = rpy_sw_mdl.name & "_Consistency_Report_" & date_str & ".csv"
                file_path = output_directory & "\" & file_name

                Dim stream_writer As New StreamWriter(file_path, False)

                Me.Model.Generate_Consistency_Report(stream_writer)

                stream_writer.Close()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                Rhapsody_App.writeToOutputWindow("out",
                    "Consistency report file full path : " & file_path & vbCrLf)

                '----------------------------------------------------------------------------------'
                ' Metrics computation
                If Me.Model.Has_Error Then
                    Rhapsody_App.writeToOutputWindow("out",
                        "Model has errors, cannot compute metrics." & vbCrLf)
                Else
                    Rhapsody_App.writeToOutputWindow("out", "Compute model metrics...")
                    Me.Model.Compute_Metrics()
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    ' Open txt file
                    Rhapsody_App.writeToOutputWindow("out", "Create report file...")
                    file_name = rpy_sw_mdl.name & "_Metrics_Report_" & date_str & ".txt"
                    file_path = output_directory & "\" & file_name

                    stream_writer = New StreamWriter(file_path, False)

                    Me.Model.Generate_Metrics_Report(stream_writer)

                    stream_writer.Close()
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    Rhapsody_App.writeToOutputWindow("out",
                        "Metrics report file full path : " & file_path & vbCrLf)
                End If

            End If

        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Generate all documents end." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub


    Private Function Get_Rhapsody_Project(rpy_app As RPApplication) As RPProject
        Dim selected_element As RPModelElement
        selected_element = rpy_app.getSelectedElement
        Dim rpy_sw_mdl As RPProject = Nothing
        If Not IsNothing(selected_element) Then
            If IsNothing(selected_element.owner) Then
                rpy_sw_mdl = CType(selected_element, RPProject)
            Else
                rpy_app.writeToOutputWindow("out", "A Rhapsody project shall be selected." & vbCrLf)
            End If
        Else
            rpy_app.writeToOutputWindow("out", "A Rhapsody project shall be selected." & vbCrLf)
        End If
        Return rpy_sw_mdl
    End Function

    Private Function Select_File(title As String, filter As String) As String
        Dim file_path As String = ""
        Dim dialog_box As OpenFileDialog
        dialog_box = New OpenFileDialog
        dialog_box.Title = title
        dialog_box.Filter = filter
        dialog_box.CheckFileExists = True
        Dim result As Forms.DialogResult = dialog_box.ShowDialog
        If result = Forms.DialogResult.OK Then
            file_path = dialog_box.FileName
        End If
        Return file_path
    End Function

    Private Function Select_Directory(rpy_app As RPApplication, file_message As String) As String
        Dim output_directory As String = ""
        rpy_app.writeToOutputWindow("out", "Select " & file_message & " directory...")
            Dim dialog_box As FolderBrowserDialog
            dialog_box = New FolderBrowserDialog
            dialog_box.Description = "Select " & file_message & " directory..."
            Dim result As Forms.DialogResult = dialog_box.ShowDialog()
            If result = Forms.DialogResult.OK Then
                output_directory = dialog_box.SelectedPath
                rpy_app.writeToOutputWindow("out", " done." & vbCrLf)
            End If
        Return output_directory
    End Function

    Private Function Get_Elapsed_Time(time As Stopwatch) As String
        Dim time_str As String
        time_str = "Elapsed time : " & _
                   time.Elapsed.Hours.ToString & "h " & _
                   time.Elapsed.Minutes.ToString & "m " & _
                   time.Elapsed.Seconds.ToString & "s."
        Return time_str
    End Function

End Class
