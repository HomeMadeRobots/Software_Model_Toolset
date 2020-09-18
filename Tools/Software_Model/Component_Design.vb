Imports rhapsody2
Imports System.Xml.Serialization


Public Class Component_Design

    Inherits SDD_Class

    Public Component_Type_Ref As Guid = Guid.Empty
    Public Operation_Realizations As List(Of Operation_Realization)
    Public Event_Reception_Realizations As List(Of Event_Reception_Realization)
    <XmlArrayItem("Part")>
    Public Parts As List(Of Internal_Design_Object)
    Public Object_Connectors As List(Of Object_Connector)
    Public Delegation_Connectors As List(Of Object_Delegation_Connector)

    Private Nb_Component_Type_Ref As Integer
    Private Nb_Invalid_Component_Type_Ref As Integer = 0 ' nb ref on not a Component_Type

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then

            Dim children_list As List(Of Software_Element)
            children_list = MyBase.Get_Children

            If Not IsNothing(Me.Operation_Realizations) Then
                children_list.AddRange(Me.Operation_Realizations)
            End If
            If Not IsNothing(Me.Event_Reception_Realizations) Then
                children_list.AddRange(Me.Event_Reception_Realizations)
            End If
            If Not IsNothing(Me.Parts) Then
                children_list.AddRange(Me.Parts)
            End If
            If Not IsNothing(Me.Object_Connectors) Then
                children_list.AddRange(Me.Object_Connectors)
            End If
            If Not IsNothing(Me.Delegation_Connectors) Then
                children_list.AddRange(Me.Delegation_Connectors)
            End If

            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        MyBase.Import_Children_From_Rhapsody_Model()
        Dim rpy_elmt As RPModelElement


        Me.Operation_Realizations = New List(Of Operation_Realization)
        Me.Event_Reception_Realizations = New List(Of Event_Reception_Realization)
        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            rpy_elmt = CType(rpy_ope, RPModelElement)
            If Is_Operation_Realization(rpy_elmt) Then
                Dim op_rea As Operation_Realization = New Operation_Realization
                Me.Operation_Realizations.Add(op_rea)
                op_rea.Import_From_Rhapsody_Model(Me, rpy_elmt)
            ElseIf Is_Event_Reception_Realization(rpy_elmt) Then
                Dim ev_recep_rea As Event_Reception_Realization = New Event_Reception_Realization
                Me.Event_Reception_Realizations.Add(ev_recep_rea)
                ev_recep_rea.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Operation_Realizations.Count = 0 Then
            Me.Operation_Realizations = Nothing
        End If
        If Me.Event_Reception_Realizations.Count = 0 Then
            Me.Event_Reception_Realizations = Nothing
        End If

        Me.Parts = New List(Of Internal_Design_Object)
        Dim rpy_instance As RPInstance
        For Each rpy_instance In CType(Me.Rpy_Element, RPClass).relations
            rpy_elmt = CType(rpy_instance, RPModelElement)
            If Is_Internal_Design_Object(rpy_elmt) Then
                Dim obj As Internal_Design_Object = New Internal_Design_Object
                Me.Parts.Add(obj)
                obj.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Parts.Count = 0 Then
            Me.Parts = Nothing
        End If

        Me.Object_Connectors = New List(Of Object_Connector)
        Me.Delegation_Connectors = New List(Of Object_Delegation_Connector)
        Dim rpy_link As RPLink
        For Each rpy_link In CType(Me.Rpy_Element, RPClass).links
            rpy_elmt = CType(rpy_link, RPModelElement)
            If Is_Connector_Prototype(CType(rpy_link, RPModelElement)) Then
                If Object_Connector.Is_Object_Connector(rpy_link) Then
                    Dim link As New Object_Connector
                    Me.Object_Connectors.Add(link)
                    link.Import_From_Rhapsody_Model(Me, rpy_elmt)
                ElseIf Object_Delegation_Connector.Is_Delegation_Connector(rpy_link) Then
                    Dim link As New Object_Delegation_Connector
                    Me.Delegation_Connectors.Add(link)
                    link.Import_From_Rhapsody_Model(Me, rpy_elmt)
                End If
            End If
        Next
        If Me.Object_Connectors.Count = 0 Then
            Me.Object_Connectors = Nothing
        End If
        If Me.Delegation_Connectors.Count = 0 Then
            Me.Delegation_Connectors = Nothing
        End If

    End Sub

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Get Component_Type_Ref
        Dim rpy_gen As RPGeneralization
        For Each rpy_gen In CType(Me.Rpy_Element, RPClass).generalizations
            If Is_Component_Type_Ref(CType(rpy_gen, RPModelElement)) Then
                Me.Nb_Component_Type_Ref += 1

                Dim referenced_rpy_elmt_guid As String
                referenced_rpy_elmt_guid = rpy_gen.baseClass.GUID
                Dim referenced_rpy_elmt As RPModelElement
                referenced_rpy_elmt = Me.Find_In_Rpy_Project(referenced_rpy_elmt_guid)

                If Is_Component_Type(referenced_rpy_elmt) Then
                    Me.Component_Type_Ref = Transform_Rpy_GUID_To_Guid(rpy_gen.baseClass.GUID)
                Else
                    Me.Nb_Invalid_Component_Type_Ref += 1
                End If

            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        ' Merge Component_Type_Ref
        ' Get the list of current Component_Type references
        Dim rpy_swct_gen_list As New List(Of RPGeneralization)
        Dim rpy_gen As RPGeneralization = Nothing
        For Each rpy_gen In rpy_class.generalizations
            If Is_Component_Type_Ref(CType(rpy_gen, RPModelElement)) Then
                rpy_swct_gen_list.Add(rpy_gen)
            End If
        Next
        ' Check Component_Type references
        If rpy_swct_gen_list.Count = 0 Then
            ' There is no Component_Type references
            ' Create one.
            Me.Set_Component_Type_Ref(rpy_class, report, True)
        Else
            Dim reference_found As Boolean = False
            Dim referenced_rpy_swct_guid As String
            referenced_rpy_swct_guid = Transform_Guid_To_Rpy_GUID(Me.Component_Type_Ref)
            For Each rpy_gen In rpy_swct_gen_list
                Dim current_rpy_swct As RPClass = CType(rpy_gen.baseClass, RPClass)
                If current_rpy_swct.GUID = referenced_rpy_swct_guid Then
                    ' No change
                    reference_found = True
                End If
            Next

            If reference_found = False Then
                Me.Set_Component_Type_Ref(rpy_class, report, True)
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Design", "Class")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Me.Set_Component_Type_Ref(CType(rpy_elmt, RPClass), report, False)
    End Sub

    Private Sub Set_Component_Type_Ref(rpy_class As RPClass, report As Report, is_merge As Boolean)
        If Me.Component_Type_Ref <> Guid.Empty Then
            Dim rpy_swct As RPClass
            rpy_swct = CType(Me.Find_In_Rpy_Project(Me.Component_Type_Ref), RPClass)
            If IsNothing(rpy_swct) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Referenced Component_Type not found : " _
                    & Me.Component_Type_Ref.ToString & ".")
            Else
                rpy_class.addGeneralization(CType(rpy_swct, RPClassifier))
                Dim rpy_gen As RPGeneralization
                rpy_gen = rpy_class.findGeneralization(rpy_swct.name)
                rpy_gen.addStereotype("Component_Type_Ref", "Generalization")
                If is_merge = True Then
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Merge Component_Type_Ref.")
                End If
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Nb_Component_Type_Ref <> 1 Or Me.Nb_Invalid_Component_Type_Ref > 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "SWCD_1",
                "Shall be associated to one and only one atomic Component_Type.")
        End If

        ' Check the references of the realized operations
        If Me.Component_Type_Ref <> Guid.Empty Then
            Dim swct As Component_Type
            swct = CType(Me.Get_Element_By_Uuid(Me.Component_Type_Ref), Component_Type)
            If Not IsNothing(Me.Operation_Realizations) Then
                For Each op_real In Me.Operation_Realizations
                    op_real.Check_Referenced_Elements(report, swct)
                Next
            End If
            If Not IsNothing(Me.Event_Reception_Realizations) Then
                For Each ev_recep_real In Me.Event_Reception_Realizations
                    ev_recep_real.Check_Referenced_Elements(report, swct)
                Next
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation

