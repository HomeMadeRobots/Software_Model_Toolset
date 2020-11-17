Imports rhapsody2
Imports System.Globalization
Imports System.Text.RegularExpressions

Public MustInherit Class Data_Type

    Inherits SMM_Classifier

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overridable Function Is_Basic_Type() As Boolean
        Return False
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Type"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Return CType(rpy_parent_pkg.addType(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Data_Type", "Type")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public MustOverride Function Get_Complexity() As Double

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of SMM_Classifier)

            Dim needed_elements As List(Of SMM_Classifier)

            Dim swct_list As List(Of Component_Type)
            swct_list = Me.Container.Get_All_Component_Types
            For Each swtc In swct_list
                needed_elements = swtc.Find_Needed_Elements
                For Each element In needed_elements
                    If element.UUID = Me.UUID Then
                        Me.Dependent_Elements.Add(swtc)
                    End If
                Next
            Next

            Dim if_list As List(Of Software_Interface)
            if_list = Me.Container.Get_All_Interfaces
            For Each sw_if In if_list
                needed_elements = sw_if.Find_Needed_Elements
                If Not IsNothing(needed_elements) Then
                    For Each element In needed_elements
                        If element.UUID = Me.UUID Then
                            Me.Dependent_Elements.Add(sw_if)
                        End If
                    Next
                End If
            Next

            Dim dt_list As List(Of Data_Type)
            dt_list = Me.Container.Get_All_Data_Types
            For Each dt In dt_list
                needed_elements = dt.Find_Needed_Elements
                If Not IsNothing(needed_elements) Then
                    For Each element In needed_elements
                        If element.UUID = Me.UUID Then
                            Me.Dependent_Elements.Add(dt)
                        End If
                    Next
                End If
            Next

        End If
        Return Me.Dependent_Elements
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Public MustOverride Function Is_Value_Valid(value As String) As Boolean

End Class


Public MustInherit Class Data_Type_Base_Typed

    Inherits Data_Type

    Public Base_Data_Type_Ref As Guid


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected MustOverride Function Get_Rpy_Data_Type() As RPModelElement

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_type As RPModelElement = Me.Get_Rpy_Data_Type
        If Not IsNothing(rpy_type) Then
            If Is_Data_Type(rpy_type) Or Is_Physical_Data_Type(rpy_type) Then
                Me.Base_Data_Type_Ref = Transform_Rpy_GUID_To_Guid(rpy_type.GUID)
            Else
                Me.Base_Data_Type_Ref = Nothing
            End If
        Else
            Me.Base_Data_Type_Ref = Nothing
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_type As RPType = CType(rpy_element, RPType)

        Dim merge_needed As Boolean = False
        If IsNothing(rpy_type.typedefBaseType) Then
            merge_needed = True
        ElseIf rpy_type.typedefBaseType.GUID <>
                Transform_Guid_To_Rpy_GUID(Me.Base_Data_Type_Ref) Then
            merge_needed = True
        End If

        If merge_needed = True Then
            rpy_type.getSaveUnit.setReadOnly(0)
            Dim referenced_rpy_type As RPType
            referenced_rpy_type = CType(Me.Find_In_Rpy_Project(Me.Base_Data_Type_Ref), RPType)
            If IsNothing(referenced_rpy_type) Then
                Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Base_Data_Type not found : " & Me.Base_Data_Type_Ref.ToString & ".")
            Else
                rpy_type.typedefBaseType = CType(referenced_rpy_type, RPClassifier)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Merge Base_Data_Type.")
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_type As RPType = CType(rpy_elmt, RPType)
        rpy_type.kind = "Typedef"
        Dim referenced_rpy_type As RPType
        referenced_rpy_type = CType(Me.Find_In_Rpy_Project(Me.Base_Data_Type_Ref), RPType)
        If IsNothing(referenced_rpy_type) Then
            Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Base_Data_Type not found : " & Me.Base_Data_Type_Ref.ToString & ".")
        Else
            rpy_type.typedefBaseType = CType(referenced_rpy_type, RPClassifier)
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Base_Data_Type_Ref = Guid.Empty Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TYP_1",
                "Referenced type shall be a Data_Type.")
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            Dim data_type As Data_Type
            data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            If Not IsNothing(data_type) Then
                If Not data_type.Is_Basic_Type Then
                    Me.Needed_Elements.Add(data_type)
                End If
            End If
        End If
        Return Me.Needed_Elements
    End Function

