Imports rhapsody2

Public MustInherit Class Data_Type

    Inherits Software_Element

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public MustOverride Function Get_Complexity() As Double

End Class


Public MustInherit Class Data_Type_Base_Typed

    Inherits Data_Type

    Public Base_Data_Type_Ref As Guid

    Protected MustOverride Function Get_Rpy_Data_Type() As RPModelElement

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_type As RPModelElement = Me.Get_Rpy_Data_Type
        If Not IsNothing(rpy_type) Then
            If Is_Data_Type(rpy_type) Or Is_Physical_Data_Type(rpy_type) Then
                Me.Base_Data_Type_Ref = Transform_GUID_To_UUID(rpy_type.GUID)
            Else
                Me.Base_Data_Type_Ref = Nothing
            End If
        Else
            Me.Base_Data_Type_Ref = Nothing
        End If
    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Base_Data_Type_Ref = Guid.Empty Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Referenced type shall be a Data_Type.")
        End If

    End Sub

End Class


Public Class Basic_Type

    Inherits Data_Type

    Public Overrides Function Get_Complexity() As Double
        Return 1
    End Function

End Class

Public Class Basic_Integer_Type
    Inherits Basic_Type
End Class

Public Class Basic_Floating_Point_Type
    Inherits Basic_Type
End Class

Public Class Basic_Boolean_Type
    Inherits Basic_Type
End Class

Public Class Basic_Integer_Array_Type
    Inherits Basic_Type
End Class


Public Class Enumerated_Data_Type

    Inherits Data_Type

    Public Enumerals As List(Of Enumerated_Data_Type_Enumeral)

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_label As RPEnumerationLiteral

        Me.Enumerals = New List(Of Enumerated_Data_Type_Enumeral)

        For Each rpy_label In CType(Me.Rpy_Element, RPType).enumerationLiterals
            Dim enumeral As Enumerated_Data_Type_Enumeral = New Enumerated_Data_Type_Enumeral
            Me.Enumerals.Add(enumeral)
            enumeral.Import_From_Rhapsody_Model(Me, rpy_label)
        Next

    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Enumerals.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall aggregate at least one Enumerated_Data_Type_Enumeral.")
        Else
            If Me.Enumerals.Count = 1 Then
                Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Should aggregate at least two Enumerated_Data_Type_Enumeral.")
            End If

            Dim enumeral_values_list As New List(Of UInteger)
            Dim enumeral_without_value_nb As Integer = 0
            For Each enumeral In Me.Enumerals

                If Not Is_Symbol_Valid(enumeral.Name) Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "TBD",
                        "Invalid enumeral symbol : " & enumeral.Name)
                End If

                If enumeral.Description = "" Then
                    Me.Add_Consistency_Check_Information_Item(report,
                        "TBD",
                        "Enumeral " & enumeral.Name & " could have a description.")
                End If

                If enumeral.Value <> "" Then
                    Dim dummy As UInteger = 0
                    Dim is_uinteger As Boolean
                    is_uinteger = UInteger.TryParse(enumeral.Value, dummy)
                    If is_uinteger = False Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "TBD",
                            "Value of " & enumeral.Name & " shall be a positive integer or empty.")
                    Else
                        enumeral_values_list.Add(dummy)
                    End If
                Else
                    enumeral_without_value_nb += 1
                End If

            Next

            If enumeral_without_value_nb > 0 And enumeral_values_list.Count > 0 Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "TBD",
                    "If at least one enumeral has a Value, all the enumerals shall have a Value.")
            End If

            If enumeral_values_list.Count > 0 Then
                If enumeral_values_list.Count <> enumeral_values_list.Distinct.Count() Then
                    Me.Add_Consistency_Check_Error_Item(report,
                    "TBD",
                    "The value of the enumerals shall be unique.")
                End If
            End If

        End If

    End Sub

    Public Overrides Function Get_Complexity() As Double
        Return 1.5
    End Function

End Class


Public Class Enumerated_Data_Type_Enumeral

    Public Name As String
    Public Description As String
    Public Value As String

    Private Parent As Enumerated_Data_Type
    Private Rpy_Element As RPEnumerationLiteral

    Public Sub Import_From_Rhapsody_Model(
            owner As Enumerated_Data_Type,
            rpy_mdl_element As RPEnumerationLiteral)

        Me.Parent = owner
        Me.Rpy_Element = rpy_mdl_element
        Me.Name = Me.Rpy_Element.name
        If Me.Rpy_Element.description <> "" Then
            Me.Description = Me.Rpy_Element.description
        End If
        If Me.Rpy_Element.value <> "" Then
            Me.Value = Me.Rpy_Element.value
        End If
    End Sub

End Class


