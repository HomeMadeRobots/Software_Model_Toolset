Imports System.Guid

Module Guid_Utilities

    Public Function Transform_GUID_To_UUID(guid As String) As Guid
        Dim uuid As Guid = Nothing
        System.Guid.TryParse(guid.Substring(5), uuid)
        Return uuid
    End Function

End Module
