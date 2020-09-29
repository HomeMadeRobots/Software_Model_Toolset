Imports rhapsody2
Imports System.Xml.Serialization


Public MustInherit Class Software_Interface
    Inherits SMM_Class

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = New List(Of SMM_Classifier)
            Dim swct_list As List(Of Component_Type)
            swct_list = Me.Container.Get_All_Component_Types
            For Each swct In swct_list
                For Each pport In swct.Provider_Ports
                    If pport.Contract_Ref = Me.UUID Then
                        If Not Me.Dependent_Elements.Contains(swct) Then
                            Me.Dependent_Elements.Add(swct)
                        End If
                    End If
                Next
                For Each rport In swct.Requirer_Ports
                    If rport.Contract_Ref = Me.UUID Then
                        If Not Me.Dependent_Elements.Contains(swct) Then
                            Me.Dependent_Elements.Add(swct)
                        End If
                    End If
                Next
            Next
        End If
        Return Me.Dependent_Elements
    End Function

    Public Overridable Function Is_Exportable(any_rpy_elmt As RPModelElement) As Boolean
        Return True
    End Function

End Class


Public Class Client_Server_Interface

    Inherits Software_Interface

    <XmlArrayItemAttribute(GetType(Synchronous_Operation)), _
     XmlArrayItemAttribute(GetType(Asynchronous_Operation)), _
     XmlArray("Operations")>
    Public Operations As New List(Of Operation_With_Arguments)
    Public Base_Interface_Ref As Guid = Guid.Empty

    Private Nb_Base_Interface_Ref As Integer = 0

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Operations)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_gen As RPGeneralization
        For Each rpy_gen In CType(Me.Rpy_Element, RPClass).generalizations
            Dim referenced_rpy_elmt_guid As String
            referenced_rpy_elmt_guid = rpy_gen.baseClass.GUID
            Dim ref_elmt_guid As Guid
            ref_elmt_guid = Transform_Rpy_GUID_To_Guid(referenced_rpy_elmt_guid)
            Dim referenced_rpy_elmt As RPModelElement
            referenced_rpy_elmt = Me.Find_In_Rpy_Project(referenced_rpy_elmt_guid)
            If Is_Client_Server_Interface(referenced_rpy_elmt) Then
                Me.Base_Interface_Ref = ref_elmt_guid
            End If
            Me.Nb_Base_Interface_Ref += 1
        Next
    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        Dim rpy_ope As RPOperation
        For Each rpy_ope In CType(Me.Rpy_Element, RPClass).operations
            If Is_Synchronous_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim operation As Synchronous_Operation = New Synchronous_Operation
                Me.Operations.Add(operation)
                operation.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            ElseIf Is_Asynchronous_Operation(CType(rpy_ope, RPModelElement)) Then
                Dim operation As Asynchronous_Operation = New Asynchronous_Operation
                Me.Operations.Add(operation)
                operation.Import_From_Rhapsody_Model(Me, CType(rpy_ope, RPModelElement))
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Public Overrides Function Is_Exportable(any_rpy_elmt As RPModelElement) As Boolean
        If Me.Base_Interface_Ref = Guid.Empty Then
            Return True
        End If
        Dim referenced_rpy_class As RPClass
        Dim rpy_proj As RPProject = CType(any_rpy_elmt.project, RPProject)
        Dim base_class_guid As String = Transform_Guid_To_Rpy_GUID(Me.Base_Interface_Ref)
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

        ' Merge Base_Interface_Ref
        Dim rpy_gen As RPGeneralization = Nothing
        Dim reference_found As Boolean = False
        If Me.Base_Interface_Ref <> Guid.Empty Then
            Dim referenced_rpy_class_guid As String
            referenced_rpy_class_guid = Transform_Guid_To_Rpy_GUID(Me.Base_Interface_Ref)
            For Each rpy_gen In rpy_class.generalizations
                Dim current_rpy_class As RPClass = CType(rpy_gen.baseClass, RPClass)
                If current_rpy_class.GUID = referenced_rpy_class_guid Then
                    ' No change
                    reference_found = True
                End If
            Next
            If reference_found = False Then
                Dim referenced_rpy_class As RPClass
                referenced_rpy_class = CType(Find_In_Rpy_Project(Me.Base_Interface_Ref), RPClass)
                If IsNothing(referenced_rpy_class) Then
                    Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Base_Interface not found : " & Me.Base_Interface_Ref.ToString & ".")
                Else
                    rpy_class.addGeneralization(CType(referenced_rpy_class, RPClassifier))
                End If
            End If
        End If
    End Sub

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Client_Server_Interface", "Class")
    End Sub

    Protected Overrides Sub Set_Rpy_Element_Attributes(
        rpy_elmt As RPModelElement,
        report As Report)

        MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)

        Dim rpy_class As RPClass = CType(Me.Rpy_Element, RPClass)

        Dim referenced_rpy_class As RPClass
        If Me.Base_Interface_Ref <> Guid.Empty Then
            referenced_rpy_class = CType(Find_In_Rpy_Project(Me.Base_Interface_Ref), RPClass)
            If IsNothing(referenced_rpy_class) Then
                Me.Add_Export_Error_Item(report,
                Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                "Base_Interface not found : " & Me.Base_Interface_Ref.ToString & ".")
            Else
                rpy_class.addGeneralization(CType(referenced_rpy_class, RPClassifier))
            End If
        End If
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Own_Consistency(report As Report)
        MyBase.Check_Own_Consistency(report)

        If Me.Operations.Count = 0 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "CSIF_1",
                "Shall provide at least one operation.")
        End If

        If Me.Nb_Base_Interface_Ref > 1 Then
            Me.Add_Consistency_Check_Error_Item(report,
                "CSIF_2",
                "Shall specialize at most 1 Client_Server_Interface.")
        End If

        If Me.Nb_Base_Interface_Ref <> 0 Then
            If Me.Base_Interface_Ref = Guid.Empty Then
                Me.Add_Consistency_Check_Error_Item(report,
                    "CSIF_3",
                    "Shall specialize a Client_Server_Interface.")
            End If
        End If

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            For Each current_ope In Me.Operations
                For Each arg In current_ope.Arguments
                    Dim data_type As Data_Type
                    data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                    If Not data_type.Is_Basic_Type Then
                        If Not Me.Needed_Elements.Contains(data_type) Then
                            Me.Needed_Elements.Add(data_type)
                        End If
                    End If
                Next
            Next
            If Me.Base_Interface_Ref = Guid.Empty Then
                Dim base_csif As Client_Server_Interface
                base_csif = CType(Me.Get_Element_By_Uuid(Me.Base_Interface_Ref), 
                            Client_Server_Interface)
                Me.Needed_Elements.Add(base_csif)
            End If
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Find_Dependent_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Dependent_Elements) Then
            Me.Dependent_Elements = MyBase.Find_Dependent_Elements()

            Dim if_list As List(Of Software_Interface)
            if_list = Me.Container.Get_All_Interfaces
            For Each sw_if In if_list
                If sw_if.GetType = GetType(Client_Server_Interface) Then
                    If CType(sw_if, Client_Server_Interface).Base_Interface_Ref = Me.UUID Then
                        Me.Dependent_Elements.Add(sw_if)
                    End If
                End If
            Next
        End If
        Return Me.Dependent_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            For Each ope In Me.Operations
                Me.Weighted_Methods_Per_Class += 1
                If Not IsNothing(ope.Arguments) Then
                    For Each arg In ope.Arguments
                        Dim data_type As Data_Type
                        data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                        Me.Weighted_Methods_Per_Class += data_type.Get_Complexity
                    Next
                End If
            Next
        End If
        Return Me.Weighted_Methods_Per_Class
    End Function

