﻿Imports rhapsody2
Imports System.IO


Public Class Root_Software_Composition

    Inherits SMM_Class_With_Delegable_Operations

    Public Component_Prototypes As New List(Of Component_Prototype)
    Public Assembly_Connectors As New List(Of Assembly_Connector)
    Public Tasks As New List(Of OS_Task)

    Private Cyclic_Dep_Mgr As Cyclic_Dependencies_Manager = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Component_Prototypes)
            children_list.AddRange(Me.Assembly_Connectors)
            children_list.AddRange(Me.Tasks)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Public Overrides Function Is_Composite() As Boolean
        Return True
    End Function

    Public Overrides Function Is_My_Part(part_uuid As Guid) As Boolean
        Dim got_it As Boolean = False
        For Each part In Me.Component_Prototypes
            If part.UUID = part_uuid Then
                got_it = True
                Exit For
            End If
        Next
        Return got_it
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        ' Ignore generalizations added to a Root_Software_Composition
        Me.Nb_Base_Class_Ref = 0 ' Could have been set to 1 by SMM_Class
    End Sub

    Protected Overrides Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean
        Return Is_Root_Software_Composition(rpy_element)
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Dim rpy_component As RPInstance
        For Each rpy_component In CType(Me.Rpy_Element, RPClass).relations
            If Is_Component_Prototype(CType(rpy_component, RPModelElement)) Then
                Dim component As New Component_Prototype
                Me.Component_Prototypes.Add(component)
                component.Import_From_Rhapsody_Model(Me, CType(rpy_component, RPModelElement))
            End If
        Next

        Dim rpy_link As RPLink
        For Each rpy_link In CType(Me.Rpy_Element, RPClass).links
            If Is_Connector_Prototype(CType(rpy_link, RPModelElement)) Then
                If Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                    Dim connector As New Assembly_Connector
                    Me.Assembly_Connectors.Add(connector)
                    connector.Import_From_Rhapsody_Model(Me, CType(rpy_link, RPModelElement))
                End If
            End If
        Next

        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            If Is_OS_Task(CType(rpy_ope, RPModelElement)) Then
                Dim ope As OS_Task = New OS_Task
                Me.Tasks.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            End If
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Root_Software_Composition", "Class")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Component_Prototypes.Count < 2 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "COMP_1",
                "Should aggregate at least two Component_Prototypes.")
        End If

        ' Check cyclic dependencies
        Dim swc_dep_cycle As New List(Of Dependent_Element)
        Dim list_of_swc As New List(Of Dependent_Element)
        For Each swc In Me.Component_Prototypes
            swc.Find_Needed_Component_Prototypes()
            list_of_swc.Add(swc)
        Next
        swc_dep_cycle = Cyclic_Dependencies_Manager.Find_First_Cyclic_Dependency(list_of_swc)
        If swc_dep_cycle.Count > 0 Then
            Me.Add_Consistency_Check_Warning_Item(report, "COMPO_2",
                "Components involved in at least one dependency cycle : " & _
                Cyclic_Dependencies_Manager.Transform_Cycle_To_String(swc_dep_cycle) & ".")
        End If
    End Sub

    Public Function Find_Cyclic_Dependencies() As List(Of String)
        Me.Cyclic_Dep_Mgr = New Cyclic_Dependencies_Manager()
        Dim list_of_swc As New List(Of Dependent_Element)
        For Each swc In Me.Component_Prototypes
            swc.Find_Needed_Component_Prototypes()
            list_of_swc.Add(swc)
        Next
        Me.Cyclic_Dep_Mgr.Find_Cyclic_Dependencies(list_of_swc)
        Return Me.Cyclic_Dep_Mgr.Get_Cycles_List_String
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            For Each swc In Me.Component_Prototypes
                Dim swct As Component_Type
                swct = CType(Me.Get_Element_By_Uuid(swc.Type_Ref), Component_Type)
                If Not Me.Needed_Elements.Contains(swct) Then
                    Me.Needed_Elements.Add(swct)
                End If
            Next
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of SMM_Classifier)
            ' The list remains empty because nothing can depend on a Root_Software_Composition.
        End If
        Return Me.Dependent_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        Return 0 ' to be implemented when OS_Task will be modeled.
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Transform_To_CLOOF(folder_path As String)
    End Sub

End Class


