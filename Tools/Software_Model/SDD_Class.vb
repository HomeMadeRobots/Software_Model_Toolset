Imports rhapsody2
Imports System.Xml.Serialization


Public MustInherit Class SDD_Class
    Inherits SMM_Class

    <XmlArrayItem("Attribute")>
    Public Attributes As List(Of Variable_Attribute)
    Public Private_Operations As List(Of Private_Operation)
    <XmlArrayItem("Sent_Event")>
    Public Sent_Events As List(Of Guid)
    <XmlArrayItem("Received_Event")>
    Public Received_Events As List(Of Guid)

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Attributes) Then
                children_list.AddRange(Me.Attributes)
            End If
            If Not IsNothing(Me.Private_Operations) Then
                children_list.AddRange(Me.Private_Operations)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Me.Sent_Events = New List(Of Guid)
        Me.Received_Events = New List(Of Guid)
        Dim rpy_dep As RPDependency
        For Each rpy_dep In CType(Me.Rpy_Element, RPClass).dependencies
            Dim rpy_elmt As RPModelElement = CType(rpy_dep, RPModelElement)
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            If Is_Sent_Event(rpy_elmt) Then
                Me.Sent_Events.Add(ref_elmt_guid)
            ElseIf Is_Received_Event(rpy_elmt) Then
                Me.Received_Events.Add(ref_elmt_guid)
            End If
        Next
        If Me.Sent_Events.Count = 0 Then
            Me.Sent_Events = Nothing
        End If
        If Me.Received_Events.Count = 0 Then
            Me.Received_Events = Nothing
        End If

    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Dim rpy_elmt As RPModelElement

        Me.Attributes = New List(Of Variable_Attribute)
        Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            rpy_elmt = CType(rpy_attribute, RPModelElement)
            If Is_Variable_Attribute(rpy_elmt) Then
                Dim attr As Variable_Attribute = New Variable_Attribute
                Me.Attributes.Add(attr)
                attr.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Attributes.Count = 0 Then
            Me.Attributes = Nothing
        End If

        Me.Private_Operations = New List(Of Private_Operation)
        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            rpy_elmt = CType(rpy_ope, RPModelElement)
            If Is_Private_Operation(rpy_elmt) Then
                Dim ope As Private_Operation = New Private_Operation
                Me.Private_Operations.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Private_Operations.Count = 0 Then
            Me.Private_Operations = Nothing
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        Merge_Dependencies(report, "Sent_Event", Me.Sent_Events, AddressOf Is_Sent_Event)
        Merge_Dependencies(report, "Received_Event", Me.Received_Events, AddressOf Is_Received_Event)

    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)

        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        Me.Set_Dependencies(report, "Sent_Event", Me.Sent_Events)
        Me.Set_Dependencies(report, "Received_Event", Me.Received_Events)

    End Sub

    Protected Sub Merge_Dependencies(
        report As Report,
        stereotype_str As String,
        element_list As List(Of Guid),
        is_of_stereotype As Func(Of RPModelElement, Boolean))

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)
        Dim rpy_dep As RPDependency
        Dim ref_found As Boolean

        For Each id In element_list
            ref_found = False
            Dim rpy_if_guid As String = Transform_Guid_To_Rpy_GUID(id)
            For Each rpy_dep In rpy_class.dependencies
                If is_of_stereotype(CType(rpy_dep, RPModelElement)) Then
                    If rpy_dep.dependsOn.GUID = rpy_if_guid Then
                        ref_found = True
                        Exit For
                    End If
                End If
            Next
            If ref_found = False Then
                Dim rpy_if As RPModelElement = Me.Find_In_Rpy_Project(rpy_if_guid)
                If IsNothing(rpy_if) Then
                    Me.Add_Export_Error_Item(report,
                        Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                        stereotype_str & " not found : " & id.ToString & ".")
                Else
                    Dim created_rpy_dep As RPDependency = rpy_class.addDependencyTo(rpy_if)
                    created_rpy_dep.addStereotype(stereotype_str, "Dependency")
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        stereotype_str & " merged : " & id.ToString & ".")
                End If
            End If
        Next
    End Sub

    Protected Sub Set_Dependencies(
        report As Report,
        stereotype_str As String,
        element_list As List(Of Guid))
        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)
        For Each elmt_ref In element_list
            Dim ref_rpy_elemt As RPModelElement = Me.Find_In_Rpy_Project(elmt_ref)
            If Not IsNothing(ref_rpy_elemt) Then
                Dim rpy_dep As RPDependency
                rpy_dep = rpy_class.addDependencyTo(ref_rpy_elemt)
                rpy_dep.addStereotype(stereotype_str, "Dependency")
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    stereotype_str & " not not found : " & elmt_ref.ToString & ".")
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation (not yet implemented)
    Public Overrides Function Compute_WMC() As Double
        Return 0
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        Return Nothing
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        Return Nothing
    End Function


End Class


Public Class Variable_Attribute
    Inherits Attribute_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Variable_Attribute", "Attribute")
    End Sub

End Class


Public Class Private_Operation
    Inherits Operation_With_Arguments

    '----------------------------------------------------------------------------------------------'
    ' General methods 


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Private_Operation", "Operation")
    End Sub

End Class