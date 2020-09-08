Imports rhapsody2

Public Class Component_Design

    Inherits Software_Element

    Public Component_Type_Ref As Guid = Guid.Empty

    Private Nb_Component_Type_Ref As Integer
    Private Nb_Invalid_Component_Type_Ref As Integer = 0 ' nb ref on not a Component_Type

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        Return Nothing
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

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
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)

        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_class As RPClass
        rpy_class = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Class"), RPClass)

        If Not IsNothing(rpy_class) Then

            Me.Merge_Rpy_Element(CType(rpy_class, RPModelElement), report)

            '---------------------------------------------------------------------------------------
            ' Merge Component_Type_Ref
            ' Get the list of current Component_Type references
            Dim rpy_swct_gen_list As New List(Of RPGeneralization)
            Dim rpy_gen As RPGeneralization = Nothing
            For Each rpy_gen In rpy_class.generalizations
                If Is_Component_Type_Ref(CType(rpy_gen, RPModelElement)) Then
                    rpy_swct_gen_list.Add(rpy_gen)
                    Exit For
                End If
            Next
            ' Check Component_Type references
            If rpy_swct_gen_list.Count = 0 Then
                ' There is no Component_Type references
                ' Create one.
                Me.Set_Component_Type_Ref(rpy_class, report)
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
                    Me.Set_Component_Type_Ref(rpy_class, report)
                End If
            End If
        Else
            rpy_class = rpy_parent_pkg.addClass(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_class, RPModelElement), report)
            rpy_class.addStereotype("Component_Design", "Class")
            Me.Set_Component_Type_Ref(rpy_class, report)
        End If

        Dim children As List(Of Software_Element) = Me.Get_Children
        If Not IsNothing(children) Then
            For Each child In children
                child.Export_To_Rhapsody(CType(rpy_class, RPModelElement), report)
            Next
        End If
    End Sub

    Private Sub Set_Component_Type_Ref(rpy_class As RPClass, report As Report)
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

End Class
