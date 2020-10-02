Imports rhapsody2
Imports System.IO

Public Class Rpy_Component_Design_Controller

    Inherits Rpy_Controller

    Private Model As New Software_Model_Container
    Private Viewer As Component_Design_Model_Creation_Form

    Public Sub Open_Component_Design_Model_Creation_View()
        Dim select_element As RPModelElement = Me.Rhapsody_App.getSelectedElement
        If Is_Component_Type(select_element) Then

            Dim mdl_unit As RPUnit = select_element.getSaveUnit
            Dim mdl_directory As String = mdl_unit.currentDirectory

            Dim swct_stereotype As RPStereotype
            swct_stereotype = CType(CType(select_element.project, RPProject).findElementByGUID(
                "GUID 9f000df0-3104-4bba-93b4-e2b6a64d3833"), RPStereotype)
            Dim profiles_dir As String = swct_stereotype.getSaveUnit.currentDirectory

            Me.Viewer = New Component_Design_Model_Creation_Form(Me,
                select_element.getFullPathName,
                profiles_dir,
                mdl_directory,
                select_element.name & "_Internal_Design")
            Me.Viewer.ShowDialog()
        End If
    End Sub

    Public Sub Create_Component_Design_Model(
        swct_type_path As String,
        profiles_directory As String,
        model_directory As String,
        model_name As String)

        ' Collect needed units
        Dim unit_directory_list As New List(Of String)
        Dim unit_name_list As New List(Of String)
        ' Get Component_Type
        Dim rpy_proj As RPProject = Me.Rhapsody_App.activeProject
        Dim rpy_swct As RPModelElement
        rpy_swct = rpy_proj.findElementsByFullName(swct_type_path, "Class")
        Me.Collect_Rpy_Unit_Data(rpy_swct, unit_name_list, unit_directory_list)
        ' Get needed and realized Interfaces
        Dim rpy_port As RPPort
        For Each rpy_port In CType(rpy_swct, RPClass).ports
            Dim rpy_if As RPModelElement = Nothing
            If Is_Provider_Port(CType(rpy_port, RPModelElement)) Then
                rpy_if = CType(rpy_port.providedInterfaces.Item(1), RPModelElement)
            ElseIf Is_Requirer_Port(CType(rpy_port, RPModelElement)) Then
                rpy_if = CType(rpy_port.requiredInterfaces.Item(1), RPModelElement)
            End If
            If Is_Event_Interface(rpy_if) Then
                Dim rpy_ev_arg As RPAttribute
                For Each rpy_ev_arg In CType(rpy_if, RPClass).attributes
                    Dim rpy_type As RPModelElement = Nothing
                    If Is_Event_Argument(CType(rpy_ev_arg, RPModelElement)) Then
                        rpy_type = CType(rpy_ev_arg.type, RPModelElement)
                        Me.Collect_Rpy_Unit_Data(rpy_type, unit_name_list, unit_directory_list)
                    End If
                Next
            ElseIf Is_Client_Server_Interface(rpy_if) Then
                Dim rpy_op As RPModelElement
                For Each rpy_op In CType(rpy_if, RPClass).operations
                    If Is_Asynchronous_Operation(rpy_op) Or Is_Synchronous_Operation(rpy_op) Then
                        Dim rpy_arg As RPArgument
                        For Each rpy_arg In CType(rpy_op, RPOperation).arguments
                            Dim rpy_type As RPModelElement
                            rpy_type = CType(rpy_arg.type, RPModelElement)
                            Me.Collect_Rpy_Unit_Data(rpy_type, unit_name_list, unit_directory_list)
                        Next
                    End If
                Next
            End If
            Me.Collect_Rpy_Unit_Data(rpy_if, unit_name_list, unit_directory_list)
        Next
        ' Get needed types from Configuration_Parameters
        Dim rpy_attr As RPAttribute
        For Each rpy_attr In CType(rpy_swct, RPClass).attributes
            Dim rpy_type As RPModelElement = Nothing
            If Is_Configuration_Parameter(CType(rpy_attr, RPModelElement)) Then
                rpy_type = CType(rpy_attr.type, RPModelElement)
                Me.Collect_Rpy_Unit_Data(rpy_type, unit_name_list, unit_directory_list)
            End If
        Next

        ' Create Rhapsody model
        Me.Rhapsody_App.createNewProject(model_directory, model_name)
        Me.Rhapsody_App.bringWindowToTop()
        rpy_proj = Me.Rhapsody_App.activeProject

        ' Remove default diagrams
        Dim rpy_diagram As RPDiagram
        For Each rpy_diagram In rpy_proj.structureDiagrams
            rpy_diagram.deleteFromProject()
        Next

        ' Add Toolset profiles
        Dim configurator As New Rhapsody_Project_Configurator(rpy_proj)
        Dim status As Rhapsody_Project_Configurator.E_Profile_Configuration_Status
        Dim toolset_directory As String
        toolset_directory = Directory.GetParent(profiles_directory).ToString
        configurator.Configure_Profiles(Me.Rhapsody_App, toolset_directory, status)

        ' Add needed packages
        Dim mdl_unit As RPUnit = rpy_proj.getSaveUnit
        Dim mdl_directory As String = mdl_unit.currentDirectory
        Dim unit_idx As Integer
        For unit_idx = 0 To unit_directory_list.Count - 1
            Dim rpy_unit As RPUnit
            Dim unit_dir As String = unit_directory_list(unit_idx)
            Dim unit_name As String = unit_name_list(unit_idx)
            Me.Rhapsody_App.addToModelByReference(unit_dir & "/" & unit_name & ".sbsx")
            Dim rpy_pkg As RPModelElement
            rpy_pkg = rpy_proj.findNestedElement(unit_name, "Package")
            rpy_unit = rpy_pkg.getSaveUnit()
            Dim unit_relative_path As String = "..\..\"
            unit_relative_path &= Rpy_Controller.Make_Relative_Path(mdl_directory, unit_dir)
            rpy_unit.setUnitPath(unit_relative_path)
        Next

        ' Add software package
        Dim rpy_design_pkg As RPPackage
        rpy_design_pkg = rpy_proj.addPackage(model_name)
        rpy_design_pkg.addStereotype("Software_Package", "Package")

        ' Add Component_Design
        Dim rpy_swct_design As RPClass
        rpy_swct_design = rpy_design_pkg.addClass(model_name)
        rpy_swct_design.addStereotype("Component_Design", "Class")

        ' Add Component_Type_Ref
        rpy_swct = rpy_proj.findElementsByFullName(swct_type_path, "Class")
        rpy_swct_design.addGeneralization(CType(rpy_swct, RPClassifier))
        Dim rpy_gen As RPGeneralization
        rpy_gen = rpy_swct_design.findGeneralization(rpy_swct.name)
        rpy_gen.addStereotype("Component_Type_Ref", "Generalization")

        ' Remove undesired package
        Dim default_pkg As RPPackage
        default_pkg = CType(rpy_proj.findNestedElementRecursive("Default", "Package"), RPPackage)
        default_pkg.deleteFromProject()

    End Sub

    Public Sub Update_Realizations()
        Me.Clear_Window()
        Me.Write_Csl_Line("Start realizations update.")
        Dim select_element As RPModelElement = Me.Rhapsody_App.getSelectedElement
        If Is_Component_Design(select_element) Then

            Dim rpy_swct_design As RPClass = CType(select_element, RPClass)
            Dim rpy_proj As RPProject = CType(rpy_swct_design.project, RPProject)

            ' Get model from Rhapsody
            Me.Write_Csl("Get model from Rhapsody...")
            Me.Model = New Software_Model_Container
            Me.Model.Import_All_From_Rhapsody_Model(rpy_proj)
            Me.Write_Csl_Line(" done.")

            ' Get the Component_Design to update
            Dim swct_design_uuid As Guid
            swct_design_uuid = Transform_Rpy_GUID_To_Guid(rpy_swct_design.GUID)
            Dim swct_design As Component_Design
            swct_design = CType(Me.Model.Get_Element_By_Uuid(swct_design_uuid), Component_Design)

            Me.Write_Csl_Line("Add missing realizations...")
            Dim added_realization_name_list As List(Of String)
            added_realization_name_list = swct_design.Add_Missing_Realizations()
            If added_realization_name_list.count = 0 Then
                Me.Write_Csl_Line("    Nothing to add.")
            End If
            For Each real_name In added_realization_name_list
                Me.Write_Csl_Line("    " & real_name)
            Next
            Me.Write_Csl_Line("done.")

        Else
            Me.Write_Csl_Line("A Component_Design shall be selected.")
        End If

        Me.Write_Csl_Line("End realizations update.")
    End Sub

    Private Sub Collect_Rpy_Unit_Data(
        rpy_element As RPModelElement,
        ByRef unit_name_list As List(Of String),
        ByRef unit_directory_list As List(Of String))

        Dim rpy_unit As RPUnit = rpy_element.getSaveUnit
        Dim unit_name As String = rpy_unit.name
        If Not unit_name_list.Contains(unit_name) Then
            If unit_name <> "Software_Data_Type_Metamodel" Then
                Dim unit_directory As String = rpy_unit.currentDirectory
                unit_name_list.Add(unit_name)
                unit_directory_list.Add(unit_directory)
            End If
        End If

    End Sub

End Class
