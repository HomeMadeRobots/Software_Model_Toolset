Imports rhapsody2
Imports System.Xml.Serialization


Public Class Component_Type

    Inherits SMM_Class

    Public OS_Operations As New List(Of OS_Operation)
    <XmlArrayItem("Configuration")>
    Public Configurations As New List(Of Configuration_Parameter)
    Public Provider_Ports As New List(Of Provider_Port)
    Public Requirer_Ports As New List(Of Requirer_Port)
    Public Parts As New List(Of Component_Type_Part)
    Public Assembly_Connectors As New List(Of Assembly_Connector)
    Public Delegation_Connectors As New List(Of Delegation_Connector)

    Private Is_Composite As Boolean = False

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Provider_Ports)
            children_list.AddRange(Me.Requirer_Ports)
            children_list.AddRange(Me.Configurations)
            children_list.AddRange(Me.Parts)

            ' Shall be added after Parts and Ports to ensure merge
            children_list.AddRange(Me.OS_Operations)
            children_list.AddRange(Me.Assembly_Connectors)
            children_list.AddRange(Me.Delegation_Connectors)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Public Function Is_Composite_Component_Type() As Boolean
        Return Me.Is_Composite
    End Function

    Public Function Is_My_Provider_Port(port_uuid As Guid) As Boolean
        Dim got_it As Boolean = False
        For Each port In Me.Get_All_Provider_Ports()
            If port.UUID = port_uuid Then
                got_it = True
                Exit For
            End If
        Next
        Return got_it
    End Function

    Public Function Is_My_Requirer_Port(port_uuid As Guid) As Boolean
        Dim got_it As Boolean = False
        For Each port In Me.Get_All_Requirer_Ports()
            If port.UUID = port_uuid Then
                got_it = True
                Exit For
            End If
        Next
        Return got_it
    End Function

    Public Function Is_My_OS_Operation(op_uuid As Guid) As Boolean
        Dim got_it As Boolean = False
        For Each op In Me.Get_All_OS_Operations()
            If op.UUID = op_uuid Then
                got_it = True
                Exit For
            End If
        Next
        Return got_it
    End Function

    Public Function Get_All_Provider_Ports() As List(Of Provider_Port)
        Dim all_pports As New List(Of Provider_Port)
        all_pports.AddRange(Me.Provider_Ports)
        Dim base_ref As Guid = Me.Base_Class_Ref
        While base_ref <> Guid.Empty
            Dim base_swct As Component_Type
            base_swct = CType(Me.Get_Element_By_Uuid(base_ref), Component_Type)
            If Not IsNothing(base_swct) Then
                all_pports.AddRange(base_swct.Provider_Ports)
                base_ref = base_swct.Base_Class_Ref
            Else
                Exit While
            End If
        End While
        Return all_pports
    End Function

    Public Function Get_All_Requirer_Ports() As List(Of Requirer_Port)
        Dim all_rports As New List(Of Requirer_Port)
        all_rports.AddRange(Me.Requirer_Ports)
        Dim base_ref As Guid = Me.Base_Class_Ref
        While base_ref <> Guid.Empty
            Dim base_swct As Component_Type
            base_swct = CType(Me.Get_Element_By_Uuid(base_ref), Component_Type)
            If Not IsNothing(base_swct) Then
                all_rports.AddRange(base_swct.Requirer_Ports)
                base_ref = base_swct.Base_Class_Ref
            Else
                Exit While
            End If
        End While
        Return all_rports
    End Function

    Public Function Get_All_OS_Operations() As List(Of OS_Operation)
        Dim all_os_op As New List(Of OS_Operation)
        all_os_op.AddRange(Me.OS_Operations)
        Dim base_ref As Guid = Me.Base_Class_Ref
        While base_ref <> Guid.Empty
            Dim base_swct As Component_Type
            base_swct = CType(Me.Get_Element_By_Uuid(base_ref), Component_Type)
            If Not IsNothing(base_swct) Then
                all_os_op.AddRange(base_swct.OS_Operations)
                base_ref = base_swct.Base_Class_Ref
            Else
                Exit While
            End If
        End While
        Return all_os_op
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean
        Return Is_Component_Type(rpy_element)
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Dim rpy_port As RPPort
        For Each rpy_port In CType(Me.Rpy_Element, RPClass).ports
            If Is_Provider_Port(CType(rpy_port, RPModelElement)) Then
                Dim pport As Provider_Port = New Provider_Port(Me)
                Me.Provider_Ports.Add(pport)
                pport.Import_From_Rhapsody_Model(Me, CType(rpy_port, RPModelElement))
            ElseIf Is_Requirer_Port(CType(rpy_port, RPModelElement)) Then
                Dim rport As Requirer_Port = New Requirer_Port(Me)
                Me.Requirer_Ports.Add(rport)
                rport.Import_From_Rhapsody_Model(Me, CType(rpy_port, RPModelElement))
            End If
        Next

        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            If Is_OS_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim ope As OS_Operation = New OS_Operation
                Me.OS_Operations.Add(ope)
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

        Dim rpy_object As RPInstance
        For Each rpy_object In CType(Me.Rpy_Element, RPClass).relations
            If Is_Component_Type_Part(CType(rpy_object, RPModelElement)) Then
                Dim part As Component_Type_Part = New Component_Type_Part(Me)
                Me.Parts.Add(part)
                part.Import_From_Rhapsody_Model(Me, CType(rpy_object, RPModelElement))
                Me.Is_Composite = True
            End If
        Next

        If Me.Is_Composite = True Then
            Dim rpy_link As RPLink
            For Each rpy_link In CType(Me.Rpy_Element, RPClass).links
                If Is_Connector_Prototype(CType(rpy_link, RPModelElement)) Then
                    Dim connector As Software_Element
                    If Delegation_Connector.Is_Delegation_Connector(rpy_link) Then
                        connector = New Delegation_Connector(Me)
                        Me.Delegation_Connectors.Add(CType(connector, Delegation_Connector))
                        connector.Import_From_Rhapsody_Model(Me, CType(rpy_link, RPModelElement))
                    ElseIf Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                        connector = New Assembly_Connector
                        Me.Assembly_Connectors.Add(CType(connector, Assembly_Connector))
                        connector.Import_From_Rhapsody_Model(Me, CType(rpy_link, RPModelElement))
                    End If
                End If
            Next
        End If

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

        If Me.Is_Composite = False Then
            If Me.Configurations.Count <> 0 Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "SWCT_2",
                    "Composite Component_Type cannot aggregate Configuration_Parameters.")
            End If
            If Me.Base_Class_Ref <> Guid.Empty Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "SWCT_3",
                    "Composite Component_Type cannot inherit from an other Component_Type.")
            End If
        Else ' Is_Composite = True

        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = MyBase.Find_Needed_Elements()

            For Each port In Me.Provider_Ports
                Dim sw_if As Software_Interface
                sw_if = CType(Me.Get_Element_By_Uuid(port.Contract_Ref), Software_Interface)
                If Not IsNothing(sw_if) Then
                    If Not Me.Needed_Elements.Contains(sw_if) Then
                        Me.Needed_Elements.Add(sw_if)
                    End If
                End If
            Next

            For Each port In Me.Requirer_Ports
                Dim sw_if As Software_Interface
                sw_if = CType(Me.Get_Element_By_Uuid(port.Contract_Ref), Software_Interface)
                If Not IsNothing(sw_if) Then
                    If Not Me.Needed_Elements.Contains(sw_if) Then
                        Me.Needed_Elements.Add(sw_if)
                    End If
                End If
            Next

            For Each conf In Me.Configurations
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(conf.Base_Data_Type_Ref), Data_Type)
                If Not IsNothing(data_type) Then
                    If Not data_type.Is_Basic_Type Then
                        If Not Me.Needed_Elements.Contains(data_type) Then
                            Me.Needed_Elements.Add(data_type)
                        End If
                    End If
                End If
            Next

        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = MyBase.Find_Dependent_Elements()
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
    Protected Owner As Component_Type

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Sub New()

    End Sub

    Public Sub New(parent_swct As Component_Type)
        Me.Owner = parent_swct
    End Sub


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

    Protected Function Get_Nb_Delegations_In_Composite() As Integer
        Dim nb_delegation As Integer = 0
        If Me.Owner.Is_Composite_Component_Type Then
            Dim rpy_port As RPPort = CType(Me.Rpy_Element, RPPort)
            Dim rpy_elmt As RPModelElement
            For Each rpy_elmt In rpy_port.references
                If Is_Connector_Prototype(rpy_elmt) Then
                    Dim rpy_link As RPLink = CType(rpy_elmt, RPLink)
                    If Delegation_Connector.Is_Delegation_Connector(rpy_link) Then
                        ' Check that this delegation belongs to my owner (composite Component_Type)
                        Dim delegation As Delegation_Connector
                        delegation = CType(Me.Get_Element_By_Rpy_Guid(rpy_link.GUID),  _
                                        Delegation_Connector)
                        If delegation.Is_Owned_By(Me.Owner.UUID) Then
                            nb_delegation += 1
                        End If
                    End If
                End If
            Next
        End If
        Return nb_delegation
    End Function

