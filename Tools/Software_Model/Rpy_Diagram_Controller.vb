Imports rhapsody2

Public Class Rpy_Diagram_Controller

    Inherits Rpy_Controller

    Private Const HRZT_SPACE As Integer = 250
    Private Const VERT_SPACE As Integer = 50

    Private Const SWCT_WIDTH As Integer = 350
    Private Const IF_WIDTH As Integer = 400
    Private Const IF_HEIGHT As Integer = 180
    Private Const ELMT_TOP_POS As Integer = 10
    Private Const PROV_IF_HRZT_POS As Integer = 10
    Private Const SWCT_HRZT_POS As Integer = PROV_IF_HRZT_POS + IF_WIDTH + HRZT_SPACE
    Private Const REQ_IF_HORIZONTAL_POS As Integer = SWCT_HRZT_POS + SWCT_WIDTH + HRZT_SPACE

    Private Const SWC_HRZT_POS As Integer = 10
    Private Const SWC_WIDTH As Integer = 350
    Private Const PROV_SWC_HRZT_POS As Integer = SWC_HRZT_POS + SWC_WIDTH + HRZT_SPACE * 2
    Private Const PORT_ASSOC_HEIGHT As Integer = 100
    Private Const RPORT_HRZT_POS As Integer = SWCT_HRZT_POS + SWCT_WIDTH
    Private Const PPORT_HRZT_POS As Integer = REQ_IF_HORIZONTAL_POS

    Public Sub Generate_Component_Type_Diagram()
        ' Initialize output window and display start message
        Dim chrono As New Global.System.Diagnostics.Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Generate Component_Type diagram..." & vbCrLf)

        ' Get selected element and check that it is a Component_Type
        Dim selected_element As RPModelElement
        selected_element = Me.Rhapsody_App.getSelectedElement
        Dim rpy_swct As RPClass = Nothing
        If Not IsNothing(selected_element) Then
            If Is_Component_Type(selected_element) Then
                rpy_swct = CType(selected_element, RPClass)
            End If
        End If
        If IsNothing(rpy_swct) Then
            Me.Rhapsody_App.writeToOutputWindow("out", _
                "A Component_Type shall be selected." & vbCrLf &
                "End Component_Type diagram generation.")
            Exit Sub
        End If

        ' Create the diagram in the same package as the Component_Type
        Dim owner_package As RPPackage = CType(rpy_swct.owner, RPPackage)
        Dim diagram_name As String
        diagram_name = "CD_" & rpy_swct.name & "_" & Now.ToString("yyyy_MM_dd_HH_mm_ss")
        Dim swct_diagram As RPObjectModelDiagram = Nothing
        swct_diagram = owner_package.addObjectModelDiagram(diagram_name)
        swct_diagram.addStereotype("PSWA_Diagram", "ObjectModelDiagram")
        swct_diagram.description = "Class diagram of Component_Type " & rpy_swct.name & "."
        swct_diagram.setPropertyValue("ObjectModelGe.Class.ShowPorts", "False")

        ' Compute Component_Type height on diagram
        Dim swct_height As Integer
        Dim rpy_pport_list As New List(Of RPPort)
        Dim rpy_rport_list As New List(Of RPPort)
        Dim rpy_port As RPPort
        For Each rpy_port In rpy_swct.ports
            If Is_Provider_Port(CType(rpy_port, RPModelElement)) Then
                rpy_pport_list.Add(rpy_port)
            ElseIf Is_Requirer_Port(CType(rpy_port, RPModelElement)) Then
                rpy_rport_list.Add(rpy_port)
            End If
        Next
        swct_height = (VERT_SPACE + IF_HEIGHT) *
                        Math.Max(rpy_pport_list.Count, rpy_rport_list.Count)

        ' Add the Component_Type in the diagram
        swct_diagram.AddNewNodeForElement(
            CType(rpy_swct, RPModelElement),
            SWCT_HRZT_POS, ELMT_TOP_POS,
            SWCT_WIDTH, swct_height)

        Dim rpy_if As RPClass
        Dim interface_y_pos As Integer = VERT_SPACE + ELMT_TOP_POS
        For Each rpy_port In rpy_pport_list
            swct_diagram.AddNewNodeForElement(
                CType(rpy_port, RPModelElement), _
                SWCT_HRZT_POS, _
                interface_y_pos, _
                0,
                0)
            If rpy_port.providedInterfaces.Count >= 1 Then
                rpy_if = CType(rpy_port.providedInterfaces.Item(1), RPClass)
                swct_diagram.AddNewNodeForElement(
                    CType(rpy_if, RPModelElement), _
                    PROV_IF_HRZT_POS,
                    interface_y_pos,
                    IF_WIDTH,
                    IF_HEIGHT)
            End If
            interface_y_pos += (VERT_SPACE + IF_HEIGHT)
        Next

        interface_y_pos = VERT_SPACE + ELMT_TOP_POS
        For Each rpy_port In rpy_rport_list
            swct_diagram.AddNewNodeForElement(
                CType(rpy_port, RPModelElement),
                SWCT_HRZT_POS + SWCT_WIDTH,
                interface_y_pos,
                0,
                0)
            If rpy_port.requiredInterfaces.Count >= 1 Then
                rpy_if = CType(rpy_port.requiredInterfaces.Item(1), RPClass)
                swct_diagram.AddNewNodeForElement(
                    CType(rpy_if, RPModelElement),
                    REQ_IF_HORIZONTAL_POS,
                    interface_y_pos, _
                    IF_WIDTH,
                    IF_HEIGHT)
            End If
            interface_y_pos += (VERT_SPACE + IF_HEIGHT)
        Next

        swct_diagram.openDiagram()

        Rhapsody_App.writeToOutputWindow("out", diagram_name & " successfully generated." & vbCrLf)

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out", "End Component_Type diagram generation." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub

    Public Sub Generate_Component_Prototype_Diagram()
        ' Initialize output window and display start message
        Dim chrono As New Global.System.Diagnostics.Stopwatch
        chrono.Start()
        Rhapsody_App.clearOutputWindow("out")
        Rhapsody_App.writeToOutputWindow("out", "Generate Component_Prototype diagram..." & vbCrLf)

        ' Get selected element and check that it is a Component_Type
        Dim selected_element As RPModelElement
        selected_element = Me.Rhapsody_App.getSelectedElement
        Dim rpy_swc As RPInstance = Nothing
        If Not IsNothing(selected_element) Then
            If Is_Component_Prototype(selected_element) Then
                rpy_swc = CType(selected_element, RPInstance)
            End If
        End If
        If IsNothing(rpy_swc) Then
            Me.Rhapsody_App.writeToOutputWindow("out", _
                "A Component_Prototype shall be selected." & vbCrLf &
                "End Component_Prototype diagram generation.")
            Exit Sub
        End If

        ' Create the diagram in the same package as the Root_Software_Composition that own the
        ' Component_Prototype
        Dim rpy_compo As RPClass = CType(rpy_swc.owner, RPClass)
        Dim owner_package As RPPackage = CType(rpy_compo.owner, RPPackage)
        Dim diagram_name As String
        diagram_name = "OD_" & rpy_swc.name & "_" & Now.ToString("yyyy_MM_dd_HH_mm_ss")
        Dim swc_diagram As RPObjectModelDiagram = Nothing
        swc_diagram = owner_package.addObjectModelDiagram(diagram_name)
        swc_diagram.addStereotype("PSWA_Diagram", "ObjectModelDiagram")
        swc_diagram.description = "Object diagram of Component_Prototype " & rpy_swc.name & "."
        swc_diagram.setPropertyValue("ObjectModelGe.Object.ShowPorts", "False")

        ' Get the list of provider Component_Prototypes and Assembly_Connectors to draw
        Dim rpy_prov_swc_list As New List(Of RPInstance)
        Dim rpy_conn_list As New List(Of RPLink)
        Dim rpy_link As RPLink
        For Each rpy_link In rpy_compo.links
            Dim prov_port As RPPort = Nothing
            Dim req_port As RPPort = Nothing
            Dim prov_swc As RPInstance = Nothing
            Dim req_swc As RPInstance = Nothing
            Assembly_Connector.Get_Connector_Info(rpy_link, prov_port, req_port, prov_swc, req_swc)
            If req_swc.GUID = rpy_swc.GUID Then
                rpy_conn_list.Add(rpy_link)
                If Not rpy_prov_swc_list.Contains(prov_swc) Then
                    rpy_prov_swc_list.Add(prov_swc)
                End If
            End If
        Next

        ' Draw the requirer Component_Prototype
        Dim swc_height As Integer
        swc_height = rpy_conn_list.Count * PORT_ASSOC_HEIGHT + rpy_prov_swc_list.Count * VERT_SPACE
        swc_diagram.AddNewNodeForElement(
            CType(rpy_swc, RPModelElement),
            SWC_HRZT_POS,
            ELMT_TOP_POS,
            SWC_WIDTH,
            swc_height)

        Dim y_pos As Integer = ELMT_TOP_POS
        For Each rpy_prov_swc In rpy_prov_swc_list

            Dim prov_port As RPPort = Nothing
            Dim req_port As RPPort = Nothing
            Dim prov_swc As RPInstance = Nothing
            Dim req_swc As RPInstance = Nothing

            ' Get the list of connector for the current provider Component_Prototype
            Dim connector_to_draw_list As New List(Of RPLink)
            Dim rpy_conn As RPLink
            For Each rpy_conn In rpy_conn_list
                Assembly_Connector.Get_Connector_Info(
                    rpy_conn, prov_port, req_port, prov_swc, req_swc)
                If prov_swc.GUID = rpy_prov_swc.GUID Then
                    connector_to_draw_list.Add(rpy_conn)
                End If
            Next

            ' Draw the provider Component_Prototype
            swc_height = connector_to_draw_list.Count * PORT_ASSOC_HEIGHT
            swc_diagram.AddNewNodeForElement(
                CType(rpy_prov_swc, RPModelElement),
                PROV_SWC_HRZT_POS,
                y_pos,
                SWC_WIDTH,
                swc_height)
            Dim port_y_pos As Integer = CInt(y_pos + PORT_ASSOC_HEIGHT / 2)

            ' Draw its Provider_Ports, the associated Requirer_Port and Assembly_Connector
            For Each rpy_conn In connector_to_draw_list

                Assembly_Connector.Get_Connector_Info(
                    rpy_conn, prov_port, req_port, prov_swc, req_swc)

                ' Draw the Requirer_Port
                Dim rp_node As RPGraphNode = Nothing
                rp_node = swc_diagram.AddNewNodeForElement(
                    CType(req_port, RPModelElement),
                    SWC_HRZT_POS + SWC_WIDTH,
                    port_y_pos,
                    0,
                    0)

                ' Draw the  Provider_Port
                Dim pp_node As RPGraphNode
                pp_node = swc_diagram.AddNewNodeForElement(
                    CType(prov_port, RPModelElement),
                    PROV_SWC_HRZT_POS,
                    port_y_pos,
                    0, 0)

                ' Draw the Assembly_Connector
                swc_diagram.AddNewEdgeForElement(
                    CType(rpy_conn, RPModelElement),
                    pp_node, 0, 0,
                    rp_node, 0, 0)

                port_y_pos += PORT_ASSOC_HEIGHT
            Next
            y_pos += swc_height + VERT_SPACE
        Next

        swc_diagram.openDiagram()

        Rhapsody_App.writeToOutputWindow("out", diagram_name & " successfully generated." & vbCrLf)

        ' Display Result to output window
        Rhapsody_App.writeToOutputWindow("out",
            "End Component_Prototype diagram generation." & vbCrLf)
        chrono.Stop()
        Rhapsody_App.writeToOutputWindow("out", Get_Elapsed_Time(chrono))
    End Sub


End Class