End Class


Public Class Operation_Realization

    Inherits Operation_With_Arguments

    Public Provider_Port_Ref As Guid = Guid.Empty
    Public Operation_Ref As Guid

    Private Nb_Provider_Port_Ref As Integer
    Private Nb_Operation_Ref As Integer


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_dep As RPDependency
        For Each rpy_dep In CType(Me.Rpy_Element, RPOperation).dependencies
            If Is_Provider_Port_Ref(CType(rpy_dep, RPModelElement)) Then
                Dim rpy_port As RPPort
                Try
                    rpy_port = CType(rpy_dep.dependsOn, RPPort)
                Catch
                    rpy_port = CType(rpy_dep.dependsOn.owner, RPPort)
                End Try
                Me.Nb_Provider_Port_Ref += 1
                Me.Provider_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_port.GUID)
            ElseIf Is_Operation_Ref(CType(rpy_dep, RPModelElement)) Then
                Me.Nb_Operation_Ref += 1
                Me.Operation_Ref = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_dep_list As New List(Of RPDependency)
        Dim rpy_dep As RPDependency = Nothing

        ' Merge Provider_Port_Ref (if exists)
        If Me.Provider_Port_Ref <> Guid.Empty Then
            ' Get the list of current Provider_Port references
            For Each rpy_dep In rpy_element.dependencies
                If Is_Provider_Port_Ref(CType(rpy_dep, RPModelElement)) Then
                    rpy_dep_list.Add(rpy_dep)
                End If
            Next
            ' Check Requirer_Port references
            If rpy_dep_list.Count = 0 Then
                ' There is no Requirer_Port references
                ' Create one.
                Me.Set_Provider_Port_Ref(rpy_element, report, True)
            Else
                Dim reference_found As Boolean = False
                Dim referenced_rpy_port_guid As String
                referenced_rpy_port_guid = Transform_Guid_To_Rpy_GUID(Me.Provider_Port_Ref)
                For Each rpy_dep In rpy_dep_list
                    Dim current_rpy_port As RPPort
                    Try
                        current_rpy_port = CType(rpy_dep.dependsOn, RPPort)
                    Catch
                        current_rpy_port = CType(rpy_dep.dependsOn.owner, RPPort)
                    End Try
                    If current_rpy_port.GUID = referenced_rpy_port_guid Then
                        ' No change
                        reference_found = True
                        Exit For
                    End If
                Next
                If reference_found = False Then
                    Me.Set_Provider_Port_Ref(rpy_element, report, True)
                End If
            End If
        End If

        ' Merge Operation_Ref
        rpy_dep_list.Clear()
        For Each rpy_dep In rpy_element.dependencies
            If Is_Operation_Ref(CType(rpy_dep, RPModelElement)) Then
                rpy_dep_list.Add(rpy_dep)
            End If
        Next
        If rpy_dep_list.Count = 0 Then
            ' There is no Operation references
            ' Create one.
            Me.Set_Operation_Ref(rpy_element, report, True)
        Else
            Dim reference_found As Boolean = False
            Dim referenced_rpy_op_guid As String
            referenced_rpy_op_guid = Transform_Guid_To_Rpy_GUID(Me.Operation_Ref)
            For Each rpy_dep In rpy_dep_list
                Dim current_rpy_op As RPOperation
                Try
                    current_rpy_op = CType(rpy_dep.dependsOn, RPOperation)
                Catch
                    current_rpy_op = CType(rpy_dep.dependsOn.owner, RPOperation)
                End Try
                If current_rpy_op.GUID = referenced_rpy_op_guid Then
                    ' No change
                    reference_found = True
                    Exit For
                End If
            Next
            If reference_found = False Then
                Me.Set_Operation_Ref(rpy_element, report, True)
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Operation_Realization", "Operation")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        If Me.Provider_Port_Ref <> Guid.Empty Then
            Me.Set_Provider_Port_Ref(rpy_elmt, report, False)
        End If
        Me.Set_Operation_Ref(rpy_elmt, report, False)
    End Sub

    Private Sub Set_Provider_Port_Ref(
        rpy_elmt As RPModelElement,
        report As Report,
        is_merge As Boolean)

        Dim rpy_port As RPModelElement = Me.Find_In_Rpy_Project(Me.Provider_Port_Ref)
        If Not IsNothing(rpy_port) Then
            Dim rpy_dep As RPDependency
            rpy_dep = rpy_elmt.addDependencyTo(rpy_port)
            rpy_dep.addStereotype("Provider_Port_Ref", "Dependency")
            If is_merge = True Then
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Merge Provider_Port_Ref.")
            End If
        Else
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Referenced Provider_Port not not found : " &
                Me.Provider_Port_Ref.ToString & ".")
        End If

    End Sub

    Private Sub Set_Operation_Ref(
        rpy_elmt As RPModelElement,
        report As Report,
        is_merge As Boolean)

        If Me.Operation_Ref <> Guid.Empty Then
            Dim rpy_op As RPModelElement = Me.Find_In_Rpy_Project(Me.Operation_Ref)
            If Not IsNothing(rpy_op) Then
                Dim rpy_dep As RPDependency
                rpy_dep = rpy_elmt.addDependencyTo(rpy_op)
                rpy_dep.addStereotype("Operation_Ref", "Dependency")
                If is_merge = True Then
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Merge Operation_Ref.")
                End If
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Referenced Operation not not found : " &
                    Me.Operation_Ref.ToString & ".")
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Nb_Operation_Ref <> 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "OPREAL_1",
                "Shall reference one and only one Operation.")
        End If

        If Me.Operation_Ref <> Guid.Empty Then
            Dim ope As SMM_Operation
            ope = CType(Me.Get_Element_By_Uuid(Me.Operation_Ref), SMM_Operation)
            If ope.GetType = GetType(Component_Operation) Then
                If Me.Provider_Port_Ref <> Guid.Empty Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "OPREAL_2",
                        "Realize a Component_Operation : shall not be linked to a Port.")
                End If
            ElseIf ope.GetType = GetType(Synchronous_Operation) Or
                    ope.GetType = GetType(Asynchronous_Operation) Then
                If Me.Provider_Port_Ref = Guid.Empty Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "OPREAL_3",
                        "Shall not be linked to a Provider_Port.")
                End If
            Else
                If Me.Provider_Port_Ref = Guid.Empty Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "OPREAL_4",
                        "Shall reference a Component_Operation " & _
                        "or a Synchronous_Operation or a Asynchronous_Operation.")
                End If
            End If
        End If

        If Me.Nb_Provider_Port_Ref > 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "OPREAL_1",
                "Shall reference only one Provider_Port.")
        End If
    End Sub

    Public Sub Check_Referenced_Elements(report As Report, swct As Component_Type)
        If Me.Operation_Ref = Guid.Empty Then
            Exit Sub
        End If

        Dim referenced_ope As SMM_Operation
        referenced_ope = CType(Me.Get_Element_By_Uuid(Me.Operation_Ref), SMM_Operation)

        Dim reference_allowed As Boolean = False

        If referenced_ope.GetType = GetType(Component_Operation) Then
            Dim ope As Component_Operation = Nothing
            For Each ope In swct.Component_Operations
                If ope.UUID = referenced_ope.UUID Then
                    reference_allowed = True
                    Exit For
                End If
            Next
            If reference_allowed = False Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "OPREAL_5",
                    "The referenced Operation shall belong to the implemented Component_Type.")
            End If
        ElseIf referenced_ope.GetType = GetType(Synchronous_Operation) Or
                referenced_ope.GetType = GetType(Asynchronous_Operation) Then

            If Me.Provider_Port_Ref = Guid.Empty Then
                Exit Sub
            End If

            Dim referenced_port As Provider_Port
            referenced_port = CType(Me.Get_Element_By_Uuid(Me.Provider_Port_Ref), Provider_Port)

            Dim port As Provider_Port = Nothing
            For Each port In swct.Provider_Ports
                If port.UUID = referenced_port.UUID Then
                    reference_allowed = True
                    Exit For
                End If
            Next
            If reference_allowed = False Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "OPREAL_6",
                    "The referenced Provider_Port shall belong to the implemented Component_Type.")
            End If

            ' Check the interface of the referenced Provider_Port.
            Dim sw_if As Software_Interface
            sw_if = CType(Me.Get_Element_By_Uuid(referenced_port.Contract_Ref), Software_Interface)
            If Not IsNothing(sw_if) Then

                If sw_if.GetType <> GetType(Client_Server_Interface) Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "OPREAL_7",
                        "The contract of the referenced Provider_Port shall " & _
                        "be a Client_Server_Interface.")
                Else
                    ' Check if the referenced Operation belong to the interface of the 
                    ' referenced Provider_Port.
                    reference_allowed = False
                    Dim cs_if As Client_Server_Interface = CType(sw_if, Client_Server_Interface)
                    Dim ope As SMM_Operation
                    For Each ope In cs_if.Operations
                        If ope.UUID = referenced_ope.UUID Then
                            reference_allowed = True
                            Exit For
                        End If
                    Next
                    If reference_allowed = False Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "OPREAL_8",
                            "The referenced Operation shall belong to the " & _
                            "Client_Server_Interface of the referenced Provider_Port.")
                    End If
                End If
            End If

        End If
    End Sub