End Class


Public Class Provider_Port

    Inherits Port

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Sub New()
    End Sub

    Public Sub New(parent_swct As Component_Type)
        MyBase.New(parent_swct)
    End Sub


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
                        If current_if_UUID <> child_csif.Base_Class_Ref Then
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

        If Me.Owner.Is_Composite_Component_Type Then
            If Me.Get_Nb_Delegations_In_Composite = 0 Then
                Me.Add_Consistency_Check_Error_Item(report, "PORT_2",
                    "Shall be delegated to only one port.")
            End If
        End If

    End Sub

End Class


Public Class Requirer_Port

    Inherits Port

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Sub New()
    End Sub

    Public Sub New(parent_swct As Component_Type)
        MyBase.New(parent_swct)
    End Sub


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


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Owner.Is_Composite_Component_Type Then
            If Me.Get_Nb_Delegations_In_Composite = 0 Then
                Me.Add_Consistency_Check_Error_Item(report, "PORT_3",
                    "Shall be delegated to at least one port.")
            End If
        End If
    End Sub

End Class


Public Class OS_Operation

    Inherits Delegable_Operation


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("OS_Operation", "Operation")
    End Sub

End Class


Public Class Component_Type_Part
    Inherits SMM_Object

    Private Owner As Component_Type

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Sub New()
        ' For serialization
    End Sub

    Public Sub New(parent_swct As Component_Type)
        Me.Owner = parent_swct
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Type_Part", "Object")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        Dim base_class As Software_Element = Nothing
        ' Check base class ref
        If Not Me.Type_Ref.Equals(Guid.Empty) Then
            base_class = Me.Get_Element_By_Uuid(Me.Type_Ref)
            If Not base_class.GetType = GetType(Component_Type) Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "PART_1", "Shall reference a Component_Type.")
                Exit Sub
            Else
                If base_class.UUID = Me.Owner.UUID Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "PART_2", "Shall not reference its parent Component_Type.")
                    Exit Sub
                End If
            End If
        End If

        ' Check connections (delegation and assembly)
        Dim nb_assembly_by_pport As New Dictionary(Of Guid, Integer)
        Dim nb_assembly_by_rport As New Dictionary(Of Guid, Integer)
        Dim nb_delegation_by_port As New Dictionary(Of Guid, Integer)
        Dim rpy_obj As RPInstance
        rpy_obj = CType(Me.Rpy_Element, RPInstance)
        Dim rpy_elmt As RPModelElement
        For Each rpy_elmt In rpy_obj.references
            If Is_Connector_Prototype(rpy_elmt) Then
                Dim rpy_link As RPLink = CType(rpy_elmt, RPLink)
                If Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                    Dim assembly As Assembly_Connector
                    assembly = CType(Me.Get_Element_By_Rpy_Guid(rpy_link.GUID), Assembly_Connector)
                    If assembly.Provider_Component_Ref = Me.UUID Then
                        If nb_assembly_by_pport.ContainsKey(assembly.Provider_Port_Ref) Then
                            nb_assembly_by_pport(assembly.Provider_Port_Ref) += 1
                        Else
                            nb_assembly_by_pport.Add(assembly.Provider_Port_Ref, 1)
                        End If
                    Else
                        If nb_assembly_by_rport.ContainsKey(assembly.Requirer_Port_Ref) Then
                            nb_assembly_by_rport(assembly.Requirer_Port_Ref) += 1
                        Else
                            nb_assembly_by_rport.Add(assembly.Requirer_Port_Ref, 1)
                        End If
                    End If
                ElseIf Delegation_Connector.Is_Delegation_Connector(rpy_link) Then
                    Dim delegation As Delegation_Connector
                    delegation = CType(Me.Get_Element_By_Rpy_Guid(rpy_link.GUID),  _
                                    Delegation_Connector)
                    If nb_delegation_by_port.ContainsKey(delegation.Part_Port_Ref) Then
                        nb_delegation_by_port(delegation.Part_Port_Ref) += 1
                    Else
                        nb_delegation_by_port.Add(delegation.Part_Port_Ref, 1)
                    End If
                End If
            End If
        Next
        Dim base_swct As Component_Type = CType(base_class, Component_Type)
        For Each pport In base_swct.Provider_Ports
            If nb_delegation_by_port.ContainsKey(pport.UUID) Then
                If nb_delegation_by_port.Item(pport.UUID) <> 1 Then
                    ' several delegation
                    Me.Add_Consistency_Check_Error_Item(report,
                        "TBD", "Provider_Port '" & pport.Name & "' delegated several time.")
                End If
            Else
                ' no delegation
                If Not nb_assembly_by_pport.ContainsKey(pport.UUID) Then
                    ' no delegation, no assembly
                    Me.Add_Consistency_Check_Information_Item(report,
                        "TBD", "Provider_Port '" & pport.Name & "' not used.")
                End If
            End If
        Next
        For Each rport In base_swct.Requirer_Ports
            If nb_delegation_by_port.ContainsKey(rport.UUID) Then
                If nb_delegation_by_port.Item(rport.UUID) <> 1 Then
                    ' several delegation
                    Me.Add_Consistency_Check_Error_Item(report,
                        "TBD", "Requirer_Port '" & rport.Name & "' delegated several time.")
                Else
                    ' one delegation
                    If nb_assembly_by_rport.ContainsKey(rport.UUID) Then
                        ' one delegation + n assembly
                        Me.Add_Consistency_Check_Error_Item(report,
                            "TBD", "Requirer_Port '" & rport.Name & "' delegated and assembled.")
                    End If
                End If
            Else
                ' No delegation
                If Not nb_assembly_by_rport.ContainsKey(rport.UUID) Then
                    ' no delegation, no assembly
                    Me.Add_Consistency_Check_Error_Item(report,
                        "TBD", "Requirer_Port '" & rport.Name & "' not used.")
                Else
                    If nb_assembly_by_rport.Item(rport.UUID) > 1 Then
                        ' to many assembly
                        Me.Add_Consistency_Check_Error_Item(report,
                            "TBD", "Requirer_Port '" & rport.Name & "' assembled several time.")
                    End If
                End If
            End If
        Next

    End Sub

