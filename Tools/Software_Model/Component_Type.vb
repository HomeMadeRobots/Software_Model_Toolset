Imports rhapsody2
Imports System.Guid

Public Class Component_Type

    Inherits Software_Class

    Public Component_Operations As List(Of Component_Operation)
    Public Component_Parameters As List(Of Component_Parameter)
    Public Provider_Ports As List(Of Provider_Port)
    Public Requirer_Ports As List(Of Requirer_Port)

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)

            If Not IsNothing(Me.Provider_Ports) Then
                children_list.AddRange(Me.Provider_Ports)
            End If
            If Not IsNothing(Me.Requirer_Ports) Then
                children_list.AddRange(Me.Requirer_Ports)
            End If
            If Not IsNothing(Me.Component_Operations) Then
                children_list.AddRange(Me.Component_Operations)
            End If
            If Not IsNothing(Me.Component_Parameters) Then
                children_list.AddRange(Me.Component_Parameters)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
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

        Me.Component_Parameters = New List(Of Component_Parameter)
         Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            If Is_Component_Parameter(CType(rpy_attribute, RPModelElement)) Then
                Dim conf As Component_Parameter = New Component_Parameter
                Me.Component_Parameters.Add(conf)
                conf.Import_From_Rhapsody_Model(Me, CType(rpy_attribute, RPModelElement))
            End If
        Next
        If Me.Component_Parameters.Count = 0 Then
            Me.Component_Parameters = Nothing
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_class As RPClass
        rpy_class = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Class"), RPClass)
        If Not IsNothing(rpy_class) Then
            Me.Merge_Rpy_Element(CType(rpy_class, RPModelElement), report)
        Else
            rpy_class = rpy_parent_pkg.addClass(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_class, RPModelElement), report)
            rpy_class.addStereotype("Component_Type", "Class")
        End If

        Dim children As List(Of Software_Element) = Me.Get_Children
        If Not IsNothing(children) Then
            For Each child In children
                child.Export_To_Rhapsody(CType(rpy_class, RPModelElement), report)
            Next
        End If
    End Sub


    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If IsNothing(Me.Provider_Ports) And IsNothing(Me.Requirer_Ports) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "SWCT_1",
                "Shall aggregate at least one Port.")
        End If

    End Sub

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of Classifier_Software_Element)

            If Not IsNothing(Me.Provider_Ports) Then
                For Each port In Me.Provider_Ports
                    Dim sw_if As Software_Interface
                    sw_if = CType(Me.Get_Element_By_Uuid(port.Contract_Ref), Software_Interface)
                    If Not Me.Needed_Elements.Contains(sw_if) Then
                        Me.Needed_Elements.Add(sw_if)
                    End If
                Next
            End If

            If Not IsNothing(Me.Requirer_Ports) Then
                For Each port In Me.Requirer_Ports
                    Dim sw_if As Software_Interface
                    sw_if = CType(Me.Get_Element_By_Uuid(port.Contract_Ref), Software_Interface)
                    If Not Me.Needed_Elements.Contains(sw_if) Then
                        Me.Needed_Elements.Add(sw_if)
                    End If
                Next
            End If

            If Not IsNothing(Me.Component_Parameters) Then
                For Each conf In Me.Component_Parameters
                    Dim data_type As Data_Type
                    data_type = CType(Me.Get_Element_By_Uuid(conf.Base_Data_Type_Ref), Data_Type)
                    If Not data_type.Is_Basic_Type Then
                        If Not Me.Needed_Elements.Contains(data_type) Then
                            Me.Needed_Elements.Add(data_type)
                        End If
                    End If
                Next
            End If

        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of Classifier_Software_Element)
            Dim compo_list As List(Of Root_Software_Composition)
            compo_list = Me.Container.Get_All_Compositions
            For Each compo In compo_list
                If Not IsNothing(compo.Component_Prototypes) Then
                    For Each swc In compo.Component_Prototypes
                        If swc.Component_Type_Ref = Me.UUID Then
                            If Not Me.Dependent_Elements.Contains(compo) Then
                                Me.Dependent_Elements.Add(compo)
                            End If
                        End If
                    Next
                End If
            Next
        End If
        Return Me.Dependent_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            If Not IsNothing(Me.Provider_Ports) Then
                For Each pport In Me.Provider_Ports
                    Dim sw_if As Software_Interface
                    sw_if = CType(Me.Get_Element_By_Uuid(pport.Contract_Ref), Software_Interface)
                    Me.Weighted_Methods_Per_Class += sw_if.Compute_WMC
                Next
            End If
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
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_port As RPPort
        rpy_port = CType(rpy_parent_class.findNestedElement(Me.Name, "Port"), RPPort)
        If Not IsNothing(rpy_port) Then
            Me.Merge_Rpy_Element(CType(rpy_port, RPModelElement), report)
            Me.Merge_Rpy_Contract(report)
        Else
            rpy_port = CType(rpy_parent_class.addNewAggr("Port", Me.Name), RPPort)
            Me.Set_Rpy_Common_Attributes(CType(rpy_port, RPModelElement), report)
            Me.Set_Stereotype()
            Me.Set_Contract(report)
        End If
    End Sub

    Protected MustOverride Sub Set_Stereotype()
    Protected MustOverride Sub Set_Contract(report As Report)
    Protected MustOverride Sub Merge_Rpy_Contract(report As Report)


    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Nb_Contracts <> 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "PORT_1",
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

    Inherits Operation

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_ope As RPOperation
        rpy_ope = CType(rpy_parent_class.findNestedElement(Me.Name, "Operation"), RPOperation)
        If Not IsNothing(rpy_ope) Then
            Me.Merge_Rpy_Element(CType(rpy_ope, RPModelElement), report)
        Else
            rpy_ope = rpy_parent_class.addOperation(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_ope, RPModelElement), report)
            rpy_ope.addStereotype("Component_Operation", "Operation")
        End If
    End Sub

End Class


Public Class Component_Parameter

    Inherits Attribute_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_attr As RPAttribute
        rpy_attr = CType(rpy_parent_class.findNestedElement(Me.Name, "Attribute"), RPAttribute)
        If Not IsNothing(rpy_attr) Then
            Me.Merge_Rpy_Element(CType(rpy_attr, RPModelElement), report)
        Else
            rpy_attr = rpy_parent_class.addAttribute(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_attr, RPModelElement), report)
            rpy_attr.addStereotype("Component_Parameter", "Attribute")
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Not Me.Base_Data_Type_Ref.Equals(Guid.Empty) Then

            Dim config_data_type As Data_Type
            config_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            Select Case config_data_type.GetType
                Case GetType(Structured_Data_Type)
                    Me.Add_Consistency_Check_Error_Item(report,
                        "PARAM_1",
                        "Type shall not be Structured_Data_Type.")
            End Select

        End If

    End Sub


End Class