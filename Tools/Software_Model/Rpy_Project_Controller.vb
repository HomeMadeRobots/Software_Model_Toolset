Imports rhapsody2
Imports System.IO

Public Class Rpy_Project_Controller
    Inherits Rpy_Controller

    Private Rpy_Proj As RPProject

    Private Configurator As Rhapsody_Project_Configurator
    Private Viewer As Rhapsody_Project_Configuration_Form

    Public Sub Open_Configuration_View()

        ' Get selected element and check that it is a Rhapsody project
        Me.Rpy_Proj = Get_Rhapsody_Project()

        Me.Configurator = New Rhapsody_Project_Configurator(Me.Rpy_Proj)

        If Not IsNothing(Rpy_Proj) Then

            ' Load last configuration choices
            Dim last_conf_choices As Dictionary(Of String, String)
            last_conf_choices = Rpy_Controller.Load_User_Choices("Rpy_Project_Config_User_Choices")

            Dim last_toolset_path As String = ""
            If last_conf_choices.ContainsKey("Toolset_Path") Then
                last_toolset_path = last_conf_choices("Toolset_Path")
            End If
            Dim last_profiles_config As Boolean = False
            If last_conf_choices.ContainsKey("Configure_Profiles") Then
                last_profiles_config = CBool(last_conf_choices("Configure_Profiles"))
            End If
            Dim last_helpers_config As Boolean = False
            If last_conf_choices.ContainsKey("Configure_Helpers") Then
                last_helpers_config = CBool(last_conf_choices("Configure_Helpers"))
            End If
            Dim last_diagram_config As Boolean = False
            If last_conf_choices.ContainsKey("Configure_Activity_Diagrams") Then
                last_diagram_config = CBool(last_conf_choices("Configure_Activity_Diagrams"))
            End If
            Dim last_types_config As Boolean = False
            If last_conf_choices.ContainsKey("Configure_Accessible_Types") Then
                last_types_config = CBool(last_conf_choices("Configure_Accessible_Types"))
            End If
            Dim last_package_config As Boolean = False
            If last_conf_choices.ContainsKey("Configure_Packages_Are_Not_Unit") Then
                last_package_config = CBool(last_conf_choices("Configure_Packages_Are_Not_Unit"))

            End If

            Me.Viewer = Rhapsody_Project_Configuration_Form.Load_Form(
                Me,
                last_toolset_path,
                last_profiles_config,
                last_helpers_config,
                last_diagram_config,
                last_package_config,
                last_types_config)
            Me.Viewer.ShowDialog()
        End If

    End Sub


    Public Sub Configure(
        toolset_path As String,
        configure_profiles As Boolean,
        configure_helpers As Boolean,
        configure_activity_diagrams As Boolean,
        configure_package As Boolean,
        configure_accessible_types As Boolean)

        Me.Viewer.Reset_Status()

        If configure_profiles = True Then
            Dim status As Rhapsody_Project_Configurator.E_Profile_Configuration_Status
            Me.Configurator.Configure_Profiles(Me.Rhapsody_App, toolset_path, status)
            Select Case status
                Case Rhapsody_Project_Configurator.E_Profile_Configuration_Status.CONFIGURATION_OK
                    Me.Viewer.Update_Profiles_Configuration_Status("OK", "Done.")
                Case Rhapsody_Project_Configurator.E_Profile_Configuration_Status.PROFILES_NOT_FOUND
                    Me.Viewer.Update_Profiles_Configuration_Status(
                        "Error",
                        "Profiles not found." & vbCrLf & "Check Toolset path.")
            End Select
        End If

        If configure_helpers = True Then
            Me.Configurator.Configure_Helper(toolset_path)
            Me.Viewer.Update_Helpers_Configuration_Status("OK", "Done.")
        End If

        If configure_activity_diagrams = True Then
            Me.Configurator.Configure_Activity_Diagrams_Default_View()
            Me.Viewer.Update_Act_Diagrams_Configuration_Status("OK", "Done.")
        End If

        If configure_package = True Then
            Me.Configurator.Configure_Packages_Are_Not_Unit()
            Me.Viewer.Update_Pkg_Are_Not_Unit_Configuration_Status("OK", "Done.")
        End If

        If configure_accessible_types = True Then
            Me.Configurator.Configure_Accessible_Types()
            Me.Viewer.Update_Accessible_Types_Configuration_Status("OK", "Done.")
        End If

        ' Save configuration choices
        Dim new_choices As New Dictionary(Of String, String)
        new_choices.Add("Toolset_Path", toolset_path)
        new_choices.Add("Configure_Profiles", configure_profiles.ToString)
        new_choices.Add("Configure_Helpers", configure_helpers.ToString)
        new_choices.Add("Configure_Activity_Diagrams", configure_activity_diagrams.ToString)
        new_choices.Add("Configure_Packages_Are_Not_Unit", configure_package.ToString)
        new_choices.Add("Configure_Accessible_Types", configure_accessible_types.ToString)
        Rpy_Controller.Save_User_Choices("Rpy_Project_Config_User_Choices", new_choices)

        Me.Rpy_Proj.save()
    End Sub

End Class