Imports rhapsody2
Imports System.IO

Public Class Rhapsody_Project_Configurator

    Private Rpy_Proj As RPProject

    Public Enum E_Profile_Configuration_Status
        CONFIGURATION_OK
        PROFILES_NOT_FOUND
    End Enum

    Private Shared Profiles_List As String() = {
        "Project_Metamodel",
        "Software_Data_Type_Metamodel",
        "Physical_Software_Architecture_Metamodel",
        "Software_Detailed_Design_Metamodel",
        "Software_Implementation_Metamodel"}

    Private Shared Helpers_Function As String(,) = New String(,) {
        {"Export to XML", "Export", "Project"},
        {"Merge from XML", "Merge", "Project"},
        {"Check", "Check", "Project"},
        {"Compute PSWA metrics", "Compute_Rpy_Soft_Model_PSWA_Metrics", "Project"},
        {"Generate All", "Generate_All", "Project"},
        {"Remove empty packages All", "Remove_Empty_Packages", "Project"},
        {"Rename Connectors", "Rename_Connectors", "Root_Software_Composition, Component_Design"},
        {"Navigate to source", "Navigate_To_Connector_Source", "Connector_Prototype"},
        {"Navigate to destination", "Navigate_To_Connector_Destination", "Connector_Prototype"},
        {"Generate diagram", "Generate_Component_Type_Diagram", "Component_Type"},
        {"Display GUID", "Display_GUID", "All"},
        {"Modify GUID", "Modify_GUID", "All"},
        {"Configure", "Configure", "Project"},
        {"Update realizations", "Update_Realizations", "Component_Design"},
        {"Create Component_Design model", "Create_Component_Design_Model", "Component_Type"},
        {"Findc cyclic dependencies", "Find_Packages_Cyclic_Dependencies", "Project"}}

    Public Sub New(rpy_proj As RPProject)
        Me.Rpy_Proj = rpy_proj
    End Sub

    Public Sub Configure_Profiles(
        rpy_app As RPApplication,
        toolset_path As String,
        ByRef status As E_Profile_Configuration_Status)

        Dim mdl_unit As RPUnit = Me.Rpy_Proj.getSaveUnit
        Dim mdl_directory As String = mdl_unit.currentDirectory

        Dim profiles_dir As String = toolset_path & "\Rhapsody_Profiles"
        Dim relative_profile_dir As String = "..\..\"
        relative_profile_dir &= Rpy_Controller.Make_Relative_Path(mdl_directory, profiles_dir)
        status = E_Profile_Configuration_Status.CONFIGURATION_OK
        For Each profile In Profiles_List
            Dim rpy_unit As RPUnit
            Dim rpy_profile_pkg As RPModelElement
            Dim profile_full_path As String = profiles_dir & "\" & profile & ".sbsx"
            If File.Exists(profile_full_path) Then
                rpy_profile_pkg = Me.Rpy_Proj.findNestedElement(profile, "Package")
                If IsNothing(rpy_profile_pkg) Then
                    rpy_app.addToModelByReference(profile_full_path)
                End If
                rpy_profile_pkg = Me.Rpy_Proj.findNestedElement(profile, "Package")
                rpy_unit = rpy_profile_pkg.getSaveUnit()
                rpy_unit.setUnitPath(relative_profile_dir)
            Else
                status = E_Profile_Configuration_Status.PROFILES_NOT_FOUND
            End If
        Next
    End Sub

    Public Sub Configure_Helper(toolset_path As String)

        Dim mdl_unit As RPUnit = Me.Rpy_Proj.getSaveUnit
        Dim mdl_directory As String = mdl_unit.currentDirectory

        Dim tool_dir As String = toolset_path & "\Tools"
        Dim relative_tool_dir As String = "..\"
        relative_tool_dir &= Rpy_Controller.Make_Relative_Path(mdl_directory, tool_dir) & _
                            "\Software_Model\bin\Release\Software_Model.exe"

        ' Create helper file
        Dim helpers_file_full_path As String
        helpers_file_full_path = mdl_directory &
                                "/" & Me.Rpy_Proj.name & "_rpy" &
                                "/" & "Software_Model_Toolset.hep"

        ' Fill helper file
        Dim writer As New StreamWriter(helpers_file_full_path, False)
        writer.WriteLine("[Helpers]")
        writer.WriteLine("numberOfElements=" & Helpers_Function.GetLength(0))
        writer.WriteLine("")

        Dim helper_idx As Integer
        For helper_idx = 0 To Helpers_Function.GetLength(0) - 1
            Dim idx_str As String = CStr(helper_idx + 1)
            writer.WriteLine("name" & idx_str & "=SMT - " & Helpers_Function(helper_idx, 0))
            writer.WriteLine("command" & idx_str & "=" & relative_tool_dir)
            writer.WriteLine("arguments" & idx_str & "=" & Helpers_Function(helper_idx, 1))
            writer.WriteLine("isVisible" & idx_str & "=1")
            writer.WriteLine("applicableTo" & idx_str & "=" & Helpers_Function(helper_idx, 2))
            writer.WriteLine("isPluginCommand" & idx_str & "=0")
            writer.WriteLine("")
        Next

        writer.Close()

        ' Configure Rhapsody project
        Me.Rpy_Proj.setPropertyValue(
            "General.Model.HelpersFile",
            Me.Rpy_Proj.name & "_rpy\Software_Model_Toolset.hep")

    End Sub

    Public Sub Configure_Activity_Diagrams_Default_View()

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

    End Sub

    Public Sub Configure_Packages_Are_Not_Unit()
        Me.Rpy_Proj.setPropertyValue("General.Model.PackageIsSavedUnit", "False")
    End Sub

    Public Sub Configure_Accessible_Types()
        Me.Rpy_Proj.setPropertyValue(
            "General.Model.CommonTypes",
            "Software_Data_Type_Metamodel::Basic_Data_Types")
    End Sub

End Class
