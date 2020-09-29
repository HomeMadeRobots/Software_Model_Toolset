﻿Imports rhapsody2
Imports System.Xml.Serialization


Public Class Component_Type

    Inherits SMM_Class

    Public Component_Operations As New List(Of Component_Operation)
    <XmlArrayItem("Configuration")>
    Public Configurations As New List(Of Configuration_Parameter)
    Public Provider_Ports As New List(Of Provider_Port)
    Public Requirer_Ports As New List(Of Requirer_Port)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Provider_Ports)
            children_list.AddRange(Me.Requirer_Ports)
            children_list.AddRange(Me.Component_Operations)
            children_list.AddRange(Me.Configurations)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

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

        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            If Is_Component_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim ope As Component_Operation = New Component_Operation
                Me.Component_Operations.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            End If
        Next

        Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            If Is_Configuration_Parameter(CType(rpy_attribute, RPModelElement)) Then
                Dim conf As Configuration_Parameter = New Configuration_Parameter
                Me.Configurations.Add(conf)
                conf.Import_From_Rhapsody_Model(Me, CType(rpy_attribute, RPModelElement))
            End If
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Type", "Class")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Provider_Ports.Count = 0 And Me.Requirer_Ports.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "SWCT_1",
                "Shall aggregate at least one Port.")
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)

            For Each port In Me.Provider_Ports
                Dim sw_if As Software_Interface
                sw_if = CType(Me.Get_Element_By_Uuid(port.Contract_Ref), Software_Interface)
                If Not Me.Needed_Elements.Contains(sw_if) Then
                    Me.Needed_Elements.Add(sw_if)
                End If
            Next

            For Each port In Me.Requirer_Ports
                Dim sw_if As Software_Interface
                sw_if = CType(Me.Get_Element_By_Uuid(port.Contract_Ref), Software_Interface)
                If Not Me.Needed_Elements.Contains(sw_if) Then
                    Me.Needed_Elements.Add(sw_if)
                End If
            Next

            For Each conf In Me.Configurations
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(conf.Base_Data_Type_Ref), Data_Type)
                If Not data_type.Is_Basic_Type Then
                    If Not Me.Needed_Elements.Contains(data_type) Then
                        Me.Needed_Elements.Add(data_type)
                    End If
                End If
            Next

        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of SMM_Classifier)
            Dim compo_list As List(Of Root_Software_Composition)
            compo_list = Me.Container.Get_All_Compositions
            For Each compo In compo_list
                For Each swc In compo.Component_Prototypes
                    If swc.Type_Ref = Me.UUID Then
                        If Not Me.Dependent_Elements.Contains(compo) Then
                            Me.Dependent_Elements.Add(compo)
                        End If
                    End If
                Next
            Next
        End If
        Return Me.Dependent_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            For Each pport In Me.Provider_Ports
                Dim sw_if As Software_Interface
                sw_if = CType(Me.Get_Element_By_Uuid(pport.Contract_Ref), Software_Interface)
                Me.Weighted_Methods_Per_Class += sw_if.Compute_WMC
            Next
        End If
        Return Me.Weighted_Methods_Per_Class
    End Function

End Class


Public MustInherit Class Port

    Inherits Software_Element

    Public Contract_Ref As Guid = Nothing

    Protected Nb_Contracts As UInteger = 0


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected MustOverride Sub Set_Contract(report As Report)
    Protected MustOverride Sub Merge_Rpy_Contract(report As Report)

    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Port"
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Me.Merge_Rpy_Contract(report)
    End Sub

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return rpy_parent_class.addNewAggr("Port", Me.Name)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Me.Set_Contract(report)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Nb_Contracts <> 1 Then
            Me.Add_Consistency_Check_Error_Item(report, "PORT_1",
                "Shall have one and only one contract.")
        End If

    End Sub

End Class


