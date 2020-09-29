Imports rhapsody2
Imports System.Xml.Serialization


Public Class Internal_Design_Class

    Inherits SDD_Class

    <XmlArrayItem("Configuration")>
    Public Configurations As New List(Of Configuration_Parameter)
    Public Public_Operations As New List(Of Public_Operation)
    <XmlArrayItem("Realized_Interface")>
    Public Realized_Interfaces As New List(Of Guid)
    <XmlArrayItem("Needed_Interface")>
    Public Needed_Interfaces As New List(Of Guid)


    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As List(Of Software_Element)
            children_list = MyBase.Get_Children
            children_list.AddRange(Me.Configurations)
            children_list.AddRange(Me.Public_Operations)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Function Is_My_Metaclass(rpy_element As RPModelElement) As Boolean
        Return Is_Internal_Design_Class(rpy_element)
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        Dim rpy_gen As RPGeneralization
        For Each rpy_gen In CType(Me.Rpy_Element, RPClass).generalizations
            Dim referenced_rpy_elmt_guid As String
            referenced_rpy_elmt_guid = rpy_gen.baseClass.GUID
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(referenced_rpy_elmt_guid)
            If Is_Realized_Interface(CType(rpy_gen, RPModelElement)) Then
                Me.Realized_Interfaces.Add(ref_elmt_guid)
            End If
        Next

        Dim rpy_dep As RPDependency
        For Each rpy_dep In CType(Me.Rpy_Element, RPClass).dependencies
            Dim rpy_elmt As RPModelElement = CType(rpy_dep, RPModelElement)
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            If Is_Needed_Interface(rpy_elmt) Then
                Me.Needed_Interfaces.Add(ref_elmt_guid)
            End If
        Next

    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        MyBase.Import_Children_From_Rhapsody_Model()
        Dim rpy_elmt As RPModelElement

        Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            rpy_elmt = CType(rpy_attribute, RPModelElement)
            If Is_Configuration_Parameter(rpy_elmt) Then
                Dim attr As Configuration_Parameter = New Configuration_Parameter
                Me.Configurations.Add(attr)
                attr.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next

        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            rpy_elmt = CType(rpy_ope, RPModelElement)
            If Is_Public_Operation(rpy_elmt) Then
                Dim ope As Public_Operation = New Public_Operation
                Me.Public_Operations.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Merge_Dependencies(report, "Needed_Interface", Me.Needed_Interfaces,
            AddressOf Is_Needed_Interface)

        ' Merge Realized_Interfaces
        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)
        Dim reference_found As Boolean = False
        Dim rpy_gen As RPGeneralization = Nothing
        For Each id In Me.Realized_Interfaces
            reference_found = False
            Dim rpy_if_guid As String = Transform_Guid_To_Rpy_GUID(id)
            For Each rpy_gen In rpy_class.generalizations
                If Is_Realized_Interface(CType(rpy_gen, RPModelElement)) Then
                    If rpy_gen.baseClass.GUID = rpy_if_guid Then
                        reference_found = True
                        Exit For
                    End If
                End If
            Next
            If reference_found = False Then
                Dim rpy_if As RPModelElement = Me.Find_In_Rpy_Project(rpy_if_guid)
                If IsNothing(rpy_if) Then
                    Me.Add_Export_Error_Item(report,
                        Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                        "Realized_Interface not found : " & id.ToString & ".")
                Else
                    Dim created_rpy_gen As RPGeneralization
                    rpy_class.addGeneralization(CType(rpy_if, RPClassifier))
                    created_rpy_gen = rpy_class.findGeneralization(rpy_if.name)
                    created_rpy_gen.addStereotype("Realized_Interface", "Generalization")
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Realized_Interface merged : " & id.ToString & ".")
                End If
            End If
        Next

    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Internal_Design_Class", "Class")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)

        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

        Me.Set_Dependencies(report, "Needed_Interface", Me.Needed_Interfaces)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)
        For Each elmt_ref In Me.Realized_Interfaces
            Dim ref_rpy_elemt As RPModelElement = Me.Find_In_Rpy_Project(elmt_ref)
            If Not IsNothing(ref_rpy_elemt) Then
                Dim created_rpy_gen As RPGeneralization
                rpy_class.addGeneralization(CType(ref_rpy_elemt, RPClassifier))
                created_rpy_gen = rpy_class.findGeneralization(ref_rpy_elemt.name)
                created_rpy_gen.addStereotype("Realized_Interface", "Generalization")
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Realized_Interface not not found : " & elmt_ref.ToString & ".")
            End If
        Next

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation (not yet implemented for Internal_Design_Class)

End Class


Public Class Public_Operation

    Inherits Operation_With_Arguments

    Public Is_Abstract As Boolean

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_op As RPOperation = CType(Me.Rpy_Element, RPOperation)
        If rpy_op.isAbstract > 0 Then
            Me.Is_Abstract = True
        Else
            Me.Is_Abstract = False
        End If
    End Sub

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)
        Dim rpy_op As RPOperation = CType(Me.Rpy_Element, RPOperation)
        If rpy_op.isAbstract > 0 Then
            If Me.Is_Abstract = False Then
                rpy_op.isAbstract = 0
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Is_Abstract merged : True -> False.")
            End If
        Else
            If Me.Is_Abstract = True Then
                rpy_op.isAbstract = 1
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Is_Abstract merged : False -> True.")
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Public_Operation", "Operation")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        If Me.Is_Abstract = True Then
            CType(Me.Rpy_Element, RPOperation).isAbstract = 1
        Else
            CType(Me.Rpy_Element, RPOperation).isAbstract = 0
        End If
    End Sub

End Class
