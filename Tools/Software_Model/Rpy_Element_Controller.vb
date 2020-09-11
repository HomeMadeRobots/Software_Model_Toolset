Imports rhapsody2

Public Class Rpy_Element_Controller
    Inherits Rpy_Controller

    Public Sub Display_Rpy_Element_GUID()
        Dim elmt_guid As String
        Dim name As String
        Dim rpy_element As RPModelElement

        Me.Clear_Window()
        rpy_element = Me.Rhapsody_App.getSelectedElement
        elmt_guid = rpy_element.GUID
        name = rpy_element.name
        Me.Write_Csl_Line("GUID of " & name & " : " & elmt_guid)
    End Sub

    Public Sub Modify_Rpy_Element_GUID()
        Dim rpy_element As RPModelElement
        rpy_element = Me.Rhapsody_App.getSelectedElement
        Dim new_guid = InputBox("Enter new GUID", "GUID modification")
        If new_guid <> "" Then
            rpy_element.GUID = new_guid
        End If
    End Sub

End Class