End Class


Public MustInherit Class Basic_Type

    Inherits Data_Type

    Public Sub New(symbol As String, guid_str As String, rpy_type As RPModelElement)
        Me.Name = symbol
        Guid.TryParse(guid_str, Me.UUID)
        Me.Rpy_Element = rpy_type
    End Sub

    Public Overrides Function Is_Basic_Type() As Boolean
        Return True
    End Function

    Public Overrides Function Get_Complexity() As Double
        Return 1
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        Return Nothing
    End Function

End Class

Public Class Basic_Integer_Type
    Inherits Basic_Type

    Public Enum E_Signedness_Type
        SIGNED
        UNSIGNED
    End Enum

    Private Size As Integer ' number of bytes
    Private Signedness As E_Signedness_Type

    Sub New(
        symbol As String,
        guid_str As String,
        rpy_type As RPModelElement,
        size As Integer,
        signedness As E_Signedness_Type)
        MyBase.New(symbol, guid_str, rpy_type)
        Me.Size = size
        Me.Signedness = signedness
    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        Dim is_valid As Boolean = False
        Select Case Me.Signedness
            Case E_Signedness_Type.SIGNED
                Dim is_int As Boolean
                Dim value_int As Int64
                is_int = Int64.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    value_int)
                If is_int = True Then
                    Select Case Me.Size
                        Case 1
                            If value_int >= -128 And value_int <= 127 Then
                                is_valid = True
                            End If
                        Case 2
                            If value_int >= -32768 And value_int <= 32767 Then
                                is_valid = True
                            End If
                        Case 4
                            If value_int >= -2147483648 And value_int <= 2147483647 Then
                                is_valid = True
                            End If
                        Case 8
                            is_valid = True
                    End Select
                End If
            Case E_Signedness_Type.UNSIGNED
                Dim is_uint As Boolean
                Dim value_uint As UInt64
                is_uint = UInt64.TryParse(
                    value,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    value_uint)
                If is_uint = True Then
                    Select Case Me.Size
                        Case 1
                            If value_uint <= 255 Then
                                is_valid = True
                            End If
                        Case 2
                            If value_uint <= 65535 Then
                                is_valid = True
                            End If
                        Case 4
                            If value_uint <= 4294967295 Then
                                is_valid = True
                            End If
                        Case 8
                            is_valid = True
                    End Select
                End If
        End Select
        Return is_valid
    End Function

End Class

Public Class Basic_Floating_Point_Type
    Inherits Basic_Type

    Sub New(symbol As String, guid_str As String, rpy_type As RPModelElement)
        MyBase.New(symbol, guid_str, rpy_type)
    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        If Regex.IsMatch(value, "^[0-9]+[.|,][0-9]+$") Then
            Return True
        Else
            Return False
        End If
    End Function

End Class

Public Class Basic_Boolean_Type
    Inherits Basic_Type


    Sub New(symbol As String, guid_str As String, rpy_type As RPModelElement)
        MyBase.New(symbol, guid_str, rpy_type)
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        If Regex.IsMatch(value, "^(true|false)$", RegexOptions.IgnoreCase) Then
            Return True
        Else
            Return False
        End If
    End Function

End Class

Public Class Basic_Integer_Array_Type
    Inherits Basic_Type

    Private Basic_Integer_Type As Basic_Integer_Type

    Public Sub New(
        symbol As String,
        guid_str As String,
        rpy_type As RPModelElement,
        basic_type As Basic_Integer_Type)
        MyBase.New(symbol, guid_str, rpy_type)
        Me.Basic_Integer_Type = basic_type
    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        Dim is_valid As Boolean = False
        If Regex.IsMatch(value, "^\[[\d|\D]*\]$") Then
            Dim values_list As String()
            values_list = Split(value.Substring(1, value.Length - 2), " ")
            If values_list.Count >= 2 Then
                Dim all_valid As Boolean = True
                For idx = 0 To values_list.Count - 1
                    If Me.Basic_Integer_Type.Is_Value_Valid(values_list(idx)) = False Then
                        all_valid = False
                        Exit For
                    End If
                Next
                is_valid = all_valid
            End If
        End If
        Return is_valid
    End Function

