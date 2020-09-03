Imports rhapsody2
Imports System.Globalization

Public MustInherit Class Data_Type

    Inherits Classifier_Software_Element

    Public Overridable Function Is_Basic_Type() As Boolean
        Return False
    End Function

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public MustOverride Function Get_Complexity() As Double

    Public Overrides Function Find_Dependent_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of Classifier_Software_Element)

            Dim needed_elements As List(Of Classifier_Software_Element)

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
                Me.Base_Data_Type_Ref = Transform_Rpy_GUID_To_Guid(rpy_type.GUID)
            Else
                Me.Base_Data_Type_Ref = Nothing
            End If
        Else
            Me.Base_Data_Type_Ref = Nothing
        End If
    End Sub

    Public Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_type As RPType = CType(rpy_element, RPType)
        If rpy_type.typedefBaseType.GUID <> Transform_Guid_To_Rpy_GUID(Me.Base_Data_Type_Ref) Then
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

    Public Overrides Sub Set_Rpy_Common_Attributes(rpy_element As RPModelElement, report As Report)
        MyBase.Set_Rpy_Common_Attributes(rpy_element, report)
        Dim rpy_type As RPType = CType(rpy_element, RPType)
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


    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Base_Data_Type_Ref = Guid.Empty Then
            Me.Add_Consistency_Check_Error_Item(report,
                "TYP_1",
                "Referenced type shall be a Data_Type.")
        End If

    End Sub

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of Classifier_Software_Element)
            Dim data_type As Data_Type
            data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
            If Not data_type.Is_Basic_Type Then
                Me.Needed_Elements.Add(data_type)
            End If
        End If
        Return Me.Needed_Elements
    End Function

End Class


Public MustInherit Class Basic_Type

    Inherits Data_Type

    Public Overrides Function Is_Basic_Type() As Boolean
        Return True
    End Function

    Public Overrides Function Get_Complexity() As Double
        Return 1
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        Return Nothing
    End Function

    Public Sub Set_Top_Package(pkg As Top_Level_Package)
        Me.Top_Package = pkg
    End Sub

    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        ' Basic_Type do not need to be exported as thay already belong to profile package.
    End Sub

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

Public Class Basic_Character_Type
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


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_type As RPType
        rpy_type = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Type"), RPType)
        If Not IsNothing(rpy_type) Then
            Me.Merge_Rpy_Element(CType(rpy_type, RPModelElement), report)

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

        Else
            rpy_type = rpy_parent_pkg.addType(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_type, RPModelElement), report)
            rpy_type.addStereotype("Data_Type", "Type")
            rpy_type.kind = "Enumeration"
            For Each enumeral In Me.Enumerals
                enumeral.Export_To_Rhapsody(rpy_type)
            Next
        End If
    End Sub


    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Enumerals.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "ENUM_1",
                "Shall aggregate at least one Enumerated_Data_Type_Enumeral.")
        Else
            If Me.Enumerals.Count = 1 Then
                Me.Add_Consistency_Check_Error_Item(report,
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

    Public Overrides Function Get_Complexity() As Double
        Return 1.5
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        Return Nothing
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

    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_type As RPType
        rpy_type = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Type"), RPType)
        If Not IsNothing(rpy_type) Then
            Me.Merge_Rpy_Element(CType(rpy_type, RPModelElement), report)
            If rpy_type.typedefMultiplicity <> CStr(Me.Multiplicity) Then
                rpy_type.getSaveUnit.setReadOnly(0)
                rpy_type.typedefMultiplicity = CStr(Me.Multiplicity)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Multplicity merged.")
            End If
        Else
            rpy_type = rpy_parent_pkg.addType(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_type, RPModelElement), report)

            rpy_type.typedefMultiplicity = CStr(Me.Multiplicity)

            rpy_type.addStereotype("Data_Type", "Type")

        End If
    End Sub

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
            NumberStyles.Any, _
            CultureInfo.InvariantCulture, _
            dummy)
        If dummy = 0 Then
            Me.Resolution = Nothing
        End If

        tag = CType(Me.Rpy_Element, RPType).getTag("Offset")
        Me.Offset = tag.value
        Dim is_decimal As Boolean
        is_decimal = Decimal.TryParse(
            Me.Offset,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            dummy)
        If is_decimal = False Then
            Me.Offset = Nothing
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_type As RPType
        rpy_type = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Type"), RPType)
        If Not IsNothing(rpy_type) Then
            Me.Merge_Rpy_Element(CType(rpy_type, RPModelElement), report)

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
            If tag.value <> Me.Resolution Then
                rpy_type.getSaveUnit.setReadOnly(0)
                rpy_type.setTagValue(tag, Me.Resolution)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Merge Resolution.")
            End If

            tag = rpy_type.getTag("Offset")
            If tag.value <> Me.Offset Then
                rpy_type.getSaveUnit.setReadOnly(0)
                rpy_type.setTagValue(tag, Me.Offset)
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Merge Offset.")
            End If

        Else
            rpy_type = rpy_parent_pkg.addType(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_type, RPModelElement), report)

            rpy_type.typedefMultiplicity = "1"

            rpy_type.addStereotype("Physical_Data_Type", "Type")

            Dim tag As RPTag
            tag = rpy_type.getTag("Unit")
            rpy_type.setTagValue(tag, Me.Unit)

            tag = rpy_type.getTag("Resolution")
            rpy_type.setTagValue(tag, Me.Resolution)

            tag = rpy_type.getTag("Offset")
            rpy_type.setTagValue(tag, Me.Offset)

        End If
    End Sub


    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Unit = "" Then
            Me.Add_Consistency_Check_Warning_Item(report,
                "PHY_2",
                "Unit shall be set.")
        End If

        If IsNothing(Me.Resolution) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "PHY_1",
                "Resolution shall be set to a non-null decimal value.")
        End If

        If IsNothing(Me.Offset) Then
            Me.Add_Consistency_Check_Error_Item(report,
                "PHY_4",
                "Offset shall be set to a numerical value.")
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

    Public Overrides Function Get_Complexity() As Double
        Return 1.2
    End Function

