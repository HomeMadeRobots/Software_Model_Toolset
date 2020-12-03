Imports rhapsody2
Imports System.Xml.Serialization
Imports System.IO


Public Class Internal_Design_Class

    Inherits SDD_Class

    <XmlArrayItem("Configuration")>
    Public Configurations As New List(Of Configuration_Parameter)
    Public Public_Operations As New List(Of Public_Operation)
    <XmlArrayItem("Event_Reception")>
    Public Event_Receptions As New List(Of Design_Class_Event_Reception)
    <XmlArrayItem("Realized_Interface")>
    Public Realized_Interfaces As New List(Of Guid)
    <XmlArrayItem("Needed_Interface")>
    Public Needed_Interfaces As New List(Of Guid)
    Public Associations As New List(Of Classes_Association)



    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As List(Of Software_Element)
            children_list = MyBase.Get_Children
            children_list.AddRange(Me.Configurations)
            children_list.AddRange(Me.Public_Operations)
            children_list.AddRange(Me.Event_Receptions)
            children_list.AddRange(Me.Associations)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Private Function Is_CS_Operation_Realization(op As Public_Operation) As Boolean
        Dim result As Boolean = False
        For Each cs_if_uuid In Me.Realized_Interfaces
            Dim cs_if As Client_Server_Interface
            cs_if = CType(Me.Get_Element_By_Uuid(cs_if_uuid), Client_Server_Interface)
            For Each cs_op In cs_if.Operations
                If cs_op.Name = op.Name Then
                    Return True
                End If
            Next
        Next
        Return result
    End Function

    Public Function Is_Abstract() As Boolean
        Dim result As Boolean = False

        ' Check if public operations are abtract
        For Each op In Me.Public_Operations
            If op.Is_Abstract = True Then
                result = True
                Exit For
            End If
        Next

        ' Check if received events are implemented
        If result = False Then
            ' TODO : take into account Received_Events from parents
            For Each ev_uuid In Me.Received_Events
                Dim is_ev_recep_realized As Boolean = False
                Dim ev As Event_Interface = CType(Me.Get_Element_By_Uuid(ev_uuid), Event_Interface)
                For Each ev_recep In Me.Event_Receptions
                    If ev.Name = ev_recep.Name Then
                        is_ev_recep_realized = True
                        Exit For
                    End If
                Next
                If is_ev_recep_realized = False Then
                    result = True
                    Exit For
                End If
            Next
        End If

        ' Check if realized interface operation's are implemented
        If result = False Then
            ' TODO : take into account Realized_Interfaces from parents
            For Each cs_if_uuid In Me.Realized_Interfaces
                Dim is_cs_if_realized As Boolean = True
                Dim cs_if As Client_Server_Interface
                cs_if = CType(Me.Get_Element_By_Uuid(cs_if_uuid), Client_Server_Interface)
                For Each cs_if_op In cs_if.Operations
                    Dim is_cs_if_op_realized As Boolean = False
                    For Each op In Me.Public_Operations
                        If op.Name = cs_if_op.Name Then
                            is_cs_if_op_realized = True
                            Exit For
                        End If
                    Next
                    If is_cs_if_op_realized = False Then
                        is_cs_if_realized = False
                        Exit For
                    End If
                Next
                If is_cs_if_realized = False Then
                    result = True
                    Exit For
                End If
            Next
        End If

        ' Check if inherited abstract operations are implemented
        If result = False Then
            ' TODO : take into abstract operations from grand parents
            Dim parent As Internal_Design_Class
            parent = CType(Me.Get_Element_By_Uuid(Me.Base_Class_Ref), Internal_Design_Class)
            If Not IsNothing(parent) Then
                Dim are_all_parent_op_realized As Boolean = True
                For Each parent_op In parent.Public_Operations
                    Dim is_parent_op_realized As Boolean = False
                    If parent_op.Is_Abstract = True Then
                        For Each op In Me.Public_Operations
                            If op.Name = parent_op.Name Then
                                is_parent_op_realized = True
                                Exit For
                            End If
                        Next
                        If is_parent_op_realized = False Then
                            are_all_parent_op_realized = False
                            Exit For
                        End If
                    End If
                Next
                If are_all_parent_op_realized = False Then
                    result = True
                End If
            End If
        End If

        Return result
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
            ElseIf Is_Event_Reception(rpy_elmt) Then
                Dim ope As Design_Class_Event_Reception = New Design_Class_Event_Reception
                Me.Event_Receptions.Add(ope)
                ope.Import_From_Rhapsody_Model(Me, rpy_elmt)
            End If
        Next

        For Each rpy_elmt In CType(Me.Rpy_Element, RPClass).relations
            If rpy_elmt.metaClass = "AssociationEnd" Then
                Dim class_assoc As New Classes_Association
                Me.Associations.Add(class_assoc)
                class_assoc.Import_From_Rhapsody_Model(Me, rpy_elmt)
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

    Public Overrides Function Is_Exportable(any_rpy_elmt As RPModelElement) As Boolean
        If Not MyBase.Is_Exportable(any_rpy_elmt) Then
            Return False
        Else
            Dim result As Boolean = True
            Dim assoc_rpy_class As RPClass
            Dim assoc_rpy_class_guid As String
            Dim rpy_proj As RPProject = CType(any_rpy_elmt.project, RPProject)
            For Each class_assoc In Me.Associations
                assoc_rpy_class_guid = Transform_Guid_To_Rpy_GUID(class_assoc.Associated_Class_Ref)
                assoc_rpy_class = CType(rpy_proj.findElementByGUID(assoc_rpy_class_guid), RPClass)
                If IsNothing(assoc_rpy_class) Then
                    result = False
                    Exit For
                End If
            Next
            Return result
        End If
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for consistency check model
    Protected Overrides Sub Check_Children_Name_Against_Inherited(report As Report)

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Overrides Function Find_Needed_Elements() As List(Of SMM_Classifier)
        If IsNothing(Me.Needed_Elements) Then
            Me.Needed_Elements = MyBase.Find_Needed_Elements()
            For Each conf In Me.Configurations
                Dim data_type As Data_Type
                data_type = CType(Me.Get_Element_By_Uuid(conf.Base_Data_Type_Ref), Data_Type)
                If Not IsNothing(data_type) Then
                    If Not Me.Needed_Elements.Contains(data_type) Then
                        Me.Needed_Elements.Add(data_type)
                    End If
                End If
            Next
            For Each current_ope In Me.Public_Operations
                If Not Me.Is_CS_Operation_Realization(current_ope) Then
                    For Each arg In current_ope.Arguments
                        Dim data_type As Data_Type
                        data_type = CType(Me.Get_Element_By_Uuid(arg.Base_Data_Type_Ref), Data_Type)
                        If Not IsNothing(data_type) Then
                            If Not Me.Needed_Elements.Contains(data_type) Then
                                Me.Needed_Elements.Add(data_type)
                            End If
                        End If
                    Next
                End If
            Next
            For Each cs_if_uuid In Me.Realized_Interfaces
                Dim cs_if As Client_Server_Interface
                cs_if = CType(Me.Get_Element_By_Uuid(cs_if_uuid), Client_Server_Interface)
                If Not IsNothing(cs_if) Then
                    If Not Me.Needed_Elements.Contains(cs_if) Then
                        Me.Needed_Elements.Add(cs_if)
                    End If
                End If
            Next
            For Each cs_if_uuid In Me.Needed_Interfaces
                Dim cs_if As Client_Server_Interface
                cs_if = CType(Me.Get_Element_By_Uuid(cs_if_uuid), Client_Server_Interface)
                If Not IsNothing(cs_if) Then
                    If Not Me.Needed_Elements.Contains(cs_if) Then
                        Me.Needed_Elements.Add(cs_if)
                    End If
                End If
            Next
            For Each assoc In Me.Associations
                Dim assoc_class As Internal_Design_Class
                assoc_class = CType(Me.Get_Element_By_Uuid(assoc.Associated_Class_Ref), 
                    Internal_Design_Class)
                If Not IsNothing(assoc_class) Then
                    If Not Me.Needed_Elements.Contains(assoc_class) Then
                        Me.Needed_Elements.Add(assoc_class)
                    End If
                End If
            Next
        End If
        Return Me.Needed_Elements
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Transform_To_CLOOF(parent_folder_path As String)
        If Me.Is_Abstract = False And Me.Is_Specialization = False Then
            Me.Create_CLOOF_Header_File(parent_folder_path)
            Me.Create_CLOOF_Source_File(parent_folder_path)
        End If
    End Sub

    Private Sub Create_CLOOF_Header_File(parent_folder_path As String)
        Dim file_stream As StreamWriter = Me.Create_C_Header_File_Stream_Writer(parent_folder_path)
        Me.Add_C_Multiple_Inclusion_Guard(file_stream)
        Me.Add_CLOOF_Inclusion_Directives(file_stream)
        Me.Add_C_Title(file_stream)

        If Me.Attributes.Count > 0 Then
            file_stream.WriteLine("typedef struct {")
            For Each attr In Me.Attributes
                Dim attr_dt As Data_Type
                attr_dt = CType(Me.Get_Element_By_Uuid(attr.Base_Data_Type_Ref), Data_Type)
                file_stream.WriteLine("    " & _
                    attr_dt.Get_CLOOF_Arg_Type_Declaration(Operation_Argument.E_STREAM.INPUT) & _
                    " " & attr.Name & ";")
            Next
            file_stream.WriteLine("} " & Me.Name & "_Var;")
            file_stream.WriteLine()
        End If

        file_stream.WriteLine("typedef struct {")
        file_stream.WriteLine()

        file_stream.WriteLine("    /* Variable attributes */")
        If Me.Attributes.Count > 0 Then
            file_stream.WriteLine("    " & Me.Name & "_Var* var_attr;")
        End If
        file_stream.WriteLine()

        file_stream.WriteLine("    /* Sent events */")
        For Each ev_uuid In Me.Sent_Events
            Dim ev_if As Event_Interface
            ev_if = CType(Me.Get_Element_By_Uuid(ev_uuid), Event_Interface)
            file_stream.WriteLine("    " & ev_if.Name & " " & ev_if.Name & ";")
        Next
        file_stream.WriteLine()

        file_stream.WriteLine("    /* Associated objects */")
        For Each assoc In Me.Associations
            Dim assoc_class As Internal_Design_Class
            assoc_class = CType(Me.Get_Element_By_Uuid(assoc.Associated_Class_Ref), 
                Internal_Design_Class)
            file_stream.WriteLine("    const " & assoc_class.Name & "* " & assoc.Name & ";")
        Next
        file_stream.WriteLine()

        file_stream.WriteLine("    /* Configurations parameters */")
        For Each conf In Me.Configurations
            Dim conf_dt As Data_Type
            conf_dt = CType(Me.Get_Element_By_Uuid(conf.Base_Data_Type_Ref), Data_Type)
            file_stream.WriteLine("    " & _
                conf_dt.Get_CLOOF_Arg_Type_Declaration(Operation_Argument.E_STREAM.INPUT) & _
                " " & conf.Name & ";")
        Next
        file_stream.WriteLine()

        file_stream.WriteLine("} " & Me.Name & ";")
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Public methods")
        For Each op In Me.Public_Operations
            op.Create_CLOOF_Declaration(file_stream, Me.Name)
        Next
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Received events")
        For Each ev In Me.Event_Receptions
            ev.Create_CLOOF_Declaration(file_stream, Me.Name)
        Next
        file_stream.WriteLine()

        file_stream.WriteLine()
        Me.Finish_C_Multiple_Inclusion_Guard(file_stream)
        file_stream.Close()
    End Sub

    Private Sub Create_CLOOF_Source_File(parent_folder_path As String)
        Dim file_stream As StreamWriter
        file_stream = Me.Create_C_Source_File_Stream_Writer(parent_folder_path)

        file_stream.WriteLine("#include """ & Me.Name & ".h""")
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Attributes access")
        For Each attr In Me.Attributes
            file_stream.WriteLine("#define My_" & attr.Name & " (Me->var_attr->" & attr.Name & ")")
        Next
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Private methods declaration")
        For Each meth In Me.Private_Operations
            meth.Create_CLOOF_Declaration(file_stream, Me.Name)
        Next
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Public methods")
        For Each meth In Me.Public_Operations
            meth.Create_CLOOF_Definition(file_stream, Me.Name)
            If Not meth Is Me.Public_Operations.Last Then
                SMM_Classifier.Add_C_Separator(file_stream)
            End If
        Next
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Received events")
        For Each meth In Me.Event_Receptions
            meth.Create_CLOOF_Definition(file_stream, Me.Name)
            If Not meth Is Me.Event_Receptions.Last Then
                SMM_Classifier.Add_C_Separator(file_stream)
            End If
        Next
        file_stream.WriteLine()

        Add_C_Title(file_stream, "Private methods definition")
        For Each meth In Me.Private_Operations
            meth.Create_CLOOF_Definition(file_stream, Me.Name)
            If Not meth Is Me.Private_Operations.Last Then
                SMM_Classifier.Add_C_Separator(file_stream)
            End If
        Next
        file_stream.WriteLine()

        file_stream.Close()
    End Sub

