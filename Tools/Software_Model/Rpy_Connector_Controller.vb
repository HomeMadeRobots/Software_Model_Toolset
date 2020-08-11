Imports rhapsody2

Public Class Rpy_Connector_Controller

    Inherits Rpy_Controller

    Public Sub Rename_Connectors()
        ' Initialize output window and display start message
        Dim chrono As New Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Rename connectors" & vbCrLf)

        ' Get selected element and check that it is a Root_Software_Composition
        Dim selected_element As RPModelElement = Rhapsody_App.getSelectedElement
        Dim rpy_composition As RPClass = Nothing
        If Is_Root_Software_Composition(selected_element) Then
            rpy_composition = CType(selected_element, RPClass)
        End If

        If IsNothing(rpy_composition) Then
            Rhapsody_App.writeToOutputWindow("out",
                "Error : a Root_Software_Composition shall be selected." & vbCrLf &
                "End connectors renaming.")
            Exit Sub
        End If

        ' Check all Assembly_Connector within the composition
        Dim rpy_link As RPLink
        For Each rpy_link In rpy_composition.links
            If Is_Assembly_Connector(CType(rpy_link, RPModelElement)) Then
                Dim connector_new_name As String
                connector_new_name = Assembly_Connector.Compute_Automatic_Name(rpy_link)
                If connector_new_name.Length <= 128 Then
                    If rpy_link.name <> connector_new_name Then
                        Rhapsody_App.writeToOutputWindow("out",
                            "Rename " & rpy_link.name & " as " & connector_new_name & vbCrLf)
                        rpy_link.name = connector_new_name
                    End If
                Else
                    Rhapsody_App.writeToOutputWindow("out",
                        "Warning : automatic name is too long (>128 characters) for " &
                        rpy_link.name & vbCrLf)
                End If
            End If
        Next

        ' Display result to output window
        Rhapsody_App.writeToOutputWindow("out", "End connectors renaming." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub


    Public Sub Navigate_To_Connector_Source()
        Dim selected_element As RPModelElement = Me.Rhapsody_App.getSelectedElement
        Dim rpy_link As RPLink = Nothing
        If Is_Assembly_Connector(selected_element) Then
            rpy_link = CType(selected_element, RPLink)
            Dim p_port As RPPort = Nothing
            Dim r_port As RPPort = Nothing
            Dim p_swc As RPInstance = Nothing
            Dim r_swc As RPInstance = Nothing
            Assembly_Connector.Get_Connector_Info(rpy_link, p_port, r_port, p_swc, r_swc)
            p_port.locateInBrowser()
        End If
    End Sub


    Public Sub Navigate_To_Connector_Destination()
        Dim selected_element As RPModelElement = Me.Rhapsody_App.getSelectedElement
        Dim rpy_link As RPLink = Nothing
        If Is_Assembly_Connector(selected_element) Then
            rpy_link = CType(selected_element, RPLink)
            Dim p_port As RPPort = Nothing
            Dim r_port As RPPort = Nothing
            Dim p_swc As RPInstance = Nothing
            Dim r_swc As RPInstance = Nothing
            Assembly_Connector.Get_Connector_Info(rpy_link, p_port, r_port, p_swc, r_swc)
            r_port.locateInBrowser()
        End If
    End Sub

End Class
