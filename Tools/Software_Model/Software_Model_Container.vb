Imports rhapsody2

Public Class Software_Model_Container

    Inherits Software_Element

    Public PSWA_Packages As List(Of PSWA_Package)

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