End Class


Public Class Event_Reception_Realization

    Inherits Operation_With_Arguments

    Public Requirer_Port_Ref As Guid = Guid.Empty
    ' Warning : 
    ' if the dependency link is created from the model, use rpy_dep.dependsOn.owner to get the 
    ' referenced port.
    ' if the dependency link is created from the by the tool (merge), use rpy_dep.dependsOn to get 
    ' referenced port.

    Private Nb_Requirer_Port_Ref As Integer


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_dep As RPDependency
        For Each rpy_dep In CType(Me.Rpy_Element, RPOperation).dependencies
            If Is_Requirer_Port_Ref(CType(rpy_dep, RPModelElement)) Then
                Dim rpy_port As RPPort
                Try
                    rpy_port = CType(rpy_dep.dependsOn, RPPort)
                Catch
                    rpy_port = CType(rpy_dep.dependsOn.owner, RPPort)
                End Try
                Me.Nb_Requirer_Port_Ref += 1
                Me.Requirer_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_port.GUID)
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        ' Merge Requirer_Port_Ref
        ' Get the list of current Requirer_Port references
        Dim rpy_dep_list As New List(Of RPDependency)
        Dim rpy_dep As RPDependency = Nothing
        For Each rpy_dep In rpy_element.dependencies
            If Is_Requirer_Port_Ref(CType(rpy_dep, RPModelElement)) Then
                rpy_dep_list.Add(rpy_dep)
            End If
        Next
        ' Check Requirer_Port references
        If rpy_dep_list.Count = 0 Then
            ' There is no Requirer_Port references
            ' Create one.
            Me.Set_Requirer_Port_Ref(rpy_element, report, True)
        Else
            Dim reference_found As Boolean = False
            Dim referenced_rpy_port_guid As String
            referenced_rpy_port_guid = Transform_Guid_To_Rpy_GUID(Me.Requirer_Port_Ref)
            For Each rpy_dep In rpy_dep_list
                Dim current_rpy_port As RPPort
                Try
                    current_rpy_port = CType(rpy_dep.dependsOn, RPPort)
                Catch
                    current_rpy_port = CType(rpy_dep.dependsOn.owner, RPPort)
                End Try
                If current_rpy_port.GUID = referenced_rpy_port_guid Then
                    ' No change
                    reference_found = True
                    Exit For
                End If
            Next
            If reference_found = False Then
                Me.Set_Requirer_Port_Ref(rpy_element, report, True)
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Reception_Realization", "Operation")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Set_Requirer_Port_Ref(rpy_elmt, report, False)
    End Sub

    Private Sub Set_Requirer_Port_Ref(
        rpy_elmt As RPModelElement,
        report As Report,
        is_merge As Boolean)

        If Me.Requirer_Port_Ref <> Guid.Empty Then
            Dim rpy_port As RPModelElement = Me.Find_In_Rpy_Project(Me.Requirer_Port_Ref)
            If Not IsNothing(rpy_port) Then
                Dim rpy_dep As RPDependency
                rpy_dep = rpy_elmt.addDependencyTo(rpy_port)
                rpy_dep.addStereotype("Requirer_Port_Ref", "Dependency")
                If is_merge = True Then
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Merge Requirer_Port_Ref.")
                End If
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Referenced Requirer_Port not not found : " &
                    Me.Requirer_Port_Ref.ToString & ".")
            End If
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Nb_Requirer_Port_Ref <> 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "EVREAL_1",
                "Shall reference one and only one Required_Port.")
        End If
    End Sub

    Public Sub Check_Referenced_Elements(report As Report, swct As Component_Type)
        Dim reference_allowed As Boolean = False
        Dim referenced_req_port As Requirer_Port
        referenced_req_port = CType(Me.Get_Element_By_Uuid(Me.Requirer_Port_Ref), Requirer_Port)
        If IsNothing(referenced_req_port) Then
            Exit Sub
        End If
        Dim req_port As Requirer_Port = Nothing
        For Each req_port In swct.Requirer_Ports
            If req_port.UUID = referenced_req_port.UUID Then
                reference_allowed = True
                Exit For
            End If
        Next
        If reference_allowed = False Then
            Me.Add_Consistency_Check_Error_Item(report,
                "EVREAL_2",
                "The referenced Required_Port shall belong to the implemented Component_Type.")
        Else
            Dim sw_if As Software_Interface
            sw_if = CType(Me.Get_Element_By_Uuid(req_port.Contract_Ref), Software_Interface)
            If Not IsNothing(sw_if) Then
                If sw_if.GetType <> GetType(Event_Interface) Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "EVREAL_3",
                        "The contract of the referenced Required_Port shall be an Event_Interface.")
                End If
            End If
        End If
    End Sub