End Class


Public MustInherit Class Public_Method
    Inherits Operation_With_Arguments

    Public Is_Abstract As Boolean
    Public Is_Virtual As Boolean

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_op As RPOperation = CType(Me.Rpy_Element, RPOperation)
        If rpy_op.isAbstract > 0 Then
            Me.Is_Abstract = True
        Else
            Me.Is_Abstract = False
            If rpy_op.isVirtual > 0 Then
                Me.Is_Virtual = True
            Else
                Me.Is_Virtual = False

            End If
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
        If rpy_op.isVirtual > 0 Then
            If Me.Is_Virtual = False Then
                rpy_op.isVirtual = 0
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Is_Virtual merged : True -> False.")
            End If
        Else
            If Me.Is_Virtual = True Then
                rpy_op.isVirtual = 1
                Me.Add_Export_Information_Item(report,
                    Merge_Report_Item.E_Merge_Status.ELEMENT_ATTRIBUTE_MERGED,
                    "Is_Virtual merged : False -> True.")
            End If
        End If
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


Public Class Public_Operation

    Inherits Public_Method


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Public_Operation", "Operation")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Create_CLOOF_Prototype(file_stream As StreamWriter, class_id As String)
        Dim is_getter As Boolean = False
        If Me.Arguments.Count = 1 Then
            If Me.Arguments(0).Stream = Operation_Argument.E_STREAM.OUTPUT Then
                is_getter = True
            End If
        End If
        Dim ref_to_me As String = "const " & class_id & "* Me"
        Dim arg_dt As Data_Type
        If is_getter = True Then
            arg_dt = CType(Me.Get_Element_By_Uuid(Me.Arguments(0).Base_Data_Type_Ref), Data_Type)
            file_stream.Write(
                arg_dt.Get_CLOOF_Arg_Type_Declaration(Operation_Argument.E_STREAM.INPUT) & " " & _
                class_id & "__" & Me.Name & "( " & ref_to_me & " )")
        Else
            file_stream.Write("void " & class_id & "__" & Me.Name & "( " & ref_to_me)
            If Me.Arguments.Count = 0 Then
                file_stream.Write(" )")
            Else
                file_stream.WriteLine(",")
                Dim is_last As Boolean = False
                For Each arg In Me.Arguments
                    If arg Is Me.Arguments.Last Then
                        is_last = True
                    End If
                    arg.Transform_To_CLOOF(file_stream, is_last, 1)
                Next
                file_stream.Write(" )")
            End If
        End If
    End Sub

