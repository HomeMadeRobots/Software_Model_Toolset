Public Interface Dependent_Element
    Function Get_Needed_Element() As List(Of Dependent_Element)
    Function Get_Uuid() As Guid
    Function Get_Name() As String
End Interface

Public Class Cyclic_Dependencies_Manager

    Private Cycles_List As New Dictionary(Of Integer, List(Of List(Of Dependent_Element)))

    Public Sub Find_Cyclic_Dependencies(ByRef elmt_list As List(Of Dependent_Element))
        For Each start_elmt In elmt_list
            Dim current_elmt_path As New List(Of Dependent_Element)
            Dim start_elmt_cyclic_dep_list As New List(Of List(Of Dependent_Element))
            Me.Complete_Cyclic_Dependency_Path(
                start_elmt,
                start_elmt.Get_Uuid,
                start_elmt_cyclic_dep_list,
                current_elmt_path)
            Me.Add_Cyclic_Dependencies_List(start_elmt_cyclic_dep_list)
        Next
    End Sub

    Public Function Get_Cycles_List() As List(Of List(Of Dependent_Element))
        Dim result As New List(Of List(Of Dependent_Element))
        For Each key In Me.Cycles_List.Keys
            result.AddRange(Me.Cycles_List(key))
        Next
        Return result
    End Function

    Public Function Get_Cycles_List_String() As List(Of String)
        Dim result As New List(Of String)
        Dim list_of_dep_list As List(Of List(Of Dependent_Element))
        For Each list_of_dep_list In Me.Cycles_List.Values
            For Each dep_list In list_of_dep_list
                Dim path_str As String = ""
                For Each elmt In dep_list
                   path_str &= elmt.Get_Name & "->"
                Next
                path_str &= dep_list.First.Get_Name
                result.Add(path_str)
            Next
        Next
        Return result
    End Function

    Private Sub Add_Cyclic_Dependencies_List(cyclic_dep_list As List(Of List(Of Dependent_Element)))
        For Each dep_list In cyclic_dep_list

            Dim dep_list_size As Integer = dep_list.Count

            ' "Sort" list
            Dim first_node As Dependent_Element = dep_list.First
            Dim first_node_idx As Integer = 0
            For idx = 1 To dep_list_size - 1
                If String.Compare(first_node.Get_Name, dep_list.Item(idx).Get_Name) >= 1 Then
                    first_node = dep_list.Item(idx)
                    first_node_idx = idx
                End If
            Next
            Dim list_to_add As New List(Of Dependent_Element)
            list_to_add.AddRange(dep_list.GetRange(first_node_idx, dep_list_size - first_node_idx))
            list_to_add.AddRange(dep_list.GetRange(0, first_node_idx))


            If Me.Cycles_List.ContainsKey(dep_list_size) Then
                Dim same_size_dep_list As List(Of List(Of Dependent_Element))
                same_size_dep_list = Me.Cycles_List(dep_list_size)
                Dim found_identical As Boolean = False
                For Each current_dep_list In same_size_dep_list
                    If list_to_add.SequenceEqual(current_dep_list) Then
                        found_identical = True
                        Exit For
                    End If
                Next
                If found_identical = False Then
                    Me.Cycles_List(dep_list_size).Add(list_to_add)
                End If
            Else
                Dim new_list As New List(Of List(Of Dependent_Element))
                new_list.Add(list_to_add)
                Me.Cycles_List.Add(dep_list_size, new_list)
            End If
        Next
    End Sub

    Private Sub Complete_Cyclic_Dependency_Path(
        current_elmt As Dependent_Element,
        start_elmt_uuid As Guid,
        start_elmt_cyclic_dep_list As List(Of List(Of Dependent_Element)),
        current_elmt_path As List(Of Dependent_Element))

        For Each elmt In current_elmt.Get_Needed_Element
            If Not current_elmt_path.Contains(elmt) Then
                Dim local_current_elmt_path As New List(Of Dependent_Element)
                local_current_elmt_path.AddRange(current_elmt_path)
                local_current_elmt_path.Add(elmt)
                If elmt.Get_Uuid = start_elmt_uuid Then
                    ' Cyclic dependency found
                    start_elmt_cyclic_dep_list.Add(local_current_elmt_path)
                End If
                Me.Complete_Cyclic_Dependency_Path(
                    elmt,
                    start_elmt_uuid,
                    start_elmt_cyclic_dep_list,
                    local_current_elmt_path)
            End If
        Next
    End Sub

End Class
