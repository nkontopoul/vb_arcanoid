Public NotInheritable Class ShFunctionality
    Public Shared Function GetClosest(ByVal ptCenter As Point, ByVal pts() As Point) As Integer ' Int defines the index
        Dim closest, pt As Point
        For Each pt In pts
            If IsNothing(closest) Then
                closest = pt
                Continue For
            End If
            If (pt.X - ptCenter.X) < (closest.X - ptCenter.X) Then
                If (pt.Y - ptCenter.Y) <= (closest.Y - ptCenter.Y) Then
                    closest = pt
                End If
            ElseIf (pt.Y - ptCenter.Y) < (closest.Y - ptCenter.Y) Then
                If (pt.X - ptCenter.X) <= (closest.X - ptCenter.X) Then
                    closest = pt
                End If
            End If
        Next
        Return Array.IndexOf(Of Point)(pts, closest)
    End Function
    Public Enum RectPtsIndex
        TopLeft = 0
        TopRight = 1
        BottomLeft = 3
        BottomRight = 2
    End Enum
    Public Shared Function GetRectPoints(ByVal rect As Rectangle) As Point()
        Dim rPts() As Point = {rect.Location, New Point(rect.X + rect.Width, rect.Y), _
        New Point(rect.X + rect.Width, rect.Y + rect.Height), New Point(rect.X, rect.Y + rect.Height)}
        Return rPts
    End Function
    Public Shared Function PointsInside(ByVal pts() As Point, ByVal rect As Rectangle) As List(Of Integer)
        Dim pt As Point, l As List(Of Integer)
        l = New List(Of Integer)
        For Each pt In pts
            If rect.Contains(pt) Then
                l.Add(Array.IndexOf(pts, pt))
            End If
        Next
        Return l
    End Function
End Class
