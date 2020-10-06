Imports rhapsody2

Public Class Rpy_Connector_Controller

    Inherits Rpy_Controller

    Public Sub Rename_Connectors()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Me.Clear_Window()
        Me.Write_Csl_Line("Rename connectors")

        ' Get selected element and check that it is a Root_Software_Composition
        Dim selected_element As RPModelElement = Rhapsody_App.getSelectedElement
        Dim rpy_class As RPClass = Nothing
        If Is_Root_Software_Composition(selected_element) _
            Or Is_Component_Design(selected_element) Then
            rpy_class = CType(selected_element, RPClass)
        End If

        If IsNothing(rpy_class) Then
            Me.Write_Csl_Line("Error : a Root_Software_Composition " &
                "or a Component_Design shall be selected.")
            Me.Write_Csl_Line("End connectors renaming.")
            Exit Sub
        End If

        ' Check all Connector_Prototype within the composition
        Dim rpy_link As RPLink
        Dim link_new_name As String
        For Each rpy_link In rpy_class.links
            If Is_Connector_Prototype(CType(rpy_link, RPModelElement)) Then
                If Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                    link_new_name = Assembly_Connector.Compute_Automatic_Name(rpy_link)
                    If link_new_name.Length <= 128 Then
                        If rpy_link.name <> link_new_name Then
                            Me.Write_Csl_Line("Rename " & rpy_link.name &
                                " as " & link_new_name)
                            rpy_link.name = link_new_name
                        End If
                    Else
                        Me.Write_Csl_Line(
                            "Warning : automatic name is too long (>128 characters) for " &
                            rpy_link.name)
                    End If
                ElseIf Object_Connector.Is_Object_Connector(rpy_link) Then
                    link_new_name = Object_Connector.Compute_Automatic_Name(rpy_link)
                    If rpy_link.name <> link_new_name Then
                        Me.Write_Csl_Line("Rename " & rpy_link.name &
                            " as " & link_new_name)
                        rpy_link.name = link_new_name
                    End If
                End If
            End If
        Next

        ' Display result to output window
        Me.Write_Csl_Line("End connectors renaming.")
        chrono.Stop()
        Me.Write_Csl(Get_Elapsed_Time(chrono))
    End Sub


    Public Sub Navigate_To_Connector_Source()
        Dim selected_element As RPModelElement = Me.Rhapsody_App.getSelectedElement
        Dim rpy_link As RPLink = Nothing
        If Is_Connector_Prototype(selected_element) Then
            rpy_link = CType(selected_element, RPLink)
            If Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                Dim p_port As RPPort = Nothing
                Dim r_port As RPPort = Nothing
                Dim p_swc As RPInstance = Nothing
                Dim r_swc As RPInstance = Nothing
                Assembly_Connector.Get_Connector_Info(rpy_link, p_port, r_port, p_swc, r_swc)
                p_port.locateInBrowser()
            ElseIf Object_Connector.Is_Object_Connector(rpy_link) Then
                Dim obj_to As RPInstance = Nothing
                Dim obj_from As RPInstance = Nothing
                Object_Connector.Get_Connector_Info(rpy_link, obj_from, obj_to)
                obj_from.locateInBrowser()
            End If
        End If
    End Sub


    Public Sub Navigate_To_Connector_Destination()
        Dim selected_element As RPModelElement = Me.Rhapsody_App.getSelectedElement
        Dim rpy_link As RPLink = Nothing
        If Is_Connector_Prototype(selected_element) Then
            rpy_link = CType(selected_element, RPLink)
            If Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                Dim p_port As RPPort = Nothing
                Dim r_port As RPPort = Nothing
                Dim p_swc As RPInstance = Nothing
                Dim r_swc As RPInstance = Nothing
                Assembly_Connector.Get_Connector_Info(rpy_link, p_port, r_port, p_swc, r_swc)
                r_port.locateInBrowser()
            ElseIf Object_Connector.Is_Object_Connector(rpy_link) Then
                Dim obj_to As RPInstance = Nothing
                Dim obj_from As RPInstance = Nothing
                Object_Connector.Get_Connector_Info(rpy_link, obj_from, obj_to)
                obj_to.locateInBrowser()
            End If
        End If
    End Sub

End Class
