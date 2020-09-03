Imports rhapsody2
Imports System.IO
Imports System.Windows
Imports System.Xml.Serialization

Public Class Rpy_Model_Controller
    Inherits Rpy_Controller

    Private Model As Software_Model_Container

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
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
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


    ' Import a model described in a xml file and merge it with the existing model in Rhapsody.
    ' Two elements from the Rhaspody model or from the xml model are merged if they match "by name",
    ' i.e. if they have the same path.
    ' If UUID are differents, UUID from Rhaspody model is taken.
    ' If Description of the xml model is empty, but not the Description from the Rhapsody model, the
    ' Description from Rhaspody is kept.
    Public Sub Import_And_Merge_By_Name()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Import and merge by name..." & vbCrLf)

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

            ' Create string of the date for created csv file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Create report
            ' Select csv file directory
            Dim output_directory As String
            output_directory = Select_Directory(Rhapsody_App, "model importation report file")
            If output_directory = "" Then
                Rhapsody_App.writeToOutputWindow("out", " no directory selected." & vbCrLf)
            Else

                Me.Model = New Software_Model_Container
                Rhapsody_App.writeToOutputWindow("out", "Get software model from XML file...")
                ' Open XML file
                Dim file_stream As New FileStream(xml_file_path, FileMode.Open)
                ' Deserialize XML file
                Dim deserialization_status As Boolean = False
                Dim serializer As New XmlSerializer(GetType(Software_Model_Container))
                Try
                    Me.Model =
                        CType(serializer.Deserialize(file_stream), Software_Model_Container)
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)
                    deserialization_status = True
                Catch
                    Rhapsody_App.writeToOutputWindow("out",
                        "Invalid xml file, cannot perform merge." & vbCrLf)
                End Try
                ' Close stuff
                file_stream.Close()

                If deserialization_status = True Then
                    Rhapsody_App.writeToOutputWindow("out", "Merge models...")
                    Me.Model.Export_To_Rhapsody(rpy_sw_mdl)
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    ' Open csv file
                    Rhapsody_App.writeToOutputWindow("out", "Create report file...")
                    Dim file_name As String =
                        rpy_sw_mdl.name & "_Importation_Report_" & date_str & ".csv"
                    Dim file_path As String = output_directory & "\" & file_name

                    Dim report_file_stream As New StreamWriter(file_path, False)

                    Me.Model.Generate_Importation_Report(report_file_stream)

                    report_file_stream.Close()
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    Rhapsody_App.writeToOutputWindow(
                        "out",
                        "Importation report file full path : " & file_path & vbCrLf)

                End If

            End If

        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Importation end." & vbCrLf)
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
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
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


    Public Sub Compute_Rpy_Soft_Model_PSWA_Metrics()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Compute PSWA metrics..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Get model from Rhapsody
            Rhapsody_App.writeToOutputWindow("out", "Get model from Rhapsody...")
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
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
                Me.Model.Compute_PSWA_Metrics()
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
                        rpy_sw_mdl.name & "_PSWA_Metrics_Report_" & date_str & ".txt"
                    Dim file_path As String = output_directory & "\" & file_name

                    Dim file_stream As New StreamWriter(file_path, False)

                    Me.Model.Generate_PSWA_Metrics_Report(file_stream)

                    file_stream.Close()
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    Rhapsody_App.writeToOutputWindow(
                        "out",
                        "Metrics report file full path : " & file_path & vbCrLf)
                End If
            End If

        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "PSWA metrics computation end." & vbCrLf)
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
                Me.Model = New Software_Model_Container
                Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
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
                    Me.Model.Compute_PSWA_Metrics()
                    Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                    ' Open txt file
                    Rhapsody_App.writeToOutputWindow("out", "Create report file...")
                    file_name = rpy_sw_mdl.name & "_Metrics_Report_" & date_str & ".txt"
                    file_path = output_directory & "\" & file_name

                    stream_writer = New StreamWriter(file_path, False)

                    Me.Model.Generate_PSWA_Metrics_Report(stream_writer)

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


    ' Remove empty packages from the Rhapsody model.
    ' It deals onmly with PSWA_Packages.
    ' A package is considered as empty if it aggregates only packages that recursively only 
    ' aggregates packages.
    Public Sub Remove_Empty_Packages()
        ' Initialize output window and display start message
        Dim chrono As New Global.System.Diagnostics.Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Remove empty PSWA_Packages..." & vbCrLf)

        ' Get selected element and check that it is a Rhapsody project
        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project(Rhapsody_App)

        If Not IsNothing(rpy_sw_mdl) Then
            Dim root_level_package As RPPackage
            For Each root_level_package In rpy_sw_mdl.packages
                If Is_PSWA_Package(CType(root_level_package, RPModelElement)) Then
                    PSWA_Package.Remove_Empty_Packages(root_level_package)
                End If
            Next
        End If

        ' Display result to output window
        Rhapsody_App.writeToOutputWindow("out", "End removing empty PSWA_Packages." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub

End Class
