Imports System.Guid

Module Guid_Utilities

    Public Function Transform_GUID_To_UUID(guid As String) As Guid
        Dim uuid As Guid = Nothing
        System.Guid.TryParse(guid.Substring(5), uuid)
        Return uuid
    End Function

    Public Function Transform_UUID_To_GUID(uuid As Guid) As String
        Dim guid As String = "GUID "
        guid = guid & uuid.ToString
        Return guid
    End Function

End Module
