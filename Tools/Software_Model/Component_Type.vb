Imports rhapsody2
Imports System.Guid

Public Class Component_Type

    Inherits Software_Element

    Public Component_Operations As List(Of Component_Operation)
    Public Component_Configurations As List(Of Component_Configuration)
    Public Provider_Ports As List(Of Provider_Port)
    Public Requirer_Ports As List(Of Requirer_Port)

    Private Nb_Inheritance As UInteger = 0
    Private Invalid_Inheritance As Boolean = False

    Public Overrides Function Get_Children() As List(Of Software_Element)
        Dim children As New List(Of Software_Element)
        If Not IsNothing(Me.Provider_Ports) Then
            For Each pp In Me.Provider_Ports
                children.Add(pp)
            Next
        End If
        If Not IsNothing(Me.Requirer_Ports) Then
            For Each rp In Me.Requirer_Ports
                children.Add(rp)
            Next
        End If
        If Not IsNothing(Me.Component_Operations) Then
            For Each op In Me.Component_Operations
                children.Add(op)
            Next
        End If
        If Not IsNothing(Me.Component_Configurations) Then
            For Each cf In Me.Component_Configurations
                children.Add(cf)
            Next
        End If
        Return children
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Provider_Ports = New List(Of Provider_Port)
        Me.Requirer_Ports = New List(Of Requirer_Port)

        Dim rpy_port As RPPort
        For Each rpy_port In CType(Me.Rpy_Element, RPClass).ports
            If Is_Provider_Port(CType(rpy_port, RPModelElement)) Then
                Dim pport As Provider_Port = New Provider_Port
                Me.Provider_Ports.Add(pport)
                pport.Import_From_Rhapsody_Model(Me, CType(rpy_port, RPModelElement))
            ElseIf Is_Requirer_Port(CType(rpy_port, RPModelElement)) Then
                Dim rport As Requirer_Port = New Requirer_Port
                Me.Requirer_Ports.Add(rport)
                rport.Import_From_Rhapsody_Model(Me, CType(rpy_port, RPModelElement))
            End If
        Next

        If Me.Provider_Ports.Count = 0 Then
            Me.Provider_Ports = Nothing
        End If
        If Me.Requirer_Ports.Count = 0 Then
            Me.Requirer_Ports = Nothing
        End If

        Me.Component_Operations = New List(Of Component_Operation)

        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            If Is_Component_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim ope As Component_Operation = New Component_Operation
                Me.Component_Operations.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            End If
        Next

        If Me.Component_Operations.Count = 0 Then
            Me.Component_Operations = Nothing
        End If

        Me.Component_Configurations = New List(Of Component_Configuration)

         Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            If Is_Component_Configuration(CType(rpy_attribute, RPModelElement)) Then
                Dim conf As Component_Configuration = New Component_Configuration
                Me.Component_Configurations.Add(conf)
                conf.Import_From_Rhapsody_Model(Me, CType(rpy_attribute, RPModelElement))
            End If
        Next

        If Me.Component_Configurations.Count = 0 Then
            Me.Component_Configurations = Nothing
        End If

    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If IsNothing(Me.Provider_Ports) And IsNothing(Me.Requirer_Ports) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall aggregate at least one Port.")
        End If

    End Sub

End Class


Public MustInherit Class Port

    Inherits Software_Element

    Public Contract_Ref As Guid = Nothing

    Protected Nb_Contracts As UInteger = 0

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Nb_Contracts <> 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall have one and only one contract.")
        End If

    End Sub

End Class


Public Class Provider_Port

    Inherits Port

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)

        Me.Nb_Contracts = CUInt(rpy_port.providedInterfaces.Count)

        If Me.Nb_Contracts >= 1 Then
            Dim prov_if As RPClass
            prov_if = CType(rpy_port.providedInterfaces.Item(1), RPClass)
            Contract_Ref = Transform_GUID_To_UUID(prov_if.GUID)
        End If

    End Sub

End Class


Public Class Requirer_Port

    Inherits Port

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)

        Me.Nb_Contracts = CUInt(rpy_port.requiredInterfaces.Count)

        If Me.Nb_Contracts >= 1 Then
            Dim req_if As RPClass
            req_if = CType(rpy_port.requiredInterfaces.Item(1), RPClass)
            Contract_Ref = Transform_GUID_To_UUID(req_if.GUID)
        End If


    End Sub

End Class


Public Class Component_Operation

    Inherits Software_Element

End Class


Public Class Component_Configuration

    Inherits Typed_Software_Element

    Public Default_Value As String = Nothing


    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPAttribute).type
        Return CType(rpy_type, RPModelElement)
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim default_val_raw As String = CType(Me.Rpy_Element, RPAttribute).defaultValue
        If default_val_raw <> "" Then
            Me.Default_Value = default_val_raw
        End If
    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Not Me.Base_Data_Type_Ref.Equals(Guid.Empty) Then

            Dim config_data_type As Data_Type
            config_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            Select Case config_data_type.GetType
                Case GetType(Structured_Data_Type)
                    Me.Add_Consistency_Check_Error_Item(report,
                        "TBD",
                        "Type shall not be Structured_Data_Type.")
            End Select

        End If

    End Sub

End Class