Public Class Component_Prototype

    Inherits SMM_Object

    Implements Dependent_Element

    Private Needed_Component_Prototypes_List As List(Of Dependent_Element) = Nothing


    '----------------------------------------------------------------------------------------------'
    ' General methods


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Prototype", "Object")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Sub Find_Needed_Component_Prototypes()
        Me.Needed_Component_Prototypes_List = New List(Of Dependent_Element)
        Dim rpy_obj As RPInstance
        rpy_obj = CType(Me.Rpy_Element, RPInstance)
        Dim rpy_elmt As RPModelElement
        For Each rpy_elmt In rpy_obj.references
            If Is_Connector_Prototype(rpy_elmt) Then
                Dim rpy_link As RPLink = CType(rpy_elmt, RPLink)
                If Assembly_Connector.Is_Assembly_Connector(rpy_link) Then
                    Dim assembly As Assembly_Connector
                    assembly = CType(Me.Get_Element_By_Rpy_Guid(rpy_link.GUID), Assembly_Connector)
                    If assembly.Requirer_Component_Ref = Me.UUID Then
                        Dim prov_swc As Component_Prototype
                        prov_swc = CType(Me.Get_Element_By_Uuid(assembly.Provider_Component_Ref), 
                            Component_Prototype)
                        If Not Me.Needed_Component_Prototypes_List.Contains(prov_swc) Then
                            Me.Needed_Component_Prototypes_List.Add(prov_swc)
                        End If
                    End If
                End If
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Not Me.Type_Ref.Equals(Guid.Empty) Then
            ' Exit sub
        End If

        ' Count references on Me
        Dim nb_assembly_by_pport As New Dictionary(Of Guid, Integer)
        Dim nb_assembly_by_rport As New Dictionary(Of Guid, Integer)
        Dim nb_delegation_by_op As New Dictionary(Of Guid, Integer)
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
                End If
            ElseIf Is_Operation_Delegation(rpy_elmt) Then
                Dim op_deleg As Operation_Delegation
                op_deleg = CType(Me.Get_Element_By_Rpy_Guid(rpy_elmt.GUID), Operation_Delegation)
                If op_deleg.Part_Ref = Me.UUID Then
                    If nb_delegation_by_op.ContainsKey(op_deleg.OS_Operation_Ref) Then
                        nb_delegation_by_op(op_deleg.OS_Operation_Ref) += 1
                    Else
                        nb_delegation_by_op.Add(op_deleg.OS_Operation_Ref, 1)
                    End If
                End If
            End If
        Next

        Dim referenced_swct As Component_Type
        referenced_swct = CType(Me.Get_Element_By_Uuid(Me.Type_Ref), Component_Type)

        ' Check connections (assembly)
        For Each pport In referenced_swct.Provider_Ports
            If Not nb_assembly_by_pport.ContainsKey(pport.UUID) Then
                ' no assembly
                Me.Add_Consistency_Check_Information_Item(report, "SWC_3",
                    "Provider_Port '" & pport.Name & "' not used.")
            End If
        Next
        For Each rport In referenced_swct.Requirer_Ports
           If Not nb_assembly_by_rport.ContainsKey(rport.UUID) Then
                ' No assembly
                Me.Add_Consistency_Check_Error_Item(report, "SWC_2",
                    "Requirer_Port '" & rport.Name & "' shall be connected.")
            Else
                If nb_assembly_by_rport.Item(rport.UUID) > 1 Then
                    Me.Add_Consistency_Check_Error_Item(report, "SWC_2",
                        "Requirer_Port '" & rport.Name & "' connected to several ports.")
                End If
            End If
        Next

        ' Check OS_Operation delegation
        For Each op In referenced_swct.OS_Operations
            If Not nb_delegation_by_op.ContainsKey(op.UUID) Then
                ' No delegation
                Me.Add_Consistency_Check_Information_Item(report,
                    "SWC_4", "OS_Operation '" & op.Name & "' not called by the OS tasks.")
            End If
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Realization of the interface Dependent_Element
    Function Get_Name() As String Implements Dependent_Element.Get_Name
        Return Me.Name
    End Function

    Function Get_Uuid() As Guid Implements Dependent_Element.Get_Uuid
        Return Me.UUID
    End Function

    Function Get_Needed_Element() As List(Of Dependent_Element) _
        Implements Dependent_Element.Get_Needed_Element
        Return Me.Needed_Component_Prototypes_List
    End Function

End Class


Public Class OS_Task

    Inherits Delegable_Operation

    '----------------------------------------------------------------------------------------------'
    ' General methods 


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("OS_Task", "Operation")
    End Sub

End Class