End Class


Public Class Structured_Data_Type

    Inherits Data_Type

    Public Fields As List(Of Structured_Data_Type_Field)

    Private Complexity As Double = 0

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Fields) Then
                children_list.AddRange(Me.Fields)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
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

    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Dim rpy_type As RPType
        rpy_type = CType(rpy_parent_pkg.findNestedElement(Me.Name, "Type"), RPType)
        If Not IsNothing(rpy_type) Then
            Me.Merge_Rpy_Element(CType(rpy_type, RPModelElement), report)
        Else
            rpy_type = rpy_parent_pkg.addType(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_type, RPModelElement), report)
            rpy_type.addStereotype("Data_Type", "Type")
            rpy_type.kind = "Structure"
        End If
        For Each field In Me.Fields
            field.Export_To_Rhapsody(CType(rpy_type, RPModelElement), report)
        Next
    End Sub


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

    Public Overrides Function Get_Complexity() As Double
        If Me.Complexity = 0 Then
            Dim field As Structured_Data_Type_Field
            For Each field In Me.Fields
                Me.Complexity += field.Get_Complexity
            Next
        End If
        Return Me.Complexity
    End Function

    Public Overrides Function Find_Needed_Elements() As List(Of Classifier_Software_Element)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of Classifier_Software_Element)
            If Not IsNothing(Me.Fields) Then
                For Each fd In Me.Fields
                    Dim data_type As Data_Type
                    data_type = CType(Me.Get_Element_By_Uuid(fd.Base_Data_Type_Ref), Data_Type)
                    If Not data_type.Is_Basic_Type Then
                        If Not Me.Needed_Elements.Contains(data_type) Then
                            Me.Needed_Elements.Add(data_type)
                        End If
                    End If
                Next
            End If
        End If
        Return Me.Needed_Elements
    End Function

End Class


Public Class Structured_Data_Type_Field

    Inherits Typed_Software_Element

    Protected Overrides Function Get_Rpy_Data_Type() As RPModelElement
        Dim rpy_type As RPClassifier = CType(Me.Rpy_Element, RPAttribute).type
        Return CType(rpy_type, RPModelElement)
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Sub Export_To_Rhapsody(rpy_parent As RPModelElement, report As Report)
        Dim rpy_parent_type As RPType = CType(rpy_parent, RPType)
        Dim rpy_attr As RPAttribute
        rpy_attr = CType(rpy_parent_type.findNestedElement(Me.Name, "Attribute"), RPAttribute)
        If Not IsNothing(rpy_attr) Then
            Me.Merge_Rpy_Element(CType(rpy_attr, RPModelElement), report)
        Else
            rpy_attr = rpy_parent_type.addAttribute(Me.Name)
            Me.Set_Rpy_Common_Attributes(CType(rpy_attr, RPModelElement), report)
        End If
    End Sub


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
                    End If
                Next
            End If
        End If

    End Sub

    Public Function Get_Complexity() As Double
        Dim referenced_data_type As Data_Type
        referenced_data_type = CType(Me.Get_Element_By_Uuid(Me.Base_Data_Type_Ref), Data_Type)
        Return referenced_data_type.Get_Complexity
    End Function

End Class