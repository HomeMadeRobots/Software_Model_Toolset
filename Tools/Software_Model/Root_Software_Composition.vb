Imports rhapsody2

Public Class Root_Software_Composition

    Inherits Software_Element

    Public Component_Prototypes As List(Of Component_Prototype)
    Public Assembly_Connectors As List(Of Assembly_Connector)

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

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        ' No child.
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

End Class


Public Class Configuration_Value

    Public Component_Configuration_Ref As Guid = Nothing
    Public Value As String

End Class