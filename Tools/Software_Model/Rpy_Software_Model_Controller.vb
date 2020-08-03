Imports rhapsody2
Imports System.IO

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
            output_directory = Select_Directory(Rhapsody_App, "csv file")
            If output_directory = "" Then
                Rhapsody_App.writeToOutputWindow("out", " no directory selected." & vbCrLf)
            Else
                ' Open csv file
                Rhapsody_App.writeToOutputWindow("out", "Create csv file...")
                Dim file_name As String =
                    rpy_sw_mdl.name & "_Consistency_Report_" & date_str & ".csv"
                Dim file_path As String = output_directory & "\" & file_name

                Dim file_stream As New StreamWriter(file_path, False)

                Me.Model.Generate_Consistency_Report(file_stream)

                file_stream.Close()
                Rhapsody_App.writeToOutputWindow("out", " done." & vbCrLf)

                Rhapsody_App.writeToOutputWindow(
                    "out",
                    "csv file full path : " & file_path & vbCrLf)
            End If


        End If

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "Export end." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))

    End Sub


    Private Function Get_Rhapsody_Project(rpy_app As RPApplication) As RPProject
        Dim selected_element As RPModelElement
        selected_element = rpy_app.getSelectedElement
        Dim rpy_sw_mdl As rpProject = Nothing
        If Not IsNothing(selected_element) Then
            If IsNothing(selected_element.owner) Then
                rpy_sw_mdl = CType(selected_element, rpProject)
            Else
                rpy_app.writeToOutputWindow("out", "A Rhapsody project shall be selected." & vbCrLf)
            End If
        Else
            rpy_app.writeToOutputWindow("out", "A Rhapsody project shall be selected." & vbCrLf)
        End If
        Return rpy_sw_mdl
    End Function

    Private Function Select_Directory(rpy_app As RPApplication, file_message As String) As String
        Dim output_directory As String = ""
        rpy_app.writeToOutputWindow("out", "Select " & file_message & " directory...")
            Dim dialog_box As FolderBrowserDialog
            dialog_box = New FolderBrowserDialog
            Dim result As Global.System.Windows.Forms.DialogResult = dialog_box.ShowDialog()
            If result = Global.System.Windows.Forms.DialogResult.OK Then
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
