Public Class Package_Selection_Form
    Inherits Form

    Private Pkg_Selection_Panel As New Panel
    Private WithEvents OK_Button As New Button
    Private WithEvents Select_All_Button As New Button
    Private WithEvents Unselect_All_Button As New Button
    Private Pkg_CheckBox_List As New List(Of CheckBox)

    Private Const Form_Width As Integer = 500
    Private Const Marge As Integer = 20

    Private Const Item_Width As Integer = Form_Width - 2 * Marge

    Private Const Text_Field_Height As Integer = 20
    Private Shared Text_Field_Size As New Size(Item_Width, Text_Field_Height)

    Private Shared CheckBox_Size As New Size(Item_Width - 40, Text_Field_Height) ' -40 : scroll bar

    Private Const Button_Width As Integer = 100
    Private Const Button_Height As Integer = 2 * Marge
    Private Shared Button_Size As New Size(Button_Width, Button_Height)

    Public Sub New(
        pkg_name_list As List(Of String),
        previous_pkg_selection_choices As Dictionary(Of String, String))

        Dim item_y_pos As Integer = Marge

        '------------------------------------------------------------------------------------------'
        ' Add package selection panel
        Dim package_selection_label As New Label
        package_selection_label.Text = "Top level packages"
        package_selection_label.Location = New Point(Marge, item_y_pos)
        package_selection_label.Size = Text_Field_Size
        Me.Controls.Add(package_selection_label)
        item_y_pos += Text_Field_Height

        Me.Pkg_Selection_Panel.AutoScroll = True
        Me.Pkg_Selection_Panel.Location = New Point(Marge, item_y_pos)
        Me.Pkg_Selection_Panel.Size = New Size(Item_Width, 500)
        Me.Controls.Add(Pkg_Selection_Panel)
        item_y_pos += Pkg_Selection_Panel.Size.Height + Marge

        ' Add a checkbox for each package
        Dim pkg_name As String
        Dim pkg_idx As Integer = 0
        For Each pkg_name In pkg_name_list
            Dim pkg_checkbox As New CheckBox
            Me.Pkg_Selection_Panel.Controls.Add(pkg_checkbox)
            Me.Pkg_CheckBox_List.Add(pkg_checkbox)
            pkg_checkbox.Location = New Point(0, pkg_idx * Text_Field_Height)
            pkg_checkbox.Size = CheckBox_Size
            pkg_checkbox.Text = pkg_name
            ' Check the check box if the package was previously transformed
            If previous_pkg_selection_choices.ContainsKey(pkg_name) Then
                pkg_checkbox.Checked = CBool(previous_pkg_selection_choices(pkg_name))
            End If
            pkg_idx = pkg_idx + 1
        Next

        '------------------------------------------------------------------------------------------'
        ' Add select/unselect all button
        Me.Select_All_Button.Text = "Select all"
        Me.Controls.Add(Me.Select_All_Button)
        Me.Select_All_Button.Size = Button_Size
        Me.Select_All_Button.Location = New Point(Marge, item_y_pos)

        Me.Unselect_All_Button.Text = "Unselect all"
        Me.Controls.Add(Me.Unselect_All_Button)
        Me.Unselect_All_Button.Size = Button_Size
        Me.Unselect_All_Button.Location = New Point(Form_Width - Button_Width - Marge, item_y_pos)

        item_y_pos += Button_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Add OK button
        OK_Button.Text = "Selection done"
        Me.Controls.Add(OK_Button)
        OK_Button.Size = Button_Size
        OK_Button.Location = New Point(Form_Width \ 2 - Button_Width \ 2, item_y_pos)
        item_y_pos += Button_Height + Marge

        '------------------------------------------------------------------------------------------'
        ' Design Form
        Me.Text = "Sodftware package selection"
        Me.ClientSize = New Size(Form_Width, item_y_pos)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.FormBorderStyle = FormBorderStyle.FixedDialog

    End Sub

    Private Sub Select_All() Handles Select_All_Button.Click
        Dim pkg_checkbox As CheckBox
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            pkg_checkbox.Checked = True
        Next
    End Sub

    Private Sub Unselect_All() Handles Unselect_All_Button.Click
        Dim pkg_checkbox As CheckBox
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            pkg_checkbox.Checked = False
        Next
    End Sub

    Private Sub Selection_Done() Handles OK_Button.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Public Function Get_Selected_Package_Name_List() As List(Of String)
        Dim pkg_name_list As New List(Of String)
        Dim pkg_checkbox As CheckBox
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            If pkg_checkbox.Checked = True Then
                pkg_name_list.Add(pkg_checkbox.Text)
            End If
        Next
        Return pkg_name_list
    End Function

    Public Function Get_Package_Selection_Choices() As Dictionary(Of String, String)
        Dim pkg_selection_choices As New Dictionary(Of String, String)
        For Each pkg_checkbox In Me.Pkg_CheckBox_List
            If pkg_checkbox.Checked = True Then
                pkg_selection_choices.Add(pkg_checkbox.Text, "True")
            Else
                pkg_selection_choices.Add(pkg_checkbox.Text, "False")
            End If
        Next
        Return pkg_selection_choices
    End Function

End Class
