Imports rhapsody2
Imports System.Globalization

Public MustInherit Class Delegable_Operation

    Inherits SMM_Operation
    Public Delegations As New List(Of Operation_Delegation)

    Private Owner As SMM_Class_With_Delegable_Operations

    Private OP_2_Rised As Boolean = False

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Sub New()
    End Sub

    Public Sub New(parent_class As SMM_Class_With_Delegable_Operations)
        Me.Owner = parent_class
    End Sub

    Public Function Get_Owner() As SMM_Class_With_Delegable_Operations
        Return Me.Owner
    End Function

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Delegations)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Public Function Shall_Be_Delegated() As Boolean
        Return Not Me.OP_2_Rised
    End Function

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        MyBase.Import_Children_From_Rhapsody_Model()
        Dim rpy_dep As RPModelElement
        For Each rpy_dep In CType(Me.Rpy_Element, RPOperation).dependencies
            If Is_Operation_Delegation(rpy_dep) Then
                Dim ope_delegation As Operation_Delegation = New Operation_Delegation(Me)
                Me.Delegations.Add(ope_delegation)
                ope_delegation.Import_From_Rhapsody_Model(Me, rpy_dep)
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)
        If Me.Owner.Is_Composite Then
            If Me.Delegations.Count = 0 Then
                Me.Add_Consistency_Check_Error_Item(report, "OP_1",
                    "Shall be delegated.")
            End If
        Else 'owner is not composite (atomic Component_Type)
            If Me.Delegations.Count <> 0 Then
                Me.Add_Consistency_Check_Error_Item(report, "OP_2",
                    "Shall not be delegated.")
                Me.OP_2_Rised = True
            End If
        End If
    End Sub

End Class


Public Class Operation_Delegation
    Inherits Software_Element

    Public Part_Ref As Guid
    Public OS_Operation_Ref As Guid
    Public Priority As UInteger

    Private Rpy_Part As RPInstance
    Private Is_Priority_UInteger As Boolean = False
    Private Owner As Delegable_Operation

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Sub New()
    End Sub

    Public Sub New(parent_op As Delegable_Operation)
        Me.Owner = parent_op
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Get Part_Ref
        Dim rpy_part As RPInstance
        rpy_part = CType(CType(Me.Rpy_Element, RPDependency).dependsOn, RPInstance)
        Me.Part_Ref = Transform_Rpy_GUID_To_Guid(rpy_part.GUID)

        Dim tag As RPTag

        ' Get Priority
        tag = Me.Rpy_Element.getTag("Priority")
        If Not IsNothing(tag) Then
            Me.Is_Priority_UInteger = UInteger.TryParse(
                tag.value,
                NumberStyles.Any, _
                CultureInfo.GetCultureInfo("en-US"), _
                Me.Priority)
        End If

        ' Get OS_Operation_Ref
        tag = Me.Rpy_Element.getTag("OS_Operation_Ref")
        If Not IsNothing(tag) Then
            If IsNothing(rpy_part.ObjectAsObjectType) Then
                Dim rpy_base_class As RPClass
                rpy_base_class = CType(rpy_part.otherClass, RPClass)
                If Not IsNothing(rpy_base_class) Then
                    Dim rpy_ope As RPModelElement
                    rpy_ope = rpy_base_class.findNestedElement(tag.value, "Operation")
                    If Not IsNothing(rpy_ope) Then
                        Me.OS_Operation_Ref = Transform_Rpy_GUID_To_Guid(rpy_ope.GUID)
                    End If
                End If
            End If
        End If

    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Dependency"
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim tag As RPTag = rpy_element.getTag("Priority")
        If Not IsNothing(tag) Then
            If Me.Priority.ToString <> tag.value Then
                rpy_element.getSaveUnit.setReadOnly(0)
                rpy_element.setTagValue(tag, Me.Priority.ToString)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Merge Priority.")
            End If
        End If
    End Sub

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_dep As RPDependency = Nothing
        Dim rpy_parent_ope As RPOperation = CType(rpy_parent, RPOperation)

        ' Dirty trick to be able to call Find_In_Rpy_Project before really assigning Rpy_Element
        Me.Rpy_Element = rpy_parent

        Me.Rpy_Part = CType(Me.Find_In_Rpy_Project(Me.Part_Ref), RPInstance)
        If Not IsNothing(Me.Rpy_Part) Then
            rpy_dep = rpy_parent_ope.addDependencyTo(CType(Me.Rpy_Part, RPModelElement))
        End If

        Return CType(rpy_dep, RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        If Not IsNothing(rpy_elmt) Then
            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

            rpy_elmt.name = Me.Name

            Dim tag As RPTag

            tag = rpy_elmt.getTag("Priority")
            If Not IsNothing(tag) Then
                rpy_elmt.setTagValue(tag, Me.Priority.ToString)
            End If

            tag = Me.Rpy_Element.getTag("OS_Operation_Ref")
            If Not IsNothing(tag) Then
                Dim referenced_op_name As String
                referenced_op_name = Me.Find_In_Rpy_Project(Me.OS_Operation_Ref).name
                rpy_elmt.setTagValue(tag, referenced_op_name)
            End If

        Else
            If IsNothing(Me.Rpy_Part) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Part not found : " & Me.Part_Ref.ToString & ".")
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Operation_Delegation", "Dependency")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Owner.Shall_Be_Delegated = False Then
            Exit Sub
        End If

        ' Check Priority
        If Is_Priority_UInteger = False Then
            Me.Add_Consistency_Check_Error_Item(report, "OPDELEG_2",
                "Priority shall be an unsigned integer.")
        End If

        ' Check Part_Ref
        If Not Me.Owner.Get_Owner.Is_My_Part(Me.Part_Ref) Then
            Me.Add_Consistency_Check_Error_Item(report, "OPDELEG_1",
                "Referenced part shall belong to the owner of my operation.")
        End If

        ' Check OS_Operation_Ref
        Dim swc As SMM_Object
        swc = CType(Me.Get_Element_By_Uuid(Me.Part_Ref), SMM_Object)
        If Not IsNothing(swc) Then
            Dim swct As Component_Type
            swct = CType(Me.Get_Element_By_Uuid(swc.Type_Ref), Component_Type)
            If Not IsNothing(swc) Then
                If Not swct.Is_My_OS_Operation(Me.OS_Operation_Ref) Then
                    Me.Add_Consistency_Check_Error_Item(report, "OPDELEG_1",
                        "Referenced operation shall belong to a part of the owner of my operation.")
                End If
            End If
        End If

    End Sub

End Class
