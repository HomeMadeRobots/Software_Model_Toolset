Imports rhapsody2

Public Class Component_Design

    Inherits Software_Class

    Public Component_Type_Ref As Guid = Guid.Empty
    Public Component_Attributes As List(Of Component_Attribute)
    Public Private_Operations As List(Of Private_Operation)
    Public Operation_Realizations As List(Of Operation_Realization)

    Private Nb_Component_Type_Ref As Integer
    Private Nb_Invalid_Component_Type_Ref As Integer = 0 ' nb ref on not a Component_Type

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Component_Attributes) Then
                children_list.AddRange(Me.Component_Attributes)
            End If
            If Not IsNothing(Me.Private_Operations) Then
                children_list.AddRange(Me.Private_Operations)
            End If
            If Not IsNothing(Me.Operation_Realizations) Then
                children_list.AddRange(Me.Operation_Realizations)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Dim rpy_elmt As RPModelElement

        Me.Component_Attributes = New List(Of Component_Attribute)
        Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            rpy_elmt = CType(rpy_attribute, RPModelElement)
            If Is_Component_Attribute(rpy_elmt) Then
                Dim attr As Component_Attribute = New Component_Attribute
                Me.Component_Attributes.Add(attr)
                attr.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Component_Attributes.Count = 0 Then
            Me.Component_Attributes = Nothing
        End If

        Me.Private_Operations = New List(Of Private_Operation)
        Me.Operation_Realizations = New List(Of Operation_Realization)
        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            rpy_elmt = CType(rpy_ope, RPModelElement)
            If Is_Private_Operation(rpy_elmt) Then
                Dim priv_op As Private_Operation = New Private_Operation
                Me.Private_Operations.Add(priv_op)
                priv_op.Import_From_Rhapsody_Model(Me, rpy_elmt)
            ElseIf Is_Operation_Realization(rpy_elmt) Then
                Dim op_rea As Operation_Realization = New Operation_Realization
                Me.Operation_Realizations.Add(op_rea)
                op_rea.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Private_Operations.Count = 0 Then
            Me.Private_Operations = Nothing
        End If
        If Me.Operation_Realizations.Count = 0 Then
            Me.Operation_Realizations = Nothing
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
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation (not yet implemented for Component_Design)
    Public Overrides Function Compute_WMC() As Double
        Return 0
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)
        Return Nothing
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        Return Nothing
    End Function

End Class


Public Class Component_Attribute

    Inherits Attribute_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Component_Attribute", "Attribute")
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
                Me.Nb_Provider_Port_Ref += 1
                Me.Provider_Port_Ref = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            ElseIf Is_Operation_Ref(CType(rpy_dep, RPModelElement)) Then
                Me.Nb_Operation_Ref += 1
                Me.Operation_Ref = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Operation_Realization", "Operation")
    End Sub

End Class