End Class


Public Class Internal_Design_Object
    Inherits SMM_Object


    '----------------------------------------------------------------------------------------------'
    ' General methods


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Internal_Design_Object", "Object")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model

End Class


Public Class Object_Connector
    Inherits Software_Connector

    Public Object_From_Ref As Guid
    Public Object_To_Ref As Guid

    ' Used for model merge
    Private Rpy_Object_From As RPInstance = Nothing
    Private Rpy_Object_To As RPInstance = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Shared Function Is_Object_Connector(rpy_link As RPLink) As Boolean
        Dim result As Boolean = False
        If IsNothing(rpy_link.fromPort) And IsNothing(rpy_link.toPort) _
            And rpy_link.toElement Is rpy_link.to _
            And rpy_link.fromElement Is rpy_link.from Then
            If Is_Internal_Design_Object(CType(rpy_link.to, RPModelElement)) _
                And Is_Internal_Design_Object(CType(rpy_link.from, RPModelElement)) Then
                result = True
            End If
        End If
        Return result
    End Function

    Public Shared Sub Get_Connector_Info(
        rpy_link As RPLink,
        ByRef obj_from As RPInstance,
        ByRef obj_to As RPInstance)
        obj_to = rpy_link.to
        obj_from = rpy_link.from
    End Sub

    Public Shared Function Compute_Automatic_Name(rpy_link As RPLink) As String
        Dim automatic_name As String
        automatic_name = rpy_link.fromElement.name & "__" & rpy_link.toElement.name
        Return automatic_name
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_link As RPLink = CType(Me.Rpy_Element, RPLink)
        Me.Object_To_Ref = Transform_Rpy_GUID_To_Guid(rpy_link.toElement.GUID)
        Me.Object_From_Ref = Transform_Rpy_GUID_To_Guid(rpy_link.fromElement.GUID)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_link As RPLink = Nothing

        ' Dirty trick to be able to call Find_In_Rpy_Project before really assigning Rpy_Element
        Me.Rpy_Element = rpy_parent

        Me.Rpy_Object_To = CType(Me.Find_In_Rpy_Project(Me.Object_To_Ref), RPInstance)
        Me.Rpy_Object_From = CType(Me.Find_In_Rpy_Project(Me.Object_From_Ref), RPInstance)

        If Not IsNothing(Me.Rpy_Object_To) And
            Not IsNothing(Me.Rpy_Object_From) Then
            rpy_link = rpy_parent_class.addLink(
                Me.Rpy_Object_From,
                Me.Rpy_Object_To,
                Nothing,
                Nothing,
                Nothing)
        End If
        Return CType(rpy_link, RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        If Not IsNothing(rpy_elmt) Then
            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
            rpy_elmt.name = Me.Name
        Else
            If IsNothing(Me.Rpy_Object_To) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "'To' Object not found : " & Me.Rpy_Object_To.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Object_From) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "'From' Object not found : " & Me.Rpy_Object_From.ToString & ".")
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model

