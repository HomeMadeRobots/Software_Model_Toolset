Imports rhapsody2
Imports System.IO

Public Class Rpy_Model_Transformer
    Inherits Rpy_Controller

    Private Model As Software_Model_Container

    Public Sub Transform_To_CLOOF()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Transform software model to C language object oriented framework...")

        ' Get selected element and check that it is a Software_Package
        Dim selected_element As RPModelElement = Rhapsody_App.getSelectedElement
        Dim rpy_pkg As RPPackage = Nothing
        Dim rpy_sw_mdl As RPProject = Nothing
        If Is_Software_Package(selected_element) Then
            rpy_pkg = CType(selected_element, RPPackage)
            rpy_sw_mdl = CType(rpy_pkg.project, RPProject)
        End If

        If IsNothing(rpy_pkg) Then
            Me.Write_Csl_Line("Error : a Software_Package shall be selected.")
            Me.Write_Csl_Line("Transformation end.")
            Exit Sub
        End If

        Dim date_str As String = Now.ToString("yyyy_MM_dd_HH_mm_ss")

        ' Get transformation options (display dialog form)
        Me.Write_Csl("Get transformation options...")

        Dim transfo_options_file As String
        transfo_options_file = "Transformation_CLOOF_" & rpy_pkg.name & "_Options"
        Dim prev_transfo_options As Dictionary(Of String, String)
        prev_transfo_options = Rpy_Controller.Load_User_Choices(transfo_options_file)
        Dim output_directory As String = ""
        If prev_transfo_options.ContainsKey("Output_Directory") Then
            output_directory = prev_transfo_options("Output_Directory")
        End If

        Dim transfo_dialog_result As DialogResult
        Dim transformation_form As New CLOOF_Transformation_Form(output_directory)
        transfo_dialog_result = transformation_form.ShowDialog()
        If transfo_dialog_result <> DialogResult.OK Then
            Me.Write_Csl_Line(" aborted.")
            Me.Write_Csl_Line("Transformation end.")
            Exit Sub
        End If

        ' Save transformation options
        Dim new_transfo_options As New Dictionary(Of String, String)
        output_directory = transformation_form.Get_Output_Directory()
        new_transfo_options.Add("Output_Directory", output_directory)
        Rpy_Controller.Save_User_Choices(transfo_options_file, new_transfo_options)

        Me.Write_Csl_Line(" done.")

        ' Get model from Rhapsody
        Me.Write_Csl("Get model from Rhapsody...")
        Me.Model = New Software_Model_Container
        Me.Model.Import_All_From_Rhapsody_Model(rpy_sw_mdl)
        Me.Write_Csl_Line(" done.")

        ' Check model
        Me.Write_Csl("Check model...")
        Me.Model.Check_Consistency()
        Me.Write_Csl_Line(" done.")

        If Me.Model.Has_Error = False Then
            ' Create destination folder
            Dim new_folder_path = output_directory & "/" & rpy_pkg.name & "_" & date_str
            Directory.CreateDirectory(new_folder_path)

            ' Transform model
            Me.Write_Csl("Transform package...")
            Dim pkg As Software_Package
            pkg = CType(Me.Model.Get_Element_By_Rpy_Guid(rpy_pkg.GUID), Software_Package)
            pkg.Transform_To_CLOOF(new_folder_path)
            Me.Write_Csl_Line(" done.")
        Else
            Dim report_file_name As String
            report_file_name = rpy_sw_mdl.name & "_Consistency_Report_" & date_str & ".csv"
            Dim report_file_path As String = output_directory & "\" & report_file_name
            Dim report_file_stream As New StreamWriter(report_file_path, False)
            Me.Model.Generate_Consistency_Report(report_file_stream)
            report_file_stream.Close()
            Me.Write_Csl_Line("Model has errors : impossible to transform.")
            Me.Write_Csl_Line("See consistency check report : " & report_file_path)
        End If

        ' Display Result to output window
        Me.Write_Csl_Line("Transformation end.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub

End Class