﻿Imports rhapsody2

Public Class Root_Software_Composition

    Inherits Software_Element

    Public Component_Prototypes As List(Of Component_Prototype)
    Public Assembly_Connectors As List(Of Assembly_Connector)

    Private Nb_Conn_By_PPort_By_Component As New Dictionary(Of Guid, Dictionary(Of Guid, Integer))
    Private Nb_Conn_By_RPort_By_Component As New Dictionary(Of Guid, Dictionary(Of Guid, Integer))

    Public Overrides Function Get_Children() As List(Of Software_Element)
        Dim children As New List(Of Software_Element)
        If Not IsNothing(Me.Component_Prototypes) Then
            For Each swc In Me.Component_Prototypes
                children.Add(swc)
            Next
        End If
        If Not IsNothing(Me.Assembly_Connectors) Then
            For Each conn In Me.Assembly_Connectors
                children.Add(conn)
            Next
        End If
        Return children
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Component_Prototypes = New List(Of Component_Prototype)

        Dim rpy_component As RPInstance

        For Each rpy_component In CType(Me.Rpy_Element, RPClass).relations
            If Is_Component_Prototype(CType(rpy_component, RPModelElement)) Then
                Dim component As New Component_Prototype
                Me.Component_Prototypes.Add(component)
                component.Import_From_Rhapsody_Model(Me, CType(rpy_component, RPModelElement))
            End If
        Next

        Me.Assembly_Connectors = New List(Of Assembly_Connector)

        Dim rpy_link As RPLink
        For Each rpy_link In CType(Me.Rpy_Element, RPClass).links
            If Is_Assembly_Connector(CType(rpy_link, RPModelElement)) Then
                Dim connector As New Assembly_Connector
                Me.Assembly_Connectors.Add(connector)
                connector.Import_From_Rhapsody_Model(Me, CType(rpy_link, RPModelElement))
            End If
        Next

    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Component_Prototypes.Count < 2 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "Should aggregate at least two Component_Prototypes.")
        End If

        For Each conn In Me.Assembly_Connectors
            Dim p_swc_port As Port = CType(Get_Element_By_Uuid(conn.Provider_Port_Ref), Port)
            Dim r_swc_port As Port = CType(Get_Element_By_Uuid(conn.Requirer_Port_Ref), Port)

            ' Check connected ports contract.
            If p_swc_port.Contract_Ref <> r_swc_port.Contract_Ref Then
                Dim r_swc As Component_Prototype
                r_swc = CType(Get_Element_By_Uuid(conn.Requirer_Component_Ref), Component_Prototype)
                r_swc.Add_Consistency_Check_Error_Item(report,
                    "TBD",
                    r_swc_port.Name & " shall be linked to a Provider_Port with the same contract.")
            End If

            ' Build dictionaries of assembly connections
            ' These dictionaries will be used for Component_Prototype the consistency check.
            Dim nb_conn As Integer
            Dim nb_conn_by_port As Dictionary(Of Guid, Integer)
            ' Treat the provider Component_Prototype
            If Nb_Conn_By_PPort_By_Component.ContainsKey(conn.Provider_Component_Ref) Then
                nb_conn_by_port = Nb_Conn_By_PPort_By_Component.Item(conn.Provider_Component_Ref)
                If nb_conn_by_port.ContainsKey(p_swc_port.UUID) Then
                    nb_conn = nb_conn_by_port.Item(p_swc_port.UUID)
                    nb_conn_by_port.Item(p_swc_port.UUID) = nb_conn + 1
                Else
                    nb_conn_by_port.Add(p_swc_port.UUID, 1)
                End If
            Else
                nb_conn_by_port = New Dictionary(Of Guid, Integer)
                nb_conn_by_port.Add(p_swc_port.UUID, 1)
                Nb_Conn_By_PPort_By_Component.Add(conn.Provider_Component_Ref, nb_conn_by_port)
            End If
            ' Treat the requirer Component_Prototype
            If Nb_Conn_By_RPort_By_Component.ContainsKey(conn.Requirer_Component_Ref) Then
                nb_conn_by_port = Nb_Conn_By_RPort_By_Component.Item(conn.Requirer_Component_Ref)
                If nb_conn_by_port.ContainsKey(r_swc_port.UUID) Then
                    nb_conn = nb_conn_by_port.Item(r_swc_port.UUID)
                    nb_conn_by_port.Item(r_swc_port.UUID) = nb_conn + 1
                Else
                    nb_conn_by_port.Add(r_swc_port.UUID, 1)
                End If
            Else
                nb_conn_by_port = New Dictionary(Of Guid, Integer)
                nb_conn_by_port.Add(r_swc_port.UUID, 1)
                Nb_Conn_By_RPort_By_Component.Add(conn.Requirer_Component_Ref, nb_conn_by_port)
            End If

        Next

    End Sub

    Public Function Get_PPort_Nb_Connection(component_uuid As Guid, port_uuid As Guid) As Integer
        Dim nb_conn As Integer = 0
        If Nb_Conn_By_PPort_By_Component.ContainsKey(component_uuid) Then
            Dim nb_conn_by_port = Nb_Conn_By_PPort_By_Component.Item(component_uuid)
            If nb_conn_by_port.ContainsKey(port_uuid) Then
                nb_conn = nb_conn_by_port.Item(port_uuid)
            End If
        End If
        Return nb_conn
    End Function

    Public Function Get_RPort_Nb_Connection(component_uuid As Guid, port_uuid As Guid) As Integer
        Dim nb_conn As Integer = 0
        If Nb_Conn_By_RPort_By_Component.ContainsKey(component_uuid) Then
            Dim nb_conn_by_port = Nb_Conn_By_RPort_By_Component.Item(component_uuid)
            If nb_conn_by_port.ContainsKey(port_uuid) Then
                nb_conn = nb_conn_by_port.Item(port_uuid)
            End If
        End If
        Return nb_conn
    End Function

