Imports rhapsody2
Imports System.Xml.Serialization
Imports System.Text.RegularExpressions

Public Class Implementation_File
    Inherits Software_Element

    Public File_Name As String = ""
    Public Language As E_LANGUAGE = E_LANGUAGE.C
    <XmlArrayItem("Implemented_Element")>
    Public Implemented_Elements As List(Of Guid)

    Public Enum E_LANGUAGE
        ASM
        C
        C_PLUS_PLUS
    End Enum


    '----------------------------------------------------------------------------------------------'
    ' General methods
    Private Function Convert_Language_String_To_Enum(lang_str As String) As E_LANGUAGE
        Dim lang As E_LANGUAGE = E_LANGUAGE.C
        Select Case lang_str
            Case "ASM"
                lang = E_LANGUAGE.ASM
            Case "C"
                lang = E_LANGUAGE.C
            Case "C_PLUS_PLUS"
                lang = E_LANGUAGE.C_PLUS_PLUS
        End Select
        Return lang
    End Function

    Private Function Convert_Language_Enum_To_String(lang As E_LANGUAGE) As String
        Dim lang_str As String = "C"
        Select Case lang
            Case E_LANGUAGE.ASM
                lang_str = "ASM"
            Case E_LANGUAGE.C
                lang_str = "C"
            Case E_LANGUAGE.C_PLUS_PLUS
                lang_str = "C_PLUS_PLUS"
        End Select
        Return lang_str
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim tag As RPTag

        ' Get File_Name
        tag = CType(CType(Me.Rpy_Element, RPModule).allTags(1), RPTag)
        If Not IsNothing(tag) Then
            Me.File_Name = tag.value
        Else
            Me.File_Name = ""
        End If

        ' Get Language
        tag = CType(Me.Rpy_Element, RPModule).getTag("Language")
        If Not IsNothing(tag) Then
            Me.Language = Convert_Language_String_To_Enum(tag.value)
        Else
            Me.Language = E_LANGUAGE.C
        End If

        ' Get Implemented_Elements
        Me.Implemented_Elements = New List(Of Guid)
        Dim rpy_dep As RPDependency
        Dim rpy_elmt_owning_dep As RPModelElement
        rpy_elmt_owning_dep = CType(CType(Me.Rpy_Element, RPModule).otherClass, RPModelElement)
        If rpy_elmt_owning_dep.dependencies.Count = 0 Then
            rpy_elmt_owning_dep = Me.Rpy_Element
        End If
        For Each rpy_dep In rpy_elmt_owning_dep.dependencies
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            If Is_Implemented_Element(CType(rpy_dep, RPModelElement)) Then
                Me.Implemented_Elements.Add(ref_elmt_guid)
            End If
        Next
        If Me.Implemented_Elements.Count = 0 Then
            Me.Implemented_Elements = Nothing
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Module"
    End Function

    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        ' Rhapsody API bug regarding findNestedElement( string, "Module")
        ' One can execute this Function while the element alreaddy exist :
        ' Use Try/Catch to prevent crash
        Dim rpy_parent_pkg As RPPackage = CType(rpy_parent, RPPackage)
        Try
            Return CType(rpy_parent_pkg.addModule(Me.Name), RPModelElement)
        Catch
            Return Nothing
        End Try
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_module As RPModule = CType(rpy_element, RPModule)
        Dim rpy_tag As RPTag

        ' Merge File_Name
        rpy_tag = rpy_module.getTag("File_Name")
        If Not IsNothing(rpy_tag) Then
            If Me.File_Name <> rpy_tag.value Then
                rpy_element.getSaveUnit.setReadOnly(0)
                rpy_module.setTagValue(rpy_tag, Me.File_Name)
            End If
        End If

        ' Merge Language
        rpy_tag = rpy_module.getTag("Language")
        If Not IsNothing(rpy_tag) Then
            Dim lang_str As String = Convert_Language_Enum_To_String(Me.Language)
            If lang_str <> rpy_tag.value Then
                rpy_element.getSaveUnit.setReadOnly(0)
                rpy_module.setTagValue(rpy_tag, lang_str)
            End If
        End If

        Me.Merge_Dependencies(
            report,
            "Implemented_Element",
            Me.Implemented_Elements,
            AddressOf Is_Implemented_Element)
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Implementation_File", "Module")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)
        ' Rhapsody API bug regarding findNestedElement( string, "Module")
        ' One can execute this Function while the element alreaddy exist :
        ' test rpy_elmt (see Create_Rpy_Element) to prevent crash
        If Not IsNothing(rpy_elmt) Then

            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

            Dim rpy_module As RPModule = CType(Rpy_Element, RPModule)
            Dim rpy_tag As RPTag

            ' Set File_Name
            rpy_tag = rpy_module.getTag("File_Name")
            If Not IsNothing(rpy_tag) Then
                rpy_module.setTagValue(rpy_tag, Me.File_Name)
            End If

            ' Merge Language
            rpy_tag = rpy_module.getTag("Language")
            If Not IsNothing(rpy_tag) Then
                Dim lang_str As String = Convert_Language_Enum_To_String(Me.Language)
                rpy_module.setTagValue(rpy_tag, lang_str)
            End If

            Me.Set_Dependencies(report, "Implemented_Element", Me.Implemented_Elements)
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        If Me.File_Name = "" Then
            Me.Add_Consistency_Check_Error_Item(report, "FILE_1", "Shall have a File_Name")
        ElseIf Not Regex.IsMatch(Me.File_Name, "\.") Then
            Me.Add_Consistency_Check_Error_Item(report, "FILE_2",
                "The File_Name shall be a string of characters with an extension (*.*).")
        ElseIf Not Regex.IsMatch(Me.File_Name.Split("."c).First, "^[a-zA-Z0-9_]+$") Then
            Me.Add_Consistency_Check_Error_Item(report, "FILE_3",
                "Allowed characters for File_Name (without the extension) are " &
                "'a' to 'z', 'A' to 'Z', '0' to '9' and _ (underscore).")
        ElseIf Not (Regex.IsMatch(Me.File_Name.Split("."c).Last, "^[ch]$") _
                    Or Regex.IsMatch(Me.File_Name.Split("."c).Last, "^cpp$")) Then
            Me.Add_Consistency_Check_Error_Item(report, "FILE_4",
                "Allowed extensions for File_Name are : .c, .h or .cpp.")
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation 

End Class