End Class


Public Class Delegation_Connector

    Inherits Software_Connector

    Public Part_Ref As Guid
    Public Part_Port_Ref As Guid
    Public Component_Type_Port_Ref As Guid

    Private Owner As Component_Type

    ' Used for model merge
    Private Rpy_Part As RPInstance = Nothing
    Private Rpy_Part_Port As RPPort = Nothing
    Private Rpy_Component_Type_Port As RPPort = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Sub New()
    End Sub

    Public Sub New(parent_swct As Component_Type)
        Me.Owner = parent_swct
    End Sub

    Public Shared Function Is_Delegation_Connector(rpy_link As RPLink) As Boolean
        ' case # 1                      case #2
        ' toPort = swc_port             fromPort = swc_port
        ' to = swc                      from = swc
        ' from = swct_port              to = swct_port
        ' fromElement = from            toElement = to
        ' fromPort = Nothing            toPort = Nothing

        Dim result As Boolean = False
        If IsNothing(rpy_link.fromPort) Then
            If Not IsNothing(rpy_link.from) Then
                If rpy_link.from.owner.GUID = rpy_link.owner.GUID Then
                    result = True
                End If
            End If
        ElseIf IsNothing(rpy_link.toPort) Then
            If Not IsNothing(rpy_link.to) Then
                If rpy_link.to.owner.GUID = rpy_link.owner.GUID Then
                    result = True
                End If
            End If
        End If

        Return result
    End Function

    Public Shared Function Compute_Automatic_Name(rpy_link As RPLink) As String
        Dim automatic_name As String
        Dim swct_port As RPPort = Nothing
        Dim part_port As RPPort = Nothing
        Dim part As RPInstance = Nothing
        Delegation_Connector.Get_Connector_Info(
            rpy_link,
            part,
            part_port, 
            swct_port)
        automatic_name = swct_port.name & "__" & part.name & "__" & part_port.name
        Return automatic_name
    End Function

    Public Shared Sub Get_Connector_Info(
        rpy_link As RPLink,
        ByRef part As RPInstance,
        ByRef part_port As RPPort,
        ByRef swct_port As RPPort)
        part_port = rpy_link.toPort
        If IsNothing(part_port) Then
            part_port = rpy_link.fromPort
            part = rpy_link.from
            swct_port = CType(rpy_link.toElement, RPPort)
        Else
            part = rpy_link.to
            swct_port = CType(rpy_link.fromElement, RPPort)
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_part_port As RPPort = Nothing
        Dim rpy_swct_port As RPPort = Nothing
        Dim rpy_part As RPInstance = Nothing

        Dim rpy_link As RPLink = CType(Me.Rpy_Element, RPLink)

        Dim part_port As RPPort = Nothing
        Dim part As RPInstance = Nothing
        Dim component_type_port As RPPort = Nothing
        Delegation_Connector.Get_Connector_Info(
            CType(Me.Rpy_Element, RPLink),
            part,
            part_port,
            component_type_port)
        Me.Part_Ref = Transform_Rpy_GUID_To_Guid(part.GUID)
        Me.Part_Port_Ref = Transform_Rpy_GUID_To_Guid(part_port.GUID)
        Me.Component_Type_Port_Ref = Transform_Rpy_GUID_To_Guid(component_type_port.GUID)

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_link As RPLink = Nothing

        ' Dirty trick to be able to call Find_In_Rpy_Project before really assigning Rpy_Element
        Me.Rpy_Element = rpy_parent

        Me.Rpy_Part = CType(Me.Find_In_Rpy_Project(Me.Part_Ref), RPInstance)
        Me.Rpy_Part_Port = CType(Me.Find_In_Rpy_Project(Me.Part_Port_Ref), RPPort)
        Me.Rpy_Component_Type_Port = CType(Me.Find_In_Rpy_Project(Me.Component_Type_Port_Ref), 
                                        RPPort)

        If Not IsNothing(Me.Rpy_Part) And
            Not IsNothing(Me.Rpy_Part_Port) And
            Not IsNothing(Me.Rpy_Component_Type_Port) Then
            rpy_link = rpy_parent_class.addLinkToPartViaPort(
                Me.Rpy_Part,
                CType(Me.Rpy_Part_Port, RPInstance),
                CType(Me.Rpy_Component_Type_Port, RPInstance),
                Nothing)
        End If
        Return CType(rpy_link, RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        If Not IsNothing(rpy_elmt) Then
            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
            rpy_elmt.name = Me.Name
        Else
            If IsNothing(Me.Rpy_Part) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Part not found : " & Me.Part_Ref.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Part_Port) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Port not found : " & Me.Part_Port_Ref.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Component_Type_Port) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Port not found : " _
                    & Me.Component_Type_Port_Ref.ToString & ".")
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        Dim part_port As Port = CType(Get_Element_By_Uuid(Me.Part_Port_Ref), Port)
        Dim swct_port As Port = CType(Get_Element_By_Uuid(Me.Component_Type_Port_Ref), Port)
        If part_port.GetType <> swct_port.GetType Then
            Me.Add_Consistency_Check_Error_Item(report, "DELEG_1",
                "Linked ports are not of the same kind.")
        End If
        If part_port.Contract_Ref <> swct_port.Contract_Ref Then
            Me.Add_Consistency_Check_Error_Item(report, "DELEG_2",
                "Linked ports do not refer to the same contract.")
        End If

    End Sub

    Public Function Is_Owned_By(owner_uuid As Guid) As Boolean
        If owner_uuid = Me.Owner.UUID Then
            Return True
        Else
            Return False
        End If
    End Function

End Class