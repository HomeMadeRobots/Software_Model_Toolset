Imports rhapsody2
Imports System.Xml.Serialization


Public MustInherit Class SDD_Class
    Inherits SMM_Class

    <XmlArrayItem("Attribute")>
    Public Attributes As List(Of Variable_Attribute)
    Public Private_Operations As List(Of Private_Operation)
    Public Event_Receptions As List(Of Event_Reception)
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
            If Not IsNothing(Me.Event_Receptions) Then
                children_list.AddRange(Me.Event_Receptions)
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
        Me.Event_Receptions = New List(Of Event_Reception)
        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            rpy_elmt = CType(rpy_ope, RPModelElement)
            If Is_Private_Operation(rpy_elmt) Then
                Dim ope As Private_Operation = New Private_Operation
                Me.Private_Operations.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            ElseIf Is_Event_Reception(rpy_elmt) Then
                Dim ope As Event_Reception = New Event_Reception
                Me.Event_Receptions.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Private_Operations.Count = 0 Then
            Me.Private_Operations = Nothing
        End If
        If Me.Event_Receptions.Count = 0 Then
            Me.Event_Receptions = Nothing
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Merge_Dependencies(report, "Sent_Event", Me.Sent_Events, AddressOf Is_Sent_Event)
        Merge_Dependencies(report, "Received_Event", Me.Received_Events, AddressOf Is_Received_Event)
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Me.Set_Dependencies(report, "Sent_Event", Me.Sent_Events)
        Me.Set_Dependencies(report, "Received_Event", Me.Received_Events)
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


Public Class Event_Reception

    Inherits Operation_With_Arguments


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Reception", "Operation")
    End Sub

End Class