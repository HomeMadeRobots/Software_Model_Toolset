﻿Imports rhapsody2
Imports System.IO

Module Software_Model_Helpers

    Public Sub main()

        Dim function_to_call As String

        If My.Application.CommandLineArgs.Count <> 0 Then
            function_to_call = My.Application.CommandLineArgs.Item(0)
        Else
            function_to_call = "Configure"
        End If

        Try
            Select Case function_to_call

                Case "Export"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Export_Rpy_Soft_Model_To_Xml()

                Case "Merge"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Import_And_Merge_By_Name()

                Case "Check"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Check_Rpy_Soft_Model()

                Case "Compute_Rpy_Soft_Model_PSWA_Metrics"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Compute_Rpy_Soft_Model_PSWA_Metrics()

                Case "Generate_All"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Generate_All()

                Case "Remove_Empty_Packages"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Remove_Empty_Packages()

                Case "Rename_Connectors"
                    Dim conn_ctrl As New Rpy_Connector_Controller
                    conn_ctrl.Rename_Connectors()

                Case "Navigate_To_Connector_Source"
                    Dim conn_ctrl As New Rpy_Connector_Controller
                    conn_ctrl.Navigate_To_Connector_Source()

                Case "Navigate_To_Connector_Destination"
                    Dim conn_ctrl As New Rpy_Connector_Controller
                    conn_ctrl.Navigate_To_Connector_Destination()

                Case "Generate_Component_Type_Diagram"
                    Dim diagram_ctrl As New Rpy_Diagram_Controller
                    diagram_ctrl.Generate_Component_Type_Diagram()

                Case "Generate_Component_Prototype_Diagram"
                    Dim diagram_ctrl As New Rpy_Diagram_Controller
                    diagram_ctrl.Generate_Component_Prototype_Diagram()

                Case "Display_GUID"
                    Dim elmt_ctrl As New Rpy_Element_Controller
                    elmt_ctrl.Display_Rpy_Element_GUID()

                Case "Modify_GUID"
                    Dim elmt_ctrl As New Rpy_Element_Controller
                    elmt_ctrl.Modify_Rpy_Element_GUID()

                Case "Configure"
                    Dim prj_ctrl As New Rpy_Project_Controller
                    prj_ctrl.Open_Configuration_View()

                Case "Update_Realizations"
                    Dim prj_ctrl As New Rpy_Component_Design_Controller
                    prj_ctrl.Update_Realizations()

                Case "Create_Component_Design_Model"
                    Dim prj_ctrl As New Rpy_Component_Design_Controller
                    prj_ctrl.Open_Component_Design_Model_Creation_View()

                Case "Find_Packages_Cyclic_Dependencies"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Find_Packages_Cyclic_Dependencies()

                Case "Find_Component_Prototypes_Cyclic_Dependencies"
                    Dim sw_mdl_ctrl As New Rpy_Model_Controller
                    sw_mdl_ctrl.Find_Component_Prototypes_Cyclic_Dependencies()

                Case "Transform_To_CLOOF"
                    Dim mdl_transf As New Rpy_Model_Transformer
                    mdl_transf.Transform_To_CLOOF()

            End Select
        Catch ex As Exception
            MsgBox("Software_Model_Tool : an error occured." & vbCrLf & vbCrLf & ex.Message)
        End Try
    End Sub

End Module
