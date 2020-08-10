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
                    Dim provider_port As RPPort = Nothing
                    Dim requirer_port As RPPort = Nothing
                    Dim provider_component As RPInstance = Nothing
                    Dim requirer_component As RPInstance = Nothing
                    Assembly_Connector.Get_Connector_Info(
                        rpy_link,
                        provider_port,
                        requirer_port,
                        provider_component,
                        requirer_component)
                    connector_new_name = requirer_component.name & "_" & _
                                            requirer_port.name & "_" & _
                                            provider_component.name & "_" & _
                                            provider_port.name
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

End Class