Public Class Provider_Port

    Inherits Port

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)

        Me.Nb_Contracts = CUInt(rpy_port.providedInterfaces.Count)

        If Me.Nb_Contracts >= 1 Then
            Dim prov_if As RPClass
            prov_if = CType(rpy_port.providedInterfaces.Item(1), RPClass)
            Contract_Ref = Transform_Rpy_GUID_To_Guid(prov_if.GUID)
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Provider_Port", "Port")
    End Sub

    Protected Overrides Sub Set_Contract(report As Report)
        Dim rpy_if As RPClass
        rpy_if = CType(Me.Find_In_Rpy_Project(Me.Contract_Ref), RPClass)
        If IsNothing(rpy_if) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Contract not found : " & Me.Contract_Ref.ToString & ".")
        Else
            CType(Me.Rpy_Element, RPPort).addProvidedInterface(rpy_if)
        End If
    End Sub

    Protected Overrides Sub Merge_Rpy_Contract(report As Report)
        Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)
        Dim current_rpy_if As RPClass = Nothing
        If rpy_port.providedInterfaces.Count >= 1 Then
            current_rpy_if = CType(rpy_port.providedInterfaces.Item(1), RPClass)
        End If
        Dim rpy_if As RPClass
        rpy_if = CType(Me.Find_In_Rpy_Project(Me.Contract_Ref), RPClass)
        If IsNothing(rpy_if) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Contract not found : " & Me.Contract_Ref.ToString & ".")
        ElseIf Not IsNothing(current_rpy_if) Then
            If current_rpy_if.GUID <> rpy_if.GUID Then
                Me.Rpy_Element.getSaveUnit.setReadOnly(0)
                rpy_port.removeProvidedInterface(current_rpy_if)
                rpy_port.addProvidedInterface(rpy_if)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Contract merged.")
            End If
        Else
            Me.Rpy_Element.getSaveUnit.setReadOnly(0)
            rpy_port.addProvidedInterface(rpy_if)
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Contract merged.")
        End If
    End Sub



    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        ' Do not call MyBase.Check_Own_Consistency
        ' Me.Nb_Contracts can be <> 1 but still OK if there is interfaces specialization.

        If Me.Contract_Ref <> Guid.Empty Then
            If CType(Me.Rpy_Element, RPPort).providedInterfaces.Count > 1 Then
                Dim rpy_interface_list As RPCollection
                rpy_interface_list = CType(Me.Rpy_Element, RPPort).providedInterfaces
                Dim interface_index As Integer = 2
                Dim child_interface As Software_Interface
                child_interface = CType(Me.Get_Element_By_Uuid(Me.Contract_Ref), Software_Interface)

                If child_interface.GetType() = GetType(Client_Server_Interface) Then
                    Dim child_csif As Client_Server_Interface
                    child_csif = CType(child_interface, Client_Server_Interface)
                    For interface_index = 2 To rpy_interface_list.Count
                        Dim rpy_current_if As RPModelElement
                        rpy_current_if = CType(rpy_interface_list.Item(interface_index), 
                                        RPModelElement)
                        Dim current_if_UUID As Guid
                        current_if_UUID = Transform_Rpy_GUID_To_Guid(rpy_current_if.GUID)
                        If current_if_UUID <> child_csif.Base_Interface_Ref Then
                            Me.Add_Consistency_Check_Error_Item(report, "PORT_1",
                                "Shall have one and only one contract.")
                            Exit For
                        End If
                        child_interface = CType(Me.Get_Element_By_Uuid(current_if_UUID),  _
                                            Software_Interface)
                        interface_index += 1
                    Next

                Else
                    ' Only Client_Server_Interface can aggregate generalization and can potentially
                    ' lead to multiple contract on provider ports.
                    Me.Add_Consistency_Check_Error_Item(report, "PORT_1",
                        "Shall have one and only one contract.")
                End If

            End If
        Else
            Me.Add_Consistency_Check_Error_Item(report, "PORT_1",
                "Shall have one and only one contract.")
        End If

    End Sub
End Class


Public Class Requirer_Port

    Inherits Port

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)

        Me.Nb_Contracts = CUInt(rpy_port.requiredInterfaces.Count)

        If Me.Nb_Contracts >= 1 Then
            Dim req_if As RPClass
            req_if = CType(rpy_port.requiredInterfaces.Item(1), RPClass)
            Contract_Ref = Transform_Rpy_GUID_To_Guid(req_if.GUID)
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Requirer_Port", "Port")
    End Sub

    Protected Overrides Sub Set_Contract(report As Report)
        Dim rpy_if As RPClass
        rpy_if = CType(Me.Find_In_Rpy_Project(Me.Contract_Ref), RPClass)
        If IsNothing(rpy_if) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Contract not found : " & Me.Contract_Ref.ToString & ".")
        Else
            CType(Me.Rpy_Element, RPPort).addRequiredInterface(rpy_if)
        End If
    End Sub

    Protected Overrides Sub Merge_Rpy_Contract(report As Report)
        Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)
        Dim current_rpy_if As RPClass = Nothing
        If rpy_port.requiredInterfaces.Count >= 1 Then
            current_rpy_if = CType(rpy_port.requiredInterfaces.Item(1), RPClass)
        End If
        Dim rpy_if As RPClass
        rpy_if = CType(Me.Find_In_Rpy_Project(Me.Contract_Ref), RPClass)
        If IsNothing(rpy_if) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Contract not found : " & Me.Contract_Ref.ToString & ".")
        ElseIf Not IsNothing(current_rpy_if) Then
            If rpy_if.GUID <> current_rpy_if.GUID Then
                Me.Rpy_Element.getSaveUnit.setReadOnly(0)
                rpy_port.removeRequiredInterface(current_rpy_if)
                rpy_port.addRequiredInterface(rpy_if)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Contract merged.")
            End If
        Else
            Me.Rpy_Element.getSaveUnit.setReadOnly(0)
            rpy_port.addRequiredInterface(rpy_if)
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Contract merged.")
        End If
    End Sub

End Class


Public Class Component_Operation

    Inherits SMM_Operation

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Operation", "Operation")
    End Sub

End Class