Public Class Array_Data_Type

    Inherits Data_Type_Base_Typed

    Public Multiplicity As UInteger

    Private Complexity As Double = 0
    Private Const Default_Complexity As Double = 1.8

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPType).typedefBaseType
        Return CType(rpy_type, RPModelElement)
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim multiplicity_str As String
        multiplicity_str = CType(Me.Rpy_Element, RPType).typedefMultiplicity
        Me.Multiplicity = 0
        UInteger.TryParse(multiplicity_str, Me.Multiplicity)
    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Multiplicity = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Multiplicity shall be a strictly positive integer value.")
        ElseIf Me.Multiplicity = 1 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "Multiplicity should be greater than 1.")
        End If

    End Sub

    Public Overrides Function Get_Complexity() As Double
        If Me.Complexity = 0 Then
            Dim computed_complexity As Double
            'If Get_Basic_Type(Me.Base_Data_Type_Ref) = Basic_Types.NON_BASIC_TYPE Then
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
                computed_complexity = Array_Data_Type.Default_Complexity * data_type.Get_Complexity
            'Else
            '    computed_complexity = Array_Data_Type.Default_Complexity
            'End If
            Me.Complexity = computed_complexity
        End If
        Return Me.Complexity
    End Function

End Class


Public Class Physical_Data_Type

    Inherits Data_Type_Base_Typed

    Public Unit As String
    Public Resolution As String
    Public Offset As String

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPType).typedefBaseType
        Return CType(rpy_type, RPModelElement)
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim tag As RPTag

        tag = CType(Me.Rpy_Element, RPType).getTag("Unit")
        Me.Unit = tag.value

        Dim dummy As Decimal = 0
        tag = CType(Me.Rpy_Element, RPType).getTag("Resolution")
        Me.Resolution = tag.value
        Decimal.TryParse(
            Me.Resolution,
            System.Globalization.NumberStyles.Any, _
            System.Globalization.CultureInfo.InvariantCulture, _
            dummy)
        If dummy = 0 Then
            Me.Resolution = Nothing
        End If

        tag = CType(Me.Rpy_Element, RPType).getTag("Offset")
        Me.Offset = tag.value
        Dim is_decimal As Boolean
        is_decimal = Decimal.TryParse(Me.Offset, System.Globalization.NumberStyles.Any, _
                                      System.Globalization.CultureInfo.InvariantCulture, dummy)
        If is_decimal = False Then
            Me.Offset = Nothing
        End If

    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Unit = "" Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "Unit shall be set.")
        End If

        If IsNothing(Me.Resolution) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Resolution shall be set to a non-null decimal value.")
        End If

        If IsNothing(Me.Offset) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Offset shall be set to a numerical value.")
        End If

        Dim referenced_type As Data_Type = CType(Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        If referenced_type.GetType <> GetType(Basic_Integer_Type) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD", _
                "Referenced type shall be a Basic_Integer_Type.")
        End If

    End Sub

    Public Overrides Function Get_Complexity() As Double
        Return 1.2
    End Function

End Class


Public Class Structured_Data_Type

    Inherits Data_Type

    Public Fields As List(Of Structured_Data_Type_Field)

    Private Complexity As Double = 0

    Public Overrides Function Get_Children() As List(Of Software_Element)
        Dim children As New List(Of Software_Element)
        If Not IsNothing(Me.Fields) Then
            For Each fd In Me.Fields
                children.Add(fd)
            Next
        End If
        Return children
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        Dim rpy_attr As RPAttribute
        Me.Fields = New List(Of Structured_Data_Type_Field)
        For Each rpy_attr In CType(Me.Rpy_Element, RPType).attributes
            Dim field As Structured_Data_Type_Field = New Structured_Data_Type_Field
            Me.Fields.Add(field)
            field.Import_From_Rhapsody_Model(Me, CType(rpy_attr, RPModelElement))
        Next
    End Sub

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Fields.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "Shall aggregate at least one field.")
        ElseIf Me.Fields.Count = 1 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "Should aggregate at least two fields.")
        End If

    End Sub

    Public Overrides Function Get_Complexity() As Double
        If Me.Complexity = 0 Then
            Dim computed_complexity As Double = 0
            Dim field As Structured_Data_Type_Field
            For Each field In Me.Fields
                computed_complexity += field.Get_Complexity
            Next
            Me.Complexity = computed_complexity
        End If
        Return Me.Complexity
    End Function

End Class


Public Class Structured_Data_Type_Field

    Inherits Typed_Software_Element

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPAttribute).type
        Return CType(rpy_type, RPModelElement)
    End Function

    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Parent.UUID = Me.Base_Data_Type_Ref Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD", _
                "Shall not reference its owner.")
        End If

    End Sub

    Public Function Get_Complexity() As Double

        Dim referenced_data_type As Data_Type
        referenced_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        Return referenced_data_type.Get_Complexity

    End Function

End Class