End Class

Public Class Basic_Character_Type
    Inherits Basic_Type

    Private Size As Integer ' character number, 0 for infinite

    Sub New(symbol As String, guid_str As String, rpy_type As RPModelElement, max_size As Integer)
        MyBase.New(symbol, guid_str, rpy_type)
        Me.Size = max_size
    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        Dim is_valid As Boolean = False
        If Me.Size = 0 Then
            is_valid = True
        ElseIf value.Length <= Me.Size Then
            is_valid = True
        End If
        Return is_valid
    End Function

End Class


Public Class Enumerated_Data_Type

    Inherits Data_Type

    Public Enumerals As New List(Of Enumerated_Data_Type_Enumeral)


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_label As RPEnumerationLiteral
        For Each rpy_label In CType(Me.Rpy_Element, RPType).enumerationLiterals
            Dim enumeral As Enumerated_Data_Type_Enumeral = New Enumerated_Data_Type_Enumeral
            Me.Enumerals.Add(enumeral)
            enumeral.Import_From_Rhapsody_Model(Me, rpy_label)
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_type As RPType = CType(rpy_element, RPType)
        Dim label_idx As Integer = 1
        Dim rpy_label_nb As Integer = CType(Me.Rpy_Element, RPType).enumerationLiterals.Count
        For Each label In Me.Enumerals
            If label_idx <= rpy_label_nb Then
                Dim rpy_label As RPEnumerationLiteral
                rpy_label =
                    CType(rpy_type.enumerationLiterals.Item(label_idx), RPEnumerationLiteral)
                If rpy_label.name <> label.Name Or
                    rpy_label.value <> label.Value Or
                    rpy_label.description <> label.Description Then
                    rpy_type.getSaveUnit.setReadOnly(0)
                    rpy_label.name = label.Name
                    rpy_label.value = label.Value
                    rpy_label.description = label.Description
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Merge enumeral #" & label_idx.ToString & ".")
                End If
            Else
                rpy_type.getSaveUnit.setReadOnly(0)
                label.Export_To_Rhapsody(rpy_type)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Add enumeral #" & label_idx.ToString & ".")
            End If
            label_idx += 1
        Next

        If rpy_label_nb > Me.Enumerals.Count Then
            rpy_type.getSaveUnit.setReadOnly(0)
            Dim rpy_label_to_remove_idx As Integer
            For rpy_label_to_remove_idx = rpy_label_nb To (Me.Enumerals.Count + 1) Step -1
                Dim rpy_label_to_remove As RPEnumerationLiteral
                rpy_label_to_remove =
                    CType(rpy_type.enumerationLiterals.Item(rpy_label_to_remove_idx), 
                    RPEnumerationLiteral)
                rpy_type.deleteEnumerationLiteral(rpy_label_to_remove)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Remove enumeral #" & label_idx.ToString & ".")
                label_idx += 1
            Next
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_type As RPType = CType(rpy_elmt, RPType)
        rpy_type.kind = "Enumeration"
        For Each enumeral In Me.Enumerals
            enumeral.Export_To_Rhapsody(rpy_type)
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Enumerals.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "ENUM_1",
                "Shall aggregate at least one Enumerated_Data_Type_Enumeral.")
        Else
            If Me.Enumerals.Count = 1 Then
                Me.Add_Consistency_Check_Warning_Item(report,
                "ENUM_2",
                "Should aggregate at least two Enumerated_Data_Type_Enumeral.")
            End If

            Dim enumeral_values_list As New List(Of UInteger)
            Dim enumeral_without_value_nb As Integer = 0
            For Each enumeral In Me.Enumerals

                If Not Is_Symbol_Valid(enumeral.Name) Then
                    Me.Add_Consistency_Check_Error_Item(report,
                        "ENUM_7",
                        "Invalid enumeral symbol : " & enumeral.Name)
                End If

                If enumeral.Description = "" Then
                    Me.Add_Consistency_Check_Information_Item(report,
                        "ENUM_6",
                        "Enumeral " & enumeral.Name & " could have a description.")
                End If

                If enumeral.Value <> "" Then
                    Dim dummy As UInteger = 0
                    Dim is_uinteger As Boolean
                    is_uinteger = UInteger.TryParse(enumeral.Value, dummy)
                    If is_uinteger = False Then
                        Me.Add_Consistency_Check_Error_Item(report,
                            "ENUM_4",
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
                    "ENUM_3",
                    "If at least one enumeral has a Value, all the enumerals shall have a Value.")
            End If

            If enumeral_values_list.Count > 0 Then
                If enumeral_values_list.Count <> enumeral_values_list.Distinct.Count() Then
                    Me.Add_Consistency_Check_Error_Item(report,
                    "ENUM_5",
                    "The value of the enumerals shall be unique.")
                End If
            End If

        End If

    End Sub

    Public Overrides Function Is_Value_Valid(data_value As String) As Boolean
        Dim is_valid As Boolean = False
        For Each current_enumeral In Me.Enumerals
            If data_value = current_enumeral.Name Then
                is_valid = True
                Exit For
            End If
        Next
        Return is_valid
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Get_Complexity() As Double
        Return 1.5
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        Return Nothing
    End Function

