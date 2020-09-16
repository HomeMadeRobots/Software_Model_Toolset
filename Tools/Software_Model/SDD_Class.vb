Imports rhapsody2
Imports System.Xml.Serialization


Public Class SDD_Class

    Inherits Software_Class

    Public Base_Class_Ref As Guid

    <XmlArrayItem("Attribute")>
    Public Attributes As List(Of Variable_Attribute)

    <XmlArrayItem("Configuration")>
    Public Configurations As List(Of Configuration_Attribute)

    Public Private_Methods As List(Of Private_Method)
    Public Public_Methods As List(Of Public_Method)
    Public Event_Receptions As List(Of Event_Reception)

    <XmlArrayItem("Send_Event")>
    Public Send_Events As List(Of Guid)
    <XmlArrayItem("Receive_Event")>
    Public Receive_Events As List(Of Guid)
    <XmlArrayItem("Realize_Interface")>
    Public Realize_Interfaces As List(Of Guid)
    <XmlArrayItem("Need_Interface")>
    Public Need_Interfaces As List(Of Guid)

    Private Nb_Base_Class_Ref As Integer = 0

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            If Not IsNothing(Me.Attributes) Then
                children_list.AddRange(Me.Attributes)
            End If
            If Not IsNothing(Me.Configurations) Then
                children_list.AddRange(Me.Configurations)
            End If
            If Not IsNothing(Me.Private_Methods) Then
                children_list.AddRange(Me.Private_Methods)
            End If
            If Not IsNothing(Me.Public_Methods) Then
                children_list.AddRange(Me.Public_Methods)
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

        Me.Realize_Interfaces = New List(Of Guid)
        Dim rpy_gen As RPGeneralization
        For Each rpy_gen In CType(Me.Rpy_Element, RPClass).generalizations
            Dim referenced_rpy_elmt_guid As String
            referenced_rpy_elmt_guid = rpy_gen.baseClass.GUID
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(referenced_rpy_elmt_guid)
            If Is_Realize_Interface(CType(rpy_gen, RPModelElement)) Then
                Me.Realize_Interfaces.Add(ref_elmt_guid)
            Else
                Dim referenced_rpy_elmt As RPModelElement
                referenced_rpy_elmt = Me.Find_In_Rpy_Project(referenced_rpy_elmt_guid)
                If Is_SDD_Class(referenced_rpy_elmt) Then
                    Me.Base_Class_Ref = ref_elmt_guid
                End If
                Nb_Base_Class_Ref += 1
            End If
        Next
        If Me.Realize_Interfaces.Count = 0 Then
            Me.Realize_Interfaces = Nothing
        End If

        Me.Send_Events = New List(Of Guid)
        Me.Receive_Events = New List(Of Guid)
        Me.Need_Interfaces = New List(Of Guid)
        Dim rpy_dep As RPDependency
        For Each rpy_dep In CType(Me.Rpy_Element, RPClass).dependencies
            Dim rpy_elmt As RPModelElement = CType(rpy_dep, RPModelElement)
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(rpy_dep.dependsOn.GUID)
            If Is_Send_Event(rpy_elmt) Then
                Me.Send_Events.Add(ref_elmt_guid)
            ElseIf Is_Receive_Event(rpy_elmt) Then
                Me.Receive_Events.Add(ref_elmt_guid)
            ElseIf Is_Need_Interface(rpy_elmt) Then
                Me.Need_Interfaces.Add(ref_elmt_guid)
            End If
        Next
        If Me.Send_Events.Count = 0 Then
            Me.Send_Events = Nothing
        End If
        If Me.Receive_Events.Count = 0 Then
            Me.Receive_Events = Nothing
        End If
        If Me.Need_Interfaces.Count = 0 Then
            Me.Need_Interfaces = Nothing
        End If
    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Dim rpy_elmt As RPModelElement

        Me.Attributes = New List(Of Variable_Attribute)
        Me.Configurations = New List(Of Configuration_Attribute)
        Dim rpy_attribute As RPAttribute
        For Each rpy_attribute In CType(Me.Rpy_Element, RPClass).attributes
            rpy_elmt = CType(rpy_attribute, RPModelElement)
            If Is_Variable_Attribute(rpy_elmt) Then
                Dim attr As Variable_Attribute = New Variable_Attribute
                Me.Attributes.Add(attr)
                attr.Import_From_Rhapsody_Model(Me, rpy_elmt)
            ElseIf Is_Configuration_Attribute(rpy_elmt) Then
                Dim attr As Configuration_Attribute = New Configuration_Attribute
                Me.Configurations.Add(attr)
                attr.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Attributes.Count = 0 Then
            Me.Attributes = Nothing
        End If
        If Me.Configurations.Count = 0 Then
            Me.Configurations = Nothing
        End If

        Me.Private_Methods = New List(Of Private_Method)
        Me.Public_Methods = New List(Of Public_Method)
        Me.Event_Receptions = New List(Of Event_Reception)
        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            rpy_elmt = CType(rpy_ope, RPModelElement)
            If Is_Private_Method(rpy_elmt) Then
                Dim ope As Private_Method = New Private_Method
                Me.Private_Methods.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            ElseIf Is_Public_Method(rpy_elmt) Then
                Dim ope As Public_Method = New Public_Method
                Me.Public_Methods.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            ElseIf Is_Event_Reception(rpy_elmt) Then
                Dim ope As Event_Reception = New Event_Reception
                Me.Event_Receptions.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next
        If Me.Private_Methods.Count = 0 Then
            Me.Private_Methods = Nothing
        End If
        If Me.Public_Methods.Count = 0 Then
            Me.Public_Methods = Nothing
        End If
        If Me.Event_Receptions.Count = 0 Then
            Me.Event_Receptions = Nothing
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Function Is_Exportable(any_rpy_elmt As RPModelElement) As Boolean
        If Me.Base_Class_Ref = Guid.Empty Then
            Return True
        End If
        Dim referenced_rpy_class As RPClass
        Dim rpy_proj As RPProject = CType(any_rpy_elmt.project, RPProject)
        Dim base_class_guid As String = Transform_Guid_To_Rpy_GUID(Me.Base_Class_Ref)
        referenced_rpy_class = CType(rpy_proj.findElementByGUID(base_class_guid), RPClass)
        If Not IsNothing(referenced_rpy_class) Then
            Return True
        Else
            Return False
        End If
    End Function

    Protected Overrides Sub Merge_Rpy_Element(rpy_element As RPModelElement, report As Report)
        MyBase.Merge_Rpy_Element(rpy_element, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        ' Merge Base_Class_Ref
        Dim rpy_gen As RPGeneralization = Nothing
        Dim reference_found As Boolean = False
        If Me.Base_Class_Ref <> Guid.Empty Then
            Dim referenced_rpy_class_guid As String
            referenced_rpy_class_guid = Transform_Guid_To_Rpy_GUID(Me.Base_Class_Ref)
            For Each rpy_gen In rpy_class.generalizations
                Dim current_rpy_class As RPClass = CType(rpy_gen.baseClass, RPClass)
                If current_rpy_class.GUID = referenced_rpy_class_guid Then
                    ' No change
                    reference_found = True
                End If
            Next
            If reference_found = False Then
                Dim referenced_rpy_class As RPClass
                referenced_rpy_class = CType(Find_In_Rpy_Project(Me.Base_Class_Ref), RPClass)
                If IsNothing(referenced_rpy_class) Then
                    Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Base_Class not found : " & Me.Base_Class_Ref.ToString & ".")
                Else
                    rpy_class.addGeneralization(CType(referenced_rpy_class, RPClassifier))
                End If
            End If
        End If

        Merge_Dependencies(report, "Send_Event", Me.Send_Events, AddressOf Is_Send_Event)
        Merge_Dependencies(report, "Receive_Event", Me.Receive_Events, AddressOf Is_Receive_Event)
        Merge_Dependencies(report, "Need_Interface", Me.Need_Interfaces,
            AddressOf Is_Need_Interface)

        ' Merge Realize_Interfaces
        For Each id In Me.Realize_Interfaces
            reference_found = False
            Dim rpy_if_guid As String = Transform_Guid_To_Rpy_GUID(id)
            For Each rpy_gen In rpy_class.generalizations
                If Is_Realize_Interface(CType(rpy_gen, RPModelElement)) Then
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
                        "Realize_Interface not found : " & id.ToString & ".")
                Else
                    Dim created_rpy_gen As RPGeneralization
                    rpy_class.addGeneralization(CType(rpy_if, RPClassifier))
                    created_rpy_gen = rpy_class.findGeneralization(rpy_if.name)
                    created_rpy_gen.addStereotype("Realize_Interface", "Generalization")
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        "Realize_Interface merged : " & id.ToString & ".")
                End If
            End If
        Next

    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("SDD_Class", "Class")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)

        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        Dim referenced_rpy_class As RPClass
        If Me.Base_Class_Ref <> Guid.Empty Then
            referenced_rpy_class = CType(Find_In_Rpy_Project(Me.Base_Class_Ref), RPClass)
            If IsNothing(referenced_rpy_class) Then
                Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Base_Class not found : " & Me.Base_Class_Ref.ToString & ".")
            Else
                rpy_class.addGeneralization(CType(referenced_rpy_class, RPClassifier))
            End If
        End If

        Me.Set_Dependencies(report, "Send_Event", Me.Send_Events)
        Me.Set_Dependencies(report, "Receive_Event", Me.Receive_Events)
        Me.Set_Dependencies(report, "Need_Interface", Me.Need_Interfaces)

        For Each elmt_ref In Me.Realize_Interfaces
            Dim ref_rpy_elemt As RPModelElement = Me.Find_In_Rpy_Project(elmt_ref)
            If Not IsNothing(ref_rpy_elemt) Then
                Dim created_rpy_gen As RPGeneralization
                rpy_class.addGeneralization(CType(ref_rpy_elemt, RPClassifier))
                created_rpy_gen = rpy_class.findGeneralization(ref_rpy_elemt.name)
                created_rpy_gen.addStereotype("Realize_Interface", "Generalization")
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Realize_Interface not not found : " & elmt_ref.ToString & ".")
            End If
        Next

    End Sub

    Private Sub Merge_Dependencies(
        report As Report,
        stereotype_str As String,
        element_list As List(Of Guid),
        is_of_stereotype As Func(Of RPModelElement, Boolean))

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)
        Dim rpy_dep As RPDependency
        Dim ref_found As Boolean

        For Each id In element_list
            ref_found = False
            Dim rpy_if_guid As String = Transform_Guid_To_Rpy_GUID(id)
            For Each rpy_dep In rpy_class.dependencies
                If is_of_stereotype(CType(rpy_dep, RPModelElement)) Then
                    If rpy_dep.dependsOn.GUID = rpy_if_guid Then
                        ref_found = True
                        Exit For
                    End If
                End If
            Next
            If ref_found = False Then
                Dim rpy_if As RPModelElement = Me.Find_In_Rpy_Project(rpy_if_guid)
                If IsNothing(rpy_if) Then
                    Me.Add_Export_Error_Item(report,
                        Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                        stereotype_str & " not found : " & id.ToString & ".")
                Else
                    Dim created_rpy_dep As RPDependency = rpy_class.addDependencyTo(rpy_if)
                    created_rpy_dep.addStereotype(stereotype_str, "Dependency")
                    Me.Add_Export_Information_Item(report,
                        Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                        stereotype_str & " merged : " & id.ToString & ".")
                End If
            End If
        Next
    End Sub

    Private Sub Set_Dependencies(
        report As Report,
        stereotype_str As String,
        element_list As List(Of Guid))
        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)
        For Each elmt_ref In element_list
            Dim ref_rpy_elemt As RPModelElement = Me.Find_In_Rpy_Project(elmt_ref)
            If Not IsNothing(ref_rpy_elemt) Then
                Dim rpy_dep As RPDependency
                rpy_dep = rpy_class.addDependencyTo(ref_rpy_elemt)
                rpy_dep.addStereotype(stereotype_str, "Dependency")
            Else
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    stereotype_str & " not not found : " & elmt_ref.ToString & ".")
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Nb_Base_Class_Ref > 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "CLASS_1",
                "Shall generalize at most 1 class.")
        End If

        If Me.Nb_Base_Class_Ref <> 0 Then
            If Me.Base_Class_Ref = Guid.Empty Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "CLASS_2",
                    "Shall generalize a SDD_Class.")
            End If
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation (not yet implemented for SDD_Class)
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


Public Class Variable_Attribute

    Inherits Attribute_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Variable_Attribute", "Attribute")
    End Sub

End Class


Public Class Configuration_Attribute

    Inherits Attribute_Software_Element


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Configuration_Attribute", "Attribute")
    End Sub

End Class


Public Class Private_Method

    Inherits Operation_With_Arguments


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Private_Method", "Operation")
    End Sub

End Class


Public Class Public_Method

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
        Me.Rpy_Element.addStereotype("Public_Method", "Operation")
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


Public Class Event_Reception

    Inherits Operation_With_Arguments


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Reception", "Operation")
    End Sub

End Class