End Class


Public Class Design_Class_Event_Reception
    Inherits Public_Method


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Sub Set_Stereotype()
        Me.Rpy_Element.addStereotype("Event_Reception", "Operation")
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for transformation
    Public Overrides Sub Create_CLOOF_Prototype(file_stream As StreamWriter, class_id As String)
        Dim ref_to_me As String = "const " & class_id & "* Me"
        file_stream.Write("void " & class_id & "__" & Me.Name & "( " & ref_to_me)
        If Me.Arguments.Count = 0 Then
            file_stream.Write(" )")
        Else
            file_stream.WriteLine(",")
            Dim is_last As Boolean = False
            For Each arg In Me.Arguments
                If arg Is Me.Arguments.Last Then
                    is_last = True
                End If
                arg.Transform_To_CLOOF(file_stream, is_last, 1)
            Next
            file_stream.Write(" )")
        End If
    End Sub

End Class


Public Class Classes_Association
    Inherits Software_Element

    Public Associated_Class_Ref As Guid
    Private Rpy_Associated_Class As RPModelElement = Nothing

    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()
        Dim rpy_rel As RPRelation = CType(Me.Rpy_Element, RPRelation)
        Me.Associated_Class_Ref = Transform_Rpy_GUID_To_Guid(rpy_rel.otherClass.GUID)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        Dim rpy_rel As RPRelation = Nothing
        Dim rpy_parent_class As RPClass = CType(rpy_parent, RPClass)

        ' Dirty trick to be able to call Find_In_Rpy_Project before really assigning Rpy_Element
        Me.Rpy_Element = rpy_parent

        Me.Rpy_Associated_Class = Me.Find_In_Rpy_Project(Me.Associated_Class_Ref)

        If Not IsNothing(Me.Rpy_Associated_Class) Then
            rpy_rel = rpy_parent_class.addUnidirectionalRelation(
                Me.Rpy_Associated_Class.name,
                Me.Rpy_Associated_Class.owner.name,
                Me.Name, "Association", "1", "")
        End If
        Return CType(rpy_rel, RPModelElement)
    End Function

    Protected Overrides Sub Set_Rpy_Element_Attributes(rpy_elmt As RPModelElement, report As Report)
        If Not IsNothing(rpy_elmt) Then
            MyBase.Set_Rpy_Element_Attributes(rpy_elmt, report)
        Else
            If IsNothing(Me.Rpy_Associated_Class) Then
                Me.Add_Export_Error_Item(report,
                    Merge_Report_Item.E_Merge_Status.MISSING_REFERENCED_ELEMENTS,
                    "Internal_Design_Class not found : " & Me.Associated_Class_Ref.ToString & ".")
            End If
        End If
    End Sub

    Protected Overrides Function Get_Rpy_Metaclass() As String
        Return "Association"
    End Function

    Protected Overrides Sub Set_Stereotype()
        ' No stereotype
    End Sub

End Class