End Class


Public Class Enumerated_Data_Type_Enumeral

    Public Name As String
    Public Description As String
    Public Value As String

    Private Parent As Enumerated_Data_Type
    Private Rpy_Element As RPEnumerationLiteral


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Sub Export_To_Rhapsody(rpy_enum As RPType)
        Dim rpy_label As RPEnumerationLiteral
        rpy_label = rpy_enum.addEnumerationLiteral(Me.Name)
        Me.Rpy_Element = rpy_label
        rpy_label.description = Me.Description
        rpy_label.value = Me.Value
    End Sub

End Class


Public Class Array_Data_Type

    Inherits Data_Type_Base_Typed

    Public Multiplicity As UInteger

    Private Complexity As Double = 0


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Function Is_Exportable(any_rpy_base As RPModelElement) As Boolean
        Dim referenced_rpy_type As RPType
        Dim rpy_proj As RPProject = CType(any_rpy_base.project, RPProject)
        Dim base_dt_guid As String = Transform_Guid_To_Rpy_GUID(Me.Base_Data_Type_Ref)
        referenced_rpy_type = CType(rpy_proj.findElementByGUID(base_dt_guid), RPType)
        If Not IsNothing(referenced_rpy_type) Then
            Return True
        Else
            Return False
        End If
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_type As RPType = CType(rpy_element, RPType)
        If rpy_type.typedefMultiplicity <> CStr(Me.Multiplicity) Then
            rpy_type.getSaveUnit.setReadOnly(0)
            rpy_type.typedefMultiplicity = CStr(Me.Multiplicity)
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Multplicity merged.")
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_type As RPType = CType(rpy_elmt, RPType)
        rpy_type.typedefMultiplicity = CStr(Me.Multiplicity)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Multiplicity = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "ARR_1",
                "Multiplicity shall be a strictly positive integer value.")
        ElseIf Me.Multiplicity = 1 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "ARR_2",
                "Multiplicity should be greater than 1.")
        End If

    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        Dim is_valid As Boolean = False
        If Regex.IsMatch(value, "^\[[\d|\D]*\]$") Then
            Dim values_list As List(Of String)
            values_list = Split(value.Substring(1, value.Length - 2), " ").ToList
            values_list.RemoveAll(Function(str) str = "")
            If values_list.Count = CDbl(Me.Multiplicity) Then
                Dim all_valid As Boolean = True
                Dim base_type As Data_Type
                base_type = CType(Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
                For idx = 0 To values_list.Count - 1
                    If base_type.Is_Value_Valid(values_list(idx)) = False Then
                        all_valid = False
                        Exit For
                    End If
                Next
                is_valid = all_valid
            End If
        End If
        Return is_valid
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Get_Complexity() As Double
        If Me.Complexity = 0 Then
            Dim data_type As Data_Type
            data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            Me.Complexity = 1.8 * data_type.Get_Complexity
        End If
        Return Me.Complexity
    End Function

End Class


Public Class Physical_Data_Type

    Inherits Data_Type_Base_Typed

    Public Unit As String
    Public Resolution As Decimal
    Public Offset As Decimal

    Private Is_Resol_Decimal As Boolean = False
    Private Is_Offset_Decimal As Boolean = False

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPType).typedefBaseType
        Return CType(rpy_type, RPModelElement)
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()

        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim tag As RPTag

        tag = CType(Me.Rpy_Element, RPType).getTag("Unit")
        Me.Unit = tag.value

        tag = CType(Me.Rpy_Element, RPType).getTag("Resolution")
        Me.Is_Resol_Decimal = Decimal.TryParse(
            tag.value.Replace(",", "."),
            NumberStyles.Any, _
            CultureInfo.GetCultureInfo("en-US"), _
            Me.Resolution)

        tag = CType(Me.Rpy_Element, RPType).getTag("Offset")
        Me.Is_Offset_Decimal = Decimal.TryParse(
            tag.value.Replace(",", "."),
            NumberStyles.Any,
            CultureInfo.GetCultureInfo("en-US"),
            Me.Offset)

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Physical_Data_Type", "Type")
    End Sub

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_type As RPType = CType(rpy_element, RPType)
        Dim tag As RPTag

        tag = rpy_type.getTag("Unit")
        If tag.value <> Me.Unit Then
            rpy_type.getSaveUnit.setReadOnly(0)
            rpy_type.setTagValue(tag, Me.Unit)
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Merge Unit.")
        End If

        tag = rpy_type.getTag("Resolution")
        Dim resol_str As String = Me.Resolution.ToString.Replace(",", ".")
        If tag.value.Replace(",", ".") <> resol_str Then
            rpy_type.getSaveUnit.setReadOnly(0)
            rpy_type.setTagValue(tag, resol_str)
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Merge Resolution.")
        End If

        tag = rpy_type.getTag("Offset")
        Dim offset_str As String = Me.Offset.ToString.Replace(",", ".")
        If tag.value.Replace(",", ".") <> offset_str Then
            rpy_type.getSaveUnit.setReadOnly(0)
            rpy_type.setTagValue(tag, offset_str)
            Me.Add_Export_Information_Item(report,
                Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                "Merge Offset.")
        End If
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_type As RPType = CType(rpy_elmt, RPType)
        rpy_type.typedefMultiplicity = "1"

        Dim tag As RPTag
        tag = rpy_type.getTag("Unit")
        rpy_type.setTagValue(tag, Me.Unit)

        tag = rpy_type.getTag("Resolution")
        rpy_type.setTagValue(tag, Me.Resolution.ToString)

        tag = rpy_type.getTag("Offset")
        rpy_type.setTagValue(tag, Me.Offset.ToString)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Unit = "" Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "PHY_2",
                "Unit shall be set.")
        End If

        If Me.Is_Resol_Decimal = False Or Me.Resolution <= 0 Then
            Me.Add_Consistency_Check_Error_Item(report, "PHY_1",
                "Resolution shall be a positive decimal value.")
        End If

        If Me.Is_Offset_Decimal = False Then
            Me.Add_Consistency_Check_Error_Item(report,
                "PHY_4",
                "Offset shall be a decimal value.")
        End If

        Dim referenced_type As Data_Type
        referenced_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        If Not IsNothing(referenced_type) Then
            If referenced_type.GetType <> GetType(Basic_Integer_Type) Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "PHY_3", _
                    "Referenced type shall be a Basic_Integer_Type.")
            End If
        Else
            ' already checked in  MyBase.Check_Own_Consistency
        End If

    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        Dim is_valid As Boolean = False
        Dim value_decimal As Decimal
        Dim is_value_decimal As Boolean
        is_value_decimal = Decimal.TryParse(
            value.Replace(",", "."),
            NumberStyles.Any, _
            CultureInfo.GetCultureInfo("en-US"), _
            value_decimal)
        If is_value_decimal = True Then
            Dim value_integer As Integer
            value_integer = CInt((value_decimal - Me.Offset) / Me.Resolution)
            Dim base_integer_type As Data_Type
            base_integer_type = CType(Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            If Not IsNothing(base_integer_type) Then
                If base_integer_type.Is_Value_Valid(value_integer.ToString) Then
                    is_valid = True
                End If
            End If
        End If

        Return is_valid
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Get_Complexity() As Double
        Return 1.2
    End Function

End Class


Public Class Structured_Data_Type

    Inherits Data_Type

    Public Fields As New List(Of Structured_Data_Type_Field)

    Private Complexity As Double = 0


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Fields)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        Dim rpy_attr As RPAttribute
        For Each rpy_attr In CType(Me.Rpy_Element, RPType).attributes
            Dim field As Structured_Data_Type_Field = New Structured_Data_Type_Field
            Me.Fields.Add(field)
            field.Import_From_Rhapsody_Model(Me, CType(rpy_attr, RPModelElement))
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Function Is_Exportable(any_rpy_base As RPModelElement) As Boolean
        Dim result As Boolean = True
        Dim rpy_proj As RPProject = CType(any_rpy_base.project, RPProject)
        For Each fd In Me.Fields
            Dim referenced_rpy_type As RPType
            Dim base_dt_guid As String = Transform_Guid_To_Rpy_GUID(fd.Base_Data_Type_Ref)
            referenced_rpy_type = CType(rpy_proj.findElementByGUID(base_dt_guid), RPType)
            If IsNothing(referenced_rpy_type) Then
                result = False
            End If
        Next
        Return result
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Dim rpy_type As RPType = CType(rpy_elmt, RPType)
        rpy_type.kind = "Structure"
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Fields.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "STRUC_1",
                "Shall aggregate at least one field.")
        ElseIf Me.Fields.Count = 1 Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "STRUC_2",
                "Should aggregate at least two fields.")
        End If

    End Sub

    Public Overrides Function Is_Value_Valid(value As String) As Boolean
        Dim is_valid As Boolean = False
        Dim struct_match As Match
        struct_match = Regex.Match(value, "^\s*\{([\d|\D]*)\}\s*$")
        If struct_match.Success = True Then
            Dim struct_fields_match As MatchCollection
            struct_fields_match = Regex.Matches(struct_match.Groups(1).Value, "([^;])+")
            Dim fields_list As New List(Of String)
            For idx = 0 To struct_fields_match.Count - 1
                fields_list.Add(struct_fields_match.Item(idx).Value.Trim)
            Next
            If fields_list.Count = CDbl(Me.Fields.Count) Then
                Dim all_valid As Boolean = True
                For idx = 0 To fields_list.Count - 1
                    Dim field_type_uuid As Guid
                    field_type_uuid = Me.Fields(idx).Base_Data_Type_Ref
                    Dim field_type As Data_Type
                    field_type = CType(Get_Element_By_Uuid(field_type_uuid), Data_Type)
                    If field_type.Is_Value_Valid(fields_list(idx)) = False Then
                        all_valid = False
                        Exit For
                    End If
                Next
                is_valid = all_valid
            End If
        End If
        Return is_valid
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Get_Complexity() As Double
        If Me.Complexity = 0 Then
            Dim field As Structured_Data_Type_Field
            For Each field In Me.Fields
                Me.Complexity += field.Get_Complexity
            Next
        End If
        Return Me.Complexity
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            For Each fd In Me.Fields
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(fd.Base_Data_Type_Ref), Data_Type)
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

End Class


Public Class Structured_Data_Type_Field

    Inherits Typed_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Attribute"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_type As RPType = CType(rpy_parent, RPType)
        Return CType(rpy_parent_type.addAttribute(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        ' No stereotype for Structured_Data_Type_Field
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        Dim referenced_data_type As Data_Type
        referenced_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        If Not IsNothing(referenced_data_type) Then
            If GetType(Structured_Data_Type) = referenced_data_type.GetType Then
                For Each field In CType(referenced_data_type, Structured_Data_Type).Fields
                    If field.UUID = Me.UUID Then
                        Me.Add_Consistency_Check_Warning_Item(report,
                        "STRUC_3", _
                        "Shall not reference its owner.")
                        Exit For
                    End If
                Next
            End If
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Function Get_Complexity() As Double
        Dim referenced_data_type As Data_Type
        referenced_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        Return referenced_data_type.Get_Complexity
    End Function


End Class