End Class


Public Class Component_Prototype

    Inherits Software_Element

    Public Component_Type_Ref As Guid = Nothing
    Public Configuration_Values As List(Of Configuration_Value)

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_component As RPInstance
        rpy_component = CType(Me.Rpy_Element, RPInstance)
        If IsNothing(rpy_component.ObjectAsObjectType) Then
            Dim rpy_base_class As RPClass
            rpy_base_class = CType(rpy_component.otherClass, RPClass)
            If Not IsNothing(rpy_base_class) Then

                Me.Component_Type_Ref = Transform_GUID_To_UUID(rpy_base_class.GUID)

                Me.Configuration_Values = New List(Of Configuration_Value)
                Dim rpy_attribute As RPAttribute
                For Each rpy_attribute In rpy_base_class.attributes
                    If Is_Component_Configuration(CType(rpy_attribute, RPModelElement)) Then
                        Dim conf_val As New Configuration_Value
                        conf_val.Component_Configuration_Ref =
                            Transform_GUID_To_UUID(rpy_attribute.GUID)
                        conf_val.Value =
                            CType(Me.Rpy_Element, RPInstance).getAttributeValue(rpy_attribute.name)
                        Me.Configuration_Values.Add(conf_val)
                    End If
                Next

                If Me.Configuration_Values.Count = 0 Then
                    Me.Configuration_Values = Nothing
                End If

            End If
        End If

    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Component_Type_Ref.Equals(Guid.Empty) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall reference a Component_Type.")
        Else
            ' Check Ports connections
            Dim port As Port
            Dim nb_conn As Integer

            Dim referenced_swct As Component_Type
            referenced_swct = CType(Me.Get_Element_By_Uuid(Me.Component_Type_Ref), Component_Type)

            If Not IsNothing(referenced_swct.Provider_Ports) Then
                For Each port In referenced_swct.Provider_Ports
                    nb_conn = CType(Me.Parent, Root_Software_Composition). _
                                Get_PPort_Nb_Connection(Me.UUID, port.UUID)
                    If nb_conn = 0 Then
                        Me.Add_Consistency_Check_Information_Item(report,
                            "TBD",
                            "Provider_Port " & port.Name & " not connected.")
                    End If
                Next
            End If

            If Not IsNothing(referenced_swct.Requirer_Ports) Then
                For Each port In referenced_swct.Requirer_Ports
                    nb_conn = CType(Me.Parent, Root_Software_Composition). _
                                Get_RPort_Nb_Connection(Me.UUID, port.UUID)
                    If nb_conn = 0 Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "TBD",
                            "Requirer_Port " & port.Name & " shall be connected.")
                    ElseIf nb_conn > 1 Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "TBD",
                            "Requirer_Port " & port.Name & " shall be connected to only one port.")
                    End If
                Next
            End If

        End If

    End Sub

End Class


Public Class Assembly_Connector

    Inherits Software_Element

    Public Provider_Component_Ref As Guid = Nothing
    Public Provider_Port_Ref As Guid = Nothing
    Public Requirer_Component_Ref As Guid = Nothing
    Public Requirer_Port_Ref As Guid = Nothing


    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Get the UUID of ports and components
        Dim rpy_provider_port As RPPort = Nothing
        Dim rpy_requirer_port As RPPort = Nothing
        Dim rpy_provider_component As RPInstance = Nothing
        Dim rpy_requirer_component As RPInstance = Nothing

        Dim rpy_link As RPLink = CType(Me.Rpy_Element, RPLink)

        rpy_provider_port = rpy_link.toPort
        If Is_Provider_Port(CType(rpy_provider_port, RPModelElement)) Then
            rpy_requirer_port = rpy_link.fromPort
            rpy_provider_component = rpy_link.to
            rpy_requirer_component = rpy_link.from
        Else
            rpy_provider_port = rpy_link.fromPort
            rpy_requirer_port = rpy_link.toPort
            rpy_provider_component = rpy_link.from
            rpy_requirer_component = rpy_link.to
        End If

        Me.Provider_Component_Ref = Transform_GUID_To_UUID(rpy_provider_component.GUID)
        Me.Provider_Port_Ref = Transform_GUID_To_UUID(rpy_provider_port.GUID)
        Me.Requirer_Component_Ref = Transform_GUID_To_UUID(rpy_requirer_component.GUID)
        Me.Requirer_Port_Ref = Transform_GUID_To_UUID(rpy_requirer_port.GUID)

    End Sub

    Private Sub Get_Connector_Info(rpy_link As RPLink, _
                                  ByRef provider_port As RPPort, _
                                  ByRef requirer_port As RPPort, _
                                  ByRef provider_component As RPInstance, _
                                  ByRef requirer_component As RPInstance)
        provider_port = rpy_link.toPort
        If Is_Provider_Port(CType(provider_port, RPModelElement)) Then
            requirer_port = rpy_link.fromPort
            provider_component = rpy_link.to
            requirer_component = rpy_link.from
        Else
            provider_port = rpy_link.fromPort
            requirer_port = rpy_link.toPort
            provider_component = rpy_link.from
            requirer_component = rpy_link.to
        End If
    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Provider_Component_Ref = Me.Requirer_Component_Ref Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall link two different components.")
        End If
    End Sub

End Class


Public Class Configuration_Value

    Public Component_Configuration_Ref As Guid = Nothing
    Public Value As String

End Class