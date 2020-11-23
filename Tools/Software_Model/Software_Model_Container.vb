Imports rhapsody2
Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text
Imports Software_Model.Basic_Integer_Type



Public Class Software_Model_Container

    Inherits Software_Element

    <XmlArrayItem("Package")>
    Public Packages As New List(Of Top_Level_Package)

    Private Elements_Dictionary_By_Uuid As New Dictionary(Of Guid, Software_Element)

    Private Consistency_Report As Consistency_Check_Report
    Private Import_Report As Report

    Private Shared Basic_Integer_Type_Data As Object(,) = New Object(,) {
        {"sint8", "e6f0bcaa-8b2b-43b6-b1e5-553b4a3f74d0", 1, E_Signedness_Type.SIGNED},
        {"sint16", "ebb36a0f-a588-4dc2-b4ff-03b59b7f0451", 2, E_Signedness_Type.SIGNED},
        {"sint32", "874ecc76-8567-4d26-babc-0a38a4c8e1df", 4, E_Signedness_Type.SIGNED},
        {"sint64", "417d1937-91ff-4bc6-ad20-46b03d49d6b5", 8, E_Signedness_Type.SIGNED},
        {"uint8", "058963e7-375f-4f57-aceb-5a2f36b75490", 1, E_Signedness_Type.UNSIGNED},
        {"uint16", "0d6a9487-2e1a-4d71-8dcb-52824fa8170d", 2, E_Signedness_Type.UNSIGNED},
        {"uint32", "3f1b684a-51d5-4374-a479-56bd195faa9e", 4, E_Signedness_Type.UNSIGNED},
        {"uint64", "f49b3ace-96bc-463c-8444-5436f4a801f1", 8, E_Signedness_Type.UNSIGNED}}

    Private Shared Basic_Floating_Type_Data As Object(,) = New Object(,) {
        {"fp32", "1045feea-03f6-4690-a89c-33134ec24f54"},
        {"fp64", "d74c7bfa-9e57-443f-ab99-96ab3cdcce0b"}}

    Private Data_Types_List As List(Of Data_Type) = Nothing
    Private Interfaces_List As List(Of Software_Interface) = Nothing
    Private Component_Types_List As List(Of Component_Type) = Nothing
    Private Compositions_List As List(Of Root_Software_Composition) = Nothing
    Private Software_Classes_List As List(Of Internal_Design_Class) = Nothing
    Private Specializable_Class_List As List(Of SMM_Class) = Nothing

    Private Nb_Interfaces_Series As Data_Series
    Private Nb_Component_Types_Series As Data_Series
    Private Nb_Data_Types_Series As Data_Series

    Private Documentation_Rate_Series As Data_Series
    Private Distance_Series As Data_Series
    Private Component_Type_WMC_Series As Data_Series
    Private Interfaces_WMC_Series As Data_Series

    Private Cyclic_Dep_Mgr As Cyclic_Dependencies_Manager = Nothing

    '----------------------------------------------------------------------------------------------'
    ' General methods 
    Public Sub Add_Element(software_element As Software_Element)
        Elements_Dictionary_By_Uuid.Add(software_element.UUID, software_element)
    End Sub

    Public Function Get_Element(element_uuid As Guid) As Software_Element
        If Me.Elements_Dictionary_By_Uuid.ContainsKey(element_uuid) = True Then
            Return Me.Elements_Dictionary_By_Uuid.Item(element_uuid)
        Else
            Return Nothing
        End If
    End Function

    Public Overrides Function Get_Children() As List(Of Software_Element)
        If IsNothing(Me.Children) Then
            Dim children_list As New List(Of Software_Element)
            children_list.AddRange(Me.Packages)
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Public Function Get_All_Compositions() As List(Of Root_Software_Composition)
        If IsNothing(Me.Compositions_List) Then
            Me.Compositions_List = New List(Of Root_Software_Composition)
            For Each top_pkg In Me.Packages
                Dim all_pkg_list As List(Of Software_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    Me.Compositions_List.AddRange(pkg.Root_Software_Compositions)
                Next
            Next
        End If
        Return Me.Compositions_List
    End Function

    Public Function Get_All_Component_Types() As List(Of Component_Type)
        If IsNothing(Me.Component_Types_List) Then
            Me.Component_Types_List = New List(Of Component_Type)
            For Each top_pkg In Me.Packages
                Dim all_pkg_list As List(Of Software_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    Me.Component_Types_List.AddRange(pkg.Component_Types)
                Next
            Next
        End If
        Return Me.Component_Types_List
    End Function

    Public Function Get_All_Interfaces() As List(Of Software_Interface)
        If IsNothing(Me.Interfaces_List) Then
            Me.Interfaces_List = New List(Of Software_Interface)
            For Each top_pkg In Me.Packages
                Dim all_pkg_list As List(Of Software_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    Me.Interfaces_List.AddRange(pkg.Software_Interfaces)
                Next
            Next
        End If
        Return Me.Interfaces_List
    End Function

    Public Function Get_All_Data_Types() As List(Of Data_Type)
        If IsNothing(Me.Data_Types_List) Then
            Me.Data_Types_List = New List(Of Data_Type)
            For Each top_pkg In Me.Packages
                Dim all_pkg_list As List(Of Software_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    Me.Data_Types_List.AddRange(pkg.Data_Types)
                Next
            Next
        End If
        Return Me.Data_Types_List
    End Function

    Public Function Get_All_Software_Classes() As List(Of Internal_Design_Class)
        If IsNothing(Me.Software_Classes_List) Then
            Me.Software_Classes_List = New List(Of Internal_Design_Class)
            For Each top_pkg In Me.Packages
                Dim all_pkg_list As List(Of Software_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    Me.Software_Classes_List.AddRange(pkg.Classes)
                Next
            Next
        End If
        Return Me.Software_Classes_List
    End Function

    Public Function Get_All_Specializable_Class() As List(Of SMM_Class)
        If IsNothing(Me.Specializable_Class_List) Then
            Me.Specializable_Class_List = New List(Of SMM_Class)
            For Each top_pkg In Me.Packages
                Dim all_pkg_list As List(Of Software_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    Me.Specializable_Class_List.AddRange(pkg.Component_Types)
                    Me.Specializable_Class_List.AddRange(pkg.Software_Interfaces)
                    Me.Specializable_Class_List.AddRange(pkg.Classes)
                Next
            Next
        End If
        Return Me.Specializable_Class_List
    End Function

    Public Sub Create_Xml(xml_file_stream As FileStream)

        ' Initialize XML writer
        Dim writer As XmlTextWriter
        writer = New XmlTextWriter(xml_file_stream, Encoding.UTF8)
        writer.Indentation = 2
        writer.IndentChar = CChar(" ")
        writer.Formatting = Formatting.Indented

        ' Serialize model
        Dim serializer As New XmlSerializer(GetType(Software_Model_Container))
        serializer.Serialize(writer, Me)

        ' Close writter
        writer.Close()

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Public Sub Import_All_From_Rhapsody_Model(rpy_proj As RPProject)

        Me.Rpy_Element = CType(rpy_proj, RPModelElement)
        Me.Container = Me

        Me.Get_Own_Data_From_Rhapsody_Model()

        Me.Import_Children_From_Rhapsody_Model()

    End Sub

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Create "virtual" Package named "Basic_Types"
        Dim basic_types_pkg As New Top_Level_Package
        basic_types_pkg.Name = "Basic_Types"
        basic_types_pkg.Description = "Automatically created Package to gathers Basic_Types."
        basic_types_pkg.UUID = Guid.NewGuid
        basic_types_pkg.Data_Types = New List(Of Data_Type)
        ' !!! IMPORTANT NOTE !!! '
        ' basic_types_pkg is not added to Me.Packages nor to Elements_Dictionary_By_Uuid but
        ' all its aggregated Data_Types are added to Elements_Dictionary_By_Uuid.
        ' It allows to make Basic_Types available for other methods.

        ' Add Basic_Types
        Dim type As Basic_Type
        Dim rpy_type As RPModelElement
        ' Treat Basic_Integer_Types
        Dim nb_int_types As Integer = Basic_Integer_Type_Data.GetLength(0)
        For type_idx = 0 To nb_int_types - 1
            rpy_type = Me.Find_In_Rpy_Project("GUID " & CStr(Basic_Integer_Type_Data(type_idx, 1)))
            type = New Basic_Integer_Type(
                CStr(Basic_Integer_Type_Data(type_idx, 0)),
                CStr(Basic_Integer_Type_Data(type_idx, 1)),
                rpy_type,
                CInt(Basic_Integer_Type_Data(type_idx, 2)),
                CType(Basic_Integer_Type_Data(type_idx, 3), E_Signedness_Type))
            basic_types_pkg.Data_Types.Add(type)
            Me.Add_Element(type)
        Next
        ' Treat Basic_Floating_Point_Types
        Dim nb_fp_types As Integer = Basic_Floating_Type_Data.GetLength(0)
        For type_idx = 0 To nb_fp_types - 1
            rpy_type = Me.Find_In_Rpy_Project("GUID " & CStr(Basic_Floating_Type_Data(type_idx, 1)))
            type = New Basic_Floating_Point_Type(
                CStr(Basic_Floating_Type_Data(type_idx, 0)),
                CStr(Basic_Floating_Type_Data(type_idx, 1)),
                rpy_type)
            basic_types_pkg.Data_Types.Add(type)
            Me.Add_Element(type)
        Next
        ' Treat Basic_Boolean_Type
        rpy_type = Me.Find_In_Rpy_Project("GUID 5df8e979-be4c-4790-87a7-f8ee053c4162")
        type = New Basic_Boolean_Type("boolean", "5df8e979-be4c-4790-87a7-f8ee053c4162", rpy_type)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat Basic_Integer_Array_Types
        rpy_type = Me.Find_In_Rpy_Project("GUID b86a2bc4-2c3f-4cff-9217-4a07af95fdd2")
        Dim base_type_uuid As Guid
        Guid.TryParse("058963e7-375f-4f57-aceb-5a2f36b75490", base_type_uuid)
        Dim base_type As Software_Element
        base_type = Me.Get_Element_By_Uuid(base_type_uuid)
        type = New Basic_Integer_Array_Type(
            "uint8_array",
            "b86a2bc4-2c3f-4cff-9217-4a07af95fdd2",
            rpy_type,
            CType(base_type, Basic_Integer_Type))
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat character
        rpy_type = Me.Find_In_Rpy_Project("GUID 0b72335d-1ae5-4182-a916-c731838ed0b7")
        type = New Basic_Character_Type(
            "character", "0b72335d-1ae5-4182-a916-c731838ed0b7", rpy_type, 1)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat characters_string
        rpy_type = Me.Find_In_Rpy_Project("GUID 9b2c2f9e-c662-4494-a932-00581b21d3bb")
        type = New Basic_Character_Type(
            "characters_string", "9b2c2f9e-c662-4494-a932-00581b21d3bb", rpy_type, 0)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)

    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()
        Dim rpy_pkg As RPPackage
        For Each rpy_pkg In CType(Me.Rpy_Element, RPProject).packages
            If Is_Software_Package(CType(rpy_pkg, RPModelElement)) Then
                Dim pkg As Top_Level_Package = New Top_Level_Package
                Me.Packages.Add(pkg)
                pkg.Import_From_Rhapsody_Model(Me, CType(rpy_pkg, RPModelElement))
            End If
        Next
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for models merge
    Protected Overrides Function Create_Rpy_Element(rpy_parent As RPModelElement) As RPModelElement
        ' Not used, Software_Model_Container is a special Software_Element
        Return Nothing
    End Function

    Protected Overrides Function Get_Rpy_Metaclass() As String
        ' Not used, Software_Model_Container is a special Software_Element
        Return "Project"
    End Function

    Protected Overrides Sub Set_Stereotype()
        ' No stereotype for Software_Model_Container
    End Sub

    Public Overloads Sub Export_To_Rhapsody(rpy_sw_mdl As RPProject)
        Dim attr_list As New List(Of String) From
            {"Path", "Kind", "Criticality", "Merge status", "Message"}
        Me.Import_Report = New Report(attr_list)
        Me.Rpy_Element = CType(rpy_sw_mdl, RPModelElement)

        ' Export packages
        For Each pkg_to_export In Me.Packages
            pkg_to_export.Export_To_Rhapsody(Me.Rpy_Element, Me.Import_Report)
        Next

        ' Export independent Data_Types
        For Each pkg In Me.Packages
            pkg.Export_Independent_Data_Types_To_Rhapsody(Me.Import_Report)
        Next

        ' Export dependent Data_Types
        ' Create list of all "dependent" Data_Types
        Dim all_dt_list As List(Of Data_Type) = Me.Get_All_Data_Types
        Dim dt_list As New List(Of Data_Type)
        For Each dt In all_dt_list
            Select Case dt.GetType
                Case GetType(Array_Data_Type)
                    dt_list.Add(dt)
                Case GetType(Structured_Data_Type)
                    dt_list.Add(dt)
            End Select
        Next
        ' While the list is not empty, export Data_Types
        Dim exported_dt_list As New List(Of Data_Type)
        Dim round_counter As Integer = 0
        Dim force_export As Boolean = False
        While dt_list.Count <> 0
            For Each pkg In Me.Packages
                pkg.Export_Dependent_Data_Types_To_Rhapsody(
                    exported_dt_list,
                    Me.Import_Report,
                    force_export)
            Next
            For Each exp_dt In exported_dt_list
                dt_list.Remove(exp_dt)
            Next
            exported_dt_list.Clear()
            round_counter += 1
            If round_counter >= 5 Then
                force_export = True
            End If
        End While

        ' Export Interfaces
        Dim if_list As New List(Of Software_Interface)
        if_list = Me.Get_All_Interfaces
        Dim exported_if_list As New List(Of Software_Interface)
        round_counter = 0
        force_export = False
        While if_list.Count <> 0
            For Each pkg In Me.Packages
                pkg.Export_Interfaces_To_Rhapsody(
                    exported_if_list,
                    Me.Import_Report,
                    force_export)
            Next
            For Each exp_if In exported_if_list
                if_list.Remove(exp_if)
            Next
            exported_if_list.Clear()
            round_counter += 1
            If round_counter >= 5 Then
                force_export = True
            End If
        End While

        ' Export Component_Types
        Dim swct_list As New List(Of Component_Type)
        swct_list = Me.Get_All_Component_Types()
        Dim exported_swct_list As New List(Of Component_Type)
        round_counter = 0
        force_export = False
        While swct_list.Count <> 0
            For Each pkg In Me.Packages
                pkg.Export_Component_Types_To_Rhapsody(
                    exported_swct_list,
                    Me.Import_Report,
                    force_export)
            Next
            For Each exp_swct In exported_swct_list
                swct_list.Remove(exp_swct)
            Next
            exported_swct_list.Clear()
            round_counter += 1
            If round_counter >= 5 Then
                force_export = True
            End If
        End While

        ' Export Compositions
        For Each pkg In Me.Packages
            pkg.Export_Compositions_To_Rhapsody(Me.Import_Report)
        Next

        ' Export Class
        Dim sdd_classes_list As New List(Of Internal_Design_Class)
        sdd_classes_list = Me.Get_All_Software_Classes()
        Dim exported_sdd_classes_list As New List(Of Internal_Design_Class)
        round_counter = 0
        force_export = False
        While sdd_classes_list.Count <> 0
            For Each pkg In Me.Packages
                pkg.Export_Classes_To_Rhapsody(
                    exported_sdd_classes_list,
                    Me.Import_Report,
                    force_export)
            Next
            For Each exp_class In exported_sdd_classes_list
                sdd_classes_list.Remove(exp_class)
            Next
            exported_sdd_classes_list.Clear()
            round_counter += 1
            If round_counter >= 5 Then
                force_export = True
            End If
        End While

        ' Export Component_Designs
        For Each pkg In Me.Packages
            pkg.Export_Component_Design_To_Rhapsody(Me.Import_Report)
        Next

        ' Export Implementation_Files
        For Each pkg In Me.Packages
            pkg.Export_Files_To_Rhapsody(Me.Import_Report)
        Next

    End Sub

    Public Sub Generate_Importation_Report(report_file_stream As StreamWriter)
        Me.Import_Report.Generate_Csv_Report(report_file_stream)
    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for model check
    Public Overloads Sub Check_Consistency()
        Me.Consistency_Report = New Consistency_Check_Report()
        Me.Check_Consistency(Me.Consistency_Report)
    End Sub

    Public Overloads Sub Check_Consistency(pkg_name_list As List(Of String))
        Me.Consistency_Report = New Consistency_Check_Report()

        Me.Check_Own_Consistency(Me.Consistency_Report)

        ' Check cyclic dependencies
        Dim pkg_dep_cycle As New List(Of Dependent_Element)
        Dim list_of_pkg As New List(Of Dependent_Element)
        For Each pkg In Me.Packages
            pkg.Find_Needed_Elements()
            list_of_pkg.Add(pkg)
        Next
        pkg_dep_cycle = Cyclic_Dependencies_Manager.Find_First_Cyclic_Dependency(list_of_pkg)
        If pkg_dep_cycle.Count > 0 Then
            Me.Add_Consistency_Check_Warning_Item(Me.Consistency_Report, "PROJ_1",
                "Packages involved in at least one dependency cycle : " & _
                Cyclic_Dependencies_Manager.Transform_Cycle_To_String(pkg_dep_cycle) & ".")
        End If

        ' Check packages
        For Each pkg In Me.Packages
            If pkg_name_list.Contains(pkg.Name) Then
                pkg.Check_Consistency(Me.Consistency_Report)
            Else
                pkg.Add_Consistency_Check_Warning_Item(Me.Consistency_Report,
                "-",
                "Not checked.")
            End If
        Next

    End Sub

    Public Sub Generate_Consistency_Report(report_file_stream As StreamWriter)
        Me.Consistency_Report.Generate_Csv_Report(report_file_stream)
    End Sub

    Public Function Has_Error() As Boolean
        If Me.Consistency_Report.Get_Error_Number > 0 Then
            Return True
        Else
            Return False
        End If
    End Function

    Public Sub Merge_Report_Analysis(prev_report As Consistency_Check_Report)
        For Each prev_item In prev_report.Get_Items
            Dim prev_analysis As String
            prev_analysis = CType(prev_item, Consistency_Check_Report_Item).Get_Analysis()
            If prev_analysis <> "" Then
                Dim prev_path = CType(prev_item, Consistency_Check_Report_Item).Get_Path()
                For Each item In Me.Consistency_Report.Get_Items
                    Dim current_path As String
                    current_path = CType(item, Consistency_Check_Report_Item).Get_Path()
                    If current_path = prev_path Then
                        Dim prev_message = prev_item.Get_Message()
                        Dim current_message As String
                        current_message = item.Get_Message()
                        If prev_message = current_message Then
                            CType(item, Consistency_Check_Report_Item).Set_Analysis(prev_analysis)
                            Exit For
                        End If
                    End If
                Next
            End If
        Next
    End Sub

    Public Function Find_Cyclic_Dependencies() As List(Of String)
        Me.Cyclic_Dep_Mgr = New Cyclic_Dependencies_Manager()
        Dim list_of_pkg As New List(Of Dependent_Element)
        For Each pkg In Me.Packages
            pkg.Find_Needed_Elements()
            list_of_pkg.Add(pkg)
        Next
        Me.Cyclic_Dep_Mgr.Find_Cyclic_Dependencies(list_of_pkg)
        Return Me.Cyclic_Dep_Mgr.Get_Cycles_List_String
    End Function

    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Sub Compute_PSWA_Metrics()
        Me.Nb_Interfaces_Series = New Data_Series
        Me.Nb_Component_Types_Series = New Data_Series
        Me.Nb_Data_Types_Series = New Data_Series

        Me.Documentation_Rate_Series = New Data_Series
        Me.Distance_Series = New Data_Series
        Me.Component_Type_WMC_Series = New Data_Series
        Me.Interfaces_WMC_Series = New Data_Series

        For Each pkg In Me.Packages
            Me.Documentation_Rate_Series.Add_Value(pkg.Get_Package_Documentation_Rate())

            pkg.Compute_Nb_Classifiers()
            Me.Nb_Interfaces_Series.Add_Value(pkg.Get_Nb_Interfaces)
            Me.Nb_Component_Types_Series.Add_Value(pkg.Get_Nb_Component_Types)
            Me.Nb_Data_Types_Series.Add_Value(pkg.Get_Nb_Data_Types)

            pkg.Find_Needed_Elements()
            pkg.Find_Dependent_Elements()
            pkg.Compute_Coupling()
            Me.Distance_Series.Add_Value(pkg.Get_Distance)
        Next

        For Each swct In Me.Get_All_Component_Types
            Me.Component_Type_WMC_Series.Add_Value(swct.Compute_WMC)
        Next

        For Each sw_if In Me.Get_All_Interfaces
            Me.Interfaces_WMC_Series.Add_Value(sw_if.Compute_WMC)
        Next

    End Sub

    Public Sub Generate_PSWA_Metrics_Report(file_path As String)
        Dim report_generator As New Metrics_Report_Generator
        report_generator.Generate_PSWA_Metrics_Report(
            file_path,
            Me.Packages,
            Me.Get_All_Interfaces,
            Me.Get_All_Component_Types,
            Me.Documentation_Rate_Series,
            Me.Nb_Data_Types_Series,
            Me.Nb_Interfaces_Series,
            Me.Nb_Component_Types_Series,
            Me.Distance_Series,
            Me.Component_Type_WMC_Series,
            Me.Interfaces_WMC_Series)
    End Sub

End Class
