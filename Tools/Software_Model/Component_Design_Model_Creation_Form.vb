Imports System.Windows
Imports System.IO

Class Component_Design_Model_Creation_Form
    Inherits SMH_Form

    Private Controller As Rpy_Component_Design_Controller

    Private WithEvents Model_Directory_TxtBx As New TextBox
    Private WithEvents Model_Directory_Button As New Button
    Private WithEvents Model_Name_TxtBx As New TextBox
    Private WithEvents Create_Model_Button As New Button

    Private WithEvents Profiles_Directory_TxtBx As New TextBox
    Private WithEvents Profiles_Directory_Button As New Button

    Private WithEvents Component_Type_TxtBx As New TextBox

    Public Sub New(
            ctrl As Rpy_Component_Design_Controller,
            default_component_type_path As String,
            default_profiles_directory As String,
            default_model_directory As String,
            default_model_name As String)

        Dim item_y_pos As Integer = Marge
        Dim inner_item_y_pos As Integer = Marge

        Me.Controller = ctrl

        Dim main_panel As New Panel
        main_panel.Location = New Point(Marge, item_y_pos)
        main_panel.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(main_panel)

        '------------------------------------------------------------------------------------------'
        ' Add Component_Type selection stuff
        Dim component_type_title As New Label
        component_type_title.Text = "Component_Type :"
        component_type_title.Location = New Point(Marge, inner_item_y_pos)
        component_type_title.Size = Label_Size
        main_panel.Controls.Add(component_type_title)
        inner_item_y_pos += component_type_title.Height

        Me.Component_Type_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Component_Type_TxtBx.Size = Path_Text_Size
        Me.Component_Type_TxtBx.Text = default_component_type_path
        main_panel.Controls.Add(Me.Component_Type_TxtBx)
        inner_item_y_pos += Me.Component_Type_TxtBx.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add profiles selection stuff
        Dim profile_path_title As New Label
        profile_path_title.Text = "Profiles path :"
        profile_path_title.Location = New Point(Marge, inner_item_y_pos)
        profile_path_title.Size = Label_Size
        main_panel.Controls.Add(profile_path_title)
        inner_item_y_pos += profile_path_title.Height

        Me.Profiles_Directory_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Profiles_Directory_TxtBx.Size = Path_Text_Size
        Me.Profiles_Directory_TxtBx.Text = default_profiles_directory
        main_panel.Controls.Add(Me.Profiles_Directory_TxtBx)

        Me.Profiles_Directory_Button.Location = New Point(Path_Button_X_Pos, inner_item_y_pos)
        Me.Profiles_Directory_Button.Size = Path_Button_Size
        Me.Profiles_Directory_Button.Text = "..."
        main_panel.Controls.Add(Me.Profiles_Directory_Button)
        inner_item_y_pos += Me.Profiles_Directory_Button.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add model path selection stuff
        Dim model_path_title As New Label
        model_path_title.Text = "Component_Design model path :"
        model_path_title.Location = New Point(Marge, inner_item_y_pos)
        model_path_title.Size = Label_Size
        main_panel.Controls.Add(model_path_title)
        inner_item_y_pos += model_path_title.Height

        Me.Model_Directory_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Model_Directory_TxtBx.Size = Path_Text_Size
        Me.Model_Directory_TxtBx.Text = default_model_directory
        main_panel.Controls.Add(Me.Model_Directory_TxtBx)

        Me.Model_Directory_Button.Location = New Point(Path_Button_X_Pos, inner_item_y_pos)
        Me.Model_Directory_Button.Size = Path_Button_Size
        Me.Model_Directory_Button.Text = "..."
        main_panel.Controls.Add(Me.Model_Directory_Button)
        inner_item_y_pos += Me.Model_Directory_Button.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add model name stuff
        Dim model_name_title As New Label
        model_name_title.Text = "Model name :"
        model_name_title.Location = New Point(Marge, inner_item_y_pos)
        model_name_title.Size = Label_Size
        main_panel.Controls.Add(model_name_title)
        inner_item_y_pos += model_name_title.Height

        Me.Model_Name_TxtBx.Location = New Point(Marge, inner_item_y_pos)
        Me.Model_Name_TxtBx.Size = Path_Text_Size
        Me.Model_Name_TxtBx.Text = default_model_name
        main_panel.Controls.Add(Me.Model_Name_TxtBx)
        inner_item_y_pos += Me.Model_Name_TxtBx.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add main button
        Me.Create_Model_Button.Text = "Create model"
        main_panel.Controls.Add(Me.Create_Model_Button)
        Me.Create_Model_Button.Size = Button_Size
        Me.Create_Model_Button.Location = New Point(
            (Form_Width - Button_Width) \ 2, inner_item_y_pos)
        inner_item_y_pos += Me.Create_Model_Button.Height + Marge

        main_panel.Size = New Size(Panel_Width, inner_item_y_pos)
        item_y_pos += main_panel.Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Create Component_Design model"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False

    End Sub

    Private Sub Model_Path_Selection_Button_Clicked() Handles Model_Directory_Button.Click
        SMH_Form.Select_Directory(
            "Select Component_Design model directory",
            Me.Model_Directory_TxtBx)
    End Sub

    Private Sub Profiles_Path_Selection_Button_Clicked() Handles Profiles_Directory_Button.Click
        SMH_Form.Select_Directory("Select Profiles directory", Me.Profiles_Directory_TxtBx)
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
