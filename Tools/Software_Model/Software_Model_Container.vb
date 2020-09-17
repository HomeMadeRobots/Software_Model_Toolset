Imports rhapsody2
Imports System.IO
Imports System.Xml
Imports System.Xml.Serialization
Imports System.Text

Public Class Software_Model_Container

    Inherits Software_Element

    <XmlArrayItem("Package")>
    Public Packages As List(Of Top_Level_Package)

    Private Elements_Dictionary_By_Uuid As New Dictionary(Of Guid, Software_Element)

    Private Consistency_Report As Consistency_Check_Report
    Private Import_Report As Report

    Private Shared Basic_Integer_Type_Name_List() As String =
        {"sint8", "sint16", "sint32", "sint64", "uint8", "uint16", "uint32", "uint64"}
    Private Shared Basic_Integer_Type_Uuid_List() As String = {
        "e6f0bcaa-8b2b-43b6-b1e5-553b4a3f74d0",
        "ebb36a0f-a588-4dc2-b4ff-03b59b7f0451",
        "874ecc76-8567-4d26-babc-0a38a4c8e1df",
        "417d1937-91ff-4bc6-ad20-46b03d49d6b5",
        "058963e7-375f-4f57-aceb-5a2f36b75490",
        "0d6a9487-2e1a-4d71-8dcb-52824fa8170d",
        "3f1b684a-51d5-4374-a479-56bd195faa9e",
        "f49b3ace-96bc-463c-8444-5436f4a801f1"}

    Private Shared Basic_Floating_Type_Name_List() As String = {"fp32", "fp64"}
    Private Shared Basic_Floating_Type_Uuid_List() As String = {
        "1045feea-03f6-4690-a89c-33134ec24f54",
        "d74c7bfa-9e57-443f-ab99-96ab3cdcce0b"}

    Private Data_Types_List As List(Of Data_Type) = Nothing
    Private Interfaces_List As List(Of Software_Interface) = Nothing
    Private Component_Types_List As List(Of Component_Type) = Nothing
    Private Compositions_List As List(Of Root_Software_Composition) = Nothing
    Private Software_Classes_List As List(Of Internal_Design_Class) = Nothing

    Private Nb_Interfaces_Series As Data_Series
    Private Nb_Component_Types_Series As Data_Series
    Private Nb_Data_Types_Series As Data_Series

    Private Documentation_Rate_Series As Data_Series
    Private Distance_Series As Data_Series
    Private Component_Type_WMC_Series As Data_Series
    Private Interfaces_WMC_Series As Data_Series

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
            If Not IsNothing(Me.Packages) Then
                children_list.AddRange(Me.Packages)
            End If
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
                    If Not IsNothing(pkg.Root_Software_Compositions) Then
                        Me.Compositions_List.AddRange(pkg.Root_Software_Compositions)
                    End If
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
                    If Not IsNothing(pkg.Component_Types) Then
                        Me.Component_Types_List.AddRange(pkg.Component_Types)
                    End If
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
                    If Not IsNothing(pkg.Software_Interfaces) Then
                        Me.Interfaces_List.AddRange(pkg.Software_Interfaces)
                    End If
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
                    If Not IsNothing(pkg.Data_Types) Then
                        Me.Data_Types_List.AddRange(pkg.Data_Types)
                    End If
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
                    If Not IsNothing(pkg.Classes) Then
                        Me.Software_Classes_List.AddRange(pkg.Classes)
                    End If
                Next
            Next
        End If
        Return Me.Software_Classes_List
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
        Dim basic_type_idx As Integer
        ' Treat Basic_Integer_Types
        Dim nb_int_types As Integer = Basic_Integer_Type_Name_List.Count
        For basic_type_idx = 0 To nb_int_types - 1
            type = New Basic_Integer_Type
            type.Name = Basic_Integer_Type_Name_List(basic_type_idx)
            type.Set_Top_Package(basic_types_pkg)
            Guid.TryParse(Basic_Integer_Type_Uuid_List(basic_type_idx), type.UUID)
            basic_types_pkg.Data_Types.Add(type)
            Me.Add_Element(type)
        Next
        ' Treat Basic_Floating_Point_Types
        Dim nb_fp_types As Integer = Basic_Floating_Type_Name_List.Count
        For basic_type_idx = 0 To nb_fp_types - 1
            type = New Basic_Floating_Point_Type
            type.Name = Basic_Floating_Type_Name_List(basic_type_idx)
            type.Set_Top_Package(basic_types_pkg)
            Guid.TryParse(Basic_Floating_Type_Uuid_List(basic_type_idx), type.UUID)
            basic_types_pkg.Data_Types.Add(type)
            Me.Add_Element(type)
        Next
        ' Treat Basic_Boolean_Type
        type = New Basic_Boolean_Type
        type.Name = "boolean"
        type.Set_Top_Package(basic_types_pkg)
        Guid.TryParse("5df8e979-be4c-4790-87a7-f8ee053c4162", type.UUID)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat Basic_Integer_Array_Types
        type = New Basic_Integer_Array_Type
        type.Name = "uint8_array"
        type.Set_Top_Package(basic_types_pkg)
        Guid.TryParse("b86a2bc4-2c3f-4cff-9217-4a07af95fdd2", type.UUID)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat character
        type = New Basic_Character_Type
        type.Name = "character"
        type.Set_Top_Package(basic_types_pkg)
        Guid.TryParse("0b72335d-1ae5-4182-a916-c731838ed0b7", type.UUID)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat characters_string
        type = New Basic_Character_Type
        type.Name = "characters_string"
        type.Set_Top_Package(basic_types_pkg)
        Guid.TryParse("9b2c2f9e-c662-4494-a932-00581b21d3bb", type.UUID)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)

    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.Packages = New List(Of Top_Level_Package)

        Dim rpy_pkg As RPPackage
        For Each rpy_pkg In CType(Me.Rpy_Element, RPPackage).packages
            If Is_PSWA_Package(CType(rpy_pkg, RPModelElement)) Then
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
        Me.Import_Report = New Report
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
            round_counter += 1
            If round_counter >= 5 Then
                force_export = True
            End If
        End While

        ' Export Interfaces
        For Each pkg In Me.Packages
            pkg.Export_Interfaces_To_Rhapsody(Me.Import_Report)
        Next

        ' Export Component_Types
        For Each pkg In Me.Packages
            pkg.Export_Component_Types_To_Rhapsody(Me.Import_Report)
        Next

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
        Me.Consistency_Report = New Consistency_Check_Report
        Me.Check_Consistency(Me.Consistency_Report)
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

        For Each swct In Me.Component_Types_List
            Me.Component_Type_WMC_Series.Add_Value(swct.Compute_WMC)
        Next

        If Not IsNothing(Me.Interfaces_List) Then
            For Each sw_if In Me.Interfaces_List
                Me.Interfaces_WMC_Series.Add_Value(sw_if.Compute_WMC)
            Next
        End If

    End Sub

    Public Sub Generate_PSWA_Metrics_Report(file_stream As StreamWriter)

        Add_Seperator(file_stream)
        file_stream.WriteLine("PSWA metrics report : " & Me.Name)
        Add_Seperator(file_stream)
        file_stream.WriteLine()
        file_stream.WriteLine()

        Add_Seperator(file_stream)
        file_stream.WriteLine("Project metrics")
        file_stream.WriteLine()
        file_stream.WriteLine(
            "Number of Packages : " &
            Me.Packages.Count)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Nb_Data_Types_Series,
            "Number of Data_Types")
        file_stream.WriteLine("    tot : " & Me.Nb_Data_Types_Series.Get_Sum)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Nb_Interfaces_Series,
            "Number of Interfaces")
        file_stream.WriteLine("    tot : " & Me.Nb_Interfaces_Series.Get_Sum)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Nb_Component_Types_Series,
            "Number of Component_Types")
            file_stream.WriteLine("    tot : " & Me.Nb_Component_Types_Series.Get_Sum)
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Documentation_Rate_Series,
            "Documentation rate")
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Distance_Series,
            "Distance")
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Component_Type_WMC_Series,
            "Component_Type WMC")
        file_stream.WriteLine()
        Me.Write_Series_Metrics(
            file_stream,
            Me.Interfaces_WMC_Series,
            "Interfaces WMC")
        Add_Seperator(file_stream)
        file_stream.WriteLine()

        For Each pkg In Me.Packages

            file_stream.WriteLine()
            Add_Seperator(file_stream)

            file_stream.WriteLine("Package : " & pkg.Name)
            file_stream.WriteLine()

            file_stream.WriteLine("Documentation rate : " &
                pkg.Get_Package_Documentation_Rate.ToString("p0"))
            file_stream.WriteLine()

            file_stream.WriteLine("Number of Data_Types : " & pkg.Get_Nb_Data_Types)
            file_stream.WriteLine("Number of Interfaces : " & pkg.Get_Nb_Interfaces)
            file_stream.WriteLine("Number of Component_Types : " & pkg.Get_Nb_Component_Types)
            file_stream.WriteLine("Number of Compositions : " & pkg.Get_Nb_Compositions)
            file_stream.WriteLine("Abstraction level : " _
                & pkg.Get_Abstraction_Level.ToString("0.00"))
            file_stream.WriteLine()

            file_stream.WriteLine("Efferent coupling : " & pkg.Get_Efferent_Coupling)
            file_stream.WriteLine("Afferent coupling : " & pkg.Get_Afferent_Coupling)
            file_stream.WriteLine("Instability : " & pkg.Get_Instability.ToString("0.00"))
            file_stream.WriteLine()

            file_stream.WriteLine("Distance : " & pkg.Get_Distance.ToString("0.00"))

            file_stream.WriteLine()
            file_stream.WriteLine("Interfaces : ")
            Dim pkg_list As List(Of Software_Package) = pkg.Get_All_Packages
            For Each current_pkg In pkg_list

                If Not IsNothing(current_pkg.Software_Interfaces) Then
                    For Each sw_if In current_pkg.Software_Interfaces
                        file_stream.WriteLine()
                        file_stream.WriteLine("    " & sw_if.Name)
                        file_stream.WriteLine("        WMC : " & sw_if.Compute_WMC())

                    Next
                End If
            Next

            file_stream.WriteLine()
            file_stream.WriteLine("Component_Types : ")
            For Each current_pkg In pkg_list
                If Not IsNothing(current_pkg.Component_Types) Then
                    For Each swct In current_pkg.Component_Types
                        file_stream.WriteLine()
                        file_stream.WriteLine("    " & swct.Name)
                        file_stream.WriteLine("        WMC : " & swct.Compute_WMC())
                    Next
                End If
            Next

        Next
    End Sub

    Private Sub Add_Seperator(file_stream As StreamWriter)
        file_stream.WriteLine("===============================================================")
    End Sub

    Private Sub Write_Series_Metrics(
        file_stream As StreamWriter,
        series As Data_Series,
        series_name As String)
        file_stream.WriteLine(series_name & " : ")
        file_stream.WriteLine("    avg : " & series.Get_Average.ToString("0.00"))
        file_stream.WriteLine("    min : " & series.Get_Min.ToString("0.00"))
        file_stream.WriteLine("    max : " & series.Get_Max.ToString("0.00"))
        file_stream.WriteLine("    dev : " & series.Get_Standard_Deviation.ToString("0.00"))
    End Sub

End Class
