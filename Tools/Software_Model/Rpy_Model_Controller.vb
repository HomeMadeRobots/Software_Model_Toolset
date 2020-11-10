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
        Me.Clear_Window()
        Me.Write_Csl_Line("Export software model to xml...")

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created xml file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Select xml file directory
            Dim output_directory As String
            output_directory = Me.Select_Directory("xml file")
            If output_directory = "" Then
                Me.Write_Csl_Line(" no directory selected.")
            Else
                ' Get model from Rhapsody
                Me.Write_Csl("Get model from Rhapsody...")
                Me.Model = New Software_Model_Container
                Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
                Me.Write_Csl_Line(" done.")


                ' Open XML file
                Me.Write_Csl("Create xml file...")
                Dim file_name As String = rpy_sw_mdl.name & "_" & date_str & ".xml"
                Dim file_path As String = output_directory & "\" & file_name
                Dim file_stream As New FileStream(file_path, FileMode.Create)

                ' Serialize model
                Me.Model.Create_Xml(file_stream)

                file_stream.Close()
                Me.Write_Csl_Line(" done.")

                Me.Write_Csl_Line("xml file full path : " & file_path)
            End If

        End If

        ' Display Result to output window
        Me.Write_Csl_Line("Export end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))

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
        Me.Clear_Window()
        Me.Write_Csl_Line("Import and merge by name...")

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then

            ' Get XML file
            Me.Write_Csl("Get XML file to merge...")
            Dim xml_file_path As String
            xml_file_path = Select_File("Select XML file", "XML file|*.xml")
            If xml_file_path = "" Then
                Me.Write_Csl_Line("no file selected.")
                Me.Write_Csl_Line("Merge end.")
                Exit Sub
            Else
                Me.Write_Csl_Line(" done.")
            End If

            ' Create string of the date for created csv file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Create report
            ' Select csv file directory
            Dim output_directory As String
            output_directory = Me.Select_Directory("model importation report file")
            If output_directory = "" Then
                Me.Write_Csl_Line(" no directory selected.")
            Else

                Me.Model = New Software_Model_Container
                Me.Write_Csl("Get software model from XML file...")
                ' Open XML file
                Dim file_stream As New FileStream(xml_file_path, FileMode.Open)
                ' Deserialize XML file
                Dim deserialization_status As Boolean = False
                Dim serializer As New XmlSerializer(GetType(Software_Model_Container))
                Try
                    Me.Model =
                        CType(serializer.Deserialize(file_stream), Software_Model_Container)
                    Me.Write_Csl_Line(" done.")
                    deserialization_status = True
                Catch
                    Me.Write_Csl_Line("Invalid xml file, cannot perform merge.")
                End Try
                ' Close stuff
                file_stream.Close()

                If deserialization_status = True Then
                    Me.Write_Csl("Merge models...")
                    Me.Model.Export_To_Rhapsody(rpy_sw_mdl)
                    Me.Write_Csl_Line(" done.")

                    ' Open csv file
                    Me.Write_Csl("Create report file...")
                    Dim file_name As String =
                        rpy_sw_mdl.name & "_Importation_Report_" & date_str & ".csv"
                    Dim file_path As String = output_directory & "\" & file_name

                    Dim report_file_stream As New StreamWriter(file_path, False)

                    Me.Model.Generate_Importation_Report(report_file_stream)

                    report_file_stream.Close()
                    Me.Write_Csl_Line(" done.")

                    Me.Write_Csl_Line("Importation report file full path : " & file_path)

                End If

            End If

        End If

        ' Display Result to output window
        Me.Write_Csl_Line("Importation end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub


    Public Sub Check_Rpy_Soft_Model()

        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Check software model consistency...")

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created csv file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Get check options (display dialog form)
            Me.Write_Csl("Get check options...")
            Dim pkg_sel_user_choices_file As String
            pkg_sel_user_choices_file = "Check_" & rpy_sw_mdl.name & "_Pkg_Choice"
            Dim prev_pkg_selection_choices As Dictionary(Of String, String)
            prev_pkg_selection_choices = Rpy_Controller.Load_User_Choices(pkg_sel_user_choices_file)

            Dim report_to_merge_user_choices_file = "Check_" & rpy_sw_mdl.name & "_Prev_Report"
            Dim prev_report_choices As Dictionary(Of String, String)
            prev_report_choices = Rpy_Controller.Load_User_Choices(report_to_merge_user_choices_file)
            Dim previous_report_path As String = ""
            If prev_report_choices.ContainsKey("Report_Full_Path") Then
                previous_report_path = prev_report_choices("Report_Full_Path")
            End If
            Dim use_previous_report As Boolean = False
            If prev_report_choices.ContainsKey("Merge_Report_Analysis") Then
                use_previous_report = CBool(prev_report_choices("Merge_Report_Analysis"))
            End If
            Dim report_directory As String = ""
            If prev_report_choices.ContainsKey("Report_Directory") Then
                report_directory = prev_report_choices("Report_Directory")
            End If

            Dim pkg_name_list As List(Of String)
            pkg_name_list = Me.Get_Software_Package_Name_List()

            Dim selection_result As DialogResult
            Dim selection_form As New Consistency_Check_Form(
                pkg_name_list,
                prev_pkg_selection_choices,
                use_previous_report,
                previous_report_path,
                report_directory)
            selection_result = selection_form.ShowDialog()
            If selection_result <> DialogResult.OK Then
                Me.Write_Csl_Line(" aborted.")
                Me.Write_Csl_Line("Check end.")
                Exit Sub
            End If

            ' Save check options
            Dim new_pkg_selection_choices As Dictionary(Of String, String)
            new_pkg_selection_choices = selection_form.Get_Package_Selection_Choices()
            Rpy_Controller.Save_User_Choices(pkg_sel_user_choices_file, new_pkg_selection_choices)
            Me.Write_Csl_Line(" done.")

            Dim new_report_to_merge_choice As New Dictionary(Of String, String)
            new_report_to_merge_choice.Add(
                "Report_Full_Path",
                selection_form.Get_Previous_Report_Path())
            new_report_to_merge_choice.Add(
                "Merge_Report_Analysis",
                selection_form.Is_Merge_Requested().ToString)
            new_report_to_merge_choice.Add(
                "Report_Directory",
                selection_form.Get_Report_Directory())
            Rpy_Controller.Save_User_Choices(report_to_merge_user_choices_file,
                new_report_to_merge_choice)

            ' Get model from Rhapsody
            Me.Write_Csl("Get model from Rhapsody...")
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
            Me.Write_Csl_Line(" done.")

            ' Check model
            Dim selected_pkg_name_list As New List(Of String)
            selected_pkg_name_list = selection_form.Get_Selected_Package_Name_List()
            Me.Write_Csl("Check model...")
            Me.Model.Check_Consistency(selected_pkg_name_list)
            Me.Write_Csl_Line(" done.")

            ' Merge warnings analysis
            If selection_form.Is_Merge_Requested = True Then
                Me.Write_Csl("Merge warnings analysis...")
                Dim prev_report As New Consistency_Check_Report
                Dim is_prev_report_valid As Boolean
                is_prev_report_valid = prev_report.Load(selection_form.Get_Previous_Report_Path)
                If is_prev_report_valid = True Then
                    Me.Model.Merge_Report_Analysis(prev_report)
                    Me.Write_Csl_Line(" done.")
                Else
                    Me.Write_Csl_Line(" previous report is invalid !")
                End If
            End If

            ' Create, save and open report
            Me.Write_Csl("Create report file...")
            Dim file_name As String =
                rpy_sw_mdl.name & "_Consistency_Report_" & date_str & ".csv"
            Dim file_path As String = selection_form.Get_Report_Directory & "\" & file_name
            Dim file_stream As New StreamWriter(file_path, False)
            Me.Model.Generate_Consistency_Report(file_stream)
            file_stream.Close()
            Me.Write_Csl_Line(" done.")
            Me.Write_Csl_Line("Consistency report file full path : " & file_path)
            Process.Start(file_path)

        End If

        ' Display Result to output window
        Me.Write_Csl_Line("Check end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))

    End Sub


    Public Sub Compute_Rpy_Soft_Model_PSWA_Metrics()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Compute PSWA metrics...")

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created file
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Get model from Rhapsody
            Me.Write_Csl("Get model from Rhapsody...")
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
            Me.Write_Csl_Line(" done.")

            ' Check model
            Me.Write_Csl("Check model...")
            Me.Model.Check_Consistency()
            Me.Write_Csl_Line(" done.")

            If Me.Model.Has_Error Then
                Me.Write_Csl_Line("Model has errors, cannot compute metrics.")
            Else
                Me.Write_Csl("Compute model metrics...")
                Me.Model.Compute_PSWA_Metrics()
                Me.Write_Csl_Line(" done.")

                ' Create report
                ' Select txt file directory
                Dim output_directory As String
                output_directory = Me.Select_Directory("model metrics report file")
                If output_directory = "" Then
                    Me.Write_Csl_Line(" no directory selected.")
                Else
                    Me.Write_Csl("Create report file...")
                    Dim file_name As String
                    file_name = rpy_sw_mdl.name & "_PSWA_Metrics_Report_" & date_str & ".xlsx"
                    Dim file_path As String = output_directory & "\" & file_name
                    Me.Model.Generate_PSWA_Metrics_Report(file_path)
                    Me.Write_Csl_Line(" done.")
                    Me.Write_Csl_Line("Metrics report file full path : " & file_path)
                End If
            End If

        End If

        ' Display Result to output window
        Me.Write_Csl_Line("PSWA metrics computation end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub


    Public Sub Generate_All()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Generate all documents...")

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then

            ' Create string of the date for created files
            Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

            ' Select txt file directory
            Dim output_directory As String
            output_directory = Me.Select_Directory("all documents")
            If output_directory = "" Then
                Me.Write_Csl_Line(" no directory selected.")
            Else

                '----------------------------------------------------------------------------------'
                ' Model extraction
                ' Get model from Rhapsody
                Me.Write_Csl("Get model from Rhapsody...")
                Me.Model = New Software_Model_Container
                Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
                Me.Write_Csl_Line(" done.")

                ' Open XML file
                Me.Write_Csl("Create xml file...")
                Dim file_name As String = rpy_sw_mdl.name & "_" & date_str & ".xml"
                Dim file_path As String = output_directory & "\" & file_name
                Dim file_stream As New FileStream(file_path, FileMode.Create)

                ' Serialize model
                Me.Model.Create_Xml(file_stream)

                file_stream.Close()
                Me.Write_Csl_Line(" done.")

                Me.Write_Csl_Line("xml file full path : " & file_path)

                '----------------------------------------------------------------------------------'
                ' Consistency check
                ' Check model
                Me.Write_Csl("Check model...")
                Me.Model.Check_Consistency()
                Me.Write_Csl_Line(" done.")

                ' Open csv file
                Me.Write_Csl("Create report file...")
                file_name = rpy_sw_mdl.name & "_Consistency_Report_" & date_str & ".csv"
                file_path = output_directory & "\" & file_name

                Dim stream_writer As New StreamWriter(file_path, False)

                Me.Model.Generate_Consistency_Report(stream_writer)

                stream_writer.Close()
                Me.Write_Csl_Line(" done.")

                Me.Write_Csl_Line("Consistency report file full path : " & file_path)

                '----------------------------------------------------------------------------------'
                ' Metrics computation
                If Me.Model.Has_Error Then
                    Me.Write_Csl_Line("Model has errors, cannot compute metrics.")
                Else
                    Me.Write_Csl("Compute model metrics...")
                    Me.Model.Compute_PSWA_Metrics()
                    Me.Write_Csl_Line(" done.")

                    ' Open txt file
                    Me.Write_Csl("Create report file...")
                    file_name = rpy_sw_mdl.name & "_Metrics_Report_" & date_str & ".txt"
                    file_path = output_directory & "\" & file_name

                    Me.Model.Generate_PSWA_Metrics_Report(file_path)

                    Me.Write_Csl_Line(" done.")

                    Me.Write_Csl_Line("Metrics report file full path : " & file_path)
                End If

            End If

        End If

        ' Display Result to output window
        Me.Write_Csl_Line("Generate all documents end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub

    Public Sub Find_Packages_Cyclic_Dependencies()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Find packages cyclic dependencies...")

        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then
            Me.Write_Csl("Get model from Rhapsody...")
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
            Me.Write_Csl_Line(" done.")


            Dim dep_list As List(Of List(Of Top_Level_Package))
            dep_list = Me.Model.Find_Cyclic_Dependencies()
            If dep_list.Count = 0 Then
                Me.Write_Csl_Line("No cyclic dependencies.")
            Else
                Me.Write_Csl_Line("Cyclic dependencies list :")
                For Each pkg_list In dep_list
                    Dim path_str As String = Top_Level_Package.Transform_Path_To_String(pkg_list)
                    Me.Write_Csl_Line("   " & path_str)
                Next
            End If
        End If

        ' Display Result to output window
        Me.Write_Csl_Line("Find packages cyclic dependencies end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub

    ' Remove empty packages from the Rhapsody model.
    ' A package is considered as empty if it aggregates only packages that recursively only 
    ' aggregates packages.
    Public Sub Remove_Empty_Packages()
        ' Initialize output window and display start message
        Dim chrono As New Global.System.Diagnostics.Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Remove empty Packages...")

        ' Get selected element and check that it is a Rhapsody project
        ' Get selected element and check that it is a Rhapsody project
        Dim rpy_sw_mdl As RPProject = Get_Rhapsody_Project()

        If Not IsNothing(rpy_sw_mdl) Then
            Dim root_level_package As RPPackage
            For Each root_level_package In rpy_sw_mdl.packages
                If Is_PSWA_Package(CType(root_level_package, RPModelElement)) Then
                    Software_Package.Remove_Empty_Packages(root_level_package)
                End If
            Next
        End If

        ' Display result to output window
        Me.Write_Csl_Line("End removing empty Packages.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub

End Class