End Class


Public Class Synchronous_Operation
    Inherits Operation_With_Arguments

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Synchronous_Operation", "Operation")
    End Sub

End Class


Public Class Asynchronous_Operation
    Inherits Operation_With_Arguments

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Asynchronous_Operation", "Operation")
    End Sub

End Class


Public Class Event_Interface

    Inherits Software_Interface

    Public Arguments As New List(Of Event_Argument)

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Arguments)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        Dim rpy_event_arg As RPAttribute
        For Each rpy_event_arg In CType(Me.Rpy_Element, RPClass).attributes
            If Is_Event_Argument(CType(rpy_event_arg, RPModelElement)) Then
                Dim arg As Event_Argument = New Event_Argument
                Me.Arguments.Add(arg)
                arg.Import_From_Rhapsody_Model(Me, CType(rpy_event_arg, RPModelElement))
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Interface", "Class")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = New List(Of SMM_Classifier)
            For Each arg In Me.Arguments
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                If Not data_type.Is_Basic_Type Then
                    If Not Me.Needed_Elements.Contains(data_type) Then
                        Me.Needed_Elements.Add(data_type)
                    End If
                End If
            Next
        End If
        Return Me.Needed_Elements
    End Function

    Public Overrides Function Compute_WMC() As Double
        If Me.Weighted_Methods_Per_Class = 0 Then
            Me.Weighted_Methods_Per_Class = 1
            For Each arg In Me.Arguments
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                Me.Weighted_Methods_Per_Class += data_type.Get_Complexity
            Next
        End If
        Return Me.Weighted_Methods_Per_Class
    End Function

End Class


Public Class Event_Argument

    Inherits Typed_Software_Element

    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Attribute"
    End Function

    Protected Overrides Function Create_Rpy_Element(
        rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)
        Return CType(rpy_parent_class.addAttribute(Me.Name), RPModelElement)
    End Function

    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Argument", "Attribute")
    End Sub

End Class