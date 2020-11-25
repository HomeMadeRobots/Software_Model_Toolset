Imports System.Windows
Imports System.IO
Imports System.Environment

Public MustInherit Class SMH_Form
    Inherits Form

    Protected Const Marge As Integer = 10
    Protected Const Item_Height As Integer = 2 * Marge
    Protected Const Form_Width As Integer = 500
    Protected Const Panel_Width As Integer = Form_Width - 2 * Marge

    Protected Const Label_Width As Integer = Panel_Width - 2 * Marge
    Protected Const Label_Height As Integer = Item_Height
    Protected Shared Label_Size As New Size(Label_Width, Label_Height)

    Protected Const Path_Button_Width As Integer = 30
    Protected Const Path_Button_Height As Integer = Item_Height
    Protected Shared Path_Button_Size As New Size(Path_Button_Width, Path_Button_Height)
    Protected Const Path_Button_X_Pos As Integer = Marge + Path_Text_Width + Marge

    Protected Const Path_Text_Width As Integer = Panel_Width - Path_Button_Width - 3 * Marge
    Protected Const Path_Text_Height As Integer = Item_Height
    Protected Shared Path_Text_Size As New Size(Path_Text_Width, Path_Text_Height)

    Protected Const Button_Width As Integer = 100
    Protected Const Button_Height As Integer = 3 * Marge
    Protected Shared Button_Size As New Size(Button_Width, Button_Height)

    Protected Shared Sub Select_Directory(
            title As String,
            ByRef directory_textbox As TextBox)
        Dim dialog_box = New FolderBrowserDialog
        If Directory.Exists(directory_textbox.Text) Then
            dialog_box.SelectedPath = directory_textbox.Text
        Else
            dialog_box.SelectedPath = GetFolderPath(SpecialFolder.UserProfile)
        End If
        dialog_box.Description = title
        Dim result As DialogResult = dialog_box.ShowDialog()
        If result = DialogResult.OK Then
            directory_textbox.Text = dialog_box.SelectedPath
        End If
    End Sub

    Protected Shared Sub Select_File(
            title As String,
            ByRef file_textbox As TextBox)
        Dim dialog_box = New OpenFileDialog
        If File.Exists(file_textbox.Text) Then
            dialog_box.FileName = file_textbox.Text
        Else
            dialog_box.InitialDirectory = GetFolderPath(SpecialFolder.UserProfile)
        End If
        dialog_box.Title = title
        Dim result As DialogResult = dialog_box.ShowDialog()
        If result = DialogResult.OK Then
            file_textbox.Text = dialog_box.FileName
        End If
    End Sub

End Class
