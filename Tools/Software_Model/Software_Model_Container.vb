Imports rhapsody2
Imports System.Xml.Serialization

Public Class Software_Model_Container

    Inherits Software_Element

    <XmlArrayItem("PSWA_Package")>
    Public PSWA_Packages As List(Of Top_Level_PSWA_Package)

    Private Elements_Dictionary_By_Uuid As New Dictionary(Of Guid, Software_Element)

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

    Private Nb_Interfaces As Data_Series
    Private Nb_Component_Types As Data_Series
    Private Nb_Data_Types As Data_Series

    Private Documentation_Rate As Data_Series
    Private Distance As Data_Series
    Private Component_Type_WMC As Data_Series
    Private Interfaces_WMC As Data_Series

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
            If Not IsNothing(Me.PSWA_Packages) Then
                children_list.AddRange(Me.PSWA_Packages)
            End If
            Me.Children = children_list
        End If
        Return Me.Children
    End Function

    Function Get_All_Compositions() As List(Of Root_Software_Composition)
        If IsNothing(Me.Compositions_List) Then
            Me.Compositions_List = New List(Of Root_Software_Composition)
            For Each top_pkg In Me.PSWA_Packages
                Dim all_pkg_list As List(Of PSWA_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    If Not IsNothing(pkg.Root_Software_Compositions) Then
                        Me.Compositions_List.AddRange(pkg.Root_Software_Compositions)
                    End If
                Next
            Next
        End If
        Return Me.Compositions_List
    End Function

    Function Get_All_Component_Types() As List(Of Component_Type)
        If IsNothing(Me.Component_Types_List) Then
            Me.Component_Types_List = New List(Of Component_Type)
            For Each top_pkg In Me.PSWA_Packages
                Dim all_pkg_list As List(Of PSWA_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    If Not IsNothing(pkg.Component_Types) Then
                        Me.Component_Types_List.AddRange(pkg.Component_Types)
                    End If
                Next
            Next
        End If
        Return Me.Component_Types_List
    End Function

    Function Get_All_Interfaces() As List(Of Software_Interface)
        If IsNothing(Me.Interfaces_List) Then
            Me.Interfaces_List = New List(Of Software_Interface)
            For Each top_pkg In Me.PSWA_Packages
                Dim all_pkg_list As List(Of PSWA_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    If Not IsNothing(pkg.Software_Interfaces) Then
                        Me.Interfaces_List.AddRange(pkg.Software_Interfaces)
                    End If
                Next
            Next
        End If
        Return Me.Interfaces_List
    End Function

    Function Get_All_Data_Types() As List(Of Data_Type)
        If IsNothing(Me.Data_Types_List) Then
            Me.Data_Types_List = New List(Of Data_Type)
            For Each top_pkg In Me.PSWA_Packages
                Dim all_pkg_list As List(Of PSWA_Package) = top_pkg.Get_All_Packages
                For Each pkg In all_pkg_list
                    If Not IsNothing(pkg.Data_Types) Then
                        Me.Data_Types_List.AddRange(pkg.Data_Types)
                    End If
                Next
            Next
        End If
        Return Me.Data_Types_List
    End Function


    '----------------------------------------------------------------------------------------------'
    ' Methods for model import from Rhapsody
    Public Sub Import_All_From_Rhapsody_Model(rpy_proj As RPProject)

        Me.Rpy_Element = CType(rpy_proj, RPModelElement)

        Me.Get_Own_Data_From_Rhapsody_Model()

        Me.Path = ""

        Me.Import_Children_From_Rhapsody_Model()

    End Sub

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Create "virtual" PSWA_Package named "Basic_Types"
        Dim basic_types_pkg As New Top_Level_PSWA_Package
        basic_types_pkg.Name = "Basic_Types"
        basic_types_pkg.Description = "Automatically created PSWA_Package to gathers Basic_Types."
        basic_types_pkg.UUID = Guid.NewGuid
        basic_types_pkg.Data_Types = New List(Of Data_Type)
        ' !!! IMPORTANT NOTE !!! '
        ' basic_types_pkg is not added to Me.PSWA_Packages nor to Elements_Dictionary_By_Uuid but
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

        Me.PSWA_Packages = New List(Of Top_Level_PSWA_Package)

        Dim rpy_pkg As RPPackage
        For Each rpy_pkg In CType(Me.Rpy_Element, RPPackage).packages
            If Is_PSWA_Package(CType(rpy_pkg, RPModelElement)) Then
                Dim pswa_pkg As Top_Level_PSWA_Package = New Top_Level_PSWA_Package
                Me.PSWA_Packages.Add(pswa_pkg)
                Me.Top_Package = pswa_pkg
                pswa_pkg.Container = Me
                pswa_pkg.Import_From_Rhapsody_Model(Me, CType(rpy_pkg, RPModelElement))
            End If
        Next

        Me.Top_Package = Nothing

    End Sub


    '----------------------------------------------------------------------------------------------'
    ' Methods for metrics computation
    Public Sub Compute_Metrics()
        Me.Nb_Interfaces = New Data_Series
        Me.Nb_Component_Types = New Data_Series
        Me.Nb_Data_Types = New Data_Series

        Me.Documentation_Rate = New Data_Series
        Me.Distance = New Data_Series
        Me.Component_Type_WMC = New Data_Series
        Me.Interfaces_WMC = New Data_Series

        For Each pkg In Me.PSWA_Packages

            Me.Documentation_Rate.Add_Value(pkg.Get_Package_Documentation_Rate())

            pkg.Compute_Nb_Classifiers()
            Me.Nb_Interfaces.Add_Value(pkg.Get_Nb_Interfaces)
            Me.Nb_Component_Types.Add_Value(pkg.Get_Nb_Component_Types)
            Me.Nb_Data_Types.Add_Value(pkg.Get_Nb_Data_Types)

            pkg.Find_Needed_Elements()
            pkg.Find_Dependent_Elements()
            pkg.Compute_Coupling()
            Me.Distance.Add_Value(pkg.Get_Distance)
        Next

        For Each swct In Me.Component_Types_List
            Me.Component_Type_WMC.Add_Value(swct.Compute_WMC)
        Next

        For Each sw_if In Me.Interfaces_List
            Me.Interfaces_WMC.Add_Value(sw_if.Compute_WMC)
        Next

    End Sub

    Public Function Get_Documentation_Rate_Series() As Data_Series
        Return Me.Documentation_Rate
    End Function

    Public Function Get_Distance_Series() As Data_Series
        Return Me.Distance
    End Function

    Public Function Get_Component_Type_WMC_Series() As Data_Series
        Return Me.Component_Type_WMC
    End Function

    Public Function Get_Interfaces_WMC_Series() As Data_Series
        Return Me.Interfaces_WMC
    End Function

    Public Function Get_Nb_Interfaces_Series() As Data_Series
        Return Me.Nb_Interfaces
    End Function

    Public Function Get_Nb_Component_Types_Series() As Data_Series
        Return Me.Nb_Component_Types
    End Function

    Public Function Get_Nb_Data_Types_Series() As Data_Series
        Return Me.Nb_Data_Types
    End Function

End Class
