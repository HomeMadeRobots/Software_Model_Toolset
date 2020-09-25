Imports System.Windows
Imports System.IO

Class Component_Design_Model_Creation_Form
    Inherits Form

    Private Controller As Rpy_Component_Design_Controller

    Private WithEvents Model_Directory_TxtBx As New TextBox
    Private WithEvents Model_Directory_Button As New Button
    Private WithEvents Model_Name_TxtBx As New TextBox
    Private WithEvents Create_Model_Button As New Button

    Private WithEvents Profiles_Directory_TxtBx As New TextBox
    Private WithEvents Profiles_Directory_Button As New Button

    Private WithEvents Component_Type_TxtBx As New TextBox

    Private Const Form_Width As Integer = 500
    Private Const Marge As Integer = 20
    Private Const Item_Height As Integer = 20
    Private Const Text_Width As Integer = Form_Width - 3 * Marge - Button_Width
    Private Const Button_Width As Integer = 2 * Marge
    Private Const Button_X_Pos As Integer = Text_Width + 2 * Marge
    Private Shared Item_Size As New Size(Text_Width, Item_Height)
    Private Shared Bttn_Size As New Size(Button_Width, Item_Height)

    Public Sub New(
        ctrl As Rpy_Component_Design_Controller,
        default_component_type_path As String,
        default_profiles_directory As String,
        default_model_directory As String,
        default_model_name As String)
        Dim item_y_pos As Integer = Marge

        Me.Controller = ctrl

        '------------------------------------------------------------------------------------------'
        ' Add Component_Type selection stuff
        Dim component_type_title As New Label
        component_type_title.Text = "Component_Type :"
        component_type_title.Location = New Point(Marge, item_y_pos)
        component_type_title.Size = Item_Size
        Me.Controls.Add(component_type_title)
        item_y_pos += Item_Height

        Me.Component_Type_TxtBx.Location = New Point(Marge, item_y_pos)
        Me.Component_Type_TxtBx.Size = New Size(Text_Width, Item_Height)
        Me.Component_Type_TxtBx.Text = default_component_type_path
        Me.Controls.Add(Me.Component_Type_TxtBx)

        item_y_pos += Item_Height + Marge


        '------------------------------------------------------------------------------------------'
        ' Add profiles selection stuff
        Dim profile_path_title As New Label
        profile_path_title.Text = "Profiles path :"
        profile_path_title.Location = New Point(Marge, item_y_pos)
        profile_path_title.Size = Item_Size
        Me.Controls.Add(profile_path_title)
        item_y_pos += Item_Height

        Me.Profiles_Directory_TxtBx.Location = New Point(Marge, item_y_pos)
        Me.Profiles_Directory_TxtBx.Size = New Size(Text_Width, Item_Height)
        Me.Profiles_Directory_TxtBx.Text = default_profiles_directory
        Me.Controls.Add(Me.Profiles_Directory_TxtBx)

        Me.Profiles_Directory_Button.Location = New Point(Button_X_Pos, item_y_pos)
        Me.Profiles_Directory_Button.Size = Bttn_Size
        Me.Profiles_Directory_Button.Text = "..."
        Me.Controls.Add(Me.Profiles_Directory_Button)

        item_y_pos += Item_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add model path selection stuff
        Dim model_path_title As New Label
        model_path_title.Text = "Component_Design model path :"
        model_path_title.Location = New Point(Marge, item_y_pos)
        model_path_title.Size = Item_Size
        Me.Controls.Add(model_path_title)
        item_y_pos += Item_Height

        Me.Model_Directory_TxtBx.Location = New Point(Marge, item_y_pos)
        Me.Model_Directory_TxtBx.Size = New Size(Text_Width, Item_Height)
        Me.Model_Directory_TxtBx.Text = default_model_directory
        Me.Controls.Add(Me.Model_Directory_TxtBx)

        Me.Model_Directory_Button.Location = New Point(Button_X_Pos, item_y_pos)
        Me.Model_Directory_Button.Size = Bttn_Size
        Me.Model_Directory_Button.Text = "..."
        Me.Controls.Add(Me.Model_Directory_Button)

        item_y_pos += Item_Height + Marge


        '------------------------------------------------------------------------------------------'
        ' Add model name stuff
        Dim model_name_title As New Label
        model_name_title.Text = "Model name :"
        model_name_title.Location = New Point(Marge, item_y_pos)
        model_name_title.Size = Item_Size
        Me.Controls.Add(model_name_title)
        item_y_pos += Item_Height

        Me.Model_Name_TxtBx.Location = New Point(Marge, item_y_pos)
        Me.Model_Name_TxtBx.Size = New Size(Text_Width, Item_Height)
        Me.Model_Name_TxtBx.Text = default_model_name
        Me.Controls.Add(Me.Model_Name_TxtBx)

        item_y_pos += Item_Height + Marge


        '------------------------------------------------------------------------------------------'
        ' Add main button
        Me.Create_Model_Button.Text = "Create model"
        Me.Controls.Add(Me.Create_Model_Button)
        Me.Create_Model_Button.Size = New Size(100, Item_Height * 2)
        Me.Create_Model_Button.Location = New Point(Form_Width \ 2 - Button_Width \ 2, item_y_pos)
        item_y_pos += Me.Create_Model_Button.Size.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Create Component_Design model"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.FontHeight = 40

    End Sub

    Private Sub Model_Path_Selection_Button_Clicked() Handles Model_Directory_Button.Click
        Dim dialog_box As FolderBrowserDialog
        dialog_box = New FolderBrowserDialog
        If Directory.Exists(Me.Model_Directory_TxtBx.Text) Then
            dialog_box.SelectedPath = Me.Model_Directory_TxtBx.Text
        Else
            dialog_box.SelectedPath =
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        End If
        dialog_box.Description = "Select Component_Design model directory"
        Dim result As Forms.DialogResult = dialog_box.ShowDialog()
        If result = Forms.DialogResult.OK Then
            Me.Model_Directory_TxtBx.Text = dialog_box.SelectedPath
        End If
    End Sub

    Private Sub Profiles_Path_Selection_Button_Clicked() Handles Profiles_Directory_Button.Click
        Dim dialog_box As FolderBrowserDialog
        dialog_box = New FolderBrowserDialog
        If Directory.Exists(Me.Profiles_Directory_TxtBx.Text) Then
            dialog_box.SelectedPath = Me.Profiles_Directory_TxtBx.Text
        Else
            dialog_box.SelectedPath =
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        End If
        dialog_box.Description = "Select Profiles directory"
        Dim result As Forms.DialogResult = dialog_box.ShowDialog()
        If result = Forms.DialogResult.OK Then
            Me.Profiles_Directory_TxtBx.Text = dialog_box.SelectedPath
        End If
    End Sub

    Private Sub Create_Model_Button_Clicked() Handles Create_Model_Button.Click
        If Directory.Exists(Me.Model_Directory_TxtBx.Text) Then
            Me.Controller.Create_Component_Design_Model(
                Me.Component_Type_TxtBx.Text,
                Me.Profiles_Directory_TxtBx.Text,
                Me.Model_Directory_TxtBx.Text,
                Me.Model_Name_TxtBx.Text)
        Else
            MsgBox("The Component_Design model path shall be fulfilled and existing.",
                MsgBoxStyle.Exclamation,
                "Component_Design model creation")
        End If
    End Sub

End Class