End Class


Public Class Object_Delegation_Connector
    Inherits Software_Connector

    Public Object_Ref As Guid
    Public Port_Ref As Guid

    ' Used for model merge
    Private Rpy_Object As RPInstance = Nothing
    Private Rpy_Port As RPPort = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods
    Public Shared Function Is_Delegation_Connector(rpy_link As RPLink) As Boolean
        Dim result As Boolean = False
        If IsNothing(rpy_link.fromPort) And IsNothing(rpy_link.toPort) _
            And rpy_link.toElement Is rpy_link.to _
            And rpy_link.fromElement Is rpy_link.from Then
            Dim to_elmt As RPModelElement = CType(rpy_link.to, RPModelElement)
            Dim from_elmt As RPModelElement = CType(rpy_link.from, RPModelElement)
            If (Is_Internal_Design_Object(to_elmt) _
                And (Is_Provider_Port(from_elmt) Or Is_Requirer_Port(from_elmt))) _
                Or Is_Internal_Design_Object(from_elmt) _
                And (Is_Provider_Port(to_elmt) Or Is_Requirer_Port(to_elmt)) Then
                result = True
            End If
        End If
        Return result
    End Function

    Public Shared Sub Get_Connector_Info(
        rpy_link As RPLink,
        ByRef port As RPPort,
        ByRef obj As RPInstance)
        obj = rpy_link.to
        If Is_Internal_Design_Object(CType(obj, RPModelElement)) Then
            port = CType(rpy_link.from, RPPort)
        Else
            obj = rpy_link.from
            port = CType(rpy_link.to, RPPort)
        End If
    End Sub

    Public Shared Function Compute_Automatic_Name(rpy_link As RPLink) As String
        Dim automatic_name As String
        Dim rpy_port As RPPort = Nothing
        Dim rpy_obj As RPInstance = Nothing
        Object_Delegation_Connector.Get_Connector_Info(rpy_link, rpy_port, rpy_obj)
        automatic_name = rpy_obj.name & "__" & rpy_port.name
        Return automatic_name
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_link As RPLink = CType(Me.Rpy_Element, RPLink)
        If Is_Internal_Design_Object(rpy_link.toElement) Then
            Me.Object_Ref = Transform_Rpy_GUID_To_Guid(rpy_link.toElement.GUID)
            Me.Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_link.fromElement.GUID)
        Else
            Me.Object_Ref = Transform_Rpy_GUID_To_Guid(rpy_link.fromElement.GUID)
            Me.Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_link.toElement.GUID)
        End If
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Dim rpy_link As RPLink = Nothing

        ' Dirty trick to be able to call Find_In_Rpy_Project before really assigning Rpy_Element
        Me.Rpy_Element = rpy_parent

        Me.Rpy_Object = CType(Me.Find_In_Rpy_Project(Me.Object_Ref), RPInstance)
        Me.Rpy_Port = CType(Me.Find_In_Rpy_Project(Me.Port_Ref), RPPort)

        If Not IsNothing(Me.Rpy_Object) And
            Not IsNothing(Me.Rpy_Port) Then
            rpy_link = rpy_parent_class.addLink(
                Me.Rpy_Object,
                CType(Me.Rpy_Port, RPInstance),
                Nothing,
                Nothing,
                Nothing)
        End If
        Return CType(rpy_link, RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        If Not IsNothing(rpy_elmt) Then
            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
            rpy_elmt.name = Me.Name
        Else
            If IsNothing(Me.Rpy_Object) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Object not found : " & Me.Rpy_Object.ToString & ".")
            End If
            If IsNothing(Me.Rpy_Port) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Port not found : " & Me.Rpy_Port.ToString & ".")
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model

End Class