Imports rhapsody2

Public Class Software_Model_Container

    Inherits Software_Element

    Public PSWA_Packages As List(Of PSWA_Package)

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
        Dim children As List(Of Software_Element) = Nothing
        If Not IsNothing(Me.PSWA_Packages) Then
            children = New List(Of Software_Element)
            For Each pkg In Me.PSWA_Packages
                children.Add(pkg)
            Next
        End If
        Return children
    End Function

    Protected Overrides Sub Get_Own_Data_From_Rhapsody_Model()
        MyBase.Get_Own_Data_From_Rhapsody_Model()

        ' Create "virtual" PSWA_Package named "Basic_Types"
        Dim basic_types_pkg As New PSWA_Package
        basic_types_pkg.Name = "Basic_Types"
        basic_types_pkg.Description = "Automatically created PSWA_Package to gathers Basic_Types."
        basic_types_pkg.Data_Types = New List(Of Data_Type)
        ' !!! IMPORTANT NOTE !!! '
        ' basic_types_pkg is not added to Me.PSWA_Packages nor to Elements_Dictionary_By_Uuid but
        ' all its aggregated Data_Types are added to Elements_Dictionary_By_Uuid.
        ' It allows to make Basic_Types available for other methods.

        ' Add Basic_Types
        Dim type As Data_Type
        Dim basic_type_idx As Integer
        ' Treat Basic_Integer_Types
        Dim nb_int_types As Integer = Basic_Integer_Type_Name_List.Count
        For basic_type_idx = 0 To nb_int_types - 1
            type = New Basic_Integer_Type
            type.Name = Basic_Integer_Type_Name_List(basic_type_idx)
            Guid.TryParse(Basic_Integer_Type_Uuid_List(basic_type_idx), type.UUID)
            basic_types_pkg.Data_Types.Add(type)
            Me.Add_Element(type)
        Next
        ' Treat Basic_Floating_Point_Types
        Dim nb_fp_types As Integer = Basic_Floating_Type_Name_List.Count
        For basic_type_idx = 0 To nb_fp_types - 1
            type = New Basic_Floating_Point_Type
            type.Name = Basic_Floating_Type_Name_List(basic_type_idx)
            Guid.TryParse(Basic_Floating_Type_Uuid_List(basic_type_idx), type.UUID)
            basic_types_pkg.Data_Types.Add(type)
            Me.Add_Element(type)
        Next
        ' Treat Basic_Boolean_Type
        type = New Basic_Boolean_Type
        type.Name = "boolean"
        Guid.TryParse("5df8e979-be4c-4790-87a7-f8ee053c4162", type.UUID)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)
        ' Treat Basic_Integer_Array_Types
        type = New Basic_Integer_Array_Type
        type.Name = "uint8_array"
        Guid.TryParse("b86a2bc4-2c3f-4cff-9217-4a07af95fdd2", type.UUID)
        basic_types_pkg.Data_Types.Add(type)
        Me.Add_Element(type)

    End Sub

    Protected Overrides Sub Import_Children_From_Rhapsody_Model()

        Me.PSWA_Packages = New List(Of PSWA_Package)

        Dim rpy_pkg As RPPackage
        For Each rpy_pkg In CType(Me.Rpy_Element, RPPackage).packages
            If Is_PSWA_Package(CType(rpy_pkg, RPModelElement)) Then
                Dim pswa_pkg As PSWA_Package = New PSWA_Package
                Me.PSWA_Packages.Add(pswa_pkg)
                pswa_pkg.Import_From_Rhapsody_Model(Me, CType(rpy_pkg, RPModelElement))
            End If
        Next

    End Sub

End Class
