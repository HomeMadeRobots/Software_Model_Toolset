Public Class Data_Series

    Private Data As New List(Of Double)


    Public Sub Add_Value(new_val As Double)
        Data.Add(new_val)
    End Sub

    Public Function Get_Max() As Double
        If Me.Data.Count <> 0 Then
            Return Me.Data.Max
        Else
            Return -1
        End If
    End Function

    Public Function Get_Min() As Double
        If Me.Data.Count <> 0 Then
            Return Me.Data.Min
        Else
            Return -1
        End If
    End Function

    Public Function Get_Average() As Double
        If Me.Data.Count <> 0 Then
            Return Me.Data.Average
        Else
            Return -1
        End If
    End Function

    Public Function Get_Sum() As Double
        If Me.Data.Count <> 0 Then
            Return Me.Data.Sum
        Else
            Return -1
        End If
    End Function

    Public Function Get_Standard_Deviation() As Double
        If Me.Data.Count <> 0 Then
            Dim std_dev As Double
            Dim average As Double = Me.Get_Average
            For index = 0 To Me.Data.Count - 1
                std_dev += (Data(index) - average) * (Data(index) - average)
            Next
            std_dev = Math.Sqrt(std_dev / Me.Data.Count)
            Return std_dev
        Else
            Return -1
        End If
    End Function

End Class
