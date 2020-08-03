Imports rhapsody2

Public MustInherit Class Data_Type

    Inherits Software_Element

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
                "A Software_Element that have the Base_Data_Type_Ref attribute " &
                "shall reference a Data_Type.")
        End If

    End Sub

End Class


Public Class Enumerated_Data_Type

    Inherits Data_Type

    Public Enumerals As List(Of Enumerated_Data_Type_Enumeral)

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        MyBase.Import_Children_From_Rhapsody_Model()

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
                "An Enumerated_Data_Type shall aggregate at least " &
                "one Enumerated_Data_Type_Enumeral.")
        Else
            If Me.Enumerals.Count = 1 Then
                Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "An Enumerated_Data_Type should aggregate at least " &
                "two Enumerated_Data_Type_Enumeral.")
            End If


        End If

    End Sub

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
        Me.Name = Rpy_Element.name
        If Rpy_Element.description <> "" Then
            Me.Description = Rpy_Element.description
        End If
        If Me.Rpy_Element.value <> "" Then
            Me.Value = Me.Rpy_Element.value
        End If
    End Sub

End Class


Public Class Array_Data_Type

    Inherits Data_Type_Base_Typed

    Public Multiplicity As UInteger

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
                "The Multiplicity shall be a strictly positive integer value.")
        ElseIf Me.Multiplicity = 1 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "The Multiplicity should be greater than 1.")
        End If

    End Sub

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
                "The Unit shall be set.")
        End If

        If IsNothing(Me.Resolution) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "The Resolution shall be set to a non-null decimal value.")
        End If

        If IsNothing(Me.Offset) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD",
                "The Offset shall be set to a numerical value.")
        End If

        If Is_Basic_Integer_Type(Me.Base_Data_Type_Ref) = False Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TBD", _
                "The referenced type shall be a Basic_Integer_Type.")
        End If

    End Sub

End Class


Public Class Structured_Data_Type

    Inherits Data_Type

    Public Fields As List(Of Structured_Data_Type_Field)

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        MyBase.Import_Children_From_Rhapsody_Model()
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
                "A Structured_Data_Type shall aggregate " &
                "at least one Structured_Data_Type_Field.")
        ElseIf Me.Fields.Count = 1 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "TBD",
                "A Structured_Data_Type should aggregate " &
                "at least two Structured_Data_Type_Fields.")
        End If

    End Sub

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
                "A Structured_Data_Type_Field shall not reference its owner.")
        End If

    End Sub

End Class