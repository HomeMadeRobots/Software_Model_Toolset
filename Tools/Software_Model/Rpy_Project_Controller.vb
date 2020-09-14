Imports rhapsody2
Imports System.IO
Imports Microsoft.VisualBasic.FileIO
Imports System.Windows

Public Class Rpy_Project_Controller
    Inherits Rpy_Controller

    Private Rpy_Proj As RPProject
    Private Mdl_Directory As String

    Private Shared Profiles_List As String() = {
        "Project_Metamodel",
        "Software_Data_Type_Metamodel",
        "Physical_Software_Architecture_Metamodel",
        "Software_Detailed_Design_Metamodel",
        "Software_Implementation_Metamodel"}

    Private Shared Helpers_Function As String(,) = New String(,) {
        {"Export to XML", "Export", "Project"},
        {"Merge to XML", "Merge", "Project"},
        {"Check", "Check", "Project"},
        {"Compute PSWA metrics", "Compute_Rpy_Soft_Model_PSWA_Metrics", "Project"},
        {"Generate All", "Generate_All", "Project"},
        {"Remove empty packages All", "Remove_Empty_Packages", "Project"},
        {"Rename Connectors", "Rename_Connectors", "Root_Software_Composition"},
        {"Navigate to source", "Navigate_To_Connector_Source", "Assembly_Connector"},
        {"Navigate to destination", "Navigate_To_Connector_Destination", "Assembly_Connector"},
        {"Generate diagram", "Generate_Component_Type_Diagram", "Component_Type"},
        {"Display GUID", "Display_GUID", "All"},
        {"Modify GUID", "Modify_GUID", "All"},
        {"Configure", "Configure", "Project"}}


    Public Sub Configure()

        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Configure Rhapsody project...")

        ' Get selected element and check that it is a Rhapsody project
        Me.Rpy_Proj = Get_Rhapsody_Project()

        If Not IsNothing(Rpy_Proj) Then
            Dim mdl_unit As RPUnit
            mdl_unit = Rpy_Proj.getSaveUnit
            Me.Mdl_Directory = mdl_unit.currentDirectory

            Dim config_form As New Project_Configuration_Form(Me)
            config_form.ShowDialog()

            Me.Rpy_Proj.save()
        End If

        ' Display result to output window
        Me.Write_Csl_Line("Configuration end.")
        chrono.Stop()
        Me.Write_Csl_Line(Get_Elapsed_Time(chrono))

    End Sub

    Public Sub Configure(
        toolset_path As String,
        configure_profiles As Boolean,
        configure_helpers As Boolean,
        configure_diagram As Boolean,
        configure_project_views As Boolean)

        If configure_profiles = True Then
            Me.Configure_Profiles(toolset_path)
        End If
        If configure_helpers = True Then
            Me.Configure_Helper(toolset_path)
        End If
        If configure_diagram = True Then
            Me.Configure_Diagrams()
        End If
        If configure_project_views = True Then
            Me.Configure_Project_View()
        End If
    End Sub

    Private Shared Function Make_Relative_Path(from_path As String, to_path As String) As String
        Dim from_uri As Uri
        Dim to_uri As Uri
        Dim relative_path As String = to_path
        from_uri = New Uri(from_path)
        to_uri = New Uri(to_path)
        If from_uri.Scheme = to_uri.Scheme Then
            Dim relative_uri As Uri
            relative_uri = from_uri.MakeRelativeUri(to_uri)
            relative_path = Uri.UnescapeDataString(relative_uri.ToString())
            If to_uri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase) Then
                relative_path = relative_path.Replace(
                                Path.AltDirectorySeparatorChar,
                                Path.DirectorySeparatorChar)
            End If
        End If
        Return relative_path
    End Function

    Private Sub Configure_Profiles(toolset_path As String)
        Dim profiles_dir As String = toolset_path & "\Rhapsody_Profiles"
        Dim relative_profile_dir As String
        relative_profile_dir = "..\..\" & Make_Relative_Path(Me.Mdl_Directory, profiles_dir)

        For Each profile In Profiles_List
            Dim rpy_unit As RPUnit
            Dim rpy_profile_pkg As RPPackage

            Dim profile_full_path As String = profiles_dir & "\" & profile & ".sbsx"

            If File.Exists(profile_full_path) Then

                rpy_profile_pkg = CType(Me.Rpy_Proj.findNestedElement(profile, "Package"), RPPackage)
                If IsNothing(rpy_profile_pkg) Then
                    Me.Write_Csl("Add " & profile & " profile...")
                    Me.Rhapsody_App.addToModelByReference(profile_full_path)
                    Me.Write_Csl_Line(" done")
                End If

                Me.Write_Csl("Set " & profile & " relative path...")
                rpy_profile_pkg = CType(Me.Rpy_Proj.findNestedElement(profile, "Package"), RPPackage)
                rpy_unit = rpy_profile_pkg.getSaveUnit()
                rpy_unit.setUnitPath(relative_profile_dir)
                Me.Write_Csl_Line(" done")
            Else
                Me.Write_Csl_Line("Error, cannot find profile : " & profile_full_path)
            End If
        Next

    End Sub

    Private Sub Configure_Helper(toolset_path As String)

    Me.Write_Csl("Configure helpers...")

        Dim tool_dir As String = toolset_path & "\Tools"
        Dim relative_tool_dir As String
        relative_tool_dir = "..\" & Make_Relative_Path(Me.Mdl_Directory, tool_dir) & _
                            "\Software_Model\bin\Release\Software_Model.exe"

        ' Create helper file
        Dim helpers_file_full_path As String
        helpers_file_full_path = Me.Mdl_Directory &
                                "/" & Me.Rpy_Proj.name & "_rpy" &
                                "/" & "Software_Model_Toolset.hep"

        ' Fill helper file
        Dim writer As New StreamWriter(helpers_file_full_path, False)
        writer.WriteLine("[Helpers]")
        writer.WriteLine("numberOfElements=" & Helpers_Function.GetLength(0))
        writer.WriteLine("")

        Dim helper_idx As Integer
        For helper_idx = 0 To Helpers_Function.GetLength(0) - 1
            Dim helper_idx_str As String = CStr(helper_idx + 1)
            writer.WriteLine("name" & helper_idx_str & "=SMT - " & Helpers_Function(helper_idx, 0))
            writer.WriteLine("command" & helper_idx_str & "=" & relative_tool_dir)
            writer.WriteLine("arguments" & helper_idx_str & "=" & Helpers_Function(helper_idx, 1))
            writer.WriteLine("isVisible" & helper_idx_str & "=1")
            writer.WriteLine("applicableTo" & helper_idx_str & "=" & Helpers_Function(helper_idx, 2))
            writer.WriteLine("isPluginCommand" & helper_idx_str & "=0")
            writer.WriteLine("")
        Next

        writer.Close()

        ' Configure Rhapsody project
        Me.Rpy_Proj.setPropertyValue(
            "General.Model.HelpersFile",
            Me.Rpy_Proj.name & "_rpy\Software_Model_Toolset.hep")

        Me.Write_Csl_Line(" done")
    End Sub

    Private Sub Configure_Diagrams()
        Me.Write_Csl("Configure Activity diagram default view...")

        Me.Rpy_Proj.setPropertyValue("Activity_diagram.Action.ShowName", "Name")

        Me.Rpy_Proj.setPropertyValue(
            "Activity_diagram.ControlFlow.line_style",
            "rounded_rectilinear_arrows")

        Me.Rpy_Proj.setPropertyValue("Activity_diagram.DecisionNode.show_name", "None")

        Me.Rpy_Proj.setPropertyValue(
            "Activity_diagram.DefaultTransition.line_style",
            "straight_arrows")

        Me.Rpy_Proj.setPropertyValue(
            "Activity_diagram.ObjectFlow.line_style",
            "rounded_rectilinear_arrows")

        Me.Rpy_Proj.setPropertyValue(
            "Activity_diagram.SendAction.ShowNotation",
            "Name")

        Me.Write_Csl_Line(" done")
    End Sub

    Private Sub Configure_Project_View()

        Me.Write_Csl("Configure PackageIsSavedUnit to false...")
        Me.Rpy_Proj.setPropertyValue("General.Model.PackageIsSavedUnit", "False")
        Me.Write_Csl_Line(" done")

        Me.Write_Csl("Configure DisplayMode to flat...")
        Me.Rpy_Proj.setPropertyValue("Browser.Settings.DisplayMode", "Flat")
        Me.Write_Csl_Line(" done")

        Me.Write_Csl("Configure common types...")
        Me.Rpy_Proj.setPropertyValue("General.Model.CommonTypes",
            "Software_Data_Type_Metamodel::Stereotypes")
        Me.Write_Csl_Line("done")

        Dim default_package As RPModelElement
        default_package = Me.Rpy_Proj.findNestedElement("Default", "Package")
        If Not IsNothing(default_package) Then
            Me.Write_Csl("Remove default package...")
            default_package.deleteFromProject()
            Me.Write_Csl_Line("done")
        End If

    End Sub

End Class


Public Class Project_Configuration_Form
    Inherits Form

    Private My_Controller As Rpy_Project_Controller

    Private Last_Configurations As Dictionary(Of String, String)

    Private WithEvents Toolset_Path_TxtBx As New TextBox
    Private WithEvents Toolset_Path_Button As New Button
    Private WithEvents Profiles_Conf_ChckBx As New CheckBox
    Private WithEvents Helper_Conf_ChckBx As New CheckBox
    Private WithEvents Diagrams_Conf_ChckBx As New CheckBox
    Private WithEvents Project_View_Conf_ChckBx As New CheckBox
    Private WithEvents Configure_Button As New Button

    Private Const Form_Width As Integer = 500
    Private Const Marge As Integer = 20
    Private Const Item_Height As Integer = 20
    Private Const Item_Width As Integer = Form_Width - 2 * Marge
    Private Shared ChckBx_Size As New Size(Item_Width, Item_Height)
    Private Const Button_Width As Integer = 100

    Public Sub New(ctrl As Rpy_Project_Controller)

        Me.My_Controller = ctrl

        ' Load last configurations
        Me.Last_Configurations = Rpy_Controller.Load_User_Record("Project_Configration")

        Dim item_y_pos As Integer = Marge

        '------------------------------------------------------------------------------------------'
        ' Add Toolset path selection stuff
        Dim toolset_path_title As New Label
        toolset_path_title.Text = "Software_Model_Toolset path :"
        toolset_path_title.Location = New Point(Marge, item_y_pos)
        toolset_path_title.Size = ChckBx_Size
        Me.Controls.Add(toolset_path_title)
        item_y_pos += Item_Height

        Me.Toolset_Path_TxtBx.Location = New Point(Marge, item_y_pos)
        Me.Toolset_Path_TxtBx.Size = New Size(Item_Width - 2 * Marge, Item_Height)
        If Me.Last_Configurations.ContainsKey("Toolset_Path") Then
            Me.Toolset_Path_TxtBx.Text = Me.Last_Configurations("Toolset_Path")
        Else
            Me.Last_Configurations.Add("Toolset_Path", "")
            Me.Toolset_Path_TxtBx.Text = ""
        End If
        Me.Toolset_Path_TxtBx.Enabled = False
        Me.Controls.Add(Me.Toolset_Path_TxtBx)

        Me.Toolset_Path_Button.Location = New Point(Item_Width, item_y_pos)
        Me.Toolset_Path_Button.Size = New Size(Marge, Item_Height)
        Me.Toolset_Path_Button.Text = "..."
        Me.Toolset_Path_Button.Enabled = False
        Me.Controls.Add(Me.Toolset_Path_Button)

        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add profiles configuration check box
        Me.Profiles_Conf_ChckBx.Location = New Point(Marge, item_y_pos)
        Me.Profiles_Conf_ChckBx.Size = ChckBx_Size
        Me.Profiles_Conf_ChckBx.Text = "Configure profile packages"
        If Me.Last_Configurations.ContainsKey("Profiles_Conf") Then
            Me.Profiles_Conf_ChckBx.Checked = CBool(Me.Last_Configurations("Profiles_Conf"))
        Else
            Me.Last_Configurations.Add("Profiles_Conf", "False")
            Me.Profiles_Conf_ChckBx.Checked = False
        End If
        Me.Controls.Add(Me.Profiles_Conf_ChckBx)
        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add Rhapsody helper configuration check box
        Me.Helper_Conf_ChckBx.Location = New Point(Marge, item_y_pos)
        Me.Helper_Conf_ChckBx.Size = ChckBx_Size
        Me.Helper_Conf_ChckBx.Text = "Configure Rhapsody helper"
        If Me.Last_Configurations.ContainsKey("Helper_Conf") Then
            Me.Helper_Conf_ChckBx.Checked = CBool(Me.Last_Configurations("Helper_Conf"))
        Else
            Me.Last_Configurations.Add("Helper_Conf", "False")
            Me.Helper_Conf_ChckBx.Checked = False
        End If
        Me.Controls.Add(Me.Helper_Conf_ChckBx)
        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add Rhapsody diagram configuration check box
        Me.Diagrams_Conf_ChckBx.Location = New Point(Marge, item_y_pos)
        Me.Diagrams_Conf_ChckBx.Size = ChckBx_Size
        Me.Diagrams_Conf_ChckBx.Text = "Configure diagram default view"
        If Me.Last_Configurations.ContainsKey("Diagrams_Conf") Then
            Me.Diagrams_Conf_ChckBx.Checked = CBool(Me.Last_Configurations("Diagrams_Conf"))
        Else
            Me.Last_Configurations.Add("Diagrams_Conf", "False")
            Me.Diagrams_Conf_ChckBx.Checked = False
        End If
        Me.Controls.Add(Me.Diagrams_Conf_ChckBx)
        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add Rhapsody project views configuration check box
        Me.Project_View_Conf_ChckBx.Location = New Point(Marge, item_y_pos)
        Me.Project_View_Conf_ChckBx.Size = ChckBx_Size
        Me.Project_View_Conf_ChckBx.Text = "Configure project default view"
        If Me.Last_Configurations.ContainsKey("Project_View_Conf") Then
            Me.Project_View_Conf_ChckBx.Checked = CBool(Me.Last_Configurations("Project_View_Conf"))
        Else
            Me.Last_Configurations.Add("Project_View_Conf", "False")
            Me.Project_View_Conf_ChckBx.Checked = False
        End If
        Me.Controls.Add(Me.Project_View_Conf_ChckBx)
        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add main button
        Me.Configure_Button.Text = "Configure"
        Me.Controls.Add(Me.Configure_Button)
        Me.Configure_Button.Size = New Size(Button_Width, Marge * 2)
        Me.Configure_Button.Location = New Point(Form_Width \ 2 - Button_Width \ 2, item_y_pos)
        item_y_pos += Me.Configure_Button.Size.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Configure Rhapsody project for Software_Model_Toolset"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

    End Sub

    Private Sub Toolset_Path_TxtBx_Change() Handles Toolset_Path_TxtBx.TextChanged
        If Me.Last_Configurations.ContainsKey("Toolset_Path") = False Then
            Me.Last_Configurations.Add("Toolset_Path", "")
        End If
        Me.Last_Configurations("Toolset_Path") = Me.Toolset_Path_TxtBx.Text
    End Sub

    Private Sub Profiles_Conf_Checked() Handles Profiles_Conf_ChckBx.CheckedChanged
        Me.Manage_Toolset_Path_Visibility()
        If Me.Last_Configurations.ContainsKey("Profiles_Conf") = False Then
            Me.Last_Configurations.Add("Profiles_Conf", "False")
        End If
        Me.Last_Configurations("Profiles_Conf") = Me.Profiles_Conf_ChckBx.Checked.ToString
    End Sub

    Private Sub Helper_Conf_Checked() Handles Helper_Conf_ChckBx.CheckedChanged
        Me.Manage_Toolset_Path_Visibility()
        If Me.Last_Configurations.ContainsKey("Helper_Conf") = False Then
            Me.Last_Configurations.Add("Helper_Conf", "False")
        End If
        Me.Last_Configurations("Helper_Conf") = Me.Helper_Conf_ChckBx.Checked.ToString
    End Sub

    Private Sub Diagrams_Conf_Checked() Handles Diagrams_Conf_ChckBx.CheckedChanged
        If Me.Last_Configurations.ContainsKey("Diagrams_Conf") = False Then
            Me.Last_Configurations.Add("Diagrams_Conf", "False")
        End If
        Me.Last_Configurations("Diagrams_Conf") = Me.Diagrams_Conf_ChckBx.Checked.ToString
    End Sub

    Private Sub Project_View_Conf_Checked() Handles Project_View_Conf_ChckBx.CheckedChanged
        If Me.Last_Configurations.ContainsKey("Project_View_Conf") = False Then
            Me.Last_Configurations.Add("Project_View_Conf", "False")
        End If
        Me.Last_Configurations("Project_View_Conf") = Me.Project_View_Conf_ChckBx.Checked.ToString
    End Sub

    Private Sub Toolset_Path_Selection_Button_Clicked() Handles Toolset_Path_Button.Click
        Dim dialog_box As FolderBrowserDialog
        dialog_box = New FolderBrowserDialog
        dialog_box.Description = "Select Software_Model_Toolset path"
        Dim result As Forms.DialogResult = dialog_box.ShowDialog()
        If result = Forms.DialogResult.OK Then
            Me.Toolset_Path_TxtBx.Text = dialog_box.SelectedPath
        End If
    End Sub

    Private Sub Manage_Toolset_Path_Visibility()
        If Me.Profiles_Conf_ChckBx.Checked = False And _
            Me.Helper_Conf_ChckBx.Checked = False Then
            Me.Toolset_Path_TxtBx.Enabled = False
            Me.Toolset_Path_Button.Enabled = False
        Else
            Me.Toolset_Path_TxtBx.Enabled = True
            Me.Toolset_Path_Button.Enabled = True
        End If
    End Sub

    Private Sub Configure_Clicked() Handles Configure_Button.Click

        ' Save configuration
        Rpy_Controller.Save_User_Record("Project_Configration", Me.Last_Configurations)

        Me.My_Controller.Configure(
            Me.Toolset_Path_TxtBx.Text,
            Me.Profiles_Conf_ChckBx.Checked,
            Me.Helper_Conf_ChckBx.Checked,
            Me.Diagrams_Conf_ChckBx.Checked,
            Me.Project_View_Conf_ChckBx.Checked)

        Me.Close()
    End